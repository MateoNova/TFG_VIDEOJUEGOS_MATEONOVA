using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Views.Attributes;

namespace Models
{

    public interface ITilemapPainter
    {

        void PaintWalkableTiles(IEnumerable<Vector2Int> tilePositions);


        void PaintWallTiles(IEnumerable<Vector2Int> tilePositions, Utils.Utils.WallPosition wallPosition);


        void PaintDoorTiles(IEnumerable<Vector2Int> tilePositions);


        void ResetAllTiles();
    }

    public class TilemapPainter : MonoBehaviour, ITilemapPainter
    {
   
        public TilemapPainter(bool randomPlacement)
        {
            //randomWalkableTilesPlacement = randomPlacement;
        }

        [SerializeField] private List<TilesetPreset> _tilesetPresets = new();
        private int _tilesetPresetIndex = -1; // The index of the current tileset preset is none

        public void AddAndSelectPreset(TilesetPreset preset)
        {
            if (preset == null) return;
            if (!_tilesetPresets.Contains(preset))
                _tilesetPresets.Add(preset);
            _tilesetPresetIndex = _tilesetPresets.IndexOf(preset);
        }

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

  
        public void PaintWalkableTiles(IEnumerable<Vector2Int> tilePositions)
        {
            var preset = GetCurrentTilesetPreset();
            if (preset == null || preset.walkableTileBases.Count == 0)
                return;

            InitializeWalkableTilesProbabilities(preset);
            var positions    = tilePositions.ToList();
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
            foreach (var (_, cellPos) in cellPositions)
            {
                doorTilemap.SetTile(cellPos, doorClosed);
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

        #region Optional: Tile Selection from Folder

    
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

        public List<TileBase> GetWalkableTileBases()
        {
            var preset = GetCurrentTilesetPreset();
            return preset?.walkableTileBases ?? new List<TileBase>();
        }

        public List<int> GetWalkableTilesPriorities()
        {
            var preset = GetCurrentTilesetPreset();
            return preset?.walkableTilesPriorities ?? new List<int>();
        }

        public bool GetRandomWalkableTilesPlacement()
        {
            var preset = GetCurrentTilesetPreset();
            return preset?.randomWalkableTilesPlacement ?? false;
        }

        public void SetRandomWalkableTilesPlacement(bool newValue)
        {
            var preset = GetCurrentTilesetPreset();
            if (preset == null) return;
            preset.randomWalkableTilesPlacement = newValue;
            InitializeWalkableTilesProbabilities(preset);
        }

        public void SetWalkableTileBases(int index, TileBase newTile)
        {
            var preset = GetCurrentTilesetPreset();
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
            var preset = GetCurrentTilesetPreset();
            preset.walkableTileBases.Clear();
        }

        public TilesetPreset GetCurrentTilesetPreset()
        {
            if (_tilesetPresetIndex < 0 || _tilesetPresetIndex >= _tilesetPresets.Count)
                return null;
            return _tilesetPresets[_tilesetPresetIndex];
        }

        public void ClearWalkableTilesPriorities()
        {
            var preset = GetCurrentTilesetPreset();
            preset.walkableTilesPriorities.Clear();
        }

        public void AddTileWalkableTileBases(Tile tile)
        {
            var preset = GetCurrentTilesetPreset();
            ;
            if (preset.walkableTileBases.Contains(tile))
            {
                Debug.LogWarning($"Tile {tile.name} already exists in walkable tile bases.");
                return;
            }

            preset.walkableTileBases.Add(tile);
            preset.walkableTilesPriorities.Add(1);
        }
        
        public void RemovePreset(TilesetPreset preset)
        {
            if (_tilesetPresets.Remove(preset))
            {
                if (_tilesetPresetIndex >= _tilesetPresets.Count)
                    _tilesetPresetIndex = _tilesetPresets.Count - 1;
            }
        }
    }
}