using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
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
        
        public static readonly List<string> PredefinedTileNames = new()
        {
            "TopLeftWall", "TripleExceptUpWall", "TopRightWall", "TopLeftInnerWall", "TopRightInnerWall",
            "RightWall", "UpWall", "TripleExceptLeftWall", "AllCornersWall", "TripleExceptRightWall",
            "TripleExceptLeftInnerWall", "TripleExceptRightInnerWall", "LeftWall", "BottomLeftWall",
            "TripleExcetDownWall", "BottomRightWall", "BottomLeftInnerWall", "BottomRightInnerWall",
            "AloneWall", "DownWall", "DoorClosed", "DoorOpen"
        };


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

        public static int GetPreviewTileSize() => 64;

        public static string AddSpacesToCamelCase(string input)
        {
            return string.IsNullOrEmpty(input) ? input : Regex.Replace(input, "(?<!^)([A-Z0-9])", " $1");
        }

        public static string GetDefaultIconContent() => "d_UnityEditor.ConsoleWindow";


        public static string GetObjectSelectorUpdateCommand() => "ObjectSelectorUpdated";

        public static bool[] GetNeighborBits(int mask)
        {
            // order: N, NE, E, SE, S, SW, W, NW
            var bits = new bool[8];
            for (var i = 0; i < 8; i++)
                bits[i] = (mask & (1 << i)) != 0;
            return bits;
        }

         public static WallPosition DetermineWallPosition(bool[] n)
        {
            // shorthand for readability
            bool N = n[0], NE = n[1], E = n[2], SE = n[3];
            bool S = n[4], SW = n[5], W = n[6], NW = n[7];

            // 1) Inner corners
            if (N && NE && E && !W) return WallPosition.TopRightInner;
            if (N && !E && (SW || NW) && W) return WallPosition.TopLeftInner;
            if (!N && E && SE && S && !W) return WallPosition.BottomRightInner;
            if (!N && !E && S && (SW || NW) && W) return WallPosition.BottomLeftInner;

            // 2) Alone
            if (!N && E && !S && W) return WallPosition.Alone;

            // 3) Triple inner corners
            if (!N && !E && !S && W && (SW || NW) && (NE || SE))
                return WallPosition.TripleExceptLeftInner;
            if (!N && E && !S && (NE || SE) && (NW || SW))
                return WallPosition.TripleExceptRightInner;

            // 4) Triple walls
            if (!N && !E && !W && NE && NW) return WallPosition.TripleExceptDown;
            if (N && (NE || NW) && !E && !S && (SW || SE))
                return WallPosition.TripleExceptUp;

            // 5) Straight walls
            if (!E && S && !W) return WallPosition.Up;
            if (N && !E && !W) return WallPosition.Down;
            if (!N && !S && W) return WallPosition.Right;
            if (!N && !S && E) return WallPosition.Left;

            // 6) All-corners
            if (!N && !E && !S && !W &&
                ((NW && NE) || (NW && SE) || (NW && SW && (NE || SE)) ||
                 (NE && SW) || (NE && SE && (NW || SW)) || (SW && SE)))
                return WallPosition.AllWallCorner;

            // 7) Triple walls (sides)
            if (!N && !E && !S && NE && SE) return WallPosition.TripleExceptLeft;
            if (!N && !W && !S && NW && SW) return WallPosition.TripleExceptRight;

            // 8) External corners
            if (!S && SW && !W) return WallPosition.TopRight;
            if (!E && SE && !S) return WallPosition.TopLeft;
            if (!N && !W && NW) return WallPosition.BottomRight;
            if (!N && !E && NE) return WallPosition.BottomLeft;

            // default
            return WallPosition.Alone;
        }
    }
}