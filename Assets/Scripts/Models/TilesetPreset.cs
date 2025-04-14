using UnityEngine;

namespace Models
{
    [CreateAssetMenu(menuName = "Tileset/Tileset Preset", fileName = "NewTilesetPreset")]
    public class TilesetPreset : ScriptableObject
    {
        [Header("Renamed Sprites")] public Sprite[] sprites;
    }
}