using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    /// <summary>
    /// Manages the settings for the selected generator.
    /// </summary>
    public class GeneratorSettings
    {
        # region Fields

        private readonly GeneratorSelection _generatorSelection;
        private VisualElement _container;

        # endregion

        # region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorSettings"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public GeneratorSettings(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
            GeneratorSelection.OnGeneratorChanged += Repaint;
        }

        /// <summary>
        /// Creates the UI for the generator settings.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
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

            var foldout = new Foldout { text = "Generator Settings", value = true };

            if (_generatorSelection.CurrentGenerator == null)
            {
                var infoLabel = StyleUtils.HelpLabel("No generator selected.");
                foldout.Add(infoLabel);
            }
            else
            {
                var settingsContainer = new VisualElement();
                var serializedObject = new SerializedObject(_generatorSelection.CurrentGenerator);

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
                        AddNoCustomAttributesField(property, serializedObject, settingsContainer);
                        continue;
                    }

                    if (CheckForConditionAttribute(fieldInfo, property, serializedObject, settingsContainer,
                            conditionFields, conditionalFields)) continue;

                    if (!CheckForConditionalAttribute(fieldInfo, property, serializedObject, settingsContainer,
                            conditionalFields))
                    {
                        AddNoCustomAttributesField(property, serializedObject, settingsContainer);
                    }
                }

                foreach (var group in conditionalFields.Keys)
                {
                    UpdateConditionalVisibility(group, serializedObject, conditionalFields);
                }

                foldout.Add(settingsContainer);
            }

            _container.Add(foldout);
            return _container;
        }

        /// <summary>
        /// Adds a field to the settings container that does not have any custom attributes.
        /// </summary>
        /// <param name="property">The serialized property to add.</param>
        /// <param name="serializedObject">The serialized object containing the property.</param>
        /// <param name="settingsContainer">The container to add the field to.</param>
        private static void AddNoCustomAttributesField(SerializedProperty property, SerializedObject serializedObject,
            VisualElement settingsContainer)
        {
            var normalField = new PropertyField(property);
            normalField.Bind(serializedObject);
            settingsContainer.Add(normalField);
        }

        /// <summary>
        /// Checks if a field has a ConditionalFieldAttribute and adds it to the settings container if it does.
        /// </summary>
        /// <param name="fieldInfo">The field information to check.</param>
        /// <param name="property">The serialized property to add.</param>
        /// <param name="serializedObject">The serialized object containing the property.</param>
        /// <param name="settingsContainer">The container to add the field to.</param>
        /// <param name="conditionalFields">A dictionary of conditional fields grouped by their condition group.</param>
        /// <returns>True if the field has a ConditionalFieldAttribute, otherwise false.</returns>
        private static bool CheckForConditionalAttribute(FieldInfo fieldInfo, SerializedProperty property,
            SerializedObject serializedObject, VisualElement settingsContainer,
            Dictionary<string, List<PropertyField>> conditionalFields)
        {
            if (fieldInfo.GetCustomAttribute(typeof(ConditionalFieldAttribute)) is not ConditionalFieldAttribute
                conditionalAttr) return false;

            var group = conditionalAttr.ConditionGroup ?? property.name;
            var conditionalPropField = new PropertyField(property);
            conditionalPropField.Bind(serializedObject);
            settingsContainer.Add(conditionalPropField);

            if (!conditionalFields.ContainsKey(group))
            {
                conditionalFields[group] = new List<PropertyField>();
            }

            conditionalFields[group].Add(conditionalPropField);
            return true;
        }

        /// <summary>
        /// Checks if a field has a ConditionAttribute and adds it to the settings container if it does.
        /// </summary>
        /// <param name="fieldInfo">The field information to check.</param>
        /// <param name="property">The serialized property to add.</param>
        /// <param name="serializedObject">The serialized object containing the property.</param>
        /// <param name="settingsContainer">The container to add the field to.</param>
        /// <param name="conditionFields">A dictionary of condition fields grouped by their condition group.</param>
        /// <param name="conditionalFields">A dictionary of conditional fields grouped by their condition group.</param>
        /// <returns>True if the field has a ConditionAttribute, otherwise false.</returns>
        private static bool CheckForConditionAttribute(FieldInfo fieldInfo, SerializedProperty property,
            SerializedObject serializedObject, VisualElement settingsContainer,
            Dictionary<string, PropertyField> conditionFields,
            Dictionary<string, List<PropertyField>> conditionalFields)
        {
            if (fieldInfo.GetCustomAttribute(typeof(ConditionAttribute)) is not ConditionAttribute conditionAttr)
                return false;

            // If no group is specified, use the field name.
            var group = conditionAttr.Group ?? property.name;
            var conditionPropField = new PropertyField(property);
            conditionPropField.Bind(serializedObject);
            settingsContainer.Add(conditionPropField);

            conditionFields[group] = conditionPropField;

            // Add a callback to update the visibility of the conditional fields.
            conditionPropField.RegisterValueChangeCallback(_ =>
            {
                UpdateConditionalVisibility(group, serializedObject, conditionalFields);
            });
            return true;
        }

        /// <summary>
        /// Updates the visibility of conditional fields based on the value of their condition field.
        /// </summary>
        /// <param name="group">The condition group to update.</param>
        /// <param name="serializedObject">The serialized object containing the fields.</param>
        /// <param name="conditionalFields">A dictionary of conditional fields grouped by their condition group.</param>
        private static void UpdateConditionalVisibility(string group, SerializedObject serializedObject,
            Dictionary<string, List<PropertyField>> conditionalFields)
        {
            var target = serializedObject.targetObject;
            var conditionField = target.GetType()
                .GetField(group, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (conditionField == null)
            {
                Debug.LogWarning($"No condition attribute found for group: '{group}'.");
                return;
            }

            if (conditionField.FieldType != typeof(bool))
            {
                Debug.LogWarning($"The condition field '{group}' isn't boolean.");
                return;
            }

            var conditionValue = (bool)conditionField.GetValue(target);

            if (!conditionalFields.TryGetValue(group, out var conditionalField)) return;
            foreach (var field in conditionalField)
            {
                field.style.display = conditionValue ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>
        /// Repaints the UI when the generator selection changes.
        /// </summary>
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

        # endregion
    }
}