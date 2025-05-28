using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Views.Attributes;

namespace Models
{
    /// <summary>
    /// Interface for painting tiles on a tilemap.
    /// </summary>
    public interface ITilemapPainter
    {
        void PaintWalkableTiles(IEnumerable<Vector2Int> tilePositions);


        void PaintWallTiles(IEnumerable<Vector2Int> tilePositions, Utils.Utils.WallPosition wallPosition);


        void PaintDoorTiles(IEnumerable<Vector2Int> tilePositions);


        void ResetAllTiles();
    }

    /// <summary>
    /// A component that handles painting tiles on tilemaps, including walkable tiles, wall tiles, and door tiles.
    /// It supports multiple tileset presets and allows for random or probability-based tile placement.
    /// </summary>
    public class TilemapPainter : MonoBehaviour, ITilemapPainter
    {
        #region fields

        [SerializeField] private List<TilesetPreset> tilesetPresets = new();
        [SerializeField] private List<float> presetCoverages = new();

        [SerializeField] public Tilemap walkableTilemap;
        [SerializeField] public Tilemap wallTilemap;
        [SerializeField] public Tilemap doorTilemap;

        private Dictionary<TileBase, float> _walkableTilesProbabilities = new();

        private int _tilesetPresetIndex = -1; // The index of the current tileset preset is none

        # endregion

        #region Initialization Helpers

        /// <summary>
        /// Initializes the TilemapPainter with the provided tilePreset.
        /// </summary>
        /// <param name="preset">The tileset preset to initialize with.</param>
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
        /// Converts a collection of 2D world positions to their corresponding cell positions in a tilemap.
        /// </summary>
        /// <param name="positions">The world positions to convert.</param>
        /// <param name="tilemap">The tilemap used for conversion.</param>
        /// <returns>
        /// A list of tuples, each containing the original world position and its corresponding cell position.
        /// </returns>
        private static List<(Vector2Int worldPos, Vector3Int cellPos)> GetCellPositions(
            IEnumerable<Vector2Int> positions,
            Tilemap tilemap)
        {
            return (from pos in positions
                let cellPos = tilemap.WorldToCell(new Vector3Int(pos.x, pos.y, 0))
                select (pos, cellPos)).ToList();
        }

        #endregion

        #region Painting Tiles

        /// <summary>
        /// Paints walkable tiles on the walkable tilemap at the specified positions.
        /// The tiles are chosen either randomly or based on defined probabilities,
        /// depending on the current tileset preset configuration.
        /// </summary>
        /// <param name="tilePositions">The world positions where walkable tiles should be painted.</param>
        public void PaintWalkableTiles(IEnumerable<Vector2Int> tilePositions)
        {
            var preset = GetCurrentTilesetPreset();
            if (preset == null || preset.walkableTileBases.Count == 0)
                return;

            InitializeWalkableTilesProbabilities(preset);
            var positions = tilePositions.ToList();
            var cellPositions = GetCellPositions(positions, walkableTilemap)
                .Select(t => t.cellPos)
                .ToArray();

            if (preset.randomWalkableTilesPlacement)
                PaintTilesRandomly(cellPositions);
            else
                PaintTilesWithProbabilities(cellPositions);
        }

        /// <summary>
        /// Paints walkable tiles at the given cell positions, selecting a random tile
        /// from the preset's walkable tiles for each position.
        /// </summary>
        /// <param name="cellPositions">
        /// A list of tuples containing the original world position and the corresponding cell position.
        /// </param>
        private void PaintTilesRandomly(Vector3Int[] cellPositions)
        {
            var preset = GetCurrentTilesetPreset();
            if (preset == null || preset.walkableTileBases.Count == 0) return;
            
            var rnd = new System.Random();
            var tile = new TileBase[cellPositions.Length];
            for (var index = 0; index < cellPositions.Length; index++)
            {
                tile[index] = preset.walkableTileBases[rnd.Next(preset.walkableTileBases.Count)];
            }
            
            walkableTilemap.SetTiles(cellPositions, tile);
        }

        /// <summary>
        /// Paints walkable tiles at the given cell positions, selecting tiles based on
        /// their defined probabilities in the current preset.
        /// </summary>
        /// <param name="cellPositions">
        /// A list of tuples containing the original world position and the corresponding cell position.
        /// </param>
        private void PaintTilesWithProbabilities(Vector3Int[] cellPositions)
        {
            var preset = GetCurrentTilesetPreset();
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
            var cellArray = new Vector3Int[cellPositions.Length];
            var tileArray = new TileBase[cellPositions.Length];
        
            for (var i = 0; i < cellPositions.Length; i++)
            {
                var cellPos = cellPositions[i];
                var randomValue = (float)rnd.NextDouble();
                foreach (var (tile, cumulative) in cumulativeList)
                {
                    if (!(randomValue <= cumulative)) continue;
                    cellArray[i] = cellPos;
                    tileArray[i] = tile;
                    break;
                }
            }
        
            walkableTilemap.SetTiles(cellArray, tileArray);
        }

        /// <summary>
        /// Paints wall tiles on the wall tilemap at the specified positions,
        /// using the tile that corresponds to the given wall position type.
        /// </summary>
        /// <param name="tilePositions">The world positions where wall tiles should be painted.</param>
        /// <param name="wallPosition">The type of wall position to determine which tile to use.</param>
        public void PaintWallTiles(IEnumerable<Vector2Int> tilePositions, Utils.Utils.WallPosition wallPosition)
        {
            var preset = GetCurrentTilesetPreset();
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
        /// Paints door tiles on the door tilemap at the specified positions,
        /// using the closed door tile from the current preset.
        /// </summary>
        /// <param name="tilePositions">The world positions where door tiles should be painted.</param>
        public void PaintDoorTiles(IEnumerable<Vector2Int> tilePositions)
        {
            var cellPositions = GetCellPositions(tilePositions, doorTilemap);
            var preset = GetCurrentTilesetPreset();
            foreach (var (_, cellPos) in cellPositions)
            {
                doorTilemap.SetTile(cellPos, preset.doorClosed);
            }
        }

        #endregion

        #region Reset Tiles

        /// <summary>
        /// Resets all tiles in the tilemaps by clearing them.
        /// </summary>
        public void ResetAllTiles()
        {
            walkableTilemap?.ClearAllTiles();
            wallTilemap?.ClearAllTiles();
            doorTilemap?.ClearAllTiles();
        }

        #endregion

        #region Tile Selection from Folder

        /// <summary>
        /// Loads all tile assets from the specified folder and populates the given lists with them.
        /// Clears the provided tile base, priority, and probability collections before adding new tiles.
        /// Each found tile is added to <paramref name="tileBases"/> with a default priority of 0.
        /// </summary>
        /// <param name="tileBases">The list to populate with loaded tile assets.</param>
        /// <param name="priorities">The list to populate with default priorities for each tile.</param>
        /// <param name="probabilities">The dictionary to clear (not populated in this method).</param>
        /// <param name="path">The folder path to search for tile assets.</param>
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
        /// Loads all walkable tile assets from the specified folder into the current tileset preset,
        /// resetting the walkable tile list and priorities.
        /// </summary>
        /// <param name="path">The folder path to search for walkable tile assets.</param>
        public void SelectWalkableTilesFromFolder(string path)
        {
            var preset = GetCurrentTilesetPreset();
            SelectTilesFromFolder(preset.walkableTileBases, preset.walkableTilesPriorities, _walkableTilesProbabilities,
                path);
        }

        #endregion

        #region Tile Collections Clearing

        /// <summary>
        /// Removes all walkable tiles from the current tileset preset and clears the walkable tilemap.
        /// This method clears the list of walkable tile bases, their priorities, and the probability dictionary,
        /// then clears all tiles from the associated walkable tilemap.
        /// </summary>
        public void RemoveAllWalkableTiles()
        {
            var preset = GetCurrentTilesetPreset();
            preset.walkableTileBases.Clear();
            preset.walkableTilesPriorities.Clear();
            _walkableTilesProbabilities.Clear();
            walkableTilemap?.ClearAllTiles();
        }

        /// <summary>
        /// Removes all wall tiles from the current tileset preset and clears the wall tilemap.
        /// This method sets all wall tile references in the preset to null and clears all tiles from the associated wall tilemap.
        /// </summary>
        public void RemoveAllWallTiles()
        {
            var preset = GetCurrentTilesetPreset();
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

        # region Presets

        /// <summary>
        /// Adds a tileset preset to the list if it is not already present and selects it as the current preset.
        /// </summary>
        /// <param name="preset">The tileset preset to add and select.</param>
        public void AddAndSelectPreset(TilesetPreset preset)
        {
            if (preset == null) return;
            if (!tilesetPresets.Contains(preset))
                tilesetPresets.Add(preset);
            _tilesetPresetIndex = tilesetPresets.IndexOf(preset);
        }

        /// <summary>
        /// Rebalances the coverage percentages for all tileset presets so that they are evenly distributed.
        /// </summary>
        public void RebalanceCoverages()
        {
            var n = tilesetPresets.Count;
            presetCoverages = Enumerable.Repeat(n > 0 ? 100f / n : 0f, n).ToList();
        }

        /// <summary>
        /// Gets the currently selected tileset preset, or null if none is selected.
        /// </summary>
        /// <returns>The current <see cref="TilesetPreset"/> or null.</returns>
        private TilesetPreset GetCurrentTilesetPreset()
        {
            if (_tilesetPresetIndex < 0 || _tilesetPresetIndex >= tilesetPresets.Count)
                return null;
            return tilesetPresets[_tilesetPresetIndex];
        }

        /// <summary>
        /// Removes the specified tileset preset from the list. Updates the selected index if necessary.
        /// </summary>
        /// <param name="preset">The tileset preset to remove.</param>
        public void RemovePreset(TilesetPreset preset)
        {
            if (!tilesetPresets.Remove(preset)) return;

            if (_tilesetPresetIndex >= tilesetPresets.Count)
                _tilesetPresetIndex = tilesetPresets.Count - 1;
        }

        /// <summary>
        /// Returns a copy of the list of all tileset presets.
        /// </summary>
        /// <returns>A list of all <see cref="TilesetPreset"/> objects.</returns>
        public List<TilesetPreset> GetAllPresets()
        {
            return tilesetPresets.ToList();
        }

        /// <summary>
        /// Returns a copy of the list of preset coverages.
        /// </summary>
        /// <returns>A list of coverage percentages for each preset.</returns>
        public List<float> GetPresetCoverages()
        {
            return presetCoverages.ToList();
        }

        /// <summary>
        /// Sets the coverage percentages for the tileset presets.
        /// </summary>
        /// <param name="coverages">A list of coverage percentages to assign.</param>
        public void SetPresetCoverages(List<float> coverages)
        {
            presetCoverages = new List<float>(coverages);
        }

        #endregion

        #region Walls

        /// <summary>
        /// Generates walls on the wall tilemap based on the provided walkable positions.
        /// Uses the current tileset preset to determine the wall tiles to use.
        /// </summary>
        /// <param name="walkable">A set of walkable positions where walls should be generated.</param>
        /// <param name="nonWall">An optional set of positions that should not be painted as walls.</param>
        public void GenerateWalls(HashSet<Vector2Int> walkable,
            HashSet<Vector2Int> nonWall = null)
        {
            var preset = GetCurrentTilesetPreset();
            var wallMap = wallTilemap;
            if (preset == null || wallMap == null) return;

            new WallAutoTiler(preset, wallMap).PaintWalls(walkable, nonWall);
        }

        #endregion
    }
}