using System;
using PropertyAttribute = NUnit.Framework.PropertyAttribute;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string ConditionGroup { get; }

    public ConditionalFieldAttribute(string conditionGroup = null)
    {
        ConditionGroup = conditionGroup;
    }
}