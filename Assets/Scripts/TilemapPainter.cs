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
    /// The Tilemap used to render the walkable tiles.
    /// </summary>
    [SerializeField] private Tilemap walkableTilemap, wallTilemap;

    /// <summary>
    /// The TileBases used for the walkable tiles.
    /// </summary>
    [SerializeField] private List<TileBase> walkableTileBases;

    /// <summary>
    /// The TileBase used for the wall tiles.
    /// </summary>
    [SerializeField] private TileBase wallTileBase;

    /// <summary>
    /// Whether to place walkable tiles randomly.
    /// </summary>
    [SerializeField] private bool randomPlacement;

    /// <summary>
    /// Priorities for each walkable tile.
    /// </summary>
    [SerializeField] private List<int> tilePriorities;

    /// <summary>
    /// Probabilities for each walkable tile.
    /// </summary>
    private Dictionary<TileBase, float> _tileProbabilities = new();

    /// <summary>
    /// Initializes the tile probabilities based on the priorities.
    /// </summary>
    private void InitializeTileProbabilities()
    {
        _tileProbabilities = new Dictionary<TileBase, float>();
        var totalPriority = tilePriorities.Sum();

        for (var i = 0; i < walkableTileBases.Count; i++)
        {
            if (i < tilePriorities.Count)
            {
                _tileProbabilities[walkableTileBases[i]] = (float)tilePriorities[i] / totalPriority;
            }
            else
            {
                Debug.LogWarning($"No priority defined for tile at index {i}. Defaulting to 0.");
                _tileProbabilities[walkableTileBases[i]] = 0f;
            }
        }
    }

    /// <summary>
    /// Renders the walkable tiles at the specified positions.
    /// </summary>
    /// <param name="tilesPositions">The positions of the tiles to render.</param>
    public void PaintWalkableTiles(IEnumerable<Vector2Int> tilesPositions)
    {
        InitializeTileProbabilities();
        if (randomPlacement)
        {
            PaintTilesRandomly(tilesPositions, walkableTilemap, walkableTileBases);
        }
        else
        {
            PaintTilesWithProbabilities(tilesPositions, walkableTilemap, walkableTileBases, _tileProbabilities);
        }
    }

    /// <summary>
    /// Renders the wall tiles at the specified positions.
    /// </summary>
    /// <param name="positions">The positions of the tiles to render.</param>
    public void PaintWallTiles(IEnumerable<Vector2Int> positions)
    {
        PaintTiles(positions, wallTilemap, wallTileBase);
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
    /// Renders tiles randomly at the specified positions on the given Tilemap with the specified TileBases.
    /// </summary>
    /// <param name="positions">The positions of the tiles to render.</param>
    /// <param name="tilemap">The Tilemap to render the tiles on.</param>
    /// <param name="tileBases">The TileBases to use for the tiles.</param>
    private static void PaintTilesRandomly(IEnumerable<Vector2Int> positions, Tilemap tilemap, List<TileBase> tileBases)
    {
        var random = new System.Random();
        foreach (var pos in positions)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)pos);
            var tileBase = tileBases[random.Next(tileBases.Count)];
            tilemap.SetTile(tilePosition, tileBase);
        }
    }

    /// <summary>
    /// Renders tiles at the specified positions on the given Tilemap with the specified TileBases and probabilities.
    /// </summary>
    /// <param name="positions">The positions of the tiles to render.</param>
    /// <param name="tilemap">The Tilemap to render the tiles on.</param>
    /// <param name="tileBases">The TileBases to use for the tiles.</param>
    /// <param name="probabilities">The probabilities for each TileBase.</param>
    private static void PaintTilesWithProbabilities(IEnumerable<Vector2Int> positions, Tilemap tilemap,
        List<TileBase> tileBases, Dictionary<TileBase, float> probabilities)
    {
        var cumulativeProbabilities = new List<float>();
        var totalProbability = probabilities.Values.Sum();
        var random = new System.Random();

        foreach (var tileBase in tileBases)
        {
            if (!probabilities.ContainsKey(tileBase))
            {
                Debug.LogError($"Probability for tile {tileBase.name} is not set.");
                return;
            }

            cumulativeProbabilities.Add(probabilities[tileBase] / totalProbability);
        }

        foreach (var pos in positions)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)pos);
            var randomValue = (float)random.NextDouble();
            var cumulativeSum = 0f;

            for (int i = 0; i < tileBases.Count; i++)
            {
                cumulativeSum += cumulativeProbabilities[i];
                if (!(randomValue <= cumulativeSum)) continue;
                
                tilemap.SetTile(tilePosition, tileBases[i]);
                break;
            }
        }
    }

    /// <summary>
    /// Clears all tiles from the grid.
    /// </summary>
    public void ResetAllTiles()
    {
        walkableTilemap?.ClearAllTiles();
        wallTilemap?.ClearAllTiles();
    }

    /// <summary>
    /// Saves the current state of the Tilemap to a file.
    /// </summary>
    /// <param name="path">The path to save the Tilemap data.</param>
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

    /// <summary>
    /// Loads the Tilemap state from a file.
    /// </summary>
    /// <param name="path">The path to load the Tilemap data from.</param>
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

    /// <summary>
    /// Finds a TileBase by its name.
    /// </summary>
    /// <param name="tileName">The name of the TileBase to find.</param>
    /// <returns>The TileBase with the specified name, or null if not found.</returns>
    private static TileBase GetTileBaseByName(string tileName)
    {
        var guids = AssetDatabase.FindAssets(tileName, new[] { "Assets/Assets/TilemapsDungeonTilesetil" });
        return guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Select(AssetDatabase.LoadAssetAtPath<TileBase>)
            .FirstOrDefault(tile => tile && tile.name == tileName);
    }
    
    public void RemoveTileAtPosition(int position)
    {
        // Validar que el índice esté dentro del rango
        if (position < 0 || position >= walkableTileBases.Count)
        {
            Debug.LogWarning("Índice fuera de rango.");
            return;
        }

        // Obtener el TileBase en la posición indicada
        TileBase tile = walkableTileBases[position];
        Debug.Log("Dictionary: " + _tileProbabilities.Count);

        // Si no es nulo, lo remueve del diccionario
        if (tile != null)
        {
            _tileProbabilities.Remove(tile);
            Debug.Log("Removed");
            Debug.Log("Dictionary: " + _tileProbabilities.Count);

        }

        // Eliminar el elemento en la posición indicada de ambas listas
        walkableTileBases.RemoveAt(position);
        tilePriorities.RemoveAt(position);
    }


    public void RemoveAllTiles()
    {
        walkableTileBases.Clear();
        tilePriorities.Clear();
        _tileProbabilities.Clear();
    }
}

/// <summary>
/// Serializable data for a Tilemap.
/// </summary>
[System.Serializable]
public class TilemapData
{
    public List<SerializableTile> walkableTiles;
    public List<SerializableTile> wallTiles;

    /// <summary>
    /// Initializes a new instance of the TilemapData class.
    /// </summary>
    /// <param name="walkableTiles">The list of walkable tiles.</param>
    /// <param name="wallTiles">The list of wall tiles.</param>
    public TilemapData(List<SerializableTile> walkableTiles, List<SerializableTile> wallTiles)
    {
        this.walkableTiles = walkableTiles;
        this.wallTiles = wallTiles;
    }
}

/// <summary>
/// Serializable representation of a tile.
/// </summary>
[System.Serializable]
public class SerializableTile
{
    public Vector3Int position;
    public string tileName;

    /// <summary>
    /// Initializes a new instance of the SerializableTile class.
    /// </summary>
    /// <param name="position">The position of the tile.</param>
    /// <param name="tileName">The name of the tile.</param>
    public SerializableTile(Vector3Int position, string tileName)
    {
        this.position = position;
        this.tileName = tileName;
    }
}