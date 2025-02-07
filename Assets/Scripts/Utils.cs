using UnityEngine;

/// <summary>
/// Utility class providing helper methods.
/// </summary>
public static class Utils
{
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
        return direction switch {
            0 => Vector2Int.up,
            1 => Vector2Int.right,
            2 => Vector2Int.down,
            _ => Vector2Int.left,
        };
    }

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
}