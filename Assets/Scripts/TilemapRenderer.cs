using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapRenderer : MonoBehaviour
{
    [SerializeField]
    private Tilemap walkableTilemap;
    [SerializeField]
    private TileBase walkableTileBase;

    public void RenderWalkableTiles(IEnumerable<Vector2Int> tilesPositions)
    {
        RenderTiles(tilesPositions, walkableTilemap, walkableTileBase);
    }

    private static void RenderTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tileBase)
    {
        foreach (var pos in positions)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)pos);
            tilemap.SetTile(tilePosition, tileBase);
        }
    }

    public void ResetAllTiles()
    {
        walkableTilemap?.ClearAllTiles();
    }
}