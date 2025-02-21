using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Manage the selection of generators in the Generation Manager.
    /// </summary>
    public class GeneratorSelection
    {
        #region Constants

        /// <summary>
        /// Key for cached generator names in EditorPrefs.
        /// </summary>
        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";

        /// <summary>
        /// Key for cached Generation Manager ID in EditorPrefs.
        /// </summary>
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";

        /// <summary>
        /// Path to the Generation Manager prefab.
        /// </summary>
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";

        #endregion

        #region Fields

        /// <summary>
        /// Indicates whether the generator selection foldout should be shown.
        /// </summary>
        private bool _showGeneratorSelection = true;

        /// <summary>
        /// Index of the selected generator.
        /// </summary>
        private int _selectedGeneratorIndex;

        /// <summary>
        /// List of available generators.
        /// </summary>
        private List<BaseGenerator> _generators = new();

        /// <summary>
        /// Cached prefab of the Generation Manager.
        /// </summary>
        private static GameObject _cachedPrefab;

        /// <summary>
        /// List of cached generator names.
        /// </summary>
        private List<string> _cachedGeneratorNames = new();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current generator selected.
        /// </summary>
        internal BaseGenerator CurrentGenerator { get; private set; }

        /// <summary>
        /// Gets the cached Generation Manager.
        /// </summary>
        private GameObject CachedGenerationManager { get; set; }

        #endregion

        #region GUI

        /// <summary>
        /// Draws the generator selection foldout.
        /// </summary>
        public void Draw()
        {
            _showGeneratorSelection = EditorGUILayout.Foldout(_showGeneratorSelection, "Generator Selection", true,
                Utils.GetSectionTitleStyle());

            if (!_showGeneratorSelection) return;
            DrawGeneratorSelection();
        }

        /// <summary>
        /// Draws the generator selection dropdown.
        /// </summary>
        private void DrawGeneratorSelection()
        {
            var generatorNames = _cachedGeneratorNames.ToArray();
            _selectedGeneratorIndex =
                EditorGUILayout.Popup("Select Generator", _selectedGeneratorIndex, generatorNames);
            SelectGenerator(_selectedGeneratorIndex);
        }

        #endregion

        #region Generator Management

        /// <summary>
        /// Selects a generator by its index.
        /// </summary>
        /// <param name="index">Index of the generator to select.</param>
        public void SelectGenerator(int index)
        {
            if (index >= 0 && index < _generators.Count)
            {
                _selectedGeneratorIndex = index;
                CurrentGenerator = _generators[_selectedGeneratorIndex];
            }
            else
            {
                Debug.LogWarning("Invalid generator index.");
            }
        }

        /// <summary>
        /// Finds all the generators in the Generation Manager and caches their names.
        /// </summary>
        public void FindAllGenerators()
        {
            if (CachedGenerationManager)
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
                Debug.LogWarning("GenerationManager not found in the scene.");
                ClearGeneratorLists();
            }
        }

        /// <summary>
        /// Gets the names of the generators in the Generation Manager.
        /// </summary>
        /// <returns>List of generator names.</returns>
        private List<string> GetGeneratorNames()
        {
            List<string> names = new();
            foreach (var generator in _generators)
            {
                if (generator)
                {
                    names.Add(generator.name);
                }
                else
                {
                    Debug.LogWarning("Generator is null.");
                }
            }

            return names;
        }

        /// <summary>
        /// Clears the generator lists and cached generator names.
        /// </summary>
        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        /// <summary>
        /// Retrieves the cached Generation Manager or instantiates a new one.
        /// </summary>
        public void RetrieveOrInitializeCachedGenerationManager()
        {
            CachedGenerationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
        }

        /// <summary>
        /// Retrieves the cached Generation Manager.
        /// </summary>
        /// <returns>The cached Generation Manager GameObject.</returns>
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
        /// Instantiates the Generation Manager prefab.
        /// </summary>
        /// <returns>The instantiated Generation Manager GameObject.</returns>
        private GameObject InstantiateGenerationManager()
        {
            if (!_cachedPrefab)
            {
                _cachedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            if (!_cachedPrefab)
            {
                Debug.LogError($"Prefab not found at path: {PrefabPath}");
                return null;
            }

            CachedGenerationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, CachedGenerationManager.GetInstanceID());
            return CachedGenerationManager;
        }

        /// <summary>
        /// Clears the cached data and resets the Generation Manager.
        /// </summary>
        public void ClearCacheData()
        {
            if (CachedGenerationManager)
            {
                EditorApplication.delayCall += () => Object.DestroyImmediate(CachedGenerationManager);
            }

            CachedGenerationManager = null;
            _cachedPrefab = null;
            CurrentGenerator = null;
            _cachedGeneratorNames.Clear();
            _generators.Clear();
            _selectedGeneratorIndex = 0;
        }

        #endregion
    }
}