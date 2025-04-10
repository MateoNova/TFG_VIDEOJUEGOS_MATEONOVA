using Editor.Controllers;
using UnityEngine.UIElements;

namespace Editor.Views
{
    /// <summary>
    /// Represents the view for generator selection in the editor.
    /// Responsible for creating and managing the UI elements related to generator selection.
    /// </summary>
    public class SelectionView
    {
        #region Fields

        /// <summary>
        /// The container for the UI elements of the selection view.
        /// </summary>
        private VisualElement _container;

        /// <summary>
        /// The controller responsible for handling generator selection logic.
        /// </summary>
        private readonly SelectionController _controller = new();

        #endregion

        #region Constructor and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionView"/> class.
        /// Subscribes to the <see cref="InitializationController.OnClearCachedData"/> event.
        /// </summary>
        public SelectionView()
        {
            InitializationController.OnClearCachedData += ClearCacheData;
        }

        /// <summary>
        /// Finalizer for the <see cref="SelectionView"/> class.
        /// Unsubscribes from the <see cref="InitializationController.OnClearCachedData"/> event to prevent memory leaks.
        /// </summary>
        ~SelectionView()
        {
            InitializationController.OnClearCachedData -= ClearCacheData;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Clears cached data and marks the container for repainting.
        /// </summary>
        private void ClearCacheData()
        {
            _container?.MarkDirtyRepaint();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the UI for the generator selection view.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements for generator selection.</returns>
        public VisualElement CreateUI()
        {
            _container = StyleUtils.SimpleContainer();

            var foldout = new Foldout { text = "Generator Selection", value = true };

            var cachedNames = _controller.CachedGeneratorNames();

            if (cachedNames == null || cachedNames.Count == 0)
            {
                var helpLabel = StyleUtils.HelpLabel(
                    "No generators found in the scene. Click the initialize button to search for them.");
                foldout.Add(helpLabel);
            }
            else
            {
                var dropdown = new DropdownField("Select Generator", cachedNames,
                    cachedNames[_controller.SelectedGeneratorIndex()]);

                dropdown.RegisterValueChangedCallback(evt => _controller.changeGenerator(evt.newValue));

                foldout.Add(dropdown);
            }

            _container.Add(foldout);

            return _container;
        }

        #endregion
    }
}