using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor
{
    public class GeneratorSettings
    {
        private readonly GeneratorSelection _generatorSelection;
        private VisualElement _container;

        public GeneratorSettings(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
            GeneratorSelection.OnGeneratorChanged += Repaint;
        }

        public VisualElement CreateUI()
        {
            if (_container == null)
            {
                _container = Utils.CreateContainer();
            }
            else
            {
                _container.Clear();
            }

            var foldout = new Foldout { text = "Generator Settings", value = true };

            if (_generatorSelection.CurrentGenerator == null)
            {
                var infoLabel = Utils.CreateHelpLabel("No generator selected.");
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

        private void Repaint()
        {
            if (_container == null)
            {
                _container = Utils.CreateContainer();
            }
            else
            {
                _container.Clear();
            }
        
            // Recreate the UI
            CreateUI();
            _container.MarkDirtyRepaint();
        }
    }
}