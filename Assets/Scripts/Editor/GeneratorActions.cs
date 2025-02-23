using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    /// <summary>
    /// Gestiona las acciones de generación del dungeon: generar, limpiar, guardar y cargar, usando UI Toolkit.
    /// </summary>
    public class GenerationActions
    {
        private readonly GeneratorSelection _generatorSelection;
        private bool _showGenerationActions = true;
        private bool _clearDungeon = true;

        public GenerationActions(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        /// <summary>
        /// Crea la interfaz de usuario para las acciones de generación.
        /// </summary>
        public VisualElement CreateUI()
        {
            VisualElement container = new VisualElement();
            container.style.marginBottom = 10;

            Foldout actionsFoldout = new Foldout() { text = "Generation Actions", value = _showGenerationActions };
            actionsFoldout.RegisterValueChangedCallback(evt => _showGenerationActions = evt.newValue);
            container.Add(actionsFoldout);

            if (!_showGenerationActions)
                return container;

            // Toggle para limpiar el dungeon
            Toggle clearToggle = new Toggle("Clear all tiles");
            clearToggle.tooltip = "This will clear all tiles before generating the dungeon";
            clearToggle.value = _clearDungeon;
            clearToggle.RegisterValueChangedCallback(evt => _clearDungeon = evt.newValue);
            actionsFoldout.Add(clearToggle);

            // Botones de acciones
            VisualElement buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Column;
            buttonsContainer.style.marginTop = 5;

            Button generateButton = new Button(() => { Generate(); })
            {
                text = "Generate Dungeon"
            };
            Button clearButton = new Button(() => { ClearDungeon(); })
            {
                text = "Clear Dungeon"
            };
            Button saveButton = new Button(() => { SaveDungeon(); })
            {
                text = "Save Dungeon"
            };
            Button loadButton = new Button(() => { LoadDungeon(); })
            {
                text = "Load Dungeon"
            };

            buttonsContainer.Add(generateButton);
            buttonsContainer.Add(clearButton);
            buttonsContainer.Add(saveButton);
            buttonsContainer.Add(loadButton);

            actionsFoldout.Add(buttonsContainer);

            return container;
        }

        private void Generate()
        {
            if (_generatorSelection.CurrentGenerator)
            {
                _generatorSelection.CurrentGenerator.RunGeneration(_clearDungeon,
                    _generatorSelection.CurrentGenerator.Origin);
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        private void ClearDungeon()
        {
            _generatorSelection.CurrentGenerator?.ClearDungeon();
        }

        private void SaveDungeon()
        {
            string path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;

            _generatorSelection.CurrentGenerator.SaveDungeon(path);
        }

        private void LoadDungeon()
        {
            string path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _generatorSelection.CurrentGenerator.LoadDungeon(path);
            }
        }
    }
}
