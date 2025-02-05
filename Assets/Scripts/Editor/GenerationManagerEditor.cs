using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class GenerationManagerWindow : EditorWindow
    {
        private List<BaseGenerator> _generators = new();
        private int _selectedGeneratorIndex;
        private BaseGenerator _currentGenerator;
        private List<string> _cachedGeneratorNames = new();

        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            GetWindow<GenerationManagerWindow>("Generation Manager");
        }

        private void OnEnable()
        {
            FindAllGenerators();
        }

        private void OnGUI()
        {
            if (_generators == null || _generators.Count == 0)
            {
                EditorGUILayout.LabelField("No generators found in the scene.");
                return;
            }

            // Use cached generator names
            _selectedGeneratorIndex = EditorGUILayout.Popup("Select Generator", _selectedGeneratorIndex, _cachedGeneratorNames.ToArray());
            SelectGenerator(_selectedGeneratorIndex);

            // Si el usuario selecciona un generador diferente
            if (_selectedGeneratorIndex != _generators.IndexOf(_currentGenerator))
            {
                SelectGenerator(_selectedGeneratorIndex);
                EditorUtility.SetDirty(this);
            }

            // Mostrar los parámetros del generador seleccionado
            if (_currentGenerator)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Generator Settings", EditorStyles.boldLabel);

                var generatorObject = new SerializedObject(_currentGenerator);
                var property = generatorObject.GetIterator();
                property.NextVisible(true); // Saltar la primera propiedad (script)

                while (property.NextVisible(false))
                {
                    EditorGUILayout.PropertyField(property, true);
                }

                generatorObject.ApplyModifiedProperties();

                // Botón para generar el dungeon
                EditorGUILayout.Space();
                if (GUILayout.Button("Generate Dungeon"))
                {
                    Generate();
                }
            }
        }

        private void FindAllGenerators()
        {
            _generators = new List<BaseGenerator>(FindObjectsByType<BaseGenerator>(FindObjectsSortMode.None));
            _cachedGeneratorNames = GetGeneratorNames();
        }

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
    }
}