using Editor.Controllers;
using UnityEngine.UIElements;

namespace Editor.Views
{
    /// <summary>
    /// View for displaying and managing generation actions.
    /// </summary>
    public class ActionsView
    {
        private bool _showGenerationActions = true;

        /// <summary>
        /// Creates the user interface for the generation actions.
        /// </summary>
        /// <param name="controller">The controller to handle actions.</param>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
        public VisualElement CreateUI(ActionsController controller)
        {
            var container = StyleUtils.SimpleContainer();

            var actionsFoldout = new Foldout { text = "Generation Actions", value = _showGenerationActions };
            actionsFoldout.RegisterValueChangedCallback(evt => _showGenerationActions = evt.newValue);
            container.Add(actionsFoldout);

            if (!_showGenerationActions)
                return container;

            AddClearToggle(actionsFoldout, controller);
            var buttonsContainer = AddActionsButtons(controller);
            actionsFoldout.Add(buttonsContainer);

            return container;
        }

        /// <summary>
        /// Adds the action buttons to the UI.
        /// </summary>
        /// <param name="controller">The controller to handle button actions.</param>
        /// <returns>A <see cref="VisualElement"/> containing the action buttons.</returns>
        private static VisualElement AddActionsButtons(ActionsController controller)
        {
            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Column;
            buttonsContainer.style.marginTop = 5;

            var generateButton = new Button(controller.Generate) { text = "Generate Dungeon" };
            var clearButton = new Button(controller.ClearDungeon) { text = "Clear Dungeon" };
            var saveButton = new Button(controller.SaveDungeon) { text = "Save Dungeon" };
            var loadButton = new Button(controller.LoadDungeon) { text = "Load Dungeon" };

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
        /// <param name="controller">The controller to handle toggle changes.</param>
        private static void AddClearToggle(Foldout actionsFoldout, ActionsController controller)
        {
            var clearToggle = StyleUtils.SimpleToggle("Clear Dungeon", controller.ClearDungeonState, "Clear the dungeon before generating.");
            clearToggle.RegisterValueChangedCallback(evt => controller.ClearDungeonState = evt.newValue);
            actionsFoldout.Add(clearToggle);
        }
    }
}