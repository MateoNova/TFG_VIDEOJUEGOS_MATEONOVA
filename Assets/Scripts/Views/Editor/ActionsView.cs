using System;
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
            var actionsFoldout = StyleUtils.ModernFoldout(string.Empty);
            ConfigureFoldout(actionsFoldout);
            return actionsFoldout;
        }

        /// <summary>
        /// Configures the foldout element with localized text and a value change callback.
        /// </summary>
        /// <param name="foldout">The foldout element to configure.</param>
        private void ConfigureFoldout(Foldout foldout)
        {
            foldout.SetLocalizedText(LocalizationKeysHelper.Actions, LocalizationKeysHelper.ActionsTable);
            foldout.RegisterValueChangedCallback(evt => _showGenerationActions = evt.newValue);
        }

        /// <summary>
        /// Adds buttons for generation actions (e.g., Generate, Clear, Save, Load) to the foldout.
        /// </summary>
        /// <param name="actionsFoldout">The foldout to which the buttons will be added.</param>
        private static void AddActionsButtons(Foldout actionsFoldout)
        {
            var buttonsContainer = StyleUtils.ColumnButtonContainer();
            AddActionButtonsToContainer(buttonsContainer);
            actionsFoldout.Add(buttonsContainer);
        }

        /// <summary>
        /// Adds action buttons to the specified container.
        /// </summary>
        /// <param name="container">The container to which the buttons will be added.</param>
        private static void AddActionButtonsToContainer(VisualElement container)
        {
            CreateAndAddButton(LocalizationKeysHelper.ActionsGenerate, container, ActionsController.Generate);
            CreateAndAddButton(LocalizationKeysHelper.ActionsClear, container, ActionsController.ClearDungeon);
            CreateAndAddButton(LocalizationKeysHelper.ActionsSave, container, ActionsController.SaveDungeon);
            CreateAndAddButton(LocalizationKeysHelper.ActionsLoad, container, ActionsController.LoadDungeon);
        }

        /// <summary>
        /// Creates a button with the specified text and action, and adds it to the given container.
        /// </summary>
        /// <param name="text">The localized text key for the button label.</param>
        /// <param name="container">The container to which the button will be added.</param>
        /// <param name="action">The action to execute when the button is clicked.</param>
        private static void CreateAndAddButton(string text, VisualElement container, Action action)
        {
            var button = new Button(action);
            button.SetLocalizedText(text, LocalizationKeysHelper.ActionsTable);
            container.Add(button);
        }

        /// <summary>
        /// Adds a toggle to the UI for clearing the dungeon before generating.
        /// </summary>
        /// <param name="actionsFoldout">The foldout to which the toggle will be added.</param>
        private void AddClearToggle(Foldout actionsFoldout)
        {
            var clearToggle = StyleUtils.SimpleToggle(
                string.Empty,
                _actionsController.ClearDungeonToggle,
                string.Empty
            );
            ConfigureClearToggle(clearToggle);
            actionsFoldout.Add(clearToggle);
        }

        /// <summary>
        /// Configures the toggle element with localized text, tooltip, and a value change callback.
        /// </summary>
        /// <param name="toggle">The toggle element to configure.</param>
        private void ConfigureClearToggle(Toggle toggle)
        {
            toggle.SetLocalizedText(LocalizationKeysHelper.ActionsClearToggle, LocalizationKeysHelper.ActionsTable);
            toggle.SetLocalizedTooltip(LocalizationKeysHelper.ActionsClearTooltip, LocalizationKeysHelper.ActionsTable);
            toggle.RegisterValueChangedCallback(evt => _actionsController.SetClearDungeon(evt.newValue));
        }
    }
}

#endif