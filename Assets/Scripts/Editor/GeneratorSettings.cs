using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor
{
    /// <summary>
    /// Manages the settings for the selected generator.
    /// </summary>
    public class GeneratorSettings
    {
        # region Fields

        private readonly GeneratorSelection _generatorSelection;
        private VisualElement _container;

        # endregion

        # region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorSettings"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public GeneratorSettings(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
            GeneratorSelection.OnGeneratorChanged += Repaint;
        }

        /// <summary>
        /// Creates the UI for the generator settings.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
        public VisualElement CreateUI()
        {
            if (_container == null)
            {
                _container = StyleUtils.SimpleContainer();
            }
            else
            {
                _container.Clear();
            }

            var foldout = new Foldout { text = "Generator Settings", value = true };

            if (_generatorSelection.CurrentGenerator == null)
            {
                var infoLabel = StyleUtils.HelpLabel("No generator selected.");
                foldout.Add(infoLabel);
            }
            else
            {
                var settingsContainer = new VisualElement();

                var serializedObject = new SerializedObject(_generatorSelection.CurrentGenerator);
                var property = serializedObject.GetIterator();
                const bool enterChildren = true;
                if (property.NextVisible(enterChildren))
                {
                    while (property.NextVisible(false))
                    {
                        if (!Utils.ShouldDisplayField(serializedObject, property.name)) continue;

                        var propField = new PropertyField(property);
                        propField.Bind(serializedObject);
                        settingsContainer.Add(propField);
                    }
                }

                foldout.Add(settingsContainer);
            }

            _container.Add(foldout);
            return _container;
        }

        /// <summary>
        /// Repaints the UI when the generator selection changes.
        /// </summary>
        private void Repaint()
        {
            if (_container == null)
            {
                _container = StyleUtils.SimpleContainer();
            }
            else
            {
                _container.Clear();
            }

            CreateUI();
            _container.MarkDirtyRepaint();
        }

        # endregion
    }
}