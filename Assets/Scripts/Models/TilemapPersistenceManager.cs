using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Models
{
    /// <summary>
    /// Manages the persistence of tilemaps, allowing saving and loading of tilemap data.
    /// </summary>
    public class TilemapPersistenceManager
    {
        /// <summary>
        /// The tilemap containing walkable tiles.
        /// </summary>
        private readonly Tilemap walkableTilemap;

        /// <summary>
        /// The tilemap containing wall tiles.
        /// </summary>
        private readonly Tilemap wallTilemap;

        /// <summary>
        /// The tilemap containing door tiles.
        /// </summary>
        private readonly Tilemap doorTilemap;

        /// <summary>
        /// Initializes a new instance of the <see cref="TilemapPersistenceManager"/> class.
        /// </summary>
        /// <param name="walkable">The tilemap for walkable tiles.</param>
        /// <param name="wall">The tilemap for wall tiles.</param>
        /// <param name="door">The tilemap for door tiles.</param>
        public TilemapPersistenceManager(Tilemap walkable, Tilemap wall, Tilemap door)
        {
            walkableTilemap = walkable;
            wallTilemap = wall;
            doorTilemap = door;
        }

        /// <summary>
        /// Converts the tiles in a tilemap to a list of serializable tiles.
        /// </summary>
        /// <param name="tilemap">The tilemap to process.</param>
        /// <param name="isDoor">Indicates whether the tiles are doors.</param>
        /// <returns>A list of serializable tiles.</returns>
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

        /// <summary>
        /// Saves the current state of the tilemaps to a file.
        /// </summary>
        /// <param name="path">The file path to save the tilemap data.</param>
        public void SaveTilemap(string path)
        {
            var data = new TilemapData(
                GetSerializableTiles(walkableTilemap),
                GetSerializableTiles(wallTilemap),
                GetSerializableTiles(doorTilemap, true)
            );
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads tilemap data from a file and applies it to the tilemaps.
        /// </summary>
        /// <param name="path">The file path to load the tilemap data from.</param>
        /// <param name="painter">The tilemap painter used to reset and paint tiles.</param>
        /// <param name="clearBeforeLoading">Indicates whether to clear the tilemaps before loading.</param>
        /// <param name="offset">The offset to apply to the tile positions.</param>
        public void LoadTilemap(string path, ITilemapPainter painter, bool clearBeforeLoading = true,
            Vector3Int offset = default)
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<TilemapData>(json);
            if (clearBeforeLoading)
                painter.ResetAllTiles();

            foreach (var tile in data.walkableTiles)
            {
                var tileBase = GetTileBaseByGuid(tile.tileGuid);
                walkableTilemap.SetTile(tile.position + offset, tileBase);
            }

            foreach (var tile in data.wallTiles)
            {
                var tileBase = GetTileBaseByGuid(tile.tileGuid);
                wallTilemap.SetTile(tile.position + offset, tileBase);
            }

            foreach (var tile in data.doorTiles)
            {
                var tileBase = GetTileBaseByGuid(tile.tileGuid);
                doorTilemap.SetTile(tile.position + offset, tileBase);
            }
        }

        /// <summary>
        /// Retrieves a tile asset from its GUID.
        /// </summary>
        /// <param name="guid">The GUID of the tile asset.</param>
        /// <returns>The tile asset as a <see cref="TileBase"/>.</returns>
        private static TileBase GetTileBaseByGuid(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
        }
    }
}