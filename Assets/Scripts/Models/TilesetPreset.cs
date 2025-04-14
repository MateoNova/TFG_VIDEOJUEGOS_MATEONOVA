using UnityEngine;
using UnityEngine.Tilemaps;

namespace Models
{
    [CreateAssetMenu(menuName = "Tileset/Tileset Preset", fileName = "NewTilesetPreset")]
    public class TilesetPreset : ScriptableObject
    {
        [Header("Renamed Tiles")] public Tile[] tiles;
    }
}