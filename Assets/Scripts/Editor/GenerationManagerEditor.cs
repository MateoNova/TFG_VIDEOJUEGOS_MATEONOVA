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
        private List<BaseGenerator> _generators = new();
        private int _selectedGeneratorIndex;
        private BaseGenerator _currentGenerator;
        private List<string> _cachedGeneratorNames = new();

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
            FindAllGenerators();
        }

        /// <summary>
        /// Draws the GUI for the window.
        /// </summary>
        private void OnGUI()
        {
            if (GUILayout.Button("Init"))
            {
                InitScene();
            }

            if (_generators == null || _generators.Count == 0)
            {
                EditorGUILayout.LabelField("No generators found in the scene.");
                return;
            }

            // Use cached generator names
            _selectedGeneratorIndex = EditorGUILayout.Popup("Select Generator", _selectedGeneratorIndex, _cachedGeneratorNames.ToArray());
            SelectGenerator(_selectedGeneratorIndex);

            // If the user selects a different generator
            if (_selectedGeneratorIndex != _generators.IndexOf(_currentGenerator))
            {
                SelectGenerator(_selectedGeneratorIndex);
                EditorUtility.SetDirty(this);
            }

            // Display the selected generator's settings
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

            // Button to generate the dungeon
            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Dungeon"))
            {
                Generate();
            }

            // Button to clear the Dungeon
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
            _generators = new List<BaseGenerator>(FindObjectsByType<BaseGenerator>(FindObjectsSortMode.None));
            _cachedGeneratorNames = GetGeneratorNames();
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
            if (_generators != null)
            {
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
            }
            else
            {
                Debug.LogWarning("Generators list is null.");
            }
            return names;
        }

        /// <summary>
        /// Initializes the scene with the necessary GameObjects.
        /// </summary>
        private void InitScene()
        {
            // Load prefabs
            var generationManager = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GenerationManager.prefab");

            // Instantiate prefabs
            if (generationManager)
            {
                PrefabUtility.InstantiatePrefab(generationManager);
            }
            
            FindAllGenerators();
        }
    }
}