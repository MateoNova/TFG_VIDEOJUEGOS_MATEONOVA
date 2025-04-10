// Editor/Model/WallGenerator.cs

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpecialCases;
using GeneralUtils;
using Generators.Models;

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

        private static Dictionary<Utils.WallPosition, HashSet<Vector2Int>> BuildInitialWallPositions(
            HashSet<Vector2Int> floorPositions)
        {
            return new Dictionary<Utils.WallPosition, HashSet<Vector2Int>>
            {
                { Utils.WallPosition.Up, GetSpecificWallPositions(floorPositions, Vector2Int.up) },
                { Utils.WallPosition.Down, GetSpecificWallPositions(floorPositions, Vector2Int.down) },
                { Utils.WallPosition.Left, GetSpecificWallPositions(floorPositions, Vector2Int.left) },
                { Utils.WallPosition.Right, GetSpecificWallPositions(floorPositions, Vector2Int.right) },
                { Utils.WallPosition.TopLeft, GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.left) },
                { Utils.WallPosition.BottomLeft, GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.left) },
                { Utils.WallPosition.TopRight, GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.right) },
                {
                    Utils.WallPosition.BottomRight,
                    GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.right)
                },
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

        private static int ApplyWallOverrides(Dictionary<Utils.WallPosition, HashSet<Vector2Int>> wallPositionsByType,
            HashSet<Vector2Int> floorPositions)
        {
            var allWallPositions = wallPositionsByType.Values.SelectMany(v => v).ToHashSet();

            var overrideRules = new List<SpecialCases.IWallOverrideCase>
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
}