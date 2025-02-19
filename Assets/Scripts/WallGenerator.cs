using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum WallPosition
{
    Up,
    Down,
    Left,
    Right,
    TopLeft,
    BottomLeft,
    TopRight,
    BottomRight
}

/// <summary>
/// Class responsible for generating walls based on the positions of walkable tiles.
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
        var wallPositions = new Dictionary<WallPosition, HashSet<Vector2Int>>
        {
            { WallPosition.Up, GetUpWallsPositions(walkableTilesPositions) },
            { WallPosition.Down, GetDownWallsPositions(walkableTilesPositions) },
            { WallPosition.Left, GetLeftWallsPositions(walkableTilesPositions) },
            { WallPosition.Right, GetRightWallsPositions(walkableTilesPositions) },
            { WallPosition.TopLeft, GetTopLeftCornerPositions(walkableTilesPositions) },
            { WallPosition.BottomLeft, GetBottomLeftCornerPositions(walkableTilesPositions) },
            { WallPosition.TopRight, GetTopRightCornerPositions(walkableTilesPositions) },
            { WallPosition.BottomRight, GetBottomRightCornerPositions(walkableTilesPositions) }
        };

        var specialWallPositions =
            GetSpecialWallPositions(walkableTilesPositions, wallPositions.Values.SelectMany(x => x).ToHashSet());

        foreach (var wallPosition in wallPositions)
        {
            tilemapPainter.PaintWallTiles(wallPosition.Value, wallPosition.Key);
        }

        tilemapPainter.PaintWallTiles(specialWallPositions, WallPosition.BottomLeft); // Ajusta según el caso especial
    }

    private static HashSet<Vector2Int> GetWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        var neighborPositions = new HashSet<Vector2Int>();

        foreach (var neighborPos in from position in floorPositions
                 from direction in Utils.Directions
                 select position + direction)
        {
            neighborPositions.Add(neighborPos);
        }

        neighborPositions.ExceptWith(floorPositions);
        return neighborPositions;
    }

    public static HashSet<Vector2Int> GetUpWallsPositions(HashSet<Vector2Int> floorPositions) =>
        GetSpecificWallPositions(floorPositions, Vector2Int.up);

    public static HashSet<Vector2Int> GetDownWallsPositions(HashSet<Vector2Int> floorPositions) =>
        GetSpecificWallPositions(floorPositions, Vector2Int.down);

    public static HashSet<Vector2Int> GetLeftWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        var leftWallPositions = GetSpecificWallPositions(floorPositions, Vector2Int.left);
        // caso especial: Si una leftwall tiene una pared a su izquierda, debe ser downwall.
        var downWallPositions = new HashSet<Vector2Int>();

        foreach (var position in leftWallPositions)
        {
            var leftPos = position + Vector2Int.left;
            if (leftWallPositions.Contains(leftPos))
            {
                downWallPositions.Add(position);
            }
        }

        leftWallPositions.ExceptWith(downWallPositions);
        return leftWallPositions;
    }

    public static HashSet<Vector2Int> GetRightWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        var rightWallPositions = GetSpecificWallPositions(floorPositions, Vector2Int.right);
        // caso especial: Si una rightwall tiene una pared a su derecha, debe ser downwall.
        var downWallPositions = new HashSet<Vector2Int>();

        foreach (var position in rightWallPositions)
        {
            var rightPos = position + Vector2Int.right;
            if (rightWallPositions.Contains(rightPos))
            {
                downWallPositions.Add(position);
            }
        }

        rightWallPositions.ExceptWith(downWallPositions);
        return rightWallPositions;
    }

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

    public static HashSet<Vector2Int> GetTopLeftCornerPositions(HashSet<Vector2Int> floorPositions) =>
        GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.left);

    public static HashSet<Vector2Int> GetBottomLeftCornerPositions(HashSet<Vector2Int> floorPositions) =>
        GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.left);

    public static HashSet<Vector2Int> GetTopRightCornerPositions(HashSet<Vector2Int> floorPositions) =>
        GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.right);

    public static HashSet<Vector2Int> GetBottomRightCornerPositions(HashSet<Vector2Int> floorPositions) =>
        GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.right);

    private static HashSet<Vector2Int> GetCornerPositions(HashSet<Vector2Int> floorPositions, Vector2Int direction1,
        Vector2Int direction2)
    {
        var cornerPositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            var neighborPos1 = position + direction1;
            var neighborPos2 = position + direction2;
            var cornerPos = position + direction1 + direction2;

            if (!floorPositions.Contains(neighborPos1) && !floorPositions.Contains(neighborPos2) &&
                !floorPositions.Contains(cornerPos))
            {
                cornerPositions.Add(cornerPos);
            }
        }

        return cornerPositions;
    }

    public static HashSet<Vector2Int> GetSpecialWallPositions(HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> wallPositions)
    {
        var specialWallPositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            var leftPos = position + Vector2Int.left;
            var downPos = position + Vector2Int.down;
            var rightPos = position + Vector2Int.right;
            var upPos = position + Vector2Int.up;

            if (floorPositions.Contains(leftPos) && floorPositions.Contains(downPos) &&
                wallPositions.Contains(rightPos) && wallPositions.Contains(upPos))
            {
                specialWallPositions.Add(position);
            }

            // Caso especial: triplewallcornerleft:  lo que ahora se pone corner bottom left si tiene una wall debajo pyes tiene que ser triplewallcornerleft
            if (wallPositions.Contains(downPos))
            {
                specialWallPositions.Add(position);
            }
        }

        return specialWallPositions;
    }
}