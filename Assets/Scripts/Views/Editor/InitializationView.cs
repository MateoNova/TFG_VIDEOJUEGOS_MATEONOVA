using System;
using Editor;
using UnityEngine.UIElements;
using EventBus = Models.Editor.EventBus;
using InitializationController = Controllers.Editor.InitializationController;

namespace Views.Editor
{
    /// <summary>
    /// Represents the view for initialization actions in the editor.
    /// Responsible for creating and managing the UI elements related to scene initialization.
    /// </summary>
    public class InitializationView
    {
        /// <summary>
        /// Constructor that subscribes to the ToolOpened event.
        /// Initializes the scene when the tool is opened.
        /// </summary>
        public InitializationView()
        {
            EventBus.ToolOpened += _controller.InitScene;
        }

        /// <summary>
        /// Called when the view is destroyed.
        /// Unsubscribes from the ToolOpened event to prevent memory leaks.
        /// </summary>
        ~InitializationView()
        {
            EventBus.ToolOpened -= _controller.InitScene;
        }

        /// <summary>
        /// The controller responsible for handling initialization logic.
        /// </summary>
        private readonly InitializationController _controller = new();

        /// <summary>
        /// Creates the UI for the initialization view.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements for initialization actions.</returns>
        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();
            var foldout = new Foldout { text = "Initialization", value = true };
            foldout.Add(CreateButtonContainer());
            container.Add(foldout);

            return container;
        }

        /// <summary>
        /// Creates a container for the initialization action buttons.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the buttons for initialization actions.</returns>
        private VisualElement CreateButtonContainer()
        {
            var buttonContainer = StyleUtils.RowButtonContainer();
            buttonContainer.Add(CreateButton("Clear and Delete", _controller.ClearCachedData, true));
            buttonContainer.Add(CreateButton("Initialize Scene", _controller.InitScene));
            buttonContainer.Add(CreateButton("Reload", _controller.ReloadAll));

            return buttonContainer;
        }

        /// <summary>
        /// Creates a button with the specified text, click action, and style.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="onClick">The action to execute when the button is clicked.</param>
        /// <param name="isPrimary">Indicates whether the button is styled as a primary button.</param>
        /// <returns>A <see cref="Button"/> configured with the specified properties.</returns>
        private static Button CreateButton(string text, Action onClick, bool isPrimary = false)
        {
            return StyleUtils.ButtonInRowContainer(text, onClick, isPrimary);
        }
    }
}