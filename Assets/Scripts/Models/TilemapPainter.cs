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

        public void PaintWalkableTiles(IEnumerable<Vector2Int> tilePositions)
        {
            var preset = GetCurrentTilesetPreset();
            if (preset == null || preset.walkableTileBases.Count == 0)
                return;

            InitializeWalkableTilesProbabilities(preset);
            var positions = tilePositions.ToList();
            var cellPositions = GetCellPositions(positions, walkableTilemap);

            if (preset.randomWalkableTilesPlacement)
                PaintTilesRandomly(cellPositions);
            else
                PaintTilesWithProbabilities(cellPositions);
        }


        private void PaintTilesRandomly(List<(Vector2Int, Vector3Int)> cellPositions)
        {
            var preset = GetCurrentTilesetPreset();
            if (preset == null || preset.walkableTileBases.Count == 0) return;

            var rnd = new System.Random();
            foreach (var (_, cell) in cellPositions)
            {
                var tile = preset.walkableTileBases[rnd.Next(preset.walkableTileBases.Count)];
                walkableTilemap.SetTile(cell, tile);
            }
        }


        private void PaintTilesWithProbabilities(List<(Vector2Int worldPos, Vector3Int cellPos)> cellPositions)
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

        public void ResetAllTiles()
        {
            walkableTilemap?.ClearAllTiles();
            wallTilemap?.ClearAllTiles();
            doorTilemap?.ClearAllTiles();
        }

        #endregion

        #region Tile Selection from Folder

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


        public void SelectWalkableTilesFromFolder(string path)
        {
            var preset = GetCurrentTilesetPreset();
            SelectTilesFromFolder(preset.walkableTileBases, preset.walkableTilesPriorities, _walkableTilesProbabilities,
                path);
        }

        #endregion

        #region Tile Collections Clearing

        public void RemoveAllWalkableTiles()
        {
            var preset = GetCurrentTilesetPreset();
            preset.walkableTileBases.Clear();
            preset.walkableTilesPriorities.Clear();
            _walkableTilesProbabilities.Clear();
            walkableTilemap?.ClearAllTiles();
        }


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

        public void AddAndSelectPreset(TilesetPreset preset)
        {
            if (preset == null) return;
            if (!tilesetPresets.Contains(preset))
                tilesetPresets.Add(preset);
            _tilesetPresetIndex = tilesetPresets.IndexOf(preset);
        }

        public void RebalanceCoverages()
        {
            var n = tilesetPresets.Count;
            presetCoverages = Enumerable.Repeat(n > 0 ? 100f / n : 0f, n).ToList();
        }

        public TilesetPreset GetCurrentTilesetPreset()
        {
            if (_tilesetPresetIndex < 0 || _tilesetPresetIndex >= tilesetPresets.Count)
                return null;
            return tilesetPresets[_tilesetPresetIndex];
        }


        public void RemovePreset(TilesetPreset preset)
        {
            if (!tilesetPresets.Remove(preset)) return;

            if (_tilesetPresetIndex >= tilesetPresets.Count)
                _tilesetPresetIndex = tilesetPresets.Count - 1;
        }


        public List<TilesetPreset> GetAllPresets()
        {
            return tilesetPresets.ToList();
        }

        public List<float> GetPresetCoverages()
        {
            return presetCoverages.ToList();
        }

        public void SetPresetCoverages(List<float> coverages)
        {
            presetCoverages = new List<float>(coverages);
        }

        #endregion

        #region Walls

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