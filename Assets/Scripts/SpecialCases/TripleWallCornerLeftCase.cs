using System.Collections.Generic;
using UnityEngine;

public class TripleWallCornerLeftCase : ISpecialWallCase
{
    public SpecialWallPosition WallPosition => SpecialWallPosition.TripleWallCornerLeft;

    public bool IsMatch(Vector2Int position, HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> wallPositions)
    {
        return floorPositions.Contains(position + Vector2Int.left) &&
               floorPositions.Contains(position + Vector2Int.down) &&
               wallPositions.Contains(position + Vector2Int.right) &&
               wallPositions.Contains(position + Vector2Int.up);
    }
}

public class DownWallCase : ISpecialWallCase
{
    public SpecialWallPosition WallPosition => SpecialWallPosition.DownWall;

    public bool IsMatch(Vector2Int position, HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> wallPositions)
    {
        return wallPositions.Contains(position + Vector2Int.down);
    }
}

/// <summary>
/// Caso: si un LeftWall tiene a su izquierda una wall, arriba floor, derecha floor y abajo wall
/// => se convierte en TopRight.
/// </summary>
public class LeftWallToTopRightCase : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TopRight;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.Left)
            return false;

        bool leftIsWall   = allWallPositions.Contains(position + Vector2Int.left);
        bool upIsFloor    = floorPositions.Contains(position + Vector2Int.up);
        bool rightIsFloor = floorPositions.Contains(position + Vector2Int.right);
        bool downIsWall   = allWallPositions.Contains(position + Vector2Int.down);

        return leftIsWall && upIsFloor && rightIsFloor && downIsWall;
    }
}

/// <summary>
/// Caso: si un RightWall tiene a su izquierda floor, arriba floor, derecha wall y abajo wall
/// => se convierte en TopLeft.
/// </summary>
public class RightWallToTopLeftCase : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TopLeft;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.Right)
            return false;

        bool leftIsFloor  = floorPositions.Contains(position + Vector2Int.left);
        bool upIsFloor    = floorPositions.Contains(position + Vector2Int.up);
        bool rightIsWall  = allWallPositions.Contains(position + Vector2Int.right);
        bool downIsWall   = allWallPositions.Contains(position + Vector2Int.down);

        return leftIsFloor && upIsFloor && rightIsWall && downIsWall;
    }
}

/// <summary>
/// Caso: si un LeftWall tiene a su izquierda wall, arriba wall, derecha floor y abajo floor
/// => se convierte en BottomRight.
/// </summary>
public class LeftWallToBottomRightCase : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.BottomRight;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.Left)
            return false;

        bool leftIsWall   = allWallPositions.Contains(position + Vector2Int.left);
        bool upIsWall     = allWallPositions.Contains(position + Vector2Int.up);
        bool rightIsFloor = floorPositions.Contains(position + Vector2Int.right);
        bool downIsFloor  = floorPositions.Contains(position + Vector2Int.down);

        return leftIsWall && upIsWall && rightIsFloor && downIsFloor;
    }
}

/// <summary>
/// Caso: si un RightWall tiene a su izquierda floor, arriba wall, derecha wall y abajo floor
/// => se convierte en BottomLeft.
/// </summary>
public class RightWallToBottomLeftCase : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.BottomLeft;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.Right)
            return false;

        bool leftIsFloor  = floorPositions.Contains(position + Vector2Int.left);
        bool upIsWall     = allWallPositions.Contains(position + Vector2Int.up);
        bool rightIsWall  = allWallPositions.Contains(position + Vector2Int.right);
        bool downIsFloor  = floorPositions.Contains(position + Vector2Int.down);

        return leftIsFloor && upIsWall && rightIsWall && downIsFloor;
    }
}
