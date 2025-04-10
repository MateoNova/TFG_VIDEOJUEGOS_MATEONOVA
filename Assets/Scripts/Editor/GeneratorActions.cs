/*using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    /// <summary>
    /// Manages the generation actions for the dungeon: generate, clear, save, and load
    /// </summary>
    public class GenerationActions
    {
        private readonly GeneratorSelection _generatorSelection;
        private bool _showGenerationActions = true;
        private bool _clearDungeon = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationActions"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public GenerationActions(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        /// <summary>
        /// Creates the user interface for the generation actions.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();

            var actionsFoldout = new Foldout { text = "Generation Actions", value = _showGenerationActions };
            actionsFoldout.RegisterValueChangedCallback(evt => _showGenerationActions = evt.newValue);
            container.Add(actionsFoldout);

            if (!_showGenerationActions)
                return container;
            AddClearToggle(actionsFoldout);

            var buttonsContainer = AddActionsButtons();

            actionsFoldout.Add(buttonsContainer);

            return container;
        }

        /// <summary>
        /// Adds the action buttons to the UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the action buttons.</returns>
        private VisualElement AddActionsButtons()
        {
            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Column;
            buttonsContainer.style.marginTop = 5;

            var generateButton = new Button(Generate) { text = "Generate Dungeon" };
            var clearButton = new Button(ClearDungeon) { text = "Clear Dungeon" };
            var saveButton = new Button(SaveDungeon) { text = "Save Dungeon" };
            var loadButton = new Button(LoadDungeon) { text = "Load Dungeon" };

            buttonsContainer.Add(generateButton);
            buttonsContainer.Add(clearButton);
            buttonsContainer.Add(saveButton);
            buttonsContainer.Add(loadButton);

            return buttonsContainer;
        }

        /// <summary>
        /// Adds the clear toggle to the UI.
        /// </summary>
        /// <param name="actionsFoldout">The foldout to add the toggle to.</param>
        private void AddClearToggle(Foldout actionsFoldout)
        {
            var clearToggle =
                StyleUtils.SimpleToggle("Clear Dungeon", _clearDungeon, "Clear the dungeon before generating.");
            clearToggle.RegisterValueChangedCallback(evt => _clearDungeon = evt.newValue);
            actionsFoldout.Add(clearToggle);
        }

        /// <summary>
        /// Generates the dungeon.
        /// </summary>
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

        /// <summary>
        /// Clears the dungeon.
        /// </summary>
        private void ClearDungeon()
        {
            _generatorSelection.CurrentGenerator?.ClearDungeon();
        }

        /// <summary>
        /// Saves the dungeon to a file.
        /// </summary>
        private void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;

            _generatorSelection.CurrentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Loads the dungeon from a file.
        /// </summary>
        private void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _generatorSelection.CurrentGenerator.LoadDungeon(path);
            }
        }
    }
}*/