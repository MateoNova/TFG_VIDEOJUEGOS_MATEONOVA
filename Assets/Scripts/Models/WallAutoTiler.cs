using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Models
{
    /// <summary>
    /// Automatically tiles walls based on a set of mask tiles and a tilemap.
    /// </summary>
    public class WallAutoTiler
    {
        private readonly TileBase[] _maskTiles;
        private readonly Tilemap _tilemap;

        private static readonly Vector2Int[] Dirs =
        {
            Vector2Int.up,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.right,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.down,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.left,
            Vector2Int.up + Vector2Int.left
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="WallAutoTiler"/> class.
        /// </summary>
        /// <param name="preset">The tileset preset containing mask tiles.</param>
        /// <param name="tilemap">The tilemap where walls will be painted.</param>
        public WallAutoTiler(TilesetPreset preset, Tilemap tilemap)
        {
            _maskTiles = preset.maskTiles;
            _tilemap = tilemap;
        }
        
        /// <summary>
        /// Paints walls on the tilemap based on the provided walkable positions.
        /// </summary>
        /// <param name="walkable">A set of walkable positions where walls should be painted.</param>
        /// <param name="nonWall">An optional set of positions that should not be painted as walls.</param>
        public void PaintWalls(HashSet<Vector2Int> walkable, HashSet<Vector2Int> nonWall = null)
        {
            var candidates = new HashSet<Vector2Int>();

            // gather wall candidates
            foreach (var nb in walkable.SelectMany(p => Dirs, (p, d) => p + d).Where(nb =>
                         !walkable.Contains(nb) && (nonWall == null || !nonWall.Contains(nb))))
                candidates.Add(nb);

            // paint by mask
            foreach (var pos in candidates)
            {
                var mask = 0;
                for (var i = 0; i < 8; i++)
                    if (walkable.Contains(pos + Dirs[i]))
                        mask |= 1 << i;

                var tile = (mask < _maskTiles.Length && _maskTiles[mask] != null)
                    ? _maskTiles[mask]
                    : _maskTiles[0];

                var cell = _tilemap.WorldToCell(new Vector3Int(pos.x, pos.y, 0));
                _tilemap.SetTile(cell, tile);
            }
        }
    }
}