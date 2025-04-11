using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BottomLeftWallToAllWallCorner = Models.BottomLeftWallToAllWallCorner;
using BottomLeftWallToTripleWallCornerExceptDown = Models.BottomLeftWallToTripleWallCornerExceptDown;
using BottomLeftWallToTripleWallCornerExceptLeft = Models.BottomLeftWallToTripleWallCornerExceptLeft;
using BottomRightWallToAllWallCorner = Models.BottomRightWallToAllWallCorner;
using BottomRightWallToTripleWallCornerExceptDown = Models.BottomRightWallToTripleWallCornerExceptDown;
using BottomRightWallToTripleWallCornerExceptRight = Models.BottomRightWallToTripleWallCornerExceptRight;
using DownWallToTripleWallCornerExceptDown = Models.DownWallToTripleWallCornerExceptDown;
using DownWallToTripleWallCornerExceptUp = Models.DownWallToTripleWallCornerExceptUp;
using DownWallToUpCase = Models.DownWallToUpCase;
using LeftWallAloneToDownCase = Models.LeftWallAloneToDownCase;
using LeftWallToBottomRightInnerCase = Models.LeftWallToBottomRightInnerCase;
using LeftWallToDownCase = Models.LeftWallToDownCase;
using LeftWallToTopRightInnerCase = Models.LeftWallToTopRightInnerCase;
using RightWallAloneToDownCase = Models.RightWallAloneToDownCase;
using RightWallToAloneCase = Models.RightWallToAloneCase;
using RightWallToBottomLeftInnerCase = Models.RightWallToBottomLeftInnerCase;
using RightWallToDownCase = Models.RightWallToDownCase;
using RightWallToTopLeftInnerCase = Models.RightWallToTopLeftInnerCase;
using RightWallToTripleWallCornerExceptLeftInnerCase = Models.RightWallToTripleWallCornerExceptLeftInnerCase;
using RightWallToTripleWallCornerExceptRightInnerCase = Models.RightWallToTripleWallCornerExceptRightInnerCase;
using TilemapPainter = Models.TilemapPainter;
using TopLeftWallAllCornerCase = Models.TopLeftWallAllCornerCase;
using TopLeftWallToAllWallCornerCase = Models.TopLeftWallToAllWallCornerCase;
using TopLeftWallToTripleCornerExceptUpCase = Models.TopLeftWallToTripleCornerExceptUpCase;
using TopRightWallToAllWallCornerCase = Models.TopRightWallToAllWallCornerCase;
using TopRightWallToTripleCornerExceptRightInner = Models.TopRightWallToTripleCornerExceptRightInner;
using TopRightWallToTripleCornerExceptUp = Models.TopRightWallToTripleCornerExceptUp;

namespace Generators.Controllers
{
    public class WallGenerator : MonoBehaviour
    {
        /// <summary>
        /// Genera y pinta las paredes a partir de las posiciones walkables.
        /// </summary>
        /// <param name="walkablePositions">Conjunto de posiciones walkables (Vector2Int).</param>
        /// <param name="painter">Instancia de TilemapPainter a usar para pintar.</param>
        /// <param name="nonWallPositions">Opcional. Conjunto de posiciones que no se pintarán como muro.</param>
        public static void GenerateWalls(HashSet<Vector2Int> walkablePositions, TilemapPainter painter,
            HashSet<Vector2Int> nonWallPositions = null)
        {
            // 1. Construir posiciones iniciales de muro a partir del área de piso.
            var wallPositionsByType = BuildInitialWallPositions(walkablePositions);
            int initialCount = wallPositionsByType.Values.Sum(set => set.Count);
            Debug.Log("Initial wall count: " + initialCount);

            // 2. Aplicar las reglas de override para afinar la clasificación de muros.
            int overridesCount = ApplyWallOverrides(wallPositionsByType, walkablePositions);
            Debug.Log("Overrides applied: " + overridesCount);

            // 3. Pintar cada tipo de muro mediante el TilemapPainter
            foreach (var kvp in wallPositionsByType)
            {
                var positions = nonWallPositions != null
                    ? new HashSet<Vector2Int>(kvp.Value.Except(nonWallPositions))
                    : kvp.Value;
                painter.PaintWallTiles(positions, kvp.Key);
            }
        }

        #region Construcción de Posiciones de Muro

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

        private static HashSet<Vector2Int> GetCornerPositions(HashSet<Vector2Int> floorPositions, Vector2Int direction1,
            Vector2Int direction2)
        {
            var corners = new HashSet<Vector2Int>();
            foreach (var pos in floorPositions)
            {
                var neighbor1 = pos + direction1;
                var neighbor2 = pos + direction2;
                var corner = pos + direction1 + direction2;
                if (!floorPositions.Contains(neighbor1) && !floorPositions.Contains(neighbor2) &&
                    !floorPositions.Contains(corner))
                    corners.Add(corner);
            }

            return corners;
        }

        #endregion

        #region Aplicación de Overwrites

        private static int ApplyWallOverrides(
            Dictionary<Utils.Utils.WallPosition, HashSet<Vector2Int>> wallPositionsByType,
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

            var changes =
                new List<(Vector2Int pos, Utils.Utils.WallPosition oldType, Utils.Utils.WallPosition newType)>();

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
}