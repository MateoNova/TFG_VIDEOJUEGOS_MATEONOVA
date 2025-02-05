using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Renders tiles on a Tilemap.
/// </summary>
public class TilemapPainter : MonoBehaviour
{
    /// <summary>
    /// The Tilemap used to render walkable tiles.
    /// </summary>
    [SerializeField]
    private Tilemap walkableTilemap, wallTilemap;

    /// <summary>
    /// The TileBase used for walkable tiles.
    /// </summary>
    [SerializeField]
    private TileBase walkableTileBase, wallTileBase;

    /// <summary>
    /// Renders the walkable tiles at the specified positions.
    /// </summary>
    /// <param name="tilesPositions">The positions of the tiles to render.</param>
    public void PaintWalkableTiles(IEnumerable<Vector2Int> tilesPositions)
    {
        PaintTiles(tilesPositions, walkableTilemap, walkableTileBase);
    }

    public void PaintWallTiles(IEnumerable<Vector2Int> position)
    {
        PaintTiles(position, wallTilemap, wallTileBase);
    }

    /// <summary>
    /// Renders tiles at the specified positions on the given Tilemap with the specified TileBase.
    /// </summary>
    /// <param name="positions">The positions of the tiles to render.</param>
    /// <param name="tilemap">The Tilemap to render the tiles on.</param>
    /// <param name="tileBase">The TileBase to use for the tiles.</param>
    private static void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tileBase)
    {
        foreach (var pos in positions)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)pos);
            tilemap.SetTile(tilePosition, tileBase);
        }
    }

    /// <summary>
    /// Clears all tiles from grid.
    /// </summary>
    public void ResetAllTiles()
    {
        walkableTilemap?.ClearAllTiles();
        wallTilemap?.ClearAllTiles();
    }

   public void SaveTilemap(string path)
   {
       var walkableTiles = new List<SerializableTile>();
       var wallTiles = new List<SerializableTile>();
   
       foreach (var pos in walkableTilemap.cellBounds.allPositionsWithin)
       {
           var tile = walkableTilemap.GetTile(pos);
           if (tile)
           {
               walkableTiles.Add(new SerializableTile(pos, tile.name));
           }
       }
   
       foreach (var pos in wallTilemap.cellBounds.allPositionsWithin)
       {
           var tile = wallTilemap.GetTile(pos);
           if (tile)
           {
               wallTiles.Add(new SerializableTile(pos, tile.name));
           }
       }
   
       var tilemapData = new TilemapData(walkableTiles, wallTiles);
       var json = JsonUtility.ToJson(tilemapData);
       System.IO.File.WriteAllText(path, json);
   }

   public void LoadTilemap(string path)
   {
       var json = System.IO.File.ReadAllText(path);
       var tilemapData = JsonUtility.FromJson<TilemapData>(json);

       ResetAllTiles();

       foreach (var tile in tilemapData.walkableTiles)
       {
           var tileBase = GetTileBaseByName(tile.tileName);
           walkableTilemap.SetTile(tile.position, tileBase);
       }

       foreach (var tile in tilemapData.wallTiles)
       {
           var tileBase = GetTileBaseByName(tile.tileName);
           wallTilemap.SetTile(tile.position, tileBase);
       }
   }

   private static TileBase GetTileBaseByName(string tileName)
   {
       // Find all assets with the specified name
       var guids = AssetDatabase.FindAssets(tileName, new[] {"Assets/Assets/TilemapsDungeonTilesetil"});
       return guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Select(AssetDatabase.LoadAssetAtPath<TileBase>).FirstOrDefault(tile => tile && tile.name == tileName);
   }
}

[System.Serializable]
public class TilemapData
{
    public List<SerializableTile> walkableTiles;
    public List<SerializableTile> wallTiles;

    public TilemapData(List<SerializableTile> walkableTiles, List<SerializableTile> wallTiles)
    {
        this.walkableTiles = walkableTiles;
        this.wallTiles = wallTiles;
    }
}

[System.Serializable]
public class SerializableTile
{
    public Vector3Int position;
    public string tileName;

    public SerializableTile(Vector3Int position, string tileName)
    {
        this.position = position;
        this.tileName = tileName;
    }
}