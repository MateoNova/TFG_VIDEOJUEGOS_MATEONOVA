using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    /// <summary>
    /// Represents the data structure for storing tilemap information, including walkable, wall, and door tiles.
    /// </summary>
    [System.Serializable]
    public class TilemapData
    {
        /// <summary>
        /// A list of tiles that are walkable.
        /// </summary>
        public List<SerializableTile> walkableTiles;

        /// <summary>
        /// A list of tiles that represent walls.
        /// </summary>
        public List<SerializableTile> wallTiles;

        /// <summary>
        /// A list of tiles that represent doors.
        /// </summary>
        public List<SerializableTile> doorTiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="TilemapData"/> class.
        /// </summary>
        /// <param name="walkableTiles">The list of walkable tiles.</param>
        /// <param name="wallTiles">The list of wall tiles.</param>
        /// <param name="doorTiles">The list of door tiles.</param>
        public TilemapData(List<SerializableTile> walkableTiles, List<SerializableTile> wallTiles,
            List<SerializableTile> doorTiles)
        {
            this.walkableTiles = walkableTiles;
            this.wallTiles = wallTiles;
            this.doorTiles = doorTiles;
        }
    }

    /// <summary>
    /// Represents a serializable tile with position, unique identifier, and door status.
    /// </summary>
    [System.Serializable]
    public class SerializableTile
    {
        /// <summary>
        /// The position of the tile in the tilemap.
        /// </summary>
        public Vector3Int position;

        /// <summary>
        /// A unique identifier for the tile.
        /// </summary>
        public string tileGuid;

        /// <summary>
        /// Indicates whether the tile is a door.
        /// </summary>
        public bool isDoor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableTile"/> class.
        /// </summary>
        /// <param name="position">The position of the tile in the tilemap.</param>
        /// <param name="tileGuid">The unique identifier for the tile.</param>
        /// <param name="isDoor">Optional. Indicates whether the tile is a door. Defaults to false.</param>
        public SerializableTile(Vector3Int position, string tileGuid, bool isDoor = false)
        {
            this.position = position;
            this.tileGuid = tileGuid;
            this.isDoor = isDoor;
        }
    }
}