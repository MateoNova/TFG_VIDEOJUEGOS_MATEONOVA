using Editor.Controllers;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class SelectionView
    {
        private VisualElement _container;
        private readonly SelectionController _controller = new SelectionController();

        public VisualElement CreateUI()
        {
            _container = StyleUtils.SimpleContainer();
            Foldout foldout = new Foldout { text = "Generator Selection", value = true };

            var cachedNames = _controller.CachedGeneratorNames();
            if (cachedNames == null || cachedNames.Count == 0)
            {
                Label helpLabel =
                    StyleUtils.HelpLabel(
                        "No generators found in the scene. Click the initialize button to search for them.");
                foldout.Add(helpLabel);
            }
            else
            {
                DropdownField dropdown = new DropdownField("Select Generator", cachedNames,
                    cachedNames[_controller.SelectedGeneratorIndex()]);
                dropdown.RegisterValueChangedCallback(evt => _controller.ChangeGenerator(evt.newValue));
                foldout.Add(dropdown);
            }

            _container.Add(foldout);
            return _container;
        }
    }
}