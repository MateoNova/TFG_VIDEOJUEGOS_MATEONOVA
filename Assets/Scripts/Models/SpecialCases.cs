using System.Collections.Generic;
using Generators.Controllers;
using UnityEngine;

namespace Generators.Models
{
    #region Interface and Base Classes

    /// <summary>
    /// Base class to centralize the validation of the original wall position.
    /// </summary>
    public abstract class BaseWallOverrideCase : IWallOverrideCase
    {
        /// <summary>
        /// The expected original wall type for this override to apply.
        /// </summary>
        protected abstract Utils.Utils.WallPosition ExpectedOriginalWall { get; }

        /// <summary>
        /// The new wall position to assign if the condition is met.
        /// </summary>
        public abstract Utils.Utils.WallPosition OverrideWallPosition { get; }

        /// <summary>
        /// Method to check the specific conditions for the override.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="floorPositions">The set of floor positions.</param>
        /// <param name="allWallPositions">The set of all wall positions.</param>
        /// <returns>True if the conditions are met, otherwise false.</returns>
        protected abstract bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions);

        public bool IsMatch(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions,
            Utils.Utils.WallPosition originalWallPosition
        )
        {
            if (originalWallPosition != ExpectedOriginalWall)
                return false;

            return Matches(position, floorPositions, allWallPositions);
        }
    }

    /// <summary>
    /// Helper class for tile-related methods to improve readability.
    /// </summary>
    public static class TileHelper
    {
        public static bool IsWall(this HashSet<Vector2Int> wallPositions, Vector2Int pos)
            => wallPositions.Contains(pos);

        public static bool IsFloor(this HashSet<Vector2Int> floorPositions, Vector2Int pos)
            => floorPositions.Contains(pos);

        public static bool AreWalls(Vector2Int position, HashSet<Vector2Int> wallPositions, params Vector2Int[] offsets)
        {
            foreach (var offset in offsets)
            {
                if (!wallPositions.Contains(position + offset))
                    return false;
            }

            return true;
        }

        public static bool AreFloors(Vector2Int position, HashSet<Vector2Int> floorPositions,
            params Vector2Int[] offsets)
        {
            foreach (var offset in offsets)
            {
                if (!floorPositions.Contains(position + offset))
                    return false;
            }

            return true;
        }
    }

    #endregion

    #region TopRight Wall Cases

    /// <summary>
    /// Override case for converting a TopRight wall to a TripleWallCornerExceptUp.
    /// </summary>
    public class TopRightWallToTripleCornerExceptUp : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.TopRight;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptUp;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            if (!floorPositions.IsFloor(position + Vector2Int.up) || allWallPositions.IsWall(position + Vector2Int.up))
            {
                var diagonalDownLeftIsFloor = floorPositions.IsFloor(position + Vector2Int.down + Vector2Int.left);
                var diagonalDownRightIsWall = allWallPositions.IsWall(position + Vector2Int.down + Vector2Int.right);
                if (diagonalDownLeftIsFloor && diagonalDownRightIsWall)
                    return false;
            }

            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.left, Vector2Int.right, Vector2Int.down);
        }
    }

    public class TopRightWallToTripleCornerExceptRightInner : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.TopRight;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptRightInner;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.left, Vector2Int.down, Vector2Int.up) &&
                   floorPositions.IsFloor(position + Vector2Int.right);
        }
    }

    public class TopRightWallToAllWallCornerCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.TopRight;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.AllWallCorner;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            var allDiagonalsAreFloor = TileHelper.AreFloors(position, floorPositions, Vector2Int.up + Vector2Int.left,
                Vector2Int.up + Vector2Int.right, Vector2Int.down + Vector2Int.left,
                Vector2Int.down + Vector2Int.right);
            if (!allDiagonalsAreFloor)
                return false;

            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.left, Vector2Int.up, Vector2Int.right,
                Vector2Int.down);
        }
    }

    #endregion

    #region TopLeft Wall Cases

    /// <summary>
    /// Override case for converting a TopLeft wall to a TripleWallCornerExceptUp.
    /// </summary>
    public class TopLeftWallToTripleCornerExceptUpCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.TopLeft;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptUp;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            var leftdowndiagonalisfloor = floorPositions.IsFloor(position + Vector2Int.left + Vector2Int.down);
            var leftupdiagonalisfloor = floorPositions.IsFloor(position + Vector2Int.left + Vector2Int.up);
            if (!leftdowndiagonalisfloor && !leftupdiagonalisfloor)
                return false;


            return allWallPositions.IsWall(position + Vector2Int.left) &&
                   floorPositions.IsFloor(position + Vector2Int.up) &&
                   allWallPositions.IsWall(position + Vector2Int.right) &&
                   allWallPositions.IsWall(position + Vector2Int.down);
        }
    }

    public class TopLeftWallAllCornerCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.TopLeft;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.AllWallCorner;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            var allDiagonalsAreFloor = TileHelper.AreFloors(position, floorPositions, Vector2Int.up + Vector2Int.left,
                Vector2Int.up + Vector2Int.right, Vector2Int.down + Vector2Int.left,
                Vector2Int.down + Vector2Int.right);
            if (!allDiagonalsAreFloor)
                return false;

            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.left, Vector2Int.up, Vector2Int.right,
                Vector2Int.down);
        }
    }

    public class TopLeftWallToAllWallCornerCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.TopLeft;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.AllWallCorner;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            var leftdownIsWall = allWallPositions.IsWall(position + Vector2Int.left + Vector2Int.down);
            var rightdownIsWall = allWallPositions.IsWall(position + Vector2Int.right + Vector2Int.down);
            var leftupIsWall = allWallPositions.IsWall(position + Vector2Int.left + Vector2Int.up);
            var rightupIsWall = allWallPositions.IsWall(position + Vector2Int.right + Vector2Int.up);
            if (leftdownIsWall || rightdownIsWall || leftupIsWall || rightupIsWall)
                return false;

            return allWallPositions.IsWall(position + Vector2Int.left) &&
                   allWallPositions.IsWall(position + Vector2Int.up) &&
                   allWallPositions.IsWall(position + Vector2Int.right) &&
                   allWallPositions.IsWall(position + Vector2Int.down);
        }
    }

    #endregion

    #region BottomRight Wall Cases

    /// <summary>
    /// Override case for converting a BottomRight wall to a TripleWallCornerExceptRight.
    /// </summary>
    public class BottomRightWallToTripleWallCornerExceptRight : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.BottomRight;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptRight;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            if (!floorPositions.IsFloor(position + Vector2Int.right))
                return false;

            var leftupdiagonalisfloor = floorPositions.IsFloor(position + Vector2Int.left + Vector2Int.up);
            var leftdowndiagonalisFloor = floorPositions.IsFloor(position + Vector2Int.left + Vector2Int.down);

            if (!(leftupdiagonalisfloor || leftdowndiagonalisFloor)) return false;

            var rightIsWall = allWallPositions.IsWall(position + Vector2Int.right);

            if (!rightIsWall)
            {
                var diagonalDownLeftisFloor = floorPositions.IsFloor(position + Vector2Int.down + Vector2Int.left);
                var diagonalDownRightisFloor = floorPositions.IsFloor(position + Vector2Int.down + Vector2Int.right);
                var diagonalDownLeftisWall = allWallPositions.IsWall(position + Vector2Int.down + Vector2Int.left);
                var diagonalDownRightisWall = allWallPositions.IsWall(position + Vector2Int.down + Vector2Int.right);
                if (!(diagonalDownLeftisFloor || diagonalDownRightisFloor ||
                      (!diagonalDownLeftisWall && diagonalDownRightisWall)))
                    return false;
            }

            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.left, Vector2Int.up, Vector2Int.down);
        }
    }

    /// <summary>
    /// Override case for converting a BottomRight wall to a TripleWallCornerExceptDown.
    /// </summary>
    public class BottomRightWallToTripleWallCornerExceptDown : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.BottomRight;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptDown;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            if (allWallPositions.IsWall(position + Vector2Int.down))
                return false;

            if (!floorPositions.IsFloor(position + Vector2Int.down))
                return false;

            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.left, Vector2Int.up, Vector2Int.right);
        }
    }

    /// <summary>
    /// Override case for converting a BottomRight wall to an AllWallCorner.
    /// </summary>
    public class BottomRightWallToAllWallCorner : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.BottomRight;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.AllWallCorner;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            var leftUpIsFloor = floorPositions.IsFloor(position + Vector2Int.left + Vector2Int.up);
            var leftDownIsFloor = floorPositions.IsFloor(position + Vector2Int.left + Vector2Int.down);
            var rightUpIsFloor = floorPositions.IsFloor(position + Vector2Int.right + Vector2Int.up);
            var rightDownIsFloor = floorPositions.IsFloor(position + Vector2Int.right + Vector2Int.down);

            if (!(leftUpIsFloor && leftDownIsFloor && rightUpIsFloor && rightDownIsFloor))
                return false;

            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.up, Vector2Int.right, Vector2Int.down,
                Vector2Int.left);
        }
    }

    #endregion

    #region BottomLeft Wall Cases

    /// <summary>
    /// Override case for converting a BottomLeft wall to a TripleWallCornerExceptDown.
    /// </summary>
    public class BottomLeftWallToTripleWallCornerExceptDown : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.BottomLeft;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptDown;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            var leftupdiagonalisfloor = floorPositions.IsFloor(position + Vector2Int.left + Vector2Int.up);
            var rightupdiagonalisfloor = floorPositions.IsFloor(position + Vector2Int.right + Vector2Int.up);
            if (!(leftupdiagonalisfloor || rightupdiagonalisfloor))
                return false;

            if (!floorPositions.IsFloor(position + Vector2Int.down))
            {
                var diagonalUpRightIsFloor = floorPositions.IsFloor(position + Vector2Int.up + Vector2Int.right);
                var diagonalUpLeftIsWall = allWallPositions.IsWall(position + Vector2Int.up + Vector2Int.left);
                if (!(diagonalUpRightIsFloor && !diagonalUpLeftIsWall))
                    return false;
            }

            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.left, Vector2Int.up, Vector2Int.right);
        }
    }

    /// <summary>
    /// Override case for converting a BottomLeft wall to a TripleWallCornerExceptLeft.
    /// </summary>
    public class BottomLeftWallToTripleWallCornerExceptLeft : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.BottomLeft;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptLeft;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            var downrightdiagonalisfloor = floorPositions.IsFloor(position + Vector2Int.down + Vector2Int.right);
            var downLeftdiagonalisfloor = floorPositions.IsFloor(position + Vector2Int.down + Vector2Int.left);
            var uprightdiagonalisfloor = floorPositions.IsFloor(position + Vector2Int.up + Vector2Int.right);
            var leftisfloor = floorPositions.IsFloor(position + Vector2Int.left);
            var downIsFloot = floorPositions.IsFloor(position + Vector2Int.down);

            if (!downrightdiagonalisfloor && !downLeftdiagonalisfloor && !leftisfloor && !downIsFloot)
                return false;

            if (!downrightdiagonalisfloor && !uprightdiagonalisfloor)
                return false;

            if (!allWallPositions.IsWall(position + Vector2Int.left))
            {
                var diagonalDownLeft = allWallPositions.IsWall(position + Vector2Int.down + Vector2Int.left);
                var diagonalDownRight = allWallPositions.IsWall(position + Vector2Int.down + Vector2Int.right);
                if ((diagonalDownLeft || diagonalDownRight) && !leftisfloor)
                    return false;
            }

            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.up, Vector2Int.right, Vector2Int.down);
        }
    }

    /// <summary>
    /// Override case for converting a BottomLeft wall to an AllWallCorner.
    /// </summary>
    public class BottomLeftWallToAllWallCorner : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.BottomLeft;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.AllWallCorner;

        protected override bool Matches(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions
        )
        {
            var alldiagonalsarefloor = TileHelper.AreFloors(position, floorPositions, Vector2Int.up + Vector2Int.left,
                Vector2Int.up + Vector2Int.right, Vector2Int.down + Vector2Int.left,
                Vector2Int.down + Vector2Int.right);
            if (!alldiagonalsarefloor)
                return false;

            return TileHelper.AreWalls(position, allWallPositions, Vector2Int.up, Vector2Int.right, Vector2Int.down,
                Vector2Int.left);
        }
    }

    #endregion

    #region Up Wall Cases

// None

    #endregion

    #region Right Wall Cases

    /// <summary>
    /// Override case for converting a Right wall to a TopLeft wall.
    /// </summary>
    public class RightWallToTopLeftInnerCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Right;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TopLeftInner;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return floorPositions.IsFloor(position + Vector2Int.left) &&
                   floorPositions.IsFloor(position + Vector2Int.up) &&
                   allWallPositions.IsWall(position + Vector2Int.right) &&
                   allWallPositions.IsWall(position + Vector2Int.down);
        }
    }

    /// <summary>
    /// Override case for converting a Right wall to a Down wall.
    /// </summary>
    public class RightWallToDownCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Right;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.Down;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return floorPositions.IsFloor(position + Vector2Int.left) &&
                   floorPositions.IsFloor(position + Vector2Int.up) &&
                   allWallPositions.IsWall(position + Vector2Int.right) &&
                   floorPositions.IsFloor(position + Vector2Int.down);
        }
    }

    /// <summary>
    /// Override case for converting a Right wall to a BottomLeft wall.
    /// </summary>
    public class RightWallToBottomLeftInnerCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Right;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.BottomLeftInner;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return floorPositions.IsFloor(position + Vector2Int.left) &&
                   allWallPositions.IsWall(position + Vector2Int.up) &&
                   allWallPositions.IsWall(position + Vector2Int.right) &&
                   floorPositions.IsFloor(position + Vector2Int.down);
        }
    }

    /// <summary>
    /// Override case for converting a Right wall to a Down wall.
    /// </summary>
    public class RightWallAloneToDownCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Right;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.Down;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return floorPositions.IsFloor(position + Vector2Int.left) &&
                   floorPositions.IsFloor(position + Vector2Int.up) &&
                   floorPositions.IsFloor(position + Vector2Int.right) &&
                   floorPositions.IsFloor(position + Vector2Int.down);
        }
    }

    /// <summary>
    /// Override case for converting a Right wall to an Alone wall.
    /// </summary>
    public class RightWallToAloneCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Right;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.Alone;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return floorPositions.IsFloor(position + Vector2Int.left) &&
                   floorPositions.IsFloor(position + Vector2Int.right);
        }
    }

    public class RightWallToTripleWallCornerExceptLeftInnerCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Right;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptLeftInner;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            if (!floorPositions.IsFloor(position + Vector2Int.right + Vector2Int.down))
                return false;

            return allWallPositions.IsWall(position + Vector2Int.right);
        }
    }

    public class RightWallToTripleWallCornerExceptRightInnerCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Right;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptRightInner;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return allWallPositions.IsWall(position + Vector2Int.left);
        }
    }

    #endregion

    #region Down Wall Cases

    /// <summary>
    /// Override case for converting a Down wall to a Up wall.
    /// </summary>
    public class DownWallToUpCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Down;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.Up;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return floorPositions.IsFloor(position + Vector2Int.down);
        }
    }


    /// <summary>
    /// Override case for converting a Down wall to a TripleWallCornerExceptUp.
    /// </summary>
    public class DownWallToTripleWallCornerExceptUp : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Down;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptUp;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            if (allWallPositions.IsWall(position + Vector2Int.down + Vector2Int.left) ||
                allWallPositions.IsWall(position + Vector2Int.down + Vector2Int.right))
                return false;

            return allWallPositions.IsWall(position + Vector2Int.left) &&
                   floorPositions.IsFloor(position + Vector2Int.up) &&
                   allWallPositions.IsWall(position + Vector2Int.right) &&
                   allWallPositions.IsWall(position + Vector2Int.down);
        }
    }

    /// <summary>
    /// Override case for converting a Down wall to a TripleWallCornerExceptDown.
    /// </summary>
    public class DownWallToTripleWallCornerExceptDown : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Down;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TripleExceptDown;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return allWallPositions.IsWall(position + Vector2Int.left) &&
                   allWallPositions.IsWall(position + Vector2Int.up) &&
                   allWallPositions.IsWall(position + Vector2Int.right);
        }
    }

    #endregion

    #region Left Wall Cases

    /// <summary>
    /// Override case for converting a Left wall to a TopRight wall.
    /// </summary>
    public class LeftWallToTopRightInnerCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Left;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.TopRightInner;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return allWallPositions.IsWall(position + Vector2Int.left) &&
                   floorPositions.IsFloor(position + Vector2Int.up) &&
                   floorPositions.IsFloor(position + Vector2Int.right) &&
                   allWallPositions.IsWall(position + Vector2Int.down);
        }
    }

    /// <summary>
    /// Override case for converting a Left wall to a BottomRight wall.
    /// </summary>
    public class LeftWallToBottomRightInnerCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Left;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.BottomRightInner;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return allWallPositions.IsWall(position + Vector2Int.left) &&
                   allWallPositions.IsWall(position + Vector2Int.up) &&
                   floorPositions.IsFloor(position + Vector2Int.right) &&
                   floorPositions.IsFloor(position + Vector2Int.down);
        }
    }

    /// <summary>
    /// Override case for converting a Left wall to a Down wall.
    /// </summary>
    public class LeftWallToDownCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Left;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.Down;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return allWallPositions.IsWall(position + Vector2Int.left) &&
                   floorPositions.IsFloor(position + Vector2Int.up) &&
                   floorPositions.IsFloor(position + Vector2Int.right) &&
                   floorPositions.IsFloor(position + Vector2Int.down);
        }
    }

    public class LeftWallAloneToDownCase : BaseWallOverrideCase
    {
        protected override Utils.Utils.WallPosition ExpectedOriginalWall => Utils.Utils.WallPosition.Left;
        public override Utils.Utils.WallPosition OverrideWallPosition => Utils.Utils.WallPosition.Down;

        protected override bool Matches(Vector2Int position, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions)
        {
            return floorPositions.IsFloor(position + Vector2Int.left) &&
                   floorPositions.IsFloor(position + Vector2Int.up) &&
                   floorPositions.IsFloor(position + Vector2Int.right) &&
                   floorPositions.IsFloor(position + Vector2Int.down);
        }
    }

    #endregion
}