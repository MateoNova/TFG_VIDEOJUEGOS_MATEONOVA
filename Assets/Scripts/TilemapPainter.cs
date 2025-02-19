using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

/// <summary>
/// Renders tiles on a Tilemap.
/// </summary>
public class TilemapPainter : MonoBehaviour
{
    #region Inspector Fields

    /// <summary>
    /// The Tilemap used to render walkable tiles.
    /// </summary>
    [SerializeField] private Tilemap walkableTilemap;

    /// <summary>
    /// The Tilemap used to render wall tiles.
    /// </summary>
    [SerializeField] private Tilemap wallTilemap;

    /// <summary>
    /// List of walkable tile bases. This allows for multiple walkable tiles to be used.
    /// </summary>
    [SerializeField] private List<TileBase> walkableTileBases;

    /// <summary>
    /// List of priorities corresponding to the walkable tiles. The higher the priority, the more likely the tile will be chosen.
    /// </summary>
    [ConditionalField("randomWalkableTilesPlacement"), SerializeField]
    private List<int> walkableTilesPriorities;

    /// <summary>
    /// Indicates if the walkable tiles should be placed randomly.
    /// </summary>
    [SerializeField] public bool randomWalkableTilesPlacement;

    /// <summary>
    /// Dictionary of walkable tiles and their probabilities. This is used to select tiles based on their assigned probabilities.
    /// </summary>
    private Dictionary<TileBase, float> _walkableTilesProbabilities = new();

    /// <summary>
    /// List of wall tile bases. This allows for multiple wall tiles to be used.
    /// </summary>
    //[SerializeField] private List<TileBase> wallTileBases;
    [SerializeField] private TileBase upWall,
        downWall,
        leftWall,
        rightWall,
        topLeftWall,
        topRightWall,
        bottomLeftWall,
        bottomRightWall,
        tripleWallCornerLeftTile,
        tripleWallCornerExcetUpTile,
        tripleWallCornerExcetDownTile,
        tripleWallCornerExceptLeft,
        tripleWallCornerExceptRight,
        allCornersWall;

    /// <summary>
    /// List of wall tile bases. This allows for multiple wall tiles to be used.
    /// </summary>
    //[SerializeField] private List<TileBase> wallTileBases;

    /// <summary>
    /// List of priorities corresponding to the wall tiles. The higher the priority, the more likely the tile will be chosen.
    /// </summary>
    [SerializeField] private List<int> wallTilesPriorities;

    /// <summary>
    /// Indicates if the wall tiles should be placed randomly.
    /// </summary>
    [SerializeField] private bool randomWallTilesPlacement;

    /// <summary>
    /// Dictionary of wall tiles and their probabilities. This is used to select tiles based on their assigned probabilities.
    /// </summary>
    private Dictionary<TileBase, float> _wallTilesProbabilities = new();

    public TilemapPainter(bool randomWalkableTilesPlacement)
    {
        this.randomWalkableTilesPlacement = randomWalkableTilesPlacement;
    }

    #endregion

    #region Probability Initialization

    /// <summary>
    /// Creates a dictionary of probabilities from the lists of tiles and priorities.
    /// </summary>
    /// <param name="tileBases">List of tile bases.</param>
    /// <param name="priorities">List of priorities corresponding to the tiles.</param>
    /// <returns>Dictionary of tiles and their probabilities.</returns>
    private static Dictionary<TileBase, float> InitializeProbabilities(List<TileBase> tileBases, List<int> priorities)
    {
        var probabilities = new Dictionary<TileBase, float>();
        var totalPriority = priorities.Sum();

        for (var i = 0; i < tileBases.Count; i++)
        {
            if (i < priorities.Count)
            {
                probabilities[tileBases[i]] = totalPriority != 0 ? (float)priorities[i] / totalPriority : 0f;
            }
            else
            {
                Debug.LogWarning($"No priority defined for tile at index {i}. Defaulting to 0.");
                probabilities[tileBases[i]] = 0f;
            }
        }

        return probabilities;
    }

    /// <summary>
    /// Initializes the probabilities for walkable tiles.
    /// </summary>
    private void InitializeWalkableTilesProbabilities() =>
        _walkableTilesProbabilities = InitializeProbabilities(walkableTileBases, walkableTilesPriorities);

    /// <summary>
    /// Initializes the probabilities for wall tiles.
    /// </summary>
    // private void InitializeWallTilesProbabilities() =>
    //     _wallTilesProbabilities = InitializeProbabilities(wallTileBases, wallTilesPriorities);

    #endregion

    #region Painting Methods

    /// <summary>
    /// Renders the walkable tiles at the specified positions.
    /// </summary>
    /// <param name="tilesPositions">Positions to render the walkable tiles.</param>
    public void PaintWalkableTiles(IEnumerable<Vector2Int> tilesPositions)
    {
        InitializeWalkableTilesProbabilities();

        if (randomWalkableTilesPlacement)
        {
            PaintTilesRandomly(tilesPositions, walkableTilemap, walkableTileBases);
        }
        else
        {
            PaintTilesWithProbabilities(tilesPositions, walkableTilemap, walkableTileBases,
                _walkableTilesProbabilities);
        }
    }

    /// <summary>
    /// Renders the wall tiles at the specified positions.
    /// </summary>
    /// <param name="tilesPositions">Positions to render the wall tiles.</param>
    public void PaintWallTiles(IEnumerable<Vector2Int> tilesPositions, Utils.WallPosition position)
    {
        TileBase tile = position switch
        {
            Utils.WallPosition.Up => upWall,
            Utils.WallPosition.Down => downWall,
            Utils.WallPosition.Left => leftWall,
            Utils.WallPosition.Right => rightWall,
            Utils.WallPosition.TopLeft => topLeftWall,
            Utils.WallPosition.BottomLeft => bottomLeftWall,
            Utils.WallPosition.TopRight => topRightWall,
            Utils.WallPosition.BottomRight => bottomRightWall,
            
            Utils.WallPosition.TripleWallCornerExceptUp => tripleWallCornerExcetUpTile,
            Utils.WallPosition.TripleWallCornerExceptDown => tripleWallCornerExcetDownTile,
            Utils.WallPosition.TripleWallCornerExceptLeft => tripleWallCornerExceptLeft,
            Utils.WallPosition.TripleWallCornerExceptRight => tripleWallCornerExceptRight,
            Utils.WallPosition.AllWallCorner => allCornersWall,
            _ => null
        };

        if (tile == null) return;

        foreach (var pos in tilesPositions)
        {
            var tilePosition = wallTilemap.WorldToCell((Vector3Int)pos);
            wallTilemap.SetTile(tilePosition, tile);
        }

        // InitializeWallTilesProbabilities();
        //
        // if (randomWallTilesPlacement)
        // {
        //     PaintTilesRandomly(tilesPositions, wallTilemap, wallTileBases);
        // }
        // else
        // {
        //     PaintTilesWithProbabilities(tilesPositions, wallTilemap, wallTileBases, _wallTilesProbabilities);
        // }
    }

    /// <summary>
    /// Renders tiles randomly at the specified positions on the given Tilemap.
    /// </summary>
    /// <param name="positions">Positions to render the tiles.</param>
    /// <param name="tilemap">Tilemap to render the tiles on.</param>
    /// <param name="tileBases">List of tile bases to choose from.</param>
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
    /// Renders tiles at the specified positions on the given Tilemap, selecting each tile according to the assigned probabilities.
    /// </summary>
    /// <param name="positions">Positions to render the tiles.</param>
    /// <param name="tilemap">Tilemap to render the tiles on.</param>
    /// <param name="tileBases">List of tile bases to choose from.</param>
    /// <param name="probabilities">Dictionary of tiles and their probabilities.</param>
    private static void PaintTilesWithProbabilities(IEnumerable<Vector2Int> positions, Tilemap tilemap,
        List<TileBase> tileBases, Dictionary<TileBase, float> probabilities)
    {
        var cumulativeProbabilities = new List<float>();
        var totalProbability = probabilities.Values.Sum();
        var random = new System.Random();

        foreach (var tileBase in tileBases)
        {
            if (!probabilities.TryGetValue(tileBase, out var probability))
            {
                Debug.LogError($"Probability for tile {tileBase.name} is not set.");
                return;
            }

            cumulativeProbabilities.Add(probability / totalProbability);
        }

        foreach (var pos in positions)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)pos);
            var randomValue = (float)random.NextDouble();
            var cumulativeSum = 0f;

            for (var i = 0; i < tileBases.Count; i++)
            {
                cumulativeSum += cumulativeProbabilities[i];
                if (!(randomValue <= cumulativeSum)) continue;
                tilemap.SetTile(tilePosition, tileBases[i]);
                break;
            }
        }
    }

    #endregion

    #region Save & Load

    /// <summary>
    /// Clears all tiles from both tilemaps.
    /// </summary>
    public void ResetAllTiles()
    {
        walkableTilemap?.ClearAllTiles();
        wallTilemap?.ClearAllTiles();
    }

    /// <summary>
    /// Converts the state of a Tilemap to a list of SerializableTile.
    /// </summary>
    /// <param name="tilemap">Tilemap to convert.</param>
    /// <returns>List of SerializableTile representing the state of the Tilemap.</returns>
    private static List<SerializableTile> GetSerializableTiles(Tilemap tilemap)
    {
        var list = new List<SerializableTile>();
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(pos);
            if (!tile) continue;
            list.Add(new SerializableTile(pos, tile.name));
        }

        return list;
    }

    /// <summary>
    /// Saves the current state of both Tilemaps to a file.
    /// </summary>
    /// <param name="path">Path to save the file.</param>
    public void SaveTilemap(string path)
    {
        var tilemapData = new TilemapData(GetSerializableTiles(walkableTilemap), GetSerializableTiles(wallTilemap));
        var json = JsonUtility.ToJson(tilemapData);
        System.IO.File.WriteAllText(path, json);
    }

    /// <summary>
    /// Loads the state of both Tilemaps from a file.
    /// </summary>
    /// <param name="path">Path to load the file from.</param>
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
    /// Finds a TileBase by its name in the specified directory.
    /// </summary>
    /// <param name="tileName">Name of the tile to find.</param>
    /// <returns>TileBase with the specified name.</returns>
    private static TileBase GetTileBaseByName(string tileName)
    {
        var guids = AssetDatabase.FindAssets(tileName, new[] { "Assets/Assets/TilemapsDungeonTilesetil" });
        return guids.Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<TileBase>)
            .FirstOrDefault(tile => tile && tile.name == tileName);
    }

    #endregion

    #region Tile Collections Management

    /// <summary>
    /// Helper method to remove a tile (and its priority) from a collection.
    /// </summary>
    /// <param name="index">Index of the tile to remove.</param>
    /// <param name="tileBases">List of tile bases.</param>
    /// <param name="priorities">List of priorities corresponding to the tiles.</param>
    /// <param name="probabilities">Dictionary of tiles and their probabilities.</param>
    private static void RemoveTileAtIndex(int index, List<TileBase> tileBases, List<int> priorities,
        Dictionary<TileBase, float> probabilities)
    {
        if (index < 0 || index >= tileBases.Count)
        {
            Debug.LogWarning("Index out of range.");
            return;
        }

        var tile = tileBases[index];

        if (tile)
        {
            probabilities.Remove(tile);
        }

        tileBases.RemoveAt(index);
        priorities.RemoveAt(index);
    }

    /// <summary>
    /// Removes a tile (by index) from the corresponding collection.
    /// </summary>
    /// <param name="position">Index of the tile to remove.</param>
    /// <param name="isWalkable">Indicates if the tile is walkable.</param>
    public void RemoveTileAtPosition(int position, bool isWalkable)
    {
        if (isWalkable)
        {
            RemoveTileAtIndex(position, walkableTileBases, walkableTilesPriorities, _walkableTilesProbabilities);
        }
        else
        {
            // RemoveTileAtIndex(position, wallTileBases, wallTilesPriorities, _wallTilesProbabilities);
        }
    }

    /// <summary>
    /// Helper method to clear the tile collections.
    /// </summary>
    /// <param name="tileBases">List of tile bases.</param>
    /// <param name="priorities">List of priorities corresponding to the tiles.</param>
    /// <param name="probabilities">Dictionary of tiles and their probabilities.</param>
    private static void ClearTileCollections(List<TileBase> tileBases, List<int> priorities,
        Dictionary<TileBase, float> probabilities)
    {
        tileBases.Clear();
        priorities.Clear();
        probabilities.Clear();
    }

    /// <summary>
    /// Removes all walkable tiles from the collection.
    /// </summary>
    public void RemoveAllWalkableTiles() =>
        ClearTileCollections(walkableTileBases, walkableTilesPriorities, _walkableTilesProbabilities);

    /// <summary>
    /// Removes all wall tiles from the collection.
    /// </summary>
    // public void RemoveAllWallTiles() =>
    //     ClearTileCollections(wallTileBases, wallTilesPriorities, _wallTilesProbabilities);

    /// <summary>
    /// Loads tiles from .asset files located in a directory, updating the corresponding collection.
    /// </summary>
    /// <param name="tileBases">List of tile bases to update.</param>
    /// <param name="priorities">List of priorities to update.</param>
    /// <param name="probabilities">Dictionary of tiles and their probabilities to update.</param>
    /// <param name="path">Path to the directory containing the .asset files.</param>
    private static void SelectTilesFromFolder(List<TileBase> tileBases, List<int> priorities,
        Dictionary<TileBase, float> probabilities, string path)
    {
        tileBases.Clear();
        priorities.Clear();
        probabilities.Clear();

        var files = System.IO.Directory.GetFiles(path, "*.asset");

        foreach (var file in files)
        {
            var relativePath = "Assets" + file.Replace(Application.dataPath, "").Replace('\\', '/');
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(relativePath);
            if (!tile)
                continue;

            tileBases.Add(tile);
            priorities.Add(0);
        }
    }

    /// <summary>
    /// Selects tiles from a folder, differentiating between floor and wall tiles.
    /// </summary>
    /// <param name="floorTiles">Indicates if the tiles are floor tiles.</param>
    /// <param name="path">Path to the folder containing the tiles.</param>
    public void SelectFromFolder(bool floorTiles, string path)
    {
        if (floorTiles)
        {
            SelectTilesFromFolder(walkableTileBases, walkableTilesPriorities, _walkableTilesProbabilities, path);
        }
        else
        {
            // SelectTilesFromFolder(wallTileBases, wallTilesPriorities, _wallTilesProbabilities, path);
        }
    }

    #endregion
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
    /// <param name="walkableTiles">List of walkable tiles.</param>
    /// <param name="wallTiles">List of wall tiles.</param>
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
    /// <param name="position">Position of the tile.</param>
    /// <param name="tileName">Name of the tile.</param>
    public SerializableTile(Vector3Int position, string tileName)
    {
        this.position = position;
        this.tileName = tileName;
    }
}