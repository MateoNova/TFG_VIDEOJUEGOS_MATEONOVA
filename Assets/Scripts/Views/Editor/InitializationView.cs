using System;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;
using Utils;
using EventBus = Models.Editor.EventBus;
using InitializationController = Controllers.Editor.InitializationController;


namespace Views.Editor
{
    public class InitializationView
    {
        private readonly InitializationController _controller = new();

        public InitializationView()
        {
            EventBus.ToolOpened += _controller.InitScene;
        }

        ~InitializationView()
        {
            EventBus.ToolOpened -= _controller.InitScene;
        }

        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();
            // Usamos la clave para la localización en el foldout
            var foldout = StyleUtils.ModernFoldout("");
            foldout.SetLocalizedText("Initialization", "InitializationTable");

            foldout.Add(CreateButtonContainer());
            foldout.Add(CreateLanguageSelector());
            container.Add(foldout);
            return container;
        }

        private DropdownField CreateLanguageSelector()
        {
            // Se puede aplicar la localización tanto en el texto del dropdown como en su título si lo deseas.
            var dropdown = StyleUtils.SimpleDropdown();
            dropdown.SetLocalizedTitle("SelectLanguage", "InitializationTable");

            if (LocalizationSettings.InitializationOperation.IsDone)
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
            else
            {
                Debug.LogError(
                    "Localization system is not initialized. Ensure it is initialized before accessing locales.");
            }

            return dropdown;
        }

        private VisualElement CreateButtonContainer()
        {
            var buttonContainer = StyleUtils.RowButtonContainer();
            // Usando claves de localización en lugar de textos fijos.
            buttonContainer.Add(CreateButton("ClearAndDelete", _controller.ClearCachedData, true));
            buttonContainer.Add(CreateButton("InitializeScene", _controller.InitScene));
            buttonContainer.Add(CreateButton("Reload", _controller.ReloadAll));

            return buttonContainer;
        }

        private static Button CreateButton(string key, Action onClick, bool isPrimary = false)
        {
            // Se crea el botón utilizando tu método de estilos.
            var button = StyleUtils.ButtonInRowContainer("", onClick, isPrimary);
            // Se actualiza y se suscribe para actualizar su texto usando la clave y la tabla.
            button.SetLocalizedText(key, "InitializationTable");
            return button;
        }
    }
}