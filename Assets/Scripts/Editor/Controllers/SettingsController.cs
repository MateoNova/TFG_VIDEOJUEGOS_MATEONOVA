using System.Collections.Generic;
using System.Reflection;
using Editor.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Controllers
{
    public class SettingsController
    {
        

        # region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorSettings"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public SettingsController()
        {
        }

        


        /// <summary>
        /// Adds a field to the settings container that does not have any custom attributes.
        /// </summary>
        /// <param name="property">The serialized property to add.</param>
        /// <param name="serializedObject">The serialized object containing the property.</param>
        /// <param name="settingsContainer">The container to add the field to.</param>
        public void AddNoCustomAttributesField(SerializedProperty property, SerializedObject serializedObject,
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
        public bool CheckForConditionalAttribute(FieldInfo fieldInfo, SerializedProperty property,
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
        public bool CheckForConditionAttribute(FieldInfo fieldInfo, SerializedProperty property,
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
        internal void UpdateConditionalVisibility(string group, SerializedObject serializedObject,
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

        

        # endregion
    }
}