#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;
using Utils;
using EventBus = Models.Editor.EventBus;
using InitializationController = Controllers.Editor.InitializationController;

namespace Views.Editor
{
    /// <summary>
    /// Represents the initialization view in the Unity Editor.
    /// Responsible for creating and managing the UI elements related to initialization actions.
    /// </summary>
    public class InitializationView
    {
        private const string LocalizationTable = "InitializationTable";

        /// <summary>
        /// Subscribes to the ToolOpened event to initialize the scene when the tool is opened.
        /// </summary>
        public InitializationView()
        {
            EventBus.ToolOpened += InitializationController.InitScene;
        }

        /// <summary>
        /// Unsubscribes from the ToolOpened event when the view is destroyed.
        /// </summary>
        ~InitializationView()
        {
            EventBus.ToolOpened -= InitializationController.InitScene;
        }

        /// <summary>
        /// Creates the UI for the initialization view.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements for initialization actions.</returns>
        public static VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();
            var foldout = CreateFoldout();

            foldout.Add(CreateButtonContainer());
            foldout.Add(CreateLanguageSelector());

            container.Add(foldout);
            return container;
        }

        /// <summary>
        /// Creates a foldout for the initialization view.
        /// </summary>
        private static Foldout CreateFoldout()
        {
            var foldout = StyleUtils.ModernFoldout("");
            foldout.SetLocalizedText("Initialization", LocalizationTable);
            return foldout;
        }

        /// <summary>
        /// Creates a dropdown field for selecting the language.
        /// </summary>
        private static DropdownField CreateLanguageSelector()
        {
            var dropdown = StyleUtils.SimpleDropdown();
            dropdown.SetLocalizedTitle("SelectLanguage", LocalizationTable);

            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                PopulateDropdown(dropdown);
            }
            else
            {
                LocalizationSettings.InitializationOperation.Completed += _ => PopulateDropdown(dropdown);
            }

            return dropdown;
        }

        /// <summary>
        /// Populates the language dropdown with available locales.
        /// </summary>
        private static void PopulateDropdown(DropdownField dropdown)
        {
            var availableLocales = LocalizationSettings.AvailableLocales?.Locales;
            if (availableLocales == null)
            {
                Debug.LogError("AvailableLocales is null. Ensure localization is configured correctly.");
                return;
            }

            foreach (var locale in availableLocales)
            {
                dropdown.choices.Add(locale.LocaleName);
            }

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

        /// <summary>
        /// Creates a container for action buttons (e.g., Clear, Initialize, Reload).
        /// </summary>
        private static VisualElement CreateButtonContainer()
        {
            var buttonContainer = StyleUtils.RowButtonContainer();

            AddButton(buttonContainer, "ClearAndDelete", InitializationController.ClearCachedData, true);
            AddButton(buttonContainer, "InitializeScene", InitializationController.InitScene);
            AddButton(buttonContainer, "Reload", InitializationController.ReloadAll);

            return buttonContainer;
        }

        /// <summary>
        /// Adds a button to the specified container.
        /// </summary>
        private static void AddButton(VisualElement container, string key, Action onClick, bool isPrimary = false)
        {
            var button = StyleUtils.ButtonInRowContainer("", onClick, isPrimary);
            button.SetLocalizedText(key, LocalizationTable);
            container.Add(button);
        }
    }
}

#endif