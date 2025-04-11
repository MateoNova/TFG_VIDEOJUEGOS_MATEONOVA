using System;
using NUnit.Framework;

namespace Views.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionAttribute : PropertyAttribute
    {
        public string Group { get; }

        public ConditionAttribute(string group = null)
        {
            Group = group;
        }
    }
}