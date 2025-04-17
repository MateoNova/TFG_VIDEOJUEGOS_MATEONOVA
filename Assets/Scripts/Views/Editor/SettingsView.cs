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
        private static bool _showOpenGraphButton;

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
        /// The foldout element for grouping generator settings.
        /// </summary>
        private Foldout _foldout;

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

            _foldout = StyleUtils.ModernFoldout("");
            _foldout.SetLocalizedText("GeneratorSettings", "SettingsTable");

            if (GeneratorService.Instance.CurrentGenerator == null)
            {
                var infoLabel = StyleUtils.HelpLabel("No generator selected.");
                _foldout.Add(infoLabel);
            }
            else
            {
                var settingsContainer = new VisualElement();
                var serializedObject = new SerializedObject(GeneratorService.Instance.CurrentGenerator);

                var conditionFields = new Dictionary<string, PropertyField>();
                var conditionalFields = new Dictionary<string, List<PropertyField>>();
                var property = serializedObject.GetIterator();
                var enterChildren = true;

                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.name == "m_Script") continue;

                    var fieldInfo = serializedObject.targetObject.GetType().GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic);

                    if (fieldInfo == null)
                    {
                        _settingsController.AddNoCustomAttributesField(property, serializedObject, settingsContainer);
                        continue;
                    }

                    if (_settingsController.CheckForConditionAttribute(fieldInfo, property, serializedObject, settingsContainer, conditionFields, conditionalFields))
                        continue;

                    if (!_settingsController.CheckForConditionalAttribute(fieldInfo, property, serializedObject, settingsContainer, conditionalFields))
                    {
                        _settingsController.AddNoCustomAttributesField(property, serializedObject, settingsContainer);
                    }
                }


                foreach (var group in conditionalFields.Keys)
                {
                    _settingsController.UpdateConditionalVisibility(group, serializedObject, conditionalFields);
                }

                _foldout.Add(settingsContainer);
            }

            _openGraphButton = new Button(() => GeneratorService.Instance.CurrentGenerator.OpenGraphWindow())
            {
                style = { display = _showOpenGraphButton ? DisplayStyle.Flex : DisplayStyle.None }
            };
            _openGraphButton.SetLocalizedText("OpenGraphWindow", "SettingsTable");

            _foldout.Add(_openGraphButton);

            _container.Add(_foldout);

            return _container;
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