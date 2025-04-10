using Editor.Controllers;
using UnityEngine.UIElements;

namespace Editor.Views
{
    /// <summary>
    /// Represents the view for the Initialization section in the editor.
    /// Responsible for creating and managing the UI elements related to initialization actions.
    /// </summary>
    public class InitializationView
    {
        #region Fields

        /// <summary>
        /// The controller responsible for handling initialization-related actions.
        /// </summary>
        private readonly InitializationController _controller = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the UI for the Initialization section.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements for the Initialization section.</returns>
        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();
            var foldout = CreateInitializationFoldout();
            container.Add(foldout);
            return container;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a foldout element for the Initialization section.
        /// </summary>
        /// <returns>A <see cref="Foldout"/> containing the UI elements for the Initialization section.</returns>
        private Foldout CreateInitializationFoldout()
        {
            var foldout = new Foldout { text = "Initialization", value = true };
            var buttonContainer = CreateButtonContainer();
            foldout.Add(buttonContainer);
            return foldout;
        }

        /// <summary>
        /// Creates a container for the buttons in the Initialization section.
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
        /// Creates a button with the specified text, action, and style.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="onClick">The action to execute when the button is clicked.</param>
        /// <param name="isFirts">Indicates whether the button represents a primary action. Default is false.</param>
        /// <returns>A <see cref="Button"/> configured with the specified parameters.</returns>
        private static Button CreateButton(string text, System.Action onClick, bool isFirts = false)
        {
            return StyleUtils.ButtonInRowContainer(text, onClick, isFirts);
        }

        #endregion
    }
}