using System.Collections.Generic;
using System.Reflection;
using Editor.Controllers;
using Editor.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class SettingsView
    {
        private static bool _showOpenGraphButton;
        private SettingsController _settingsController = new SettingsController();
        private VisualElement _container;
        private Button _openGraphButton;
        private Foldout _foldout;

        public SettingsView()
        {
            // Suscripción a eventos centralizados
            EventBus.ToggleOpenGraphButton += ShowOpenGraphWindowButton;
            EventBus.GeneratorChanged += Repaint;
        }

        public VisualElement CreateUI()
        {
            _container = _container ?? StyleUtils.SimpleContainer();
            _container.Clear();

            _foldout = new Foldout { text = "Generator Settings", value = true };

            if (GeneratorService.Instance.CurrentGenerator == null)
            {
                Label infoLabel = StyleUtils.HelpLabel("No generator selected.");
                _foldout.Add(infoLabel);
            }
            else
            {
                VisualElement settingsContainer = new VisualElement();
                SerializedObject serializedObject = new SerializedObject(GeneratorService.Instance.CurrentGenerator);

                Dictionary<string, PropertyField> conditionFields = new Dictionary<string, PropertyField>();
                Dictionary<string, List<PropertyField>> conditionalFields =
                    new Dictionary<string, List<PropertyField>>();

                SerializedProperty property = serializedObject.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.name == "m_Script") continue;

                    FieldInfo fieldInfo = serializedObject.targetObject.GetType()
                        .GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic);
                    if (fieldInfo == null)
                    {
                        _settingsController.AddNoCustomAttributesField(property, serializedObject, settingsContainer);
                        continue;
                    }

                    if (_settingsController.CheckForConditionAttribute(fieldInfo, property, serializedObject,
                            settingsContainer, conditionFields, conditionalFields))
                        continue;

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

                _foldout.Add(settingsContainer);
            }

            _openGraphButton = new Button(() => GeneratorService.Instance.CurrentGenerator.OpenGraphWindow())
            {
                text = "Open Graph Window",
                style = { display = _showOpenGraphButton ? DisplayStyle.Flex : DisplayStyle.None }
            };

            _foldout.Add(_openGraphButton);
            _container.Add(_foldout);
            return _container;
        }

        private void ShowOpenGraphWindowButton(bool show)
        {
            _showOpenGraphButton = show;
            // Se puede actualizar la UI si es necesario
            Repaint();
        }

        private void Repaint()
        {
            if (_container == null) _container = StyleUtils.SimpleContainer();
            else _container.Clear();

            CreateUI();
            _container.MarkDirtyRepaint();
        }
    }
}