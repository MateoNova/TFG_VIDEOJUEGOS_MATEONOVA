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
    # region Fields: Walkable Tiles

    /// <summary>
    /// The Tilemap used to render walkable tiles.
    /// </summary>
    [SerializeField] private Tilemap walkableTilemap;

    /// <summary>
    /// List of walkable tile bases. This allows for multiple walkable tiles to be used.
    /// </summary>
    [SerializeField, WalkableTileGroup(isTileBases: true)]
    private List<TileBase> walkableTileBases = new();

    /// <summary>
    /// List of priorities corresponding to the walkable tiles. The higher the priority, the more likely the tile will be chosen.
    /// </summary>
    [WalkableTileGroup(isTilePriorities: true), ConditionalField("randomWalkableTilesPlacement"), SerializeField]
    private List<int> walkableTilesPriorities = new();

    /// <summary>
    /// Indicates if the walkable tiles should be placed randomly.
    /// </summary>
    [SerializeField] public bool randomWalkableTilesPlacement;

    /// <summary>
    /// Dictionary of walkable tiles and their probabilities. This is used to select tiles based on their assigned probabilities.
    /// </summary>
    private Dictionary<TileBase, float> _walkableTilesProbabilities = new();

    # endregion

    #region Fields: Wall Tiles

    /// <summary>
    /// The Tilemap used to render wall tiles.
    /// </summary>
    [SerializeField] private Tilemap wallTilemap;

    [SerializeField, WallTileGroup("Cardinal Directions")]
    private TileBase upWall;

    [SerializeField, WallTileGroup("Cardinal Directions")]
    private TileBase downWall;

    [SerializeField, WallTileGroup("Cardinal Directions")]
    private TileBase leftWall;

    [SerializeField, WallTileGroup("Cardinal Directions")]
    private TileBase rightWall;

    [SerializeField, WallTileGroup("External Corners")]
    private TileBase topLeftWall;

    [SerializeField, WallTileGroup("External Corners")]
    private TileBase topRightWall;

    [SerializeField, WallTileGroup("External Corners")]
    private TileBase bottomLeftWall;

    [SerializeField, WallTileGroup("External Corners")]
    private TileBase bottomRightWall;

    [SerializeField, WallTileGroup("External Corners")]
    private TileBase allCornersWall;

    [SerializeField, WallTileGroup("Inner Corners")]
    private TileBase topLeftInnerWall;

    [SerializeField, WallTileGroup("Inner Corners")]
    private TileBase topRightInnerWall;

    [SerializeField, WallTileGroup("Inner Corners")]
    private TileBase bottomLeftInnerWall;

    [SerializeField, WallTileGroup("Inner Corners")]
    private TileBase bottomRightInnerWall;

    [SerializeField, WallTileGroup("Triple Walls")]
    private TileBase tripleExceptUpWall;

    [SerializeField, WallTileGroup("Triple Walls")]
    private TileBase tripleExcetDownWall;

    [SerializeField, WallTileGroup("Triple Walls")]
    private TileBase tripleExceptLeftWall;

    [SerializeField, WallTileGroup("Triple Walls")]
    private TileBase tripleExceptRightWall;

    [SerializeField, WallTileGroup("Triple Inner Walls")]
    private TileBase tripleExceptLeftInnerWall;

    [SerializeField, WallTileGroup("Triple Inner Walls")]
    private TileBase tripleExceptRightInnerWall;

    [SerializeField, WallTileGroup("Alone Walls")]
    private TileBase aloneWall;

    #endregion

    # region Initialization: Walkable Tiles

    /// <summary>
    /// Initializes the probabilities for walkable tiles.
    /// </summary>
    private void InitializeWalkableTilesProbabilities() =>
        _walkableTilesProbabilities = InitializeProbabilities(walkableTileBases, walkableTilesPriorities);

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

    # endregion

    # region Paint: Walkable Tiles

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

    # endregion

    # region Paint: Wall Tiles

    /// <summary>
    /// Renders the wall tiles at the specified positions.
    /// </summary>
    /// <param name="tilesPositions">Positions to render the wall tiles.</param>
    /// <param name="position"> Position of the wall tile.</param>
    public void PaintWallTiles(IEnumerable<Vector2Int> tilesPositions, Utils.WallPosition position)
    {
        var tile = position switch
        {
            Utils.WallPosition.Up => upWall,
            Utils.WallPosition.Down => downWall,
            Utils.WallPosition.Left => leftWall,
            Utils.WallPosition.Right => rightWall,
            Utils.WallPosition.TopLeft => topLeftWall,
            Utils.WallPosition.BottomLeft => bottomLeftWall,
            Utils.WallPosition.TopRight => topRightWall,
            Utils.WallPosition.BottomRight => bottomRightWall,

            Utils.WallPosition.TripleExceptUp => tripleExceptUpWall,
            Utils.WallPosition.TripleExceptDown => tripleExcetDownWall,
            Utils.WallPosition.TripleExceptLeft => tripleExceptLeftWall,
            Utils.WallPosition.TripleExceptRight => tripleExceptRightWall,
            Utils.WallPosition.AllWallCorner => allCornersWall,
            Utils.WallPosition.TopLeftInner => topLeftInnerWall,
            Utils.WallPosition.TopRightInner => topRightInnerWall,
            Utils.WallPosition.BottomLeftInner => bottomLeftInnerWall,
            Utils.WallPosition.BottomRightInner => bottomRightInnerWall,
            Utils.WallPosition.Alone => aloneWall,
            Utils.WallPosition.TripleExceptLeftInner => tripleExceptLeftInnerWall,
            Utils.WallPosition.TripleExceptRightInner => tripleExceptRightInnerWall,
            _ => null
        };

        if (!tile) return;

        foreach (var pos in tilesPositions)
        {
            var tilePosition = wallTilemap.WorldToCell((Vector3Int)pos);
            wallTilemap.SetTile(tilePosition, tile);
        }
    }

    # endregion

    public TilemapPainter(bool randomWalkableTilesPlacement)
    {
        this.randomWalkableTilesPlacement = randomWalkableTilesPlacement;
    }


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
            var assetPath = AssetDatabase.GetAssetPath(tile);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            list.Add(new SerializableTile(pos, guid));
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
            var tileBase = GetTileBaseByGuid(tile.tileGUID);
            walkableTilemap.SetTile(tile.position, tileBase);
        }

        foreach (var tile in tilemapData.wallTiles)
        {
            var tileBase = GetTileBaseByGuid(tile.tileGUID);
            wallTilemap.SetTile(tile.position, tileBase);
        }
    }

    private static TileBase GetTileBaseByGuid(string guid)
    {
        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        return AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
    }

    #endregion

    #region Tile Collections Management

    /// <summary>
    /// Helper method to clear the tile collections.
    /// </summary>
    /// <param name="tileBases">List of tile bases.</param>
    /// <param name="priorities">List of priorities corresponding to the tiles.</param>
    /// <param name="probabilities">Dictionary of tiles and their probabilities.</param>
    private static void ClearTileCollections(List<TileBase> tileBases, List<int> priorities,
        Dictionary<TileBase, float> probabilities)
    {
        tileBases?.Clear();
        priorities?.Clear();
        probabilities?.Clear();
    }

    /// <summary>
    /// Removes all walkable tiles from the collection.
    /// </summary>
    public void RemoveAllWalkableTiles() =>
        ClearTileCollections(walkableTileBases, walkableTilesPriorities, _walkableTilesProbabilities);

    /// <summary>
    /// Removes all wall tiles from the collection.
    /// </summary>
    public void RemoveAllWallTiles()
    {
        upWall = null;
        downWall = null;
        leftWall = null;
        rightWall = null;
        topLeftWall = null;
        topRightWall = null;
        bottomLeftWall = null;
        bottomRightWall = null;
        allCornersWall = null;
        topLeftInnerWall = null;
        topRightInnerWall = null;
        bottomLeftInnerWall = null;
        bottomRightInnerWall = null;
        tripleExceptUpWall = null;
        tripleExcetDownWall = null;
        tripleExceptLeftWall = null;
        tripleExceptRightWall = null;
        aloneWall = null;
    }

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
        tileBases ??= new List<TileBase>();
        priorities ??= new List<int>();
        probabilities ??= new Dictionary<TileBase, float>();

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
    /// <param name="path">Path to the folder containing the tiles.</param>
    public void SelectWalkableTilesFromFolder(string path)
    {
        SelectTilesFromFolder(walkableTileBases, walkableTilesPriorities, _walkableTilesProbabilities, path);
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
    public string tileGUID;

    public SerializableTile(Vector3Int position, string tileGUID)
    {
        this.position = position;
        this.tileGUID = tileGUID;
    }
}