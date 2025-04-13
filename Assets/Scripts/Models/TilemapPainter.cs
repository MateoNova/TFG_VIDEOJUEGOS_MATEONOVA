using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Views.Attributes;

namespace Models
{
    /// <summary>
    /// Interface defining the contract for a TilemapPainter.
    /// </summary>
    public interface ITilemapPainter
    {
        /// <summary>
        /// Paints walkable tiles on the tilemap.
        /// </summary>
        /// <param name="tilePositions">The positions of the tiles to paint.</param>
        void PaintWalkableTiles(IEnumerable<Vector2Int> tilePositions);

        /// <summary>
        /// Paints wall tiles on the tilemap.
        /// </summary>
        /// <param name="tilePositions">The positions of the tiles to paint.</param>
        /// <param name="wallPosition">The type of wall to paint.</param>
        void PaintWallTiles(IEnumerable<Vector2Int> tilePositions, Utils.Utils.WallPosition wallPosition);

        /// <summary>
        /// Paints door tiles on the tilemap.
        /// </summary>
        /// <param name="tilePositions">The positions of the tiles to paint.</param>
        void PaintDoorTiles(IEnumerable<Vector2Int> tilePositions);

        /// <summary>
        /// Resets all tiles on the tilemap.
        /// </summary>
        void ResetAllTiles();
    }

    /// <summary>
    /// A class responsible for managing and painting tiles on a tilemap.
    /// </summary>
    public class TilemapPainter : MonoBehaviour, ITilemapPainter
    {
        /// <summary>
        /// Constructor for TilemapPainter.
        /// </summary>
        /// <param name="randomPlacement">Determines if walkable tiles should be placed randomly.</param>
        public TilemapPainter(bool randomPlacement)
        {
            randomWalkableTilesPlacement = randomPlacement;
        }

        #region Walkable Tiles

        [SerializeField] public Tilemap walkableTilemap;
        [SerializeField] public List<TileBase> walkableTileBases = new();
        [SerializeField] public List<int> walkableTilesPriorities = new();
        [SerializeField] public bool randomWalkableTilesPlacement;
        private Dictionary<TileBase, float> _walkableTilesProbabilities = new();

        #endregion

        #region Wall Tiles

        [SerializeField] public Tilemap wallTilemap; // Tilemap for wall tiles.

        // Various wall tile types categorized by their positions.
        [SerializeField, WallTileGroup("CardinalDirections")]
        private TileBase upWall;

        [SerializeField, WallTileGroup("CardinalDirections")]
        private TileBase downWall;

        [SerializeField, WallTileGroup("CardinalDirections")]
        private TileBase leftWall;

        [SerializeField, WallTileGroup("CardinalDirections")]
        private TileBase rightWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        private TileBase topLeftWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        private TileBase topRightWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        private TileBase bottomLeftWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        private TileBase bottomRightWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        private TileBase allCornersWall;

        [SerializeField, WallTileGroup("InnerCorners")]
        private TileBase topLeftInnerWall;

        [SerializeField, WallTileGroup("InnerCorners")]
        private TileBase topRightInnerWall;

        [SerializeField, WallTileGroup("InnerCorners")]
        private TileBase bottomLeftInnerWall;

        [SerializeField, WallTileGroup("InnerCorners")]
        private TileBase bottomRightInnerWall;

        [SerializeField, WallTileGroup("TripleWalls")]
        private TileBase tripleExceptUpWall;

        [SerializeField, WallTileGroup("TripleWalls")]
        private TileBase tripleExcetDownWall;

        [SerializeField, WallTileGroup("TripleWalls")]
        private TileBase tripleExceptLeftWall;

        [SerializeField, WallTileGroup("TripleWalls")]
        private TileBase tripleExceptRightWall;

        [SerializeField, WallTileGroup("TripleInnerWalls")]
        private TileBase tripleExceptLeftInnerWall;

        [SerializeField, WallTileGroup("TripleInnerWalls")]
        private TileBase tripleExceptRightInnerWall;

        [SerializeField, WallTileGroup("AloneWalls")]
        private TileBase aloneWall;

        #endregion

        #region Door Tiles

        [SerializeField] public Tilemap doorTilemap;
        [SerializeField] private TileBase doorTileBase;

        #endregion

        #region Initialization Helpers

        /// <summary>
        /// Initializes the probabilities for walkable tiles based on their priorities.
        /// </summary>
        private void InitializeWalkableTilesProbabilities()
        {
            _walkableTilesProbabilities = new Dictionary<TileBase, float>();
            var totalPriority = walkableTilesPriorities.Sum();
            for (var i = 0; i < walkableTileBases.Count; i++)
            {
                var prob = (i < walkableTilesPriorities.Count && totalPriority != 0)
                    ? (float)walkableTilesPriorities[i] / totalPriority
                    : 0f;
                if (i >= walkableTilesPriorities.Count)
                    Debug.LogWarning($"No priority defined for tile at index {i}. Defaulting to 0.");
                _walkableTilesProbabilities[walkableTileBases[i]] = prob;
            }
        }

        /// <summary>
        /// Converts world positions to cell positions for a given tilemap.
        /// </summary>
        /// <param name="positions">World positions to convert.</param>
        /// <param name="tilemap">The tilemap to use for conversion.</param>
        /// <returns>A list of tuples containing world and cell positions.</returns>
        private List<(Vector2Int worldPos, Vector3Int cellPos)> GetCellPositions(IEnumerable<Vector2Int> positions,
            Tilemap tilemap)
        {
            var list = new List<(Vector2Int, Vector3Int)>();
            foreach (var pos in positions)
            {
                var cellPos = tilemap.WorldToCell(new Vector3Int(pos.x, pos.y, 0));
                list.Add((pos, cellPos));
            }

            return list;
        }

        #endregion

        #region Painting Tiles

        /// <summary>
        /// Paints walkable tiles on the tilemap.
        /// </summary>
        /// <param name="tilePositions">The positions of the tiles to paint.</param>
        public void PaintWalkableTiles(IEnumerable<Vector2Int> tilePositions)
        {
            InitializeWalkableTilesProbabilities();
            var positions = tilePositions.ToList();
            var cellPositions = GetCellPositions(positions, walkableTilemap);

            if (randomWalkableTilesPlacement)
                PaintTilesRandomly(cellPositions);
            else
                PaintTilesWithProbabilities(cellPositions);

            Debug.Log($"Number of walkable tiles painted: {positions.Count}");
        }

        /// <summary>
        /// Paints walkable tiles randomly.
        /// </summary>
        /// <param name="cellPositions">The cell positions to paint.</param>
        private void PaintTilesRandomly(List<(Vector2Int worldPos, Vector3Int cellPos)> cellPositions)
        {
            var rnd = new System.Random();
            foreach (var (_, cellPos) in cellPositions)
            {
                var tile = walkableTileBases[rnd.Next(walkableTileBases.Count)];
                walkableTilemap.SetTile(cellPos, tile);
            }
        }

        /// <summary>
        /// Paints walkable tiles based on their probabilities.
        /// </summary>
        /// <param name="cellPositions">The cell positions to paint.</param>
        private void PaintTilesWithProbabilities(List<(Vector2Int worldPos, Vector3Int cellPos)> cellPositions)
        {
            var total = _walkableTilesProbabilities.Values.Sum();
            List<(TileBase tile, float cumulative)> cumulativeList = new();
            var accumulator = 0f;
            foreach (var tile in walkableTileBases)
            {
                if (!_walkableTilesProbabilities.TryGetValue(tile, out var prob))
                {
                    Debug.LogError($"Probability for tile {tile.name} is not set.");
                    continue;
                }

                accumulator += prob / total;
                cumulativeList.Add((tile, accumulator));
            }

            var rnd = new System.Random();
            foreach (var (_, cellPos) in cellPositions)
            {
                var randomValue = (float)rnd.NextDouble();
                foreach (var (tile, cumulative) in cumulativeList)
                {
                    if (!(randomValue <= cumulative)) continue;
                    
                    walkableTilemap.SetTile(cellPos, tile);
                    break;
                }
            }
        }

        /// <summary>
        /// Paints wall tiles on the tilemap.
        /// </summary>
        /// <param name="tilePositions">The positions of the tiles to paint.</param>
        /// <param name="wallPosition">The type of wall to paint.</param>
        public void PaintWallTiles(IEnumerable<Vector2Int> tilePositions, Utils.Utils.WallPosition wallPosition)
        {
            var tile = wallPosition switch
            {
                Utils.Utils.WallPosition.Up => upWall,
                Utils.Utils.WallPosition.Down => downWall,
                Utils.Utils.WallPosition.Left => leftWall,
                Utils.Utils.WallPosition.Right => rightWall,
                Utils.Utils.WallPosition.TopLeft => topLeftWall,
                Utils.Utils.WallPosition.BottomLeft => bottomLeftWall,
                Utils.Utils.WallPosition.TopRight => topRightWall,
                Utils.Utils.WallPosition.BottomRight => bottomRightWall,
                Utils.Utils.WallPosition.TripleExceptUp => tripleExceptUpWall,
                Utils.Utils.WallPosition.TripleExceptDown => tripleExcetDownWall,
                Utils.Utils.WallPosition.TripleExceptLeft => tripleExceptLeftWall,
                Utils.Utils.WallPosition.TripleExceptRight => tripleExceptRightWall,
                Utils.Utils.WallPosition.AllWallCorner => allCornersWall,
                Utils.Utils.WallPosition.TopLeftInner => topLeftInnerWall,
                Utils.Utils.WallPosition.TopRightInner => topRightInnerWall,
                Utils.Utils.WallPosition.BottomLeftInner => bottomLeftInnerWall,
                Utils.Utils.WallPosition.BottomRightInner => bottomRightInnerWall,
                Utils.Utils.WallPosition.Alone => aloneWall,
                Utils.Utils.WallPosition.TripleExceptLeftInner => tripleExceptLeftInnerWall,
                Utils.Utils.WallPosition.TripleExceptRightInner => tripleExceptRightInnerWall,
                _ => null
            };

            if (tile == null)
            {
                Debug.LogWarning($"No tile defined for wall position: {wallPosition}");
                return;
            }

            var cellPositions = GetCellPositions(tilePositions, wallTilemap);
            foreach (var (_, cellPos) in cellPositions)
            {
                wallTilemap.SetTile(cellPos, tile);
            }
        }

        /// <summary>
        /// Paints door tiles on the tilemap.
        /// </summary>
        /// <param name="tilePositions">The positions of the tiles to paint.</param>
        public void PaintDoorTiles(IEnumerable<Vector2Int> tilePositions)
        {
            var cellPositions = GetCellPositions(tilePositions, doorTilemap);
            foreach (var (_, cellPos) in cellPositions)
            {
                doorTilemap.SetTile(cellPos, doorTileBase);
            }
        }

        #endregion

        #region Reset Tiles

        /// <summary>
        /// Clears all tiles from the tilemaps.
        /// </summary>
        public void ResetAllTiles()
        {
            walkableTilemap?.ClearAllTiles();
            wallTilemap?.ClearAllTiles();
            doorTilemap?.ClearAllTiles();
        }

        #endregion

        #region Optional: Tile Selection from Folder

        /// <summary>
        /// Selects tiles from a folder and populates the provided collections.
        /// </summary>
        /// <param name="tileBases">The list to store tile bases.</param>
        /// <param name="priorities">The list to store tile priorities.</param>
        /// <param name="probabilities">The dictionary to store tile probabilities.</param>
        /// <param name="path">The folder path to search for tiles.</param>
        private static void SelectTilesFromFolder(List<TileBase> tileBases, List<int> priorities,
            Dictionary<TileBase, float> probabilities, string path)
        {
            tileBases.Clear();
            priorities.Clear();
            probabilities.Clear();

            var files = System.IO.Directory.GetFiles(path, "*.asset");
            foreach (var file in files)
            {
                var relPath = "Assets" + file.Replace(Application.dataPath, "").Replace('\\', '/');
                var tile = UnityEditor.AssetDatabase.LoadAssetAtPath<TileBase>(relPath);
                if (tile == null) continue;
                tileBases.Add(tile);
                priorities.Add(0);
            }
        }

        /// <summary>
        /// Selects walkable tiles from a folder.
        /// </summary>
        /// <param name="path">The folder path to search for tiles.</param>
        public void SelectWalkableTilesFromFolder(string path)
        {
            SelectTilesFromFolder(walkableTileBases, walkableTilesPriorities, _walkableTilesProbabilities, path);
        }

        #endregion

        #region Tile Collections Clearing

        /// <summary>
        /// Clears all walkable tile collections and the associated tilemap.
        /// </summary>
        public void RemoveAllWalkableTiles()
        {
            walkableTileBases.Clear();
            walkableTilesPriorities.Clear();
            _walkableTilesProbabilities.Clear();
            walkableTilemap?.ClearAllTiles();
        }

        /// <summary>
        /// Clears all wall tile references and the associated tilemap.
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
            tripleExceptLeftInnerWall = null;
            tripleExceptRightInnerWall = null;
            aloneWall = null;

            wallTilemap?.ClearAllTiles();
        }

        #endregion
    }
}