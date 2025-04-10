using System.Collections.Generic;
using System.Reflection;
using Editor.Controllers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class SettingsView
    {
        private static bool _showOpenGraphButton;

        
        private SettingsController _settingsController = new();
        
        private VisualElement _container;

        
        private Button _openGraphButton;
        private Foldout _foldout;

        public SettingsView()
        {
            SelectionController.ShowButtonOpenGraphWindow += ShowOpenGraphWindowButton;
            SelectionController.OnGeneratorChanged += Repaint;

        }
        
        ~SettingsView()
        {
            SelectionController.ShowButtonOpenGraphWindow -= ShowOpenGraphWindowButton;
            SelectionController.OnGeneratorChanged -= Repaint;



        }
        
        
        public VisualElement CreateUI()
        {
            if (_container == null)
            {
                _container = StyleUtils.SimpleContainer();
            }
            else
            {
                _container.Clear();
            }

            _foldout = new Foldout { text = "Generator Settings", value = true };

            if (_settingsController.getCurrentGenerator() == null)
            {
                var infoLabel = StyleUtils.HelpLabel("No generator selected.");
                _foldout.Add(infoLabel);
            }
            else
            {
                var settingsContainer = new VisualElement();
                var serializedObject = new SerializedObject(_settingsController.getCurrentGenerator());

                var conditionFields = new Dictionary<string, PropertyField>();
                var conditionalFields = new Dictionary<string, List<PropertyField>>();

                var property = serializedObject.GetIterator();
                var enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.name == "m_Script") continue; // Skip the script field.

                    var fieldInfo = serializedObject.targetObject.GetType().GetField(property.name,
                        BindingFlags.Instance | BindingFlags.NonPublic);

                    if (fieldInfo == null)
                    {
                        _settingsController.AddNoCustomAttributesField(property, serializedObject, settingsContainer);
                        continue;
                    }

                    if (_settingsController.CheckForConditionAttribute(fieldInfo, property, serializedObject, settingsContainer,
                            conditionFields, conditionalFields)) continue;

                    if (!_settingsController.CheckForConditionalAttribute(fieldInfo, property, serializedObject, settingsContainer,
                            conditionalFields))
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

            _openGraphButton = new Button(() => _settingsController.getCurrentGenerator().OpenGraphWindow())
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
        }

        public static bool GetShowOpenGraphButton() => _showOpenGraphButton;
        
        private void Repaint()
        {
            if (_container == null)
            {
                _container = StyleUtils.SimpleContainer();
            }
            else
            {
                _container.Clear();
            }

            CreateUI();
            _container.MarkDirtyRepaint();
        }
    }
    
    
}