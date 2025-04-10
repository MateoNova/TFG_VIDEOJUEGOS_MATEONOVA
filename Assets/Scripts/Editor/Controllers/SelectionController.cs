using System;
using System.Collections.Generic;
using Editor.Models;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Controllers
{
    /// <summary>
    /// Manages the selection of generators within the editor.
    /// </summary>
    public class SelectionController
    {
        #region Fields

        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";

        private int _selectedGeneratorIndex;
        private List<BaseGenerator> _generators = new();
        private static GameObject _cachedPrefab;
        private List<string> _cachedGeneratorNames = new();

        public List<String> getCached()
        {
            return _cachedGeneratorNames;
        }
        public int SelectedGeneratorIndex => _selectedGeneratorIndex;

        public static Action<bool> ShowButtonOpenGraphWindow;
        
        private GameObject CachedGenerationManager { get; set; }


        public SelectionController()
        {
            InitializationController._onInitScene += InitScene;
            InitializationController._onClearCachedData += ClearCacheData;
            
        }
        
        ~SelectionController()
        {
            InitializationController._onInitScene -= InitScene;
            InitializationController._onClearCachedData -= ClearCacheData;

        }
        
        
        

        private void InitScene()
        {
            RetrieveOrInitializeCachedGenerationManager();
            FindAllGenerators();
            SelectGenerator(0);
        }

        #endregion

        #region Methods

     

        /// <summary>
        /// Selects a generator by its index.
        /// </summary>
        /// <param name="index">The index of the generator to select.</param>
        public void SelectGenerator(int index)
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
            switch (Attribute.IsDefined( GeneratorService.Instance.CurrentGenerator.GetType(), typeof(OpenGraphEditorAttribute)))
            {
                case true:
                    ShowButtonOpenGraphWindow?.Invoke(true);
                    break;
                case false when GeneratorSettings.GetShowOpenGraphButton():
                    ShowButtonOpenGraphWindow?.Invoke(false);
                    break;
            }

          
            
        }

        /// <summary>
        /// Finds all generators in the scene.
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
                Debug.LogWarning("Generation Manager no encontrado en la escena.");
                ClearGeneratorLists();
            }
        }

        /// <summary>
        /// Gets the names of all generators.
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
            if (cachedId != -1)
            {
                return EditorUtility.InstanceIDToObject(cachedId) as GameObject;
            }

            return null;
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
                Debug.LogError($"Prefab wasn't found in route: {PrefabPath}");
                return null;
            }

            CachedGenerationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, CachedGenerationManager.GetInstanceID());
            return CachedGenerationManager;
        }

        /// <summary>
        /// Clears the cached data.
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

        public void changeGenerator(string evtNewValue)
        {
            _selectedGeneratorIndex = _cachedGeneratorNames.IndexOf(evtNewValue);
            SelectGenerator(_selectedGeneratorIndex);
        }
    }
}
