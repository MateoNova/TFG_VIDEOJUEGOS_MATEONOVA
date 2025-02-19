using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
///  Class responsible for generating walls based on the positions of walkable tiles.
/// </summary>
public class WallGenerator : MonoBehaviour
{
    /// <summary>
    /// Generates walls based on the positions of walkable tiles.
    /// </summary>
    /// <param name="walkableTilesPositions">A set of positions of walkable tiles.</param>
    /// <param name="tilemapPainter">The TilemapRenderer used to render the wall tiles.</param>
    public static void GenerateWalls(HashSet<Vector2Int> walkableTilesPositions, TilemapPainter tilemapPainter)
    {
        var upWallPositions = GetUpWallsPositions(walkableTilesPositions);
        var downWallPositions = GetDownWallsPositions(walkableTilesPositions);
        var leftWallPositions = GetLeftWallsPositions(walkableTilesPositions);
        var rightWallPositions = GetRightWallsPositions(walkableTilesPositions);

        foreach (var position in upWallPositions)
        {
            tilemapPainter.PaintWallTiles(new[] { position }, "up");
        }

        foreach (var position in downWallPositions)
        {
            tilemapPainter.PaintWallTiles(new[] { position }, "down");
        }

        foreach (var position in leftWallPositions)
        {
            tilemapPainter.PaintWallTiles(new[] { position }, "left");
        }

        foreach (var position in rightWallPositions)
        {
            tilemapPainter.PaintWallTiles(new[] { position }, "right");
        }
    }

    /// <summary>
    /// Gets the positions of walls based on the positions of floor tiles.
    /// </summary>
    /// <param name="floorPositions">A set of positions of floor tiles.</param>
    /// <returns>A set of positions of wall tiles.</returns>
    private static HashSet<Vector2Int> GetWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        var neighborPositions = new HashSet<Vector2Int>();

        // Iterate through each floor position and its neighboring positions
        foreach (var neighborPos in from position in floorPositions
                 from direction in Utils.Directions
                 select position + direction)
        {
            neighborPositions.Add(neighborPos);
        }

        // Remove positions that are already occupied by floor tiles
        neighborPositions.ExceptWith(floorPositions);
        return neighborPositions;
    }

    /// <summary>
    /// Gets the positions of up walls based on the positions of floor tiles.
    /// </summary>
    /// <param name="floorPositions">A set of positions of floor tiles.</param>
    /// <returns>A set of positions of up walls.</returns>
    public static HashSet<Vector2Int> GetUpWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        return GetSpecificWallPositions(floorPositions, Vector2Int.up);
    }

    /// <summary>
    /// Gets the positions of down walls based on the positions of floor tiles.
    /// </summary>
    /// <param name="floorPositions">A set of positions of floor tiles.</param>
    /// <returns>A set of positions of down walls.</returns>
    public static HashSet<Vector2Int> GetDownWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        return GetSpecificWallPositions(floorPositions, Vector2Int.down);
    }

    /// <summary>
    /// Gets the positions of left walls based on the positions of floor tiles.
    /// </summary>
    /// <param name="floorPositions">A set of positions of floor tiles.</param>
    /// <returns>A set of positions of left walls.</returns>
    public static HashSet<Vector2Int> GetLeftWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        return GetSpecificWallPositions(floorPositions, Vector2Int.left);
    }

    /// <summary>
    /// Gets the positions of right walls based on the positions of floor tiles.
    /// </summary>
    /// <param name="floorPositions">A set of positions of floor tiles.</param>
    /// <returns>A set of positions of right walls.</returns>
    public static HashSet<Vector2Int> GetRightWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        return GetSpecificWallPositions(floorPositions, Vector2Int.right);
    }

    /// <summary>
    /// Gets the positions of walls in a specific direction based on the positions of floor tiles.
    /// </summary>
    /// <param name="floorPositions">A set of positions of floor tiles.</param>
    /// <param name="direction">The direction to check for walls.</param>
    /// <returns>A set of positions of walls in the specified direction.</returns>
    private static HashSet<Vector2Int> GetSpecificWallPositions(HashSet<Vector2Int> floorPositions,
        Vector2Int direction)
    {
        var wallPositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            var neighborPos = position + direction;
            if (!floorPositions.Contains(neighborPos))
            {
                wallPositions.Add(neighborPos);
            }
        }

        return wallPositions;
    }
}