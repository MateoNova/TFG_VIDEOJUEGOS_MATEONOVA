using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Editor window for managing dungeon generation.
    /// </summary>
    public class GenerationManagerWindow : EditorWindow
    {
        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";

        private List<BaseGenerator> _generators = new();
        private int _selectedGeneratorIndex;
        private BaseGenerator _currentGenerator;
        
        private List<string> _cachedGeneratorNames = new();
        private GameObject _cachedGenerationManager;
        private static GameObject _cachedPrefab;

        /// <summary>
        /// Shows the Generation Manager window.
        /// </summary>
        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            GetWindow<GenerationManagerWindow>("Generation Manager");
        }

        /// <summary>
        /// Called when the window is enabled.
        /// </summary>
        private void OnEnable()
        {
            InitScene();
        }

        private void InitScene()
        {
            _cachedGenerationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
            FindAllGenerators();
            SelectGenerator(0);
        }

        /// <summary>
        /// Draws the GUI for the window.
        /// </summary>
        private void OnGUI()
        {
            DrawButtons();
            if (_generators == null || _generators.Count == 0)
            {
                EditorGUILayout.LabelField("No generators found in the scene.");
                return;
            }
            DrawGeneratorSelection();
            DrawGeneratorSettings();
        }

        private void DrawButtons()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Clear Cached Data"))
            {
                ClearCachedData();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Initialize Scene"))
            {
                InitScene();
            }
        }

        private void DrawGeneratorSelection()
        {
            _selectedGeneratorIndex = EditorGUILayout.Popup("Select Generator", _selectedGeneratorIndex, _cachedGeneratorNames.ToArray());
            SelectGenerator(_selectedGeneratorIndex);
        }

        private void DrawGeneratorSettings()
        {
            if (!_currentGenerator) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generator Settings", EditorStyles.boldLabel);

            var generatorObject = new SerializedObject(_currentGenerator);
            var property = generatorObject.GetIterator();
            property.NextVisible(true); // Skip the first property (script)

            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }

            generatorObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Dungeon"))
            {
                Generate();
            }

            if (GUILayout.Button("Clear Dungeon"))
            {
                _currentGenerator.ClearDungeon();
            }
        }

        /// <summary>
        /// Finds all generators in the scene.
        /// </summary>
        private void FindAllGenerators()
        {
            if (_cachedGenerationManager)
            {
                _generators = new List<BaseGenerator>(_cachedGenerationManager.GetComponentsInChildren<BaseGenerator>());
                _cachedGeneratorNames = GetGeneratorNames();
                CacheGeneratorNames();
            }
            else
            {
                Debug.LogWarning("GenerationManager not found in the scene.");
                ClearGeneratorLists();
            }
        }

        private void CacheGeneratorNames()
        {
            if (_cachedGeneratorNames.Count > 0)
            {
                EditorPrefs.SetString(CachedGeneratorNamesKey, string.Join(",", _cachedGeneratorNames));
            }
        }

        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        /// <summary>
        /// Generates the dungeon using the selected generator.
        /// </summary>
        private void Generate()
        {
            if (_currentGenerator)
            {
                _currentGenerator.GenerateDungeon();
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        /// <summary>
        /// Selects a generator by index.
        /// </summary>
        /// <param name="index">The index of the generator to select.</param>
        private void SelectGenerator(int index)
        {
            if (index == _selectedGeneratorIndex && _currentGenerator) return;

            if (index >= 0 && index < _generators.Count)
            {
                _selectedGeneratorIndex = index;
                _currentGenerator = _generators[_selectedGeneratorIndex];
            }
            else
            {
                Debug.LogWarning("Invalid generator index.");
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
                    Debug.LogWarning("Generator is null.");
                }
            }
            return names;
        }

        private static GameObject RetrieveCachedGenerationManager()
        {
            var cachedGenerationManagerId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
            if (cachedGenerationManagerId != -1)
            {
                return EditorUtility.InstanceIDToObject(cachedGenerationManagerId) as GameObject;
            }
            return null;
        }

        private GameObject InstantiateGenerationManager()
        {
            if (!_cachedPrefab)
            {
                _cachedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            if (!_cachedPrefab) { return null; }

            _cachedGenerationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, _cachedGenerationManager.GetInstanceID());
            return _cachedGenerationManager;
        }

        /// <summary>
        /// Clears the cached data stored in EditorPrefs.
        /// </summary>
        private void ClearCachedData()
        {
            EditorPrefs.DeleteAll();

            DestroyImmediate(_cachedGenerationManager);
            _cachedGenerationManager = null;
            _cachedPrefab = null;
            _currentGenerator = null;
            _cachedGeneratorNames.Clear();
            _generators.Clear();
            _selectedGeneratorIndex = 0;
            
            // Repaint the window to reflect the changes.
            EditorApplication.delayCall += Repaint;
        }
    }
}