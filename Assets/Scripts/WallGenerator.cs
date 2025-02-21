using System.Collections.Generic;
using System.Linq;
using SpecialCases;
using UnityEngine;

/// <summary>
/// Class responsible for generating walls based on the positions of walkable tiles.
/// </summary>
public class WallGenerator : MonoBehaviour
{
    /// <summary>
    /// Generates walls based on the positions of walkable tiles.
    /// </summary>
    /// <param name="walkableTilesPositions">A set of positions of walkable tiles.</param>
    /// <param name="tilemapPainter">The TilemapPainter used to paint the walls.</param>
    public static void GenerateWalls(HashSet<Vector2Int> walkableTilesPositions, TilemapPainter tilemapPainter)
    {
        // 1. Build the initial dictionary of wall positions
        var wallPositionsByType = BuildInitialWallPositions(walkableTilesPositions);

        // 2. Apply override rules to adjust the classification of walls
        ApplyWallOverrides(wallPositionsByType, walkableTilesPositions);

        // 3. Paint the walls (iterate through each type and paint the corresponding positions)
        foreach (var kvp in wallPositionsByType)
        {
            tilemapPainter.PaintWallTiles(kvp.Value, kvp.Key);
        }
    }

    #region Wall Positions Construction

    /// <summary>
    /// Builds the initial dictionary of wall positions based on directions and corners.
    /// </summary>
    private static Dictionary<Utils.WallPosition, HashSet<Vector2Int>> BuildInitialWallPositions(
        HashSet<Vector2Int> floorPositions)
    {
        var wallPositions = new Dictionary<Utils.WallPosition, HashSet<Vector2Int>>
        {
            { Utils.WallPosition.Up, GetSpecificWallPositions(floorPositions, Vector2Int.up) },
            { Utils.WallPosition.Down, GetSpecificWallPositions(floorPositions, Vector2Int.down) },
            { Utils.WallPosition.Left, GetSpecificWallPositions(floorPositions, Vector2Int.left) },
            { Utils.WallPosition.Right, GetSpecificWallPositions(floorPositions, Vector2Int.right) },
            { Utils.WallPosition.TopLeft, GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.left) },
            { Utils.WallPosition.BottomLeft, GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.left) },
            { Utils.WallPosition.TopRight, GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.right) },
            { Utils.WallPosition.BottomRight, GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.right) },
            // Initially empty: will be filled by overrides
            { Utils.WallPosition.TripleWallCornerExceptUp, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleWallCornerExceptDown, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleWallCornerExceptLeft, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleWallCornerExceptRight, new HashSet<Vector2Int>() },
            { Utils.WallPosition.AllWallCorner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleWallCornerLeft, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TopLeftInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TopRightInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.BottomLeftInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.BottomRightInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.Alone, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleWallCornerExceptLeftInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleWallCornerExceptRightInner, new HashSet<Vector2Int>() }
        };

        return wallPositions;
    }

    /// <summary>
    /// Gets the wall positions in a given direction relative to the walkable tiles.
    /// </summary>
    private static HashSet<Vector2Int> GetSpecificWallPositions(HashSet<Vector2Int> floorPositions,
        Vector2Int direction)
    {
        var wallPositions = new HashSet<Vector2Int>();
        foreach (var pos in floorPositions)
        {
            var neighbor = pos + direction;
            if (!floorPositions.Contains(neighbor))
            {
                wallPositions.Add(neighbor);
            }
        }

        return wallPositions;
    }

    /// <summary>
    /// Gets the corner wall positions by combining two directions.
    /// </summary>
    private static HashSet<Vector2Int> GetCornerPositions(HashSet<Vector2Int> floorPositions, Vector2Int direction1,
        Vector2Int direction2)
    {
        var cornerPositions = new HashSet<Vector2Int>();

        foreach (var pos in floorPositions)
        {
            var neighbor1 = pos + direction1;
            var neighbor2 = pos + direction2;
            var corner = pos + direction1 + direction2;

            if (!floorPositions.Contains(neighbor1) &&
                !floorPositions.Contains(neighbor2) &&
                !floorPositions.Contains(corner))
            {
                cornerPositions.Add(corner);
            }
        }

        return cornerPositions;
    }

    #endregion

    #region Wall Overrides Application

    /// <summary>
    /// Applies override rules to adjust the classification of wall positions.
    /// </summary>
    private static void ApplyWallOverrides(
        Dictionary<Utils.WallPosition, HashSet<Vector2Int>> wallPositionsByType,
        HashSet<Vector2Int> floorPositions)
    {
        // Combine all wall positions into a single set for easier checks.
        var allWallPositions = wallPositionsByType.Values.SelectMany(v => v).ToHashSet();

        // List of defined override rules (these classes implement IWallOverrideCase).
        var overrideRules = new List<IWallOverrideCase>
        {
            new DownWallToUpCase(),
            new LeftWallToTopRightInnerCase(),
            new RightWallToTopLeftInnerCase(),
            new LeftWallToBottomRightInnerCase(),
            new RightWallToBottomLeftInnerCase(),
            new RightWallToDownCase(),
            new LeftWallToDownCase(),
            new TopRightWallToTripleCornerExceptUp(),
            new TopLeftWallToTripleCornerExceptUpCase(),
            new DownWallToTripleWallCornerExceptUp(),
            new DownWallToTripleWallCornerExceptDown(),
            new BottomRightWallToTripleWallCornerExceptDown(),
            new BottomLeftWallToTripleWallCornerExceptDown(),
            new BottomLeftWallToTripleWallCornerExceptLeft(),
            new BottomRightWallToTripleWallCornerExceptRight(),
            new BottomLeftWallToAllWallCorner(),
            new BottomRightWallToAllWallCorner(),
            new RightWallAloneToDownCase(),
            new LeftWallAloneToDownCase(),
            new RightWallToAloneCase(),
            new TopLeftWallToAllWallCornerCase(),
            new TopRightWallToAllWallCornerCase(),
            new RightWallToTripleWallCornerExceptLeftInnerCase(),
            new RightWallToTripleWallCornerExceptRightInnerCase(),
            new TopLeftWallAllCornerCase(),
        };

        // Store changes without modifying sets while iterating.
        var changes = new List<(Vector2Int pos, Utils.WallPosition oldType, Utils.WallPosition newType)>();

        // Iterate through each wall type and its positions
        foreach (var wallType in wallPositionsByType.Keys.ToList())
        {
            foreach (var pos in wallPositionsByType[wallType])
            {
                // Evaluate override rules until one applies.
                foreach (var rule in overrideRules)
                {
                    if (rule.IsMatch(pos, floorPositions, allWallPositions, wallType))
                    {
                        changes.Add((pos, wallType, rule.OverrideWallPosition));
                        break; // Exit after applying the first matching rule
                    }
                }
            }
        }

        // Apply changes: remove the position from the original type and add it to the new type.
        foreach (var (pos, oldType, newType) in changes)
        {
            wallPositionsByType[oldType].Remove(pos);
            wallPositionsByType[newType].Add(pos);
        }
    }

    #endregion
}