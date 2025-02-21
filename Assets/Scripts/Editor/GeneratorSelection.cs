using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class GeneratorSelection
    {
        
        private static GeneratorSelection _instance;

        public static GeneratorSelection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GeneratorSelection();
                }
                return _instance;
            }
        }
        
        private GeneratorSelection()
        {
            // Constructor privado para evitar instanciación externa
        }
        
        /// <summary>
        /// Key for cached generator names in EditorPrefs.
        /// </summary>
        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";
        
        /// <summary>
        /// Key for cached generation manager ID in EditorPrefs.
        /// </summary>
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";
        
        /// <summary>
        /// Path to the Generation Manager prefab.
        /// </summary>
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";
        
        private bool _showGeneratorSelection = true;
        
        /// <summary>
        /// Index of the selected generator.
        /// </summary>
        private int _selectedGeneratorIndex;
        
        /// <summary>
        /// List of all generators in the scene.
        /// </summary>
        private List<BaseGenerator> _generators = new();
        
        /// <summary>
        /// Cached prefab of the Generation Manager.
        /// </summary>
        private static GameObject _cachedPrefab;


        /// <summary>
        /// Currently selected generator.
        /// </summary>
        public BaseGenerator _currentGenerator;
        
        /// <summary>
        /// Cached Generation Manager GameObject.
        /// </summary>
        private GameObject _cachedGenerationManager;
        
        /// <summary>
        /// Cached generator names.
        /// </summary>
        private List<string> _cachedGeneratorNames = new();

        public void Draw()
        {
            _showGeneratorSelection = EditorGUILayout.Foldout(_showGeneratorSelection, "Generator Selection", true);
            if (_showGeneratorSelection)
            {
                EditorGUILayoutExtensions.DrawSectionTitle("Generator Selection");
                DrawGeneratorSelection();
            }
        }
        
        private void DrawGeneratorSelection()
        {
            _selectedGeneratorIndex = EditorGUILayout.Popup("Select Generator", _selectedGeneratorIndex,
                _cachedGeneratorNames.ToArray());
            /*if (_isInitialized)
            {*/
                SelectGenerator(_selectedGeneratorIndex);
            //}
        }

        internal void SelectGenerator(int index)
        {
            if (index >= 0 && index < _generators.Count)
            {
                _selectedGeneratorIndex = index;
                _currentGenerator = _generators[_selectedGeneratorIndex];
                //Repaint();
            }
            else
            {
                Debug.LogWarning("Invalid generator index.");
            }
        }

        internal void FindAllGenerators()
        {
            if (_cachedGenerationManager != null)
            {
                _generators =
                    new List<BaseGenerator>(_cachedGenerationManager.GetComponentsInChildren<BaseGenerator>());
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
        
        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        public void RetrieveOrInitializeCachedGenerationManager()
        {
            _cachedGenerationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
        }
        
        private static GameObject RetrieveCachedGenerationManager()
        {
            var cachedId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
            if (cachedId != -1)
            {
                return EditorUtility.InstanceIDToObject(cachedId) as GameObject;
            }

            return null;
        }
        
        private GameObject InstantiateGenerationManager()
        {
            if (!_cachedPrefab)
            {
                _cachedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            if (!_cachedPrefab)
            {
                return null;
            }

            _cachedGenerationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, _cachedGenerationManager.GetInstanceID());
            return _cachedGenerationManager;
        }
    }
}