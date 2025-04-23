using System.Collections.Generic;
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
        /// The controller responsible for handling generator selection logic.
        /// </summary>
        private readonly SelectionController controller = new();

        /// <summary>
        /// Creates the UI for the generator selection view.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements for generator selection.</returns>
        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();

            var foldout = StyleUtils.ModernFoldout(string.Empty);
            foldout.SetLocalizedText(LocalizationKeysHelper.SelectionFoldout, LocalizationKeysHelper.SelectionTable);

            var cachedNames = controller.CachedGeneratorNames();

            if (cachedNames == null || cachedNames.Count == 0) AddHelpLabel(foldout);
            else AddDropdown(cachedNames, foldout);

            container.Add(foldout);
            return container;
        }

        /// <summary>
        /// Adds a dropdown element to the foldout for selecting a generator.
        /// </summary>
        /// <param name="cachedNames">The list of cached generator names.</param>
        /// <param name="foldout">The foldout to which the dropdown will be added.</param>
        private void AddDropdown(List<string> cachedNames, Foldout foldout)
        {
            var dropdown = StyleUtils.SimpleDropdown();
            dropdown.SetLocalizedTitle(LocalizationKeysHelper.SelectionGeneratorDropdown,
                LocalizationKeysHelper.SelectionTable);

            dropdown.choices = cachedNames;

            var selectedIndex = controller.SelectedGeneratorIndex();
            if (selectedIndex < 0 || selectedIndex >= cachedNames.Count)
            {
                selectedIndex = 0;
            }

            dropdown.value = cachedNames[selectedIndex];
            dropdown.RegisterValueChangedCallback(evt => controller.ChangeGenerator(evt.newValue));

            foldout.Add(dropdown);
        }

        /// <summary>
        /// Adds a help label to the foldout when no generators are found.
        /// </summary>
        /// <param name="foldout">The foldout to which the help label will be added.</param>
        private void AddHelpLabel(Foldout foldout)
        {
            var helpLabel = StyleUtils.HelpLabel(string.Empty);

            helpLabel.SetLocalizedText(LocalizationKeysHelper.SelectionNoGeneratorFound,
                LocalizationKeysHelper.SelectionTable);

            foldout.Add(helpLabel);
        }
    }
}

# endif