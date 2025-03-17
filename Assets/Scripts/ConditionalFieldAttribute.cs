using System;
using UnityEditor;
using UnityEngine;
using PropertyAttribute = NUnit.Framework.PropertyAttribute;

/// <summary>
/// Attribute to conditionally display a field in the Unity Inspector based on the value of another field.
/// </summary>
// [AttributeUsage(AttributeTargets.Field)]
// public class ConditionalFieldAttribute : PropertyAttribute
// {
//     /// <summary>
//     /// Gets the name of the field that determines the condition.
//     /// </summary>
//     public string ConditionFieldName { get; private set; }
//
//     /// <summary>
//     /// Initializes a new instance of the <see cref="ConditionalFieldAttribute"/> class.
//     /// </summary>
//     /// <param name="conditionFieldName">The name of the field that determines the condition.</param>
//     public ConditionalFieldAttribute(string conditionFieldName)
//     {
//         ConditionFieldName = conditionFieldName;
//     }
// }
[AttributeUsage(AttributeTargets.Field)]
public class WallTileGroupAttribute : Attribute
{
    public string GroupName { get; }

    public WallTileGroupAttribute(string groupName)
    {
        GroupName = groupName;
    }
}

//[AttributeUsage(AttributeTargets.Field)]
public class WalkableTileGroupAttribute : Attribute
{
    public bool IsTileBases { get; }
    public bool IsTilePriorities { get; }

    public WalkableTileGroupAttribute(bool isTileBases = false, bool isTilePriorities = false)
    {
        IsTileBases = isTileBases;
        IsTilePriorities = isTilePriorities;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class ConditionAttribute : PropertyAttribute
{
    public string Group { get; }

    public ConditionAttribute(string group = null)
    {
        Group = group;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string ConditionGroup { get; }

    public ConditionalFieldAttribute(string conditionGroup = null)
    {
        ConditionGroup = conditionGroup;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class OpenGraphEditorAttribute : Attribute
{
}

[CustomPropertyDrawer(typeof(OpenGraphEditorAttribute))]
public class OpenGraphEditorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label);

        if (GUI.Button(new Rect(position.xMax - 60, position.y, 60, position.height), "Open"))
        {
            GraphBasedGenerator.GraphWindow.ShowWindow();
        }
    }
}