using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Utils;
using EventBus = Models.Editor.EventBus;
using GeneratorService = Models.Editor.GeneratorService;
using SettingsController = Controllers.Editor.SettingsController;
using StyleUtils = Utils.StyleUtils;

#if UNITY_EDITOR

namespace Views.Editor
{
    /// <summary>
    /// Represents the settings view for the editor.
    /// Responsible for displaying and managing the UI elements related to generator settings.
    /// </summary>
    public class SettingsView
    {
        /// <summary>
        /// Indicates whether the "Open Graph Window" button should be displayed.
        /// </summary>
        private bool _showOpenGraphButton;

        /// <summary>
        /// The controller responsible for handling settings logic.
        /// </summary>
        private readonly SettingsController _settingsController = new();

        /// <summary>
        /// The container for the UI elements of the settings view.
        /// </summary>
        private VisualElement _container;

        /// <summary>
        /// The button for opening the graph window.
        /// </summary>
        private Button _openGraphButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView"/> class.
        /// Subscribes to centralized events for UI updates.
        /// </summary>
        public SettingsView()
        {
            EventBus.ToggleOpenGraphButton += ShowOpenGraphWindowButton;
            EventBus.GeneratorChanged += Repaint;
        }

        /// <summary>
        /// Creates the UI for the settings view.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements for generator settings.</returns>
        public VisualElement CreateUI()
        {
            _container ??= StyleUtils.SimpleContainer();
            _container.Clear();

            var foldout = StyleUtils.SimpleExpander(string.Empty, out var body);
            foldout.SetLocalizedText(LocalizationKeysHelper.SettingsFoldout, LocalizationKeysHelper.SettingsTable);

            AddGeneratorSettings(body);
            AddOpenGraphButton(body);
            _container.Add(foldout);

            return _container;
        }

        /// <summary>
        /// Adds generator-specific settings to the foldout.
        /// Displays a placeholder message if no generator is selected.
        /// </summary>
        private void AddGeneratorSettings(VisualElement foldout)
        {
            if (GeneratorService.Instance.CurrentGenerator == null) AddNoGeneratorSelectedLabel(foldout);
            else AddGeneratorProperties(foldout);
        }

        /// <summary>
        /// Adds a label to the foldout indicating that no generator is selected.
        /// </summary>
        private void AddNoGeneratorSelectedLabel(VisualElement foldout)
        {
            var infoLabel = StyleUtils.HelpLabel("No generator selected.");
            foldout.Add(infoLabel);
        }

        /// <summary>
        /// Adds the properties of the selected generator to the UI.
        /// </summary>
        private void AddGeneratorProperties(VisualElement foldout)
        {
            var settingsContainer = new VisualElement();
            var serializedObject = new SerializedObject(GeneratorService.Instance.CurrentGenerator);

            // Dictionaries to manage conditional and conditionally visible fields.
            var conditionFields = new Dictionary<string, PropertyField>();
            var conditionalFields = new Dictionary<string, List<PropertyField>>();
            var property = serializedObject.GetIterator();
            var enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.name == "m_Script") continue;

                var fieldInfo = serializedObject.targetObject.GetType()
                    .GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic);

                if (fieldInfo == null)
                {
                    // Add fields without custom attributes to the UI.
                    _settingsController.AddNoCustomAttributesField(property, serializedObject, settingsContainer);
                    continue;
                }

                // Check for and handle condition attributes.
                if (_settingsController.CheckForConditionAttribute(fieldInfo, property, serializedObject,
                        settingsContainer, conditionFields, conditionalFields))
                    continue;

                // Check for and handle conditional attributes.
                if (!_settingsController.CheckForConditionalAttribute(fieldInfo, property, serializedObject,
                        settingsContainer, conditionalFields))
                {
                    _settingsController.AddNoCustomAttributesField(property, serializedObject, settingsContainer);
                }
            }

            foreach (var group in conditionalFields.Keys)
            {
                _settingsController.UpdateConditionalVisibility(group, serializedObject, conditionalFields);
            }

            foldout.Add(settingsContainer);
        }

        /// <summary>
        /// Adds the "Open Graph Window" button to the foldout.
        /// </summary>
        private void AddOpenGraphButton(VisualElement foldout)
        {
            _openGraphButton = StyleUtils.DisplayChangeButton(_showOpenGraphButton,
                () => GeneratorService.Instance.CurrentGenerator.OpenGraphWindow());

            _openGraphButton.SetLocalizedText(LocalizationKeysHelper.SettingsGraphWindow,
                LocalizationKeysHelper.SettingsTable);
            foldout.Add(_openGraphButton);
        }

        /// <summary>
        /// Toggles the visibility of the "Open Graph Window" button.
        /// </summary>
        /// <param name="show">Indicates whether the button should be shown.</param>
        private void ShowOpenGraphWindowButton(bool show)
        {
            _showOpenGraphButton = show;
            Repaint();
        }

        /// <summary>
        /// Repaints the UI by recreating it and marking it for a visual update.
        /// </summary>
        private void Repaint()
        {
            if (_container == null) _container = StyleUtils.SimpleContainer();
            else _container.Clear();

            CreateUI();
            _container.MarkDirtyRepaint();
        }
    }
}

#endif