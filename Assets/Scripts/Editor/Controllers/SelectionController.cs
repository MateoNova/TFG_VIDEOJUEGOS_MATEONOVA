using System;
using System.Collections.Generic;
using Editor.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Controllers
{
    /// <summary>
    /// Manages the selection of generators within the editor.
    /// Handles generator initialization, selection, and caching.
    /// </summary>
    public class SelectionController
    {
        #region Constants

        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";

        #endregion

        #region Fields

        private int _selectedGeneratorIndex;
        private List<BaseGenerator> _generators = new();
        private static GameObject _cachedPrefab;
        private List<string> _cachedGeneratorNames = new();

        /// <summary>
        /// Event to toggle the visibility of the "Open Graph Window" button.
        /// </summary>
        public static Action<bool> ShowButtonOpenGraphWindow;

        /// <summary>
        /// The cached generation manager instance.
        /// </summary>
        private GameObject CachedGenerationManager { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the cached generator names.
        /// </summary>
        public List<string> CachedGeneratorNames() => _cachedGeneratorNames;

        /// <summary>
        /// Gets the index of the currently selected generator.
        /// </summary>
        public int SelectedGeneratorIndex() => _selectedGeneratorIndex;

        #endregion

        #region Constructor and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionController"/> class.
        /// Subscribes to initialization and cache clearing events.
        /// </summary>
        public SelectionController()
        {
            InitializationController.OnInitScene += InitScene;
            InitializationController.OnClearCachedData += ClearCacheData;
        }

        /// <summary>
        /// Finalizer for the <see cref="SelectionController"/> class.
        /// Unsubscribes from initialization and cache clearing events to prevent memory leaks.
        /// </summary>
        ~SelectionController()
        {
            InitializationController.OnInitScene -= InitScene;
            InitializationController.OnClearCachedData -= ClearCacheData;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes the selected generator based on the provided name.
        /// </summary>
        /// <param name="evtNewValue">The name of the new generator to select.</param>
        public void changeGenerator(string evtNewValue)
        {
            _selectedGeneratorIndex = _cachedGeneratorNames.IndexOf(evtNewValue);
            SelectGenerator(_selectedGeneratorIndex);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the scene by retrieving or initializing the generation manager,
        /// finding all generators, and selecting the first one.
        /// </summary>
        private void InitScene()
        {
            RetrieveOrInitializeCachedGenerationManager();
            FindAllGenerators();
            SelectGenerator(0);
        }

        /// <summary>
        /// Selects a generator by its index.
        /// </summary>
        /// <param name="index">The index of the generator to select.</param>
        private void SelectGenerator(int index)
        {
            if (index < 0 || index >= _generators.Count)
            {
                Debug.LogWarning("Index out of range.");
                return;
            }

            _selectedGeneratorIndex = index;
            var selectedGenerator = _generators[_selectedGeneratorIndex];
            GeneratorService.Instance.SetCurrentGenerator(selectedGenerator);

            // Check if the selected generator has the OpenGraphEditor attribute
            if (Attribute.IsDefined(GeneratorService.Instance.CurrentGenerator.GetType(),
                    typeof(OpenGraphEditorAttribute)))
            {
                ShowButtonOpenGraphWindow?.Invoke(true);
            }
            else if (GeneratorSettings.GetShowOpenGraphButton())
            {
                ShowButtonOpenGraphWindow?.Invoke(false);
            }
        }

        /// <summary>
        /// Finds all generators in the scene and caches their names.
        /// </summary>
        private void FindAllGenerators()
        {
            if (CachedGenerationManager != null)
            {
                _generators = new List<BaseGenerator>(CachedGenerationManager.GetComponentsInChildren<BaseGenerator>());
                _cachedGeneratorNames = GetGeneratorNames();

                if (_cachedGeneratorNames.Count > 0)
                {
                    EditorPrefs.SetString(CachedGeneratorNamesKey, string.Join(",", _cachedGeneratorNames));
                }
            }
            else
            {
                Debug.LogWarning("Generation Manager not found in the scene.");
                ClearGeneratorLists();
            }
        }

        /// <summary>
        /// Retrieves the names of all generators.
        /// </summary>
        /// <returns>A list of generator names.</returns>
        private List<string> GetGeneratorNames()
        {
            var names = new List<string>();
            foreach (var generator in _generators)
            {
                if (generator)
                {
                    names.Add(generator.name);
                }
                else
                {
                    Debug.LogWarning("Generator not found.");
                }
            }

            return names;
        }

        /// <summary>
        /// Clears the lists of generators and cached generator names.
        /// </summary>
        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        /// <summary>
        /// Retrieves or initializes the cached generation manager.
        /// </summary>
        private void RetrieveOrInitializeCachedGenerationManager()
        {
            CachedGenerationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
        }

        /// <summary>
        /// Retrieves the cached generation manager.
        /// </summary>
        /// <returns>The cached generation manager, or null if not found.</returns>
        private static GameObject RetrieveCachedGenerationManager()
        {
            var cachedId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
            return cachedId != -1 ? EditorUtility.InstanceIDToObject(cachedId) as GameObject : null;
        }

        /// <summary>
        /// Instantiates the generation manager from the prefab.
        /// </summary>
        /// <returns>The instantiated generation manager.</returns>
        private GameObject InstantiateGenerationManager()
        {
            if (_cachedPrefab == null)
            {
                _cachedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            if (_cachedPrefab == null)
            {
                Debug.LogError($"Prefab wasn't found at path: {PrefabPath}");
                return null;
            }

            CachedGenerationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, CachedGenerationManager.GetInstanceID());
            return CachedGenerationManager;
        }

        /// <summary>
        /// Clears the cached data, including the generation manager and generator lists.
        /// </summary>
        private void ClearCacheData()
        {
            if (CachedGenerationManager)
            {
                Object.DestroyImmediate(CachedGenerationManager);
            }

            CachedGenerationManager = null;
            _cachedPrefab = null;
            GeneratorService.Instance.SetCurrentGenerator(null);
            _cachedGeneratorNames.Clear();
            _generators.Clear();
            _selectedGeneratorIndex = 0;
        }

        #endregion
    }
}