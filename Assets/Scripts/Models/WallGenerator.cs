using System.Collections.Generic;
using System.Linq;
using Generators.Controllers;
using UnityEngine;

// Asumiendo que las clases de override están en Models o un namespace similar.

namespace Models
{
    public class WallGenerator : MonoBehaviour
    {
        public static void GenerateWalls(HashSet<Vector2Int> walkablePositions, ITilemapPainter painter,
            HashSet<Vector2Int> nonWallPositions = null)
        {
            // 1. Construir posiciones iniciales de muro
            var wallPositionsByType = BuildInitialWallPositions(walkablePositions);
            Debug.Log("Initial wall count: " + wallPositionsByType.Values.Sum(set => set.Count));

            // 2. Aplicar override rules de manera optimizada
            int overridesCount = ApplyWallOverrides(wallPositionsByType, walkablePositions);
            Debug.Log("Overrides applied: " + overridesCount);

            // 3. Pintar cada tipo de muro
            foreach (var kvp in wallPositionsByType)
            {
                var positions = (nonWallPositions != null)
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

        #region Aplicación de Overrides

        private static int ApplyWallOverrides(
            Dictionary<Utils.Utils.WallPosition, HashSet<Vector2Int>> wallPositionsByType,
            HashSet<Vector2Int> floorPositions)
        {
            // Consolidar todas las posiciones de muro para optimizar las búsquedas
            HashSet<Vector2Int> allWallPositions = wallPositionsByType.Values.SelectMany(v => v).ToHashSet();
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
                             .ToList()) // ToList para evitar modificar la colección durante el recorrido
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