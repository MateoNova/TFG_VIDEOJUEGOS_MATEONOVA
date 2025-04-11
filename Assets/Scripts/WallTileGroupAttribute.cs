using System;

[AttributeUsage(AttributeTargets.Field)]
public class WallTileGroupAttribute : Attribute
{
    public string GroupName { get; }

    public WallTileGroupAttribute(string groupName)
    {
        GroupName = groupName;
    }
}