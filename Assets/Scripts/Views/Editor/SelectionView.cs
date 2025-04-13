using UnityEngine.UIElements;
using SelectionController = Controllers.Editor.SelectionController;
using StyleUtils = Utils.StyleUtils;
using Utils; // Asegúrate de incluir el namespace donde está LocalizationUIHelper

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

            // Creamos el foldout y asignamos el texto localizado
            var foldout = new Foldout { value = true };
            // Se utiliza la clave "GeneratorSelection" de la tabla "SelectionTable"
            foldout.SetLocalizedText("GeneratorSelection", "SelectionTable");

            var cachedNames = _controller.CachedGeneratorNames();

            if (cachedNames == null || cachedNames.Count == 0)
            {
                // Se crea un help label y se le asigna un mensaje localizado
                var helpLabel = StyleUtils.HelpLabel("");
                helpLabel.SetLocalizedText("NoGeneratorsFound", "SelectionTable");
                foldout.Add(helpLabel);
            }
            else
            {
                // Se crea el dropdown sin texto fijo en el constructor y se actualiza su label mediante LocalizationUIHelper
                var dropdown = new DropdownField();
                dropdown.SetLocalizedTitle("SelectGenerator", "SelectionTable");

                dropdown.choices = cachedNames;

                // Validate the selected index
                var selectedIndex = _controller.SelectedGeneratorIndex();
                if (selectedIndex < 0 || selectedIndex >= cachedNames.Count)
                {
                    selectedIndex = 0; // Default to the first item if the index is invalid
                }

                // Set the initial value
                dropdown.value = cachedNames[selectedIndex];
                dropdown.RegisterValueChangedCallback(evt => _controller.ChangeGenerator(evt.newValue));

                foldout.Add(dropdown);
            }

            _container.Add(foldout);
            return _container;
        }
    }
}