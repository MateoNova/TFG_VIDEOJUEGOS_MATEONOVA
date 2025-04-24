using System.Collections.Generic;
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
        public static void GenerateWalls(HashSet<Vector2Int> walkable, ITilemapPainter painter,
            HashSet<Vector2Int> nonWall = null)
        {
            var tp = painter as TilemapPainter;
            if (tp == null) return;

            var preset = tp.GetCurrentTilesetPreset();
            var wallMap = tp.wallTilemap;
            if (preset == null || wallMap == null) return;

            // use optimized autotiler
            new WallAutoTiler(preset, wallMap).PaintWalls(walkable, nonWall);

        }
    }
}