using System.Collections.Generic;
using UnityEngine;

#region TopRight Wall Cases

public class TopRightWallToTripleCornerExceptUp : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TripleWallCornerExceptUp;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.TopRight)
            return false;

        var upIsFloor = floorPositions.Contains(position + Vector2Int.up);
        if (!upIsFloor)
        {
            var diagonalDownLeftIsFloor = floorPositions.Contains(position + Vector2Int.down + Vector2Int.left);
            var diagonalDownRightIsWall = allWallPositions.Contains(position + Vector2Int.down + Vector2Int.right);

            if (!(diagonalDownLeftIsFloor && !diagonalDownRightIsWall))
                return false;
        }

        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);
        var downIsWall = allWallPositions.Contains(position + Vector2Int.down);

        return leftIsWall && rightIsWall && downIsWall;
    }
}

#endregion

#region TopLeft Wall Cases

public class TopLeftWallToTripleCornerCase : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TripleWallCornerExceptUp;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.TopLeft)
            return false;

        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        var upIsFloor = floorPositions.Contains(position + Vector2Int.up);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);
        var downIsWall = allWallPositions.Contains(position + Vector2Int.down);

        return leftIsWall && upIsFloor && rightIsWall && downIsWall;
    }
}

#endregion

#region BottomRight Wall Cases

public class BottomRightWallToTripleWallCornerExceptRight : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TripleWallCornerExceptRight;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.BottomRight)
            return false;

        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);

        if (!rightIsWall)
        {
            var diagonalDownLeft = allWallPositions.Contains(position + Vector2Int.down + Vector2Int.left);
            var diagonalDownRight = allWallPositions.Contains(position + Vector2Int.down + Vector2Int.right);

            if (diagonalDownLeft || diagonalDownRight)
                return false;
        }

        var upIsWall = allWallPositions.Contains(position + Vector2Int.up);
        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        var downIsWall = allWallPositions.Contains(position + Vector2Int.down);


        return upIsWall && leftIsWall && downIsWall;
    }
}

public class BottomRightWallToTripleWallCornerExceptDown : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TripleWallCornerExceptDown;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.BottomRight)
            return false;

        var downIsFloor = floorPositions.Contains(position + Vector2Int.down);

        if (!downIsFloor) return false;

        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        var upIsWall = allWallPositions.Contains(position + Vector2Int.up);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);

        return leftIsWall && upIsWall && rightIsWall;
    }
}

public class BottomRightWallToAllWallCorner : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.AllWallCorner;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.BottomRight)
            return false;

        var upIsWall = allWallPositions.Contains(position + Vector2Int.up);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);
        var downIsWall = allWallPositions.Contains(position + Vector2Int.down);
        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);


        return upIsWall && rightIsWall && downIsWall && leftIsWall;
    }
}

#endregion

#region BottomLeft Wall Cases

public class BottomLeftWallToTripleWallCornerExceptDown : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TripleWallCornerExceptDown;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.BottomLeft)
            return false;

        var downIsFloor = floorPositions.Contains(position + Vector2Int.down);

        if (!downIsFloor)
        {
            var diagonalUpRightIsFloor = floorPositions.Contains(position + Vector2Int.up + Vector2Int.right);
            var diagonalUpLeftIsWall = allWallPositions.Contains(position + Vector2Int.up + Vector2Int.left);

            if (!(diagonalUpRightIsFloor && !diagonalUpLeftIsWall))
                return false;
        }

        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        var upIsWall = allWallPositions.Contains(position + Vector2Int.up);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);

        return leftIsWall && upIsWall && rightIsWall;
    }
}

public class BottomLeftWallToTripleWallCornerExceptLeft : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TripleWallCornerExceptLeft;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.BottomLeft)
            return false;

        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);

        if (!leftIsWall)
        {
            var diagonalDownLeft = allWallPositions.Contains(position + Vector2Int.down + Vector2Int.left);
            var diagonalDownRight = allWallPositions.Contains(position + Vector2Int.down + Vector2Int.right);

            if (diagonalDownLeft || diagonalDownRight)
                return false;
        }

        var upIsWall = allWallPositions.Contains(position + Vector2Int.up);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);
        var downIsWall = allWallPositions.Contains(position + Vector2Int.down);


        return upIsWall && rightIsWall && downIsWall;
    }
}

public class BottomLeftWallToAllWallCorner : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.AllWallCorner;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.BottomLeft)
            return false;

        var upIsWall = allWallPositions.Contains(position + Vector2Int.up);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);
        var downIsWall = allWallPositions.Contains(position + Vector2Int.down);
        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);

        return upIsWall && rightIsWall && downIsWall && leftIsWall;
    }
}

#endregion

#region Up Wall Cases

//None

#endregion

#region Right Wall Cases

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

        bool leftIsFloor = floorPositions.Contains(position + Vector2Int.left);
        bool upIsFloor = floorPositions.Contains(position + Vector2Int.up);
        bool rightIsWall = allWallPositions.Contains(position + Vector2Int.right);
        bool downIsWall = allWallPositions.Contains(position + Vector2Int.down);

        return leftIsFloor && upIsFloor && rightIsWall && downIsWall;
    }
}

public class RightWallToDownCase : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.Down;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.Right)
            return false;

        var leftIsFloor = floorPositions.Contains(position + Vector2Int.left);
        var upIsFloor = floorPositions.Contains(position + Vector2Int.up);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);
        var downIsFloor = floorPositions.Contains(position + Vector2Int.down);

        return leftIsFloor && upIsFloor && rightIsWall && downIsFloor;
    }
}

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

        bool leftIsFloor = floorPositions.Contains(position + Vector2Int.left);
        bool upIsWall = allWallPositions.Contains(position + Vector2Int.up);
        bool rightIsWall = allWallPositions.Contains(position + Vector2Int.right);
        bool downIsFloor = floorPositions.Contains(position + Vector2Int.down);

        return leftIsFloor && upIsWall && rightIsWall && downIsFloor;
    }
}

#endregion

#region Down Wall Cases

public class DownWallToTripleWallCornerExceptUp : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TripleWallCornerExceptUp;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.Down)
            return false;

        var diagonalDownLeft = allWallPositions.Contains(position + Vector2Int.down + Vector2Int.left);
        var diagonalDownRight = allWallPositions.Contains(position + Vector2Int.down + Vector2Int.right);

        if (diagonalDownLeft || diagonalDownRight)
            return false;

        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        var upIsFloor = floorPositions.Contains(position + Vector2Int.up);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);
        var downIsWall = allWallPositions.Contains(position + Vector2Int.down);

        return leftIsWall && upIsFloor && rightIsWall && downIsWall;
    }
}

public class DownWallToTripleWallCornerExceptDown : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.TripleWallCornerExceptDown;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.Down)
            return false;

        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        var upIsWall = allWallPositions.Contains(position + Vector2Int.up);
        var rightIsWall = allWallPositions.Contains(position + Vector2Int.right);

        return leftIsWall && upIsWall && rightIsWall;
    }
}

#endregion

#region Left Wall Cases

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

        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        var upIsFloor = floorPositions.Contains(position + Vector2Int.up);
        var rightIsFloor = floorPositions.Contains(position + Vector2Int.right);
        var downIsWall = allWallPositions.Contains(position + Vector2Int.down);

        return leftIsWall && upIsFloor && rightIsFloor && downIsWall;
    }
}

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

        bool leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        bool upIsWall = allWallPositions.Contains(position + Vector2Int.up);
        bool rightIsFloor = floorPositions.Contains(position + Vector2Int.right);
        bool downIsFloor = floorPositions.Contains(position + Vector2Int.down);

        return leftIsWall && upIsWall && rightIsFloor && downIsFloor;
    }
}

public class LeftWallToDownCase : IWallOverrideCase
{
    public WallPosition OverrideWallPosition => WallPosition.Down;

    public bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    )
    {
        if (originalWallPosition != WallPosition.Left)
            return false;

        var leftIsWall = allWallPositions.Contains(position + Vector2Int.left);
        var upIsFloor = floorPositions.Contains(position + Vector2Int.up);
        var rightIsFloor = floorPositions.Contains(position + Vector2Int.right);
        var downIsFloor = floorPositions.Contains(position + Vector2Int.down);

        return leftIsWall && upIsFloor && rightIsFloor && downIsFloor;
    }
}

#endregion