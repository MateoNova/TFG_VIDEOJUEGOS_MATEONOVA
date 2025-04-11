using System;
using System.Collections.Generic;
using System.Linq;
using Controllers.Generators;
using Editor.Models;
using Generators.Controllers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Controllers
{
    /// <summary>
    /// Controller responsible for managing the selection of generators in the editor.
    /// Handles initialization, caching, and generator selection logic.
    /// </summary>
    public class SelectionController
    {
        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";

        private int _selectedGeneratorIndex;
        private List<BaseGenerator> _generators = new();
        private static GameObject _cachedPrefab;
        private List<string> _cachedGeneratorNames = new();
        private GameObject _generationManager;

        /// <summary>
        /// Gets the cached generator names for display in the UI.
        /// </summary>
        /// <returns>A list of cached generator names.</returns>
        public List<string> CachedGeneratorNames() => _cachedGeneratorNames;

        /// <summary>
        /// Gets the index of the currently selected generator.
        /// </summary>
        /// <returns>The index of the selected generator.</returns>
        public int SelectedGeneratorIndex() => _selectedGeneratorIndex;

        /// <summary>
        /// Constructor that subscribes to initialization and cache clearing events.
        /// </summary>
        public SelectionController()
        {
            EventBus.InitScene += InitScene;
            EventBus.ClearCachedData += ClearCacheData;
        }

        /// <summary>
        /// Changes the selected generator based on the provided name.
        /// </summary>
        /// <param name="newGeneratorName">The name of the new generator to select.</param>
        public void ChangeGenerator(string newGeneratorName)
        {
            _selectedGeneratorIndex = _cachedGeneratorNames.IndexOf(newGeneratorName);
            SelectGenerator(_selectedGeneratorIndex);
        }

        /// <summary>
        /// Initializes the scene by retrieving or initializing the generation manager,
        /// finding all generators, and selecting the first generator.
        /// </summary>
        private void InitScene()
        {
            RetrieveOrInitializeCachedGenerationManager();
            FindAllGenerators();
            SelectGenerator(0);
            EventBus.OnReload();
        }

        /// <summary>
        /// Selects a generator by its index and updates the current generator in the service.
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

            EventBus.OnToggleOpenGraphButton(Attribute.IsDefined(selectedGenerator.GetType(),
                typeof(OpenGraphEditorAttribute)));
        }

        /// <summary>
        /// Finds all generators in the scene and caches their names.
        /// </summary>
        private void FindAllGenerators()
        {
            if (_generationManager != null)
            {
                _generators = new List<BaseGenerator>(_generationManager.GetComponentsInChildren<BaseGenerator>());
                _cachedGeneratorNames = _generators.Where(g => g != null).Select(g => g.name).ToList();

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
        /// Clears the generator lists and removes cached generator names from editor preferences.
        /// </summary>
        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        /// <summary>
        /// Retrieves or initializes the cached generation manager in the scene.
        /// </summary>
        private void RetrieveOrInitializeCachedGenerationManager()
        {
            _generationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
        }

        /// <summary>
        /// Retrieves the cached generation manager from editor preferences.
        /// </summary>
        /// <returns>The cached generation manager GameObject, or null if not found.</returns>
        private static GameObject RetrieveCachedGenerationManager()
        {
            var cachedId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
            return cachedId != -1 ? EditorUtility.InstanceIDToObject(cachedId) as GameObject : null;
        }

        /// <summary>
        /// Instantiates the generation manager prefab and caches its instance ID.
        /// </summary>
        /// <returns>The instantiated generation manager GameObject.</returns>
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

            var generationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, generationManager.GetInstanceID());

            return generationManager;
        }

        /// <summary>
        /// Clears cached data, including the cached generation manager and generator lists.
        /// </summary>
        private void ClearCacheData()
        {
            var cachedGenerationManager = RetrieveCachedGenerationManager();
            if (cachedGenerationManager)
            {
                Object.DestroyImmediate(cachedGenerationManager);
            }

            EditorPrefs.DeleteKey(CachedGenerationManagerIdKey);
            GeneratorService.Instance.SetCurrentGenerator(null);
            _cachedGeneratorNames.Clear();
            _generators.Clear();
            _selectedGeneratorIndex = 0;
        }
    }
}