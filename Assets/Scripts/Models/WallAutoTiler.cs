using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Models
{
    public class WallAutoTiler
    {
        private readonly TileBase[] _maskTiles;
        private readonly Tilemap _tilemap;
        private static readonly Vector2Int[] _dirs = {
            Vector2Int.up,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.right,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.down,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.left,
            Vector2Int.up + Vector2Int.left
        };

        public WallAutoTiler(TilesetPreset preset, Tilemap tilemap)
        {
            _maskTiles = preset.maskTiles;
            _tilemap = tilemap;
        }

        public void PaintWalls(HashSet<Vector2Int> walkable, HashSet<Vector2Int> nonWall = null)
        {
            var candidates = new HashSet<Vector2Int>();
            // gather wall candidates
            foreach (var p in walkable)
            foreach (var d in _dirs)
            {
                var nb = p + d;
                if (!walkable.Contains(nb) && (nonWall == null || !nonWall.Contains(nb)))
                    candidates.Add(nb);
            }

            // paint by mask
            foreach (var pos in candidates)
            {
                int mask = 0;
                for (int i = 0; i < 8; i++)
                    if (walkable.Contains(pos + _dirs[i])) mask |= 1 << i;

                var tile = (mask < _maskTiles.Length && _maskTiles[mask] != null)
                    ? _maskTiles[mask]
                    : _maskTiles[0];

                var cell = _tilemap.WorldToCell(new Vector3Int(pos.x, pos.y, 0));
                _tilemap.SetTile(cell, tile);
            }
        }
    }
}