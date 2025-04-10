using System.Collections.Generic;
using UnityEngine;

namespace Generators.Models
{
    [System.Serializable]
    public class TilemapData
    {
        public List<SerializableTile> walkableTiles;
        public List<SerializableTile> wallTiles;
        public List<SerializableTile> doorTiles;

        public TilemapData(List<SerializableTile> walkableTiles, List<SerializableTile> wallTiles,
            List<SerializableTile> doorTiles)
        {
            this.walkableTiles = walkableTiles;
            this.wallTiles = wallTiles;
            this.doorTiles = doorTiles;
        }
    }

    [System.Serializable]
    public class SerializableTile
    {
        public Vector3Int position;
        public string tileGUID;
        public bool isDoor;

        public SerializableTile(Vector3Int position, string tileGUID, bool isDoor = false)
        {
            this.position = position;
            this.tileGUID = tileGUID;
            this.isDoor = isDoor;
        }
    }
}