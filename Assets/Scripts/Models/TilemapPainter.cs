using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
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
            //randomWalkableTilesPlacement = randomPlacement;
        }

        [SerializeField] private List<TilesetPreset> _tilesetPresets = new();
        private int _tilesetPresetIndex = 0;

        #region Walkable Tiles

        [SerializeField] public Tilemap walkableTilemap;

        private Dictionary<TileBase, float> _walkableTilesProbabilities = new();

        #endregion

        [SerializeField] public Tilemap wallTilemap;

        #region Door Tiles

        [SerializeField] public Tilemap doorTilemap;
        [SerializeField] private TileBase doorClosed;
        [SerializeField] private TileBase doorOpen;

        #endregion

        #region Initialization Helpers

        /// <summary>
        /// Initializes the probabilities for walkable tiles based on their priorities.
        /// </summary>
        /// <param name="preset"></param>
        private void InitializeWalkableTilesProbabilities(TilesetPreset preset)
        {
            _walkableTilesProbabilities = new Dictionary<TileBase, float>();
            var totalPriority = preset.walkableTilesPriorities.Sum();
            for (var i = 0; i < preset.walkableTileBases.Count; i++)
            {
                var prob = (i < preset.walkableTilesPriorities.Count && totalPriority != 0)
                    ? (float)preset.walkableTilesPriorities[i] / totalPriority
                    : 0f;
                if (i >= preset.walkableTilesPriorities.Count)
                    Debug.LogWarning($"No priority defined for tile at index {i}. Defaulting to 0.");
                _walkableTilesProbabilities[preset.walkableTileBases[i]] = prob;
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
            var preset = _tilesetPresets[_tilesetPresetIndex];
            InitializeWalkableTilesProbabilities(preset);
            var positions = tilePositions.ToList();
            var cellPositions = GetCellPositions(positions, walkableTilemap);

            if (preset.randomWalkableTilesPlacement)
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
            var preset = _tilesetPresets[_tilesetPresetIndex];
            var rnd = new System.Random();
            foreach (var (_, cellPos) in cellPositions)
            {
                var tile = preset.walkableTileBases[rnd.Next(preset.walkableTileBases.Count)];
                walkableTilemap.SetTile(cellPos, tile);
            }
        }

        /// <summary>
        /// Paints walkable tiles based on their probabilities.
        /// </summary>
        /// <param name="cellPositions">The cell positions to paint.</param>
        private void PaintTilesWithProbabilities(List<(Vector2Int worldPos, Vector3Int cellPos)> cellPositions)
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            var total = _walkableTilesProbabilities.Values.Sum();
            List<(TileBase tile, float cumulative)> cumulativeList = new();
            var accumulator = 0f;
            foreach (var tile in preset.walkableTileBases)
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
            var preset = _tilesetPresets[_tilesetPresetIndex];
            var tile = wallPosition switch
            {
                Utils.Utils.WallPosition.Up => preset.upWall,
                Utils.Utils.WallPosition.Down => preset.downWall,
                Utils.Utils.WallPosition.Left => preset.leftWall,
                Utils.Utils.WallPosition.Right => preset.rightWall,
                Utils.Utils.WallPosition.TopLeft => preset.topLeftWall,
                Utils.Utils.WallPosition.BottomLeft => preset.bottomLeftWall,
                Utils.Utils.WallPosition.TopRight => preset.topRightWall,
                Utils.Utils.WallPosition.BottomRight => preset.bottomRightWall,
                Utils.Utils.WallPosition.TripleExceptUp => preset.tripleExceptUpWall,
                Utils.Utils.WallPosition.TripleExceptDown => preset.tripleExcetDownWall,
                Utils.Utils.WallPosition.TripleExceptLeft => preset.tripleExceptLeftWall,
                Utils.Utils.WallPosition.TripleExceptRight => preset.tripleExceptRightWall,
                Utils.Utils.WallPosition.AllWallCorner => preset.allCornersWall,
                Utils.Utils.WallPosition.TopLeftInner => preset.topLeftInnerWall,
                Utils.Utils.WallPosition.TopRightInner => preset.topRightInnerWall,
                Utils.Utils.WallPosition.BottomLeftInner => preset.bottomLeftInnerWall,
                Utils.Utils.WallPosition.BottomRightInner => preset.bottomRightInnerWall,
                Utils.Utils.WallPosition.Alone => preset.aloneWall,
                Utils.Utils.WallPosition.TripleExceptLeftInner => preset.tripleExceptLeftInnerWall,
                Utils.Utils.WallPosition.TripleExceptRightInner => preset.tripleExceptRightInnerWall,
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
                doorTilemap.SetTile(cellPos, doorClosed);
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
            var preset = _tilesetPresets[_tilesetPresetIndex];
            SelectTilesFromFolder(preset.walkableTileBases, preset.walkableTilesPriorities, _walkableTilesProbabilities,
                path);
        }

        #endregion

        #region Tile Collections Clearing

        /// <summary>
        /// Clears all walkable tile collections and the associated tilemap.
        /// </summary>
        public void RemoveAllWalkableTiles()
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            preset.walkableTileBases.Clear();
            preset.walkableTilesPriorities.Clear();
            _walkableTilesProbabilities.Clear();
            walkableTilemap?.ClearAllTiles();
        }

        /// <summary>
        /// Clears all wall tile references and the associated tilemap.
        /// </summary>
        public void RemoveAllWallTiles()
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            preset.upWall = null;
            preset.downWall = null;
            preset.leftWall = null;
            preset.rightWall = null;
            preset.topLeftWall = null;
            preset.topRightWall = null;
            preset.bottomLeftWall = null;
            preset.bottomRightWall = null;
            preset.allCornersWall = null;
            preset.topLeftInnerWall = null;
            preset.topRightInnerWall = null;
            preset.bottomLeftInnerWall = null;
            preset.bottomRightInnerWall = null;
            preset.tripleExceptUpWall = null;
            preset.tripleExcetDownWall = null;
            preset.tripleExceptLeftWall = null;
            preset.tripleExceptRightWall = null;
            preset.tripleExceptLeftInnerWall = null;
            preset.tripleExceptRightInnerWall = null;
            preset.aloneWall = null;

            wallTilemap?.ClearAllTiles();
        }

        #endregion

        public List<TileBase> GetWalkableTileBases()
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            return preset.walkableTileBases;
        }

        public List<int> GetWalkableTilesPriorities()
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            return preset.walkableTilesPriorities;
        }

        public bool GetRandomWalkableTilesPlacement()
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            return preset.randomWalkableTilesPlacement;
        }

        public void SetRandomWalkableTilesPlacement(bool evtNewValue)
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            if (preset.randomWalkableTilesPlacement == evtNewValue) return;
            preset.randomWalkableTilesPlacement = evtNewValue;
            InitializeWalkableTilesProbabilities(preset);
        }

        public void SetWalkableTileBases(int index, TileBase newTile)
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            if (index < 0 || index >= preset.walkableTileBases.Count)
            {
                Debug.LogError($"Index {index} is out of range for walkable tile bases.");
                return;
            }

            if (preset.walkableTileBases[index] == newTile) return;
            preset.walkableTileBases[index] = newTile;
            InitializeWalkableTilesProbabilities(preset);
        }

        public void ClearWalkableTileBases()
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            preset.walkableTileBases.Clear();
        }

        public void ClearWalkableTilesPriorities()
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            preset.walkableTilesPriorities.Clear();
        }

        public void AddTileWalkableTileBases(Tile tile)
        {
            var preset = _tilesetPresets[_tilesetPresetIndex];
            if (preset.walkableTileBases.Contains(tile))
            {
                Debug.LogWarning($"Tile {tile.name} already exists in walkable tile bases.");
                return;
            }

            preset.walkableTileBases.Add(tile);
            preset.walkableTilesPriorities.Add(1);
        }

        public TilesetPreset GetCurrentTilesetPreset()
        {
            if (_tilesetPresets != null && _tilesetPresets.Count != 0) return _tilesetPresets[_tilesetPresetIndex];
            Debug.LogError("No tileset presets available.");
            return null;
        }
    }
}