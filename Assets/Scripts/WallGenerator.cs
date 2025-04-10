using System.Collections.Generic;
using System.Linq;
using GeneralUtils;
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
    /// <param name="nonWallPositions"></param>
    public static void GenerateWalls(HashSet<Vector2Int> walkableTilesPositions, TilemapPainter tilemapPainter,
        HashSet<Vector2Int> nonWallPositions = null)
    {
        var wallPositionsByType = BuildInitialWallPositions(walkableTilesPositions);
        var initialWallCount = wallPositionsByType.Values.Sum(set => set.Count);
        Debug.Log("Cantidad inicial de paredes generadas: " + initialWallCount);

        var overrideCount = ApplyWallOverrides(wallPositionsByType, walkableTilesPositions);
        Debug.Log("Cantidad de overrides aplicados: " + overrideCount);

        foreach (var kvp in wallPositionsByType)
        {
            var filteredPositions = kvp.Value;
            if (nonWallPositions != null)
            {
                filteredPositions = new HashSet<Vector2Int>(kvp.Value.Except(nonWallPositions));
            }
            tilemapPainter.PaintWallTiles(filteredPositions, kvp.Key);
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
            { Utils.WallPosition.TripleExceptUp, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleExceptDown, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleExceptLeft, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleExceptRight, new HashSet<Vector2Int>() },
            { Utils.WallPosition.AllWallCorner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TopLeftInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TopRightInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.BottomLeftInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.BottomRightInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.Alone, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleExceptLeftInner, new HashSet<Vector2Int>() },
            { Utils.WallPosition.TripleExceptRightInner, new HashSet<Vector2Int>() }
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
    private static int ApplyWallOverrides(
        Dictionary<Utils.WallPosition, HashSet<Vector2Int>> wallPositionsByType,
        HashSet<Vector2Int> floorPositions)
{
    var allWallPositions = wallPositionsByType.Values.SelectMany(v => v).ToHashSet();

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
        new TopRightWallToTripleCornerExceptRightInner()
    };

    var changes = new List<(Vector2Int pos, Utils.WallPosition oldType, Utils.WallPosition newType)>();

    foreach (var wallType in wallPositionsByType.Keys.ToList())
    {
        foreach (var pos in wallPositionsByType[wallType])
        {
            foreach (var rule in overrideRules)
            {
                if (rule.IsMatch(pos, floorPositions, allWallPositions, wallType))
                {
                    changes.Add((pos, wallType, rule.OverrideWallPosition));
                    break; 
                }
            }
        }
    }

    foreach (var (pos, oldType, newType) in changes)
    {
        wallPositionsByType[oldType].Remove(pos);
        wallPositionsByType[newType].Add(pos);
    }
    
    return changes.Count;
}


    #endregion
}