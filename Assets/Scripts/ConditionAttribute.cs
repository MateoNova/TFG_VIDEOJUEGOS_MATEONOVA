using System;
using NUnit.Framework;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionAttribute : PropertyAttribute
{
    public string Group { get; }

    public ConditionAttribute(string group = null)
    {
        Group = group;
    }
}