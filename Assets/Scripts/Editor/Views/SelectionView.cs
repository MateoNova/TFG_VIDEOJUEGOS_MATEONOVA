using Editor.Controllers;
using UnityEngine.UIElements;

namespace Editor.Views
{
   public class SelectionView
   {
       private VisualElement _container;
       private SelectionController _controller = new();
       
       public SelectionView()
       {
           InitializationController._onClearCachedData += ClearCacheData;

       }
       
       

       private void Reload()
       {
           //reload the ui
           CreateUI();
       }

       ~SelectionView()
       {
           InitializationController._onClearCachedData -= ClearCacheData;
       }

       private void ClearCacheData()
       {
           _container.MarkDirtyRepaint();
       }


       /// <summary>
        /// Creates the UI for the generator selection.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
        public VisualElement CreateUI()
        {
            _container = StyleUtils.SimpleContainer();

            var foldout = new Foldout { text = "Generator Selection", value = true };

            var cachedNames = _controller.getCached();
            
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
                    cachedNames[_controller.SelectedGeneratorIndex]);

                dropdown.RegisterValueChangedCallback(evt =>
                {
                    _controller.changeGenerator(evt.newValue);
                });

                foldout.Add(dropdown);
            }

            _container.Add(foldout);
            return _container;
        }
    }
}