using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Renders tiles on a Tilemap.
/// </summary>
public class TilemapRenderer : MonoBehaviour
{
    /// <summary>
    /// The Tilemap used to render walkable tiles.
    /// </summary>
    [SerializeField]
    private Tilemap walkableTilemap;

    /// <summary>
    /// The TileBase used for walkable tiles.
    /// </summary>
    [SerializeField]
    private TileBase walkableTileBase;

    /// <summary>
    /// Renders the walkable tiles at the specified positions.
    /// </summary>
    /// <param name="tilesPositions">The positions of the tiles to render.</param>
    public void RenderWalkableTiles(IEnumerable<Vector2Int> tilesPositions)
    {
        RenderTiles(tilesPositions, walkableTilemap, walkableTileBase);
    }

    /// <summary>
    /// Renders tiles at the specified positions on the given Tilemap with the specified TileBase.
    /// </summary>
    /// <param name="positions">The positions of the tiles to render.</param>
    /// <param name="tilemap">The Tilemap to render the tiles on.</param>
    /// <param name="tileBase">The TileBase to use for the tiles.</param>
    private static void RenderTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tileBase)
    {
        foreach (var pos in positions)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)pos);
            tilemap.SetTile(tilePosition, tileBase);
        }
    }

    /// <summary>
    /// Clears all tiles from the walkable Tilemap.
    /// </summary>
    public void ResetAllTiles()
    {
        walkableTilemap?.ClearAllTiles();
    }
}