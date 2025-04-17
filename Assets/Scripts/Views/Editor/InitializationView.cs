using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;
using Utils;
using EventBus = Models.Editor.EventBus;
using InitializationController = Controllers.Editor.InitializationController;

#if UNITY_EDITOR

namespace Views.Editor
{
    /// <summary>
    /// Represents the initialization view in the Unity Editor.
    /// Responsible for creating and managing the UI elements related to initialization actions.
    /// </summary>
    public class InitializationView
    {
        /// <summary>
        /// The controller responsible for handling initialization logic.
        /// </summary>
        private readonly InitializationController _controller = new();

        /// <summary>
        /// Subscribes to the ToolOpened event to initialize the scene when the tool is opened.
        /// </summary>
        public InitializationView()
        {
            EventBus.ToolOpened += _controller.InitScene;
        }

        /// <summary>
        /// Unsubscribes from the ToolOpened event when the view is destroyed.
        /// </summary>
        ~InitializationView()
        {
            EventBus.ToolOpened -= _controller.InitScene;
        }

        /// <summary>
        /// Creates the UI for the initialization view.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements for initialization actions.</returns>
        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();

            var foldout = StyleUtils.ModernFoldout("");
            foldout.SetLocalizedText("Initialization", "InitializationTable");

            foldout.Add(CreateButtonContainer());
            foldout.Add(CreateLanguageSelector());

            container.Add(foldout);
            return container;
        }

        /// <summary>
        /// Creates a dropdown field for selecting the language.
        /// </summary>
        /// <returns>A <see cref="DropdownField"/> for language selection.</returns>
        private DropdownField CreateLanguageSelector()
        {
            var dropdown = StyleUtils.SimpleDropdown();
            dropdown.SetLocalizedTitle("SelectLanguage", "InitializationTable");

            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                PopulateDropdown(dropdown);
            }
            else
            {
                LocalizationSettings.InitializationOperation.Completed += _ => { PopulateDropdown(dropdown); };
            }

            return dropdown;
        }

        /// <summary>
        /// Populates the language dropdown with available locales.
        /// </summary>
        /// <param name="dropdown">The dropdown field to populate.</param>
        private void PopulateDropdown(DropdownField dropdown)
        {
            var availableLocales = LocalizationSettings.AvailableLocales?.Locales;
            if (availableLocales != null)
            {
                foreach (var locale in availableLocales)
                    dropdown.choices.Add(locale.LocaleName);

                dropdown.value = LocalizationSettings.SelectedLocale?.LocaleName ?? dropdown.choices[0];

                dropdown.RegisterValueChangedCallback(evt =>
                {
                    var selectedLocale = availableLocales.Find(locale => locale.LocaleName == evt.newValue);
                    if (selectedLocale != null)
                    {
                        LocalizationSettings.SelectedLocale = selectedLocale;
                    }
                });
            }
            else
            {
                Debug.LogError("AvailableLocales is null. Ensure localization is configured correctly.");
            }
        }

        /// <summary>
        /// Creates a container for action buttons (e.g., Clear, Initialize, Reload).
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the action buttons.</returns>
        private VisualElement CreateButtonContainer()
        {
            var buttonContainer = StyleUtils.RowButtonContainer();

            buttonContainer.Add(CreateButton("ClearAndDelete", _controller.ClearCachedData, true));
            buttonContainer.Add(CreateButton("InitializeScene", _controller.InitScene));
            buttonContainer.Add(CreateButton("Reload", _controller.ReloadAll));

            return buttonContainer;
        }

        /// <summary>
        /// Creates a button with the specified properties.
        /// </summary>
        /// <param name="key">The localization key for the button text.</param>
        /// <param name="onClick">The action to execute when the button is clicked.</param>
        /// <param name="isPrimary">Indicates whether the button is styled as a primary button.</param>
        /// <returns>A <see cref="Button"/> configured with the specified properties.</returns>
        private static Button CreateButton(string key, Action onClick, bool isPrimary = false)
        {
            var button = StyleUtils.ButtonInRowContainer("", onClick, isPrimary);
            button.SetLocalizedText(key, "InitializationTable");
            return button;
        }
    }
}

#endif