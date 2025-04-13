using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models
{
    /// <summary>
    /// Responsible for generating walls around walkable positions in a dungeon.
    /// </summary>
    public class WallGenerator : MonoBehaviour
    {
        /// <summary>
        /// Generates walls based on walkable positions and paints them using the provided tilemap painter.
        /// </summary>
        /// <param name="walkablePositions">A set of positions that are walkable.</param>
        /// <param name="painter">The tilemap painter used to paint the walls.</param>
        /// <param name="nonWallPositions">Optional. A set of positions where walls should not be placed.</param>
        public static void GenerateWalls(HashSet<Vector2Int> walkablePositions, ITilemapPainter painter,
            HashSet<Vector2Int> nonWallPositions = null)
        {
            // 1. Build initial wall positions
            var wallPositionsByType = BuildInitialWallPositions(walkablePositions);
            Debug.Log("Initial wall count: " + wallPositionsByType.Values.Sum(set => set.Count));

            // 2. Apply override rules to optimize wall placement
            var overridesCount = ApplyWallOverrides(wallPositionsByType, walkablePositions);
            Debug.Log("Overrides applied: " + overridesCount);

            // 3. Paint each type of wall
            foreach (var kvp in wallPositionsByType)
            {
                var positions = (nonWallPositions != null)
                    ? new HashSet<Vector2Int>(kvp.Value.Except(nonWallPositions))
                    : kvp.Value;
                painter.PaintWallTiles(positions, kvp.Key);
            }
        }

        #region Wall Position Construction

        /// <summary>
        /// Builds the initial wall positions based on the given floor positions.
        /// </summary>
        /// <param name="floorPositions">A set of positions representing the floor.</param>
        /// <returns>A dictionary mapping wall types to their respective positions.</returns>
        private static Dictionary<Utils.Utils.WallPosition, HashSet<Vector2Int>> BuildInitialWallPositions(
            HashSet<Vector2Int> floorPositions)
        {
            return new Dictionary<Utils.Utils.WallPosition, HashSet<Vector2Int>>
            {
                { Utils.Utils.WallPosition.Up, GetSpecificWallPositions(floorPositions, Vector2Int.up) },
                { Utils.Utils.WallPosition.Down, GetSpecificWallPositions(floorPositions, Vector2Int.down) },
                { Utils.Utils.WallPosition.Left, GetSpecificWallPositions(floorPositions, Vector2Int.left) },
                { Utils.Utils.WallPosition.Right, GetSpecificWallPositions(floorPositions, Vector2Int.right) },
                {
                    Utils.Utils.WallPosition.TopLeft, GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.left)
                },
                {
                    Utils.Utils.WallPosition.BottomLeft,
                    GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.left)
                },
                {
                    Utils.Utils.WallPosition.TopRight,
                    GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.right)
                },
                {
                    Utils.Utils.WallPosition.BottomRight,
                    GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.right)
                },
                { Utils.Utils.WallPosition.TripleExceptUp, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.TripleExceptDown, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.TripleExceptLeft, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.TripleExceptRight, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.AllWallCorner, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.TopLeftInner, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.TopRightInner, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.BottomLeftInner, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.BottomRightInner, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.Alone, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.TripleExceptLeftInner, new HashSet<Vector2Int>() },
                { Utils.Utils.WallPosition.TripleExceptRightInner, new HashSet<Vector2Int>() }
            };
        }

        /// <summary>
        /// Gets wall positions in a specific direction relative to the floor positions.
        /// </summary>
        /// <param name="floorPositions">A set of positions representing the floor.</param>
        /// <param name="direction">The direction to check for wall positions.</param>
        /// <returns>A set of wall positions in the specified direction.</returns>
        private static HashSet<Vector2Int> GetSpecificWallPositions(HashSet<Vector2Int> floorPositions,
            Vector2Int direction)
        {
            var positions = new HashSet<Vector2Int>();
            foreach (var pos in floorPositions)
            {
                var neighbor = pos + direction;
                if (!floorPositions.Contains(neighbor))
                    positions.Add(neighbor);
            }

            return positions;
        }

        /// <summary>
        /// Gets corner wall positions based on two directions relative to the floor positions.
        /// </summary>
        /// <param name="floorPositions">A set of positions representing the floor.</param>
        /// <param name="direction1">The first direction to check for wall positions.</param>
        /// <param name="direction2">The second direction to check for wall positions.</param>
        /// <returns>A set of corner wall positions.</returns>
        private static HashSet<Vector2Int> GetCornerPositions(HashSet<Vector2Int> floorPositions, Vector2Int direction1,
            Vector2Int direction2)
        {
            var corners = new HashSet<Vector2Int>();
            foreach (var pos in floorPositions)
            {
                Vector2Int neighbor1 = pos + direction1;
                Vector2Int neighbor2 = pos + direction2;
                Vector2Int corner = pos + direction1 + direction2;
                if (!floorPositions.Contains(neighbor1) && !floorPositions.Contains(neighbor2) &&
                    !floorPositions.Contains(corner))
                    corners.Add(corner);
            }

            return corners;
        }

        #endregion

        #region Wall Overrides Application

        /// <summary>
        /// Applies override rules to adjust wall positions based on specific conditions.
        /// </summary>
        /// <param name="wallPositionsByType">A dictionary mapping wall types to their respective positions.</param>
        /// <param name="floorPositions">A set of positions representing the floor.</param>
        /// <returns>The number of overrides applied.</returns>
        private static int ApplyWallOverrides(
            Dictionary<Utils.Utils.WallPosition, HashSet<Vector2Int>> wallPositionsByType,
            HashSet<Vector2Int> floorPositions)
        {
            // Consolidate all wall positions for optimized lookups
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

            var changes =
                new List<(Vector2Int pos, Utils.Utils.WallPosition oldType, Utils.Utils.WallPosition newType)>();
            foreach (var wallType in wallPositionsByType.Keys.ToList())
            {
                foreach (var pos in
                         wallPositionsByType[wallType]
                             .ToList()) // ToList to avoid modifying the collection during iteration
                {
                    foreach (var rule in overrideRules)
                    {
                        if (!rule.IsMatch(pos, floorPositions, allWallPositions, wallType)) continue;

                        changes.Add((pos, wallType, rule.OverrideWallPosition));
                        break;
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
}