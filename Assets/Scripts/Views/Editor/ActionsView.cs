using UnityEngine.UIElements;
using ActionsController = Controllers.Editor.ActionsController;
using StyleUtils = Utils.StyleUtils;

namespace Views.Editor
{
    /// <summary>
    /// Represents the view for generation actions in the editor.
    /// Responsible for creating and managing the UI elements related to generation actions.
    /// </summary>
    public class ActionsView
    {
        /// <summary>
        /// Indicates whether the generation actions foldout is expanded or collapsed.
        /// </summary>
        private bool _showGenerationActions = true;

        /// <summary>
        /// The controller responsible for handling generation actions logic.
        /// </summary>
        private readonly ActionsController _actionsController = new();

        /// <summary>
        /// Creates the UI for the generation actions view.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements for generation actions.</returns>
        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();
            var actionsFoldout = new Foldout { text = "Generation Actions", value = _showGenerationActions };
            actionsFoldout.RegisterValueChangedCallback(evt => _showGenerationActions = evt.newValue);
            container.Add(actionsFoldout);

            if (!_showGenerationActions) return container;

            AddClearToggle(actionsFoldout);
            var buttonsContainer = AddActionsButtons();
            actionsFoldout.Add(buttonsContainer);

            return container;
        }

        /// <summary>
        /// Adds buttons for generation actions (e.g., Generate, Clear, Save, Load) to the UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the action buttons.</returns>
        private VisualElement AddActionsButtons()
        {
            // Create a container for the buttons.
            var buttonsContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginTop = 5
                }
            };

            var generateButton = new Button(_actionsController.Generate) { text = "Generate Dungeon" };
            buttonsContainer.Add(generateButton);

            var clearButton = new Button(ActionsController.ClearDungeon) { text = "Clear Dungeon" };
            buttonsContainer.Add(clearButton);

            var saveButton = new Button(_actionsController.SaveDungeon) { text = "Save Dungeon" };
            buttonsContainer.Add(saveButton);

            var loadButton = new Button(_actionsController.LoadDungeon) { text = "Load Dungeon" };
            buttonsContainer.Add(loadButton);

            return buttonsContainer;
        }

        /// <summary>
        /// Adds a toggle to the UI for clearing the dungeon before generating.
        /// </summary>
        /// <param name="actionsFoldout">The foldout to which the toggle will be added.</param>
        private void AddClearToggle(Foldout actionsFoldout)
        {
            var clearToggle = StyleUtils.SimpleToggle(
                "Clear Dungeon",
                _actionsController.ClearDungeonToggle,
                "Clear the dungeon before generating."
            );

            clearToggle.RegisterValueChangedCallback(evt => _actionsController.SetClearDungeon(evt.newValue));

            actionsFoldout.Add(clearToggle);
        }
    }
}