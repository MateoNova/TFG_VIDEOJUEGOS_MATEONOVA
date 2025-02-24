using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

/// <summary>
/// Utility class providing helper methods.
/// </summary>
public static class Utils
{
    public enum WallPosition
    {
        Up,
        Down,
        Left,
        Right,
        TopLeft,
        BottomLeft,
        TopRight,
        BottomRight,
        TripleExceptUp,
        TripleExceptDown,
        TripleExceptLeft,
        TripleExceptRight,
        AllWallCorner,
        TopLeftInner,
        TopRightInner,
        BottomLeftInner,
        BottomRightInner,
        Alone,
        TripleExceptLeftInner,
        TripleExceptRightInner
    }


    /// <summary>
    /// Array of cardinal directions (up, down, left, right).
    /// </summary>
    public static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    /// <summary>
    /// Gets a random cardinal direction (up, right, down, left).
    /// </summary>
    /// <returns>A Vector2Int representing a random cardinal direction.</returns>
    public static Vector2Int GetRandomCardinalDirection()
    {
        var direction = Random.Range(0, 4);
        return direction switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.right,
            2 => Vector2Int.down,
            _ => Vector2Int.left
        };
    }

    /// <summary>
    /// Gets a perpendicular direction to the given direction.
    /// </summary>
    /// <param name="direction">The original direction.</param>
    /// <returns>A Vector2Int representing the perpendicular direction.</returns>
    public static Vector2Int GetPerpendicularDirection(Vector2Int direction)
    {
        return direction switch
        {
            _ when direction == Vector2Int.up => Vector2Int.right,
            _ when direction == Vector2Int.right => Vector2Int.down,
            _ when direction == Vector2Int.down => Vector2Int.left,
            _ => Vector2Int.up
        };
    }

    /// <summary>
    /// Gets the opposite direction to the given direction.
    /// </summary>
    public static bool ShouldDisplayField(SerializedObject serializedObject, string propertyName,
        System.Reflection.BindingFlags fieldBindingFlags = System.Reflection.BindingFlags.NonPublic,
        System.Reflection.BindingFlags conditionalFieldBindingFlags = System.Reflection.BindingFlags.NonPublic)
    {
        fieldBindingFlags |= System.Reflection.BindingFlags.Instance;
        conditionalFieldBindingFlags |= System.Reflection.BindingFlags.Instance;
        var targetObject = serializedObject.targetObject;
        var field = targetObject.GetType().GetField(propertyName, fieldBindingFlags);

        if (field == null) return true;
        var conditionalAttribute =
            (ConditionalFieldAttribute)Attribute.GetCustomAttribute(field, typeof(ConditionalFieldAttribute));

        if (conditionalAttribute == null) return true;
        var conditionField = targetObject.GetType()
            .GetField(conditionalAttribute.ConditionFieldName, conditionalFieldBindingFlags);

        if (conditionField == null) return true;
        var conditionValue = (bool)conditionField.GetValue(targetObject);

        return conditionValue;
    }

    public static int GetPreviewTileSize() => 64;
    
    public static string AddSpacesToCamelCase(string input)
    {
        return string.IsNullOrEmpty(input) ? input : Regex.Replace(input, "(?<!^)([A-Z0-9])", " $1");
    }

    public static StyleLength GemImGuiHeight() => 60;

    public static string GetDefaultIconContent() => "d_UnityEditor.ConsoleWindow";


    public static string GetObjectSelectorUpdateCommand() => "ObjectSelectorUpdated";
}