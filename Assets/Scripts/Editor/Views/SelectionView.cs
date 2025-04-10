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
        /// <summary>
        /// The container for the UI elements of the selection view.
        /// </summary>
        private VisualElement _container;

        /// <summary>
        /// The controller responsible for handling generator selection logic.
        /// </summary>
        private readonly SelectionController _controller = new();

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
                var helpLabel =
                    StyleUtils.HelpLabel(
                        "No generators found in the scene. Click the initialize button to search for them.");
                foldout.Add(helpLabel);
            }
            else
            {
                var dropdown = new DropdownField("Select Generator", cachedNames,
                    cachedNames[_controller.SelectedGeneratorIndex()]);
                dropdown.RegisterValueChangedCallback(evt => _controller.ChangeGenerator(evt.newValue));

                foldout.Add(dropdown);
            }

            _container.Add(foldout);

            return _container;
        }
    }
}