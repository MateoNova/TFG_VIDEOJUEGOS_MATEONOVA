using UnityEditor;

namespace Editor
{
    /// <summary>
    /// Manage the settings of the generator.
    /// </summary>
    public class GeneratorSettings
    {
        #region Fields

        /// <summary>
        /// Indicates whether the generator settings foldout should be shown.
        /// </summary>
        private bool _showGeneratorSettings = true;

        /// <summary>
        /// Reference to the GeneratorSelection instance.
        /// </summary>
        private readonly GeneratorSelection _generatorSelection;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorSettings"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public GeneratorSettings(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        #region Drawing

        /// <summary>
        /// Draws the generator settings foldout.
        /// </summary>
        public void Draw()
        {
            _showGeneratorSettings = EditorGUILayout.Foldout(_showGeneratorSettings, "Generator Settings", true);
            if (_showGeneratorSettings)
            {
                DrawGeneratorSettings();
            }
        }

        /// <summary>
        /// Draws the generator settings.
        /// </summary>
        private void DrawGeneratorSettings()
        {
            if (!_generatorSelection.CurrentGenerator) return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                SerializedObject generatorObject = new(_generatorSelection.CurrentGenerator);
                var property = generatorObject.GetIterator();
                // Skip the first property (m_Script)
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

        #endregion
    }
}