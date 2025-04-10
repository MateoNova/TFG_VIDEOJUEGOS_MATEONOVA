using Editor.Controllers;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class ActionsView
    {
        private bool _showGenerationActions = true;
        private ActionsController _actionsController = new();

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
        
        private VisualElement AddActionsButtons()
        {
            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Column;
            buttonsContainer.style.marginTop = 5;

            var generateButton = new Button(_actionsController.Generate) { text = "Generate Dungeon" };
            var clearButton = new Button(_actionsController.ClearDungeon) { text = "Clear Dungeon" };
            var saveButton = new Button(_actionsController.SaveDungeon) { text = "Save Dungeon" };
            var loadButton = new Button(_actionsController.LoadDungeon) { text = "Load Dungeon" };

            buttonsContainer.Add(generateButton);
            buttonsContainer.Add(clearButton);
            buttonsContainer.Add(saveButton);
            buttonsContainer.Add(loadButton);

            return buttonsContainer;
        }
        
        private void AddClearToggle(Foldout actionsFoldout)
        {
            var clearToggle =
                StyleUtils.SimpleToggle("Clear Dungeon", _actionsController._clearDungeon, "Clear the dungeon before generating.");
            clearToggle.RegisterValueChangedCallback(evt =>
            {
                _actionsController.clearToggle(evt.newValue);
            });
            actionsFoldout.Add(clearToggle);
        }
    }
}