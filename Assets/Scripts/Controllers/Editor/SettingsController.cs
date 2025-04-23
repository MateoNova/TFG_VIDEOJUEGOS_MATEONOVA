using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Views.Attributes;

namespace Controllers.Editor
{
    /// <summary>
    /// Controller responsible for managing the dynamic display of fields in the Unity Editor
    /// based on custom attributes and conditions.
    /// </summary>
    public class SettingsController
    {
        /// <summary>
        /// Adds a field to the UI container without checking for custom attributes.
        /// </summary>
        /// <param name="property">The serialized property to add.</param>
        /// <param name="serializedObject">The serialized object containing the property.</param>
        /// <param name="container">The UI container to which the field will be added.</param>
        public void AddNoCustomAttributesField(SerializedProperty property, SerializedObject serializedObject,
            VisualElement container)
        {
            var normalField = new PropertyField(property);
            normalField.Bind(serializedObject);
            container.Add(normalField);
        }

        /// <summary>
        /// Checks if a field has a ConditionalFieldAttribute and adds it to the UI container if present.
        /// </summary>
        /// <param name="fieldInfo">The field's metadata.</param>
        /// <param name="property">The serialized property to check.</param>
        /// <param name="serializedObject">The serialized object containing the property.</param>
        /// <param name="container">The UI container to which the field will be added.</param>
        /// <param name="conditionalFields">A dictionary to track fields grouped by their condition.</param>
        /// <returns>True if the attribute is found and processed; otherwise, false.</returns>
        public bool CheckForConditionalAttribute(FieldInfo fieldInfo, SerializedProperty property,
            SerializedObject serializedObject, VisualElement container,
            Dictionary<string, List<PropertyField>> conditionalFields)
        {
            if (fieldInfo.GetCustomAttribute(typeof(ConditionalFieldAttribute)) is not ConditionalFieldAttribute
                conditionalAttr)
                return false;

            var group = conditionalAttr.ConditionGroup ?? property.name;
            var conditionalPropField = new PropertyField(property);
            conditionalPropField.Bind(serializedObject);
            container.Add(conditionalPropField);

            if (!conditionalFields.ContainsKey(group))
            {
                conditionalFields[group] = new List<PropertyField>();
            }

            conditionalFields[group].Add(conditionalPropField);
            return true;
        }

        /// <summary>
        /// Checks if a field has a ConditionAttribute and adds it to the UI container if present.
        /// Registers a callback to update the visibility of dependent fields.
        /// </summary>
        /// <param name="fieldInfo">The field's metadata.</param>
        /// <param name="property">The serialized property to check.</param>
        /// <param name="serializedObject">The serialized object containing the property.</param>
        /// <param name="container">The UI container to which the field will be added.</param>
        /// <param name="conditionFields">A dictionary to track condition fields by group.</param>
        /// <param name="conditionalFields">A dictionary to track fields grouped by their condition.</param>
        /// <returns>True if the attribute is found and processed; otherwise, false.</returns>
        public bool CheckForConditionAttribute(FieldInfo fieldInfo, SerializedProperty property,
            SerializedObject serializedObject, VisualElement container,
            Dictionary<string, PropertyField> conditionFields,
            Dictionary<string, List<PropertyField>> conditionalFields)
        {
            if (fieldInfo.GetCustomAttribute(typeof(ConditionAttribute)) is not ConditionAttribute conditionAttr)
                return false;

            // If no group is specified, use the field name as the group.
            var group = conditionAttr.Group ?? property.name;
            var conditionPropField = new PropertyField(property);
            conditionPropField.Bind(serializedObject);
            container.Add(conditionPropField);

            conditionFields[group] = conditionPropField;

            // Register a callback to update the visibility of dependent fields.
            conditionPropField.RegisterValueChangeCallback(_ =>
            {
                UpdateConditionalVisibility(group, serializedObject, conditionalFields);
            });
            return true;
        }

        /// <summary>
        /// Updates the visibility of fields based on the value of a condition field.
        /// </summary>
        /// <param name="group">The group name of the condition field.</param>
        /// <param name="serializedObject">The serialized object containing the condition field.</param>
        /// <param name="conditionalFields">A dictionary of fields grouped by their condition.</param>
        public void UpdateConditionalVisibility(string group, SerializedObject serializedObject,
            Dictionary<string, List<PropertyField>> conditionalFields)
        {
            object target = serializedObject.targetObject;
            var conditionField = target.GetType()
                .GetField(group, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            if (conditionField == null)
            {
                Debug.LogWarning($"Condition field not found for group: '{group}'.");
                return;
            }

            if (conditionField.FieldType != typeof(bool))
            {
                Debug.LogWarning($"Condition field '{group}' is not of type boolean.");
                return;
            }

            var conditionValue = (bool)conditionField.GetValue(target);
            if (!conditionalFields.TryGetValue(group, out var fields)) return;

            foreach (var field in fields)
            {
                field.style.display = conditionValue ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}