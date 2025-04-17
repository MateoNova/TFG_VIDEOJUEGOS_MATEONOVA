using UnityEngine.UIElements;
using SelectionController = Controllers.Editor.SelectionController;
using StyleUtils = Utils.StyleUtils;
using Utils;

#if UNITY_EDITOR

namespace Views.Editor
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

            var foldout = StyleUtils.ModernFoldout("");
            foldout.SetLocalizedText("GeneratorSelection", "SelectionTable");

            var cachedNames = _controller.CachedGeneratorNames();

            if (cachedNames == null || cachedNames.Count == 0)
            {
                var helpLabel = StyleUtils.HelpLabel("");
                helpLabel.SetLocalizedText("NoGeneratorsFound", "SelectionTable");
                foldout.Add(helpLabel);
            }
            else
            {
                var dropdown = StyleUtils.SimpleDropdown();
                dropdown.SetLocalizedTitle("SelectGenerator", "SelectionTable");

                dropdown.choices = cachedNames;

                var selectedIndex = _controller.SelectedGeneratorIndex();
                if (selectedIndex < 0 || selectedIndex >= cachedNames.Count)
                {
                    selectedIndex = 0;
                }

                dropdown.value = cachedNames[selectedIndex];
                dropdown.RegisterValueChangedCallback(evt => _controller.ChangeGenerator(evt.newValue));

                foldout.Add(dropdown);
            }

            _container.Add(foldout);
            return _container;
        }
    }
}

# endif