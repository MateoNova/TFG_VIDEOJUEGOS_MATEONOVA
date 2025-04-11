using System;

namespace Views.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class WallTileGroupAttribute : Attribute
    {
        public string GroupName { get; }

        public WallTileGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}