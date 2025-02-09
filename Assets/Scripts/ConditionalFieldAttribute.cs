using System;
using NUnit.Framework;

/// <summary>
/// Attribute to conditionally display a field in the Unity Inspector based on the value of another field.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ConditionalFieldAttribute : PropertyAttribute
{
    /// <summary>
    /// Gets the name of the field that determines the condition.
    /// </summary>
    public string ConditionFieldName { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalFieldAttribute"/> class.
    /// </summary>
    /// <param name="conditionFieldName">The name of the field that determines the condition.</param>
    public ConditionalFieldAttribute(string conditionFieldName)
    {
        ConditionFieldName = conditionFieldName;
    }
}