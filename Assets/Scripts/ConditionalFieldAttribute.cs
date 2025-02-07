using System;
using NUnit.Framework;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string ConditionFieldName { get; private set; }

    public ConditionalFieldAttribute(string conditionFieldName)
    {
        ConditionFieldName = conditionFieldName;
    }
}