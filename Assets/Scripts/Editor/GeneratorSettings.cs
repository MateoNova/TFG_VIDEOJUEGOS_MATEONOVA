using UnityEditor;

namespace Editor
{
    public class GeneratorSettings
    {
        private bool _showGeneratorSettings = true;
        private readonly GeneratorSelection _generatorSelection;

        public GeneratorSettings(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        public void Draw()
        {
            _showGeneratorSettings = EditorGUILayout.Foldout(_showGeneratorSettings, "Generator Settings", true);
            if (_showGeneratorSettings)
            {
                DrawGeneratorSettings();
            }
        }

        private void DrawGeneratorSettings()
        {
            if (!_generatorSelection.CurrentGenerator) return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                SerializedObject generatorObject = new(_generatorSelection.CurrentGenerator);
                var property = generatorObject.GetIterator();
                property.NextVisible(true);

                while (property.NextVisible(false))
                {
                    if (Utils.ShouldDisplayField(generatorObject, property.name))
                    {
                        EditorGUILayout.PropertyField(property, true);
                    }
                }

                generatorObject.ApplyModifiedProperties();
            }
        }
    }
}