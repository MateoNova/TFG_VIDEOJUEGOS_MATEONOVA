using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Controllers
{
    public class SettingsController
    {
        public SettingsController()
        {
        }

        public void AddNoCustomAttributesField(SerializedProperty property, SerializedObject serializedObject,
            VisualElement container)
        {
            PropertyField normalField = new PropertyField(property);
            normalField.Bind(serializedObject);
            container.Add(normalField);
        }

        public bool CheckForConditionalAttribute(FieldInfo fieldInfo, SerializedProperty property,
            SerializedObject serializedObject, VisualElement container,
            Dictionary<string, List<PropertyField>> conditionalFields)
        {
            if (fieldInfo.GetCustomAttribute(typeof(ConditionalFieldAttribute)) is not ConditionalFieldAttribute
                conditionalAttr)
                return false;

            string group = conditionalAttr.ConditionGroup ?? property.name;
            PropertyField conditionalPropField = new PropertyField(property);
            conditionalPropField.Bind(serializedObject);
            container.Add(conditionalPropField);

            if (!conditionalFields.ContainsKey(group))
            {
                conditionalFields[group] = new List<PropertyField>();
            }

            conditionalFields[group].Add(conditionalPropField);
            return true;
        }

        public bool CheckForConditionAttribute(FieldInfo fieldInfo, SerializedProperty property,
            SerializedObject serializedObject, VisualElement container,
            Dictionary<string, PropertyField> conditionFields,
            Dictionary<string, List<PropertyField>> conditionalFields)
        {
            if (fieldInfo.GetCustomAttribute(typeof(ConditionAttribute)) is not ConditionAttribute conditionAttr)
                return false;

            // Si no se especifica grupo, se usa el nombre del campo.
            string group = conditionAttr.Group ?? property.name;
            PropertyField conditionPropField = new PropertyField(property);
            conditionPropField.Bind(serializedObject);
            container.Add(conditionPropField);

            conditionFields[group] = conditionPropField;
            // Se añade callback para actualizar la visibilidad de los campos condicionales.
            conditionPropField.RegisterValueChangeCallback(_ =>
            {
                UpdateConditionalVisibility(group, serializedObject, conditionalFields);
            });
            return true;
        }

        public void UpdateConditionalVisibility(string group, SerializedObject serializedObject,
            Dictionary<string, List<PropertyField>> conditionalFields)
        {
            object target = serializedObject.targetObject;
            FieldInfo conditionField = target.GetType()
                .GetField(group, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (conditionField == null)
            {
                Debug.LogWarning($"No se encontró el campo condición para el grupo: '{group}'.");
                return;
            }

            if (conditionField.FieldType != typeof(bool))
            {
                Debug.LogWarning($"El campo condición '{group}' no es de tipo booleano.");
                return;
            }

            bool conditionValue = (bool)conditionField.GetValue(target);
            if (!conditionalFields.TryGetValue(group, out List<PropertyField> fields)) return;

            foreach (var field in fields)
            {
                field.style.display = conditionValue ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}