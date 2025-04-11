using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Models
{
    public class TilemapPersistenceManager
    {
        private Tilemap walkableTilemap;
        private Tilemap wallTilemap;
        private Tilemap doorTilemap;

        public TilemapPersistenceManager(Tilemap walkable, Tilemap wall, Tilemap door)
        {
            walkableTilemap = walkable;
            wallTilemap = wall;
            doorTilemap = door;
        }

        private static List<SerializableTile> GetSerializableTiles(Tilemap tilemap, bool isDoor = false)
        {
            var list = new List<SerializableTile>();
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                var tile = tilemap.GetTile(pos);
                if (tile == null) continue;
                var assetPath = AssetDatabase.GetAssetPath(tile);
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                list.Add(new SerializableTile(pos, guid, isDoor));
            }
            return list;
        }

        public void SaveTilemap(string path)
        {
            var data = new TilemapData(
                GetSerializableTiles(walkableTilemap),
                GetSerializableTiles(wallTilemap),
                GetSerializableTiles(doorTilemap, true)
            );
            var json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(path, json);
        }

        public void LoadTilemap(string path, ITilemapPainter painter, bool clearBeforeLoading = true, Vector3Int offset = default)
        {
            var json = System.IO.File.ReadAllText(path);
            var data = JsonUtility.FromJson<TilemapData>(json);
            if (clearBeforeLoading)
                painter.ResetAllTiles();

            foreach (var tile in data.walkableTiles)
            {
                var tileBase = GetTileBaseByGuid(tile.tileGUID);
                walkableTilemap.SetTile(tile.position + offset, tileBase);
            }
            foreach (var tile in data.wallTiles)
            {
                var tileBase = GetTileBaseByGuid(tile.tileGUID);
                wallTilemap.SetTile(tile.position + offset, tileBase);
            }
            foreach (var tile in data.doorTiles)
            {
                var tileBase = GetTileBaseByGuid(tile.tileGUID);
                doorTilemap.SetTile(tile.position + offset, tileBase);
            }
        }

        private static TileBase GetTileBaseByGuid(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
        }
    }
}
