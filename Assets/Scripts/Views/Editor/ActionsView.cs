using UnityEngine.UIElements;
using ActionsController = Controllers.Editor.ActionsController;
using StyleUtils = Utils.StyleUtils;
using Utils;

#if UNITY_EDITOR

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
            var actionsFoldout = CreateActionsFoldout();
            container.Add(actionsFoldout);

            if (!_showGenerationActions) return container;

            AddClearToggle(actionsFoldout);
            AddActionsButtons(actionsFoldout);

            return container;
        }

        /// <summary>
        /// Creates the foldout for generation actions.
        /// </summary>
        /// <returns>A <see cref="Foldout"/> element for grouping generation actions.</returns>
        private Foldout CreateActionsFoldout()
        {
            var actionsFoldout = StyleUtils.ModernFoldout("");
            actionsFoldout.SetLocalizedText("GenerationActions", "ActionsTable");
            actionsFoldout.RegisterValueChangedCallback(evt => _showGenerationActions = evt.newValue);
            return actionsFoldout;
        }

        /// <summary>
        /// Adds buttons for generation actions (e.g., Generate, Clear, Save, Load) to the foldout.
        /// </summary>
        /// <param name="actionsFoldout">The foldout to which the buttons will be added.</param>
        private void AddActionsButtons(Foldout actionsFoldout)
        {
            var buttonsContainer = StyleUtils.ColumnButtonContainer();

            CreateButton("GenerateDungeon", buttonsContainer, _actionsController.Generate);
            CreateButton("ClearDungeon", buttonsContainer, _actionsController.ClearDungeon);
            CreateButton("SaveDungeon", buttonsContainer, _actionsController.SaveDungeon);
            CreateButton("LoadDungeon", buttonsContainer, _actionsController.LoadDungeon);

            actionsFoldout.Add(buttonsContainer);
        }
        
        /// <summary>
        /// Creates a button with the specified text and action, and adds it to the given container.
        /// </summary>
        private void CreateButton(string text, VisualElement container, System.Action action)
        {
            var button = new Button(action);
            button.SetLocalizedText(text, "ActionsTable");
            container.Add(button);
        }

        /// <summary>
        /// Adds a toggle to the UI for clearing the dungeon before generating.
        /// </summary>
        /// <param name="actionsFoldout">The foldout to which the toggle will be added.</param>
        private void AddClearToggle(Foldout actionsFoldout)
        {
            var clearToggle = StyleUtils.SimpleToggle(
                "",
                _actionsController.ClearDungeonToggle,
                ""
            );

            clearToggle.SetLocalizedText("ClearDungeonToggle", "ActionsTable");
            clearToggle.SetLocalizedTooltip("ClearDungeonTooltip", "ActionsTable");
            clearToggle.RegisterValueChangedCallback(evt => _actionsController.SetClearDungeon(evt.newValue));

            actionsFoldout.Add(clearToggle);
        }
    }
}

#endif