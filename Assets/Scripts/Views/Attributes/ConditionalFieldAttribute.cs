using System;
using PropertyAttribute = NUnit.Framework.PropertyAttribute;

namespace Views.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public string ConditionGroup { get; }

        public ConditionalFieldAttribute(string conditionGroup = null)
        {
            ConditionGroup = conditionGroup;
        }
    }
}