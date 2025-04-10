using System.Collections.Generic;
using System.Linq;
using GeneralUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Generators.Models
{
    public class TilemapPainter : MonoBehaviour
    {
        #region Walkable Tiles

        [SerializeField] internal Tilemap walkableTilemap;

        [SerializeField, WalkableTileGroup(isTileBases: true)]
        public List<TileBase> walkableTileBases = new();

        [SerializeField, WalkableTileGroup(isTilePriorities: true)]
        public List<int> walkableTilesPriorities = new();

        [SerializeField] public bool randomWalkableTilesPlacement;
        private Dictionary<TileBase, float> _walkableTilesProbabilities = new();

        #endregion

        #region Wall Tiles

        [SerializeField] internal Tilemap wallTilemap;

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

        #region Door Tiles

        [SerializeField] internal Tilemap doorTilemap;
        [SerializeField] private TileBase doorTileBase;

        #endregion

        #region Inicialización de Walkable Tiles

        private void InitializeWalkableTilesProbabilities() =>
            _walkableTilesProbabilities = InitializeProbabilities(walkableTileBases, walkableTilesPriorities);

        private static Dictionary<TileBase, float> InitializeProbabilities(List<TileBase> tileBases,
            List<int> priorities)
        {
            var probabilities = new Dictionary<TileBase, float>();
            var totalPriority = priorities.Sum();

            for (var i = 0; i < tileBases.Count; i++)
            {
                if (i < priorities.Count)
                    probabilities[tileBases[i]] = totalPriority != 0 ? (float)priorities[i] / totalPriority : 0f;
                else
                {
                    Debug.LogWarning($"No priority defined for tile at index {i}. Defaulting to 0.");
                    probabilities[tileBases[i]] = 0f;
                }
            }

            return probabilities;
        }

        #endregion

        #region Métodos de Pintado

        public void PaintWalkableTiles(IEnumerable<Vector2Int> tilesPositions)
        {
            InitializeWalkableTilesProbabilities();
            var positions = tilesPositions.ToList();
            if (randomWalkableTilesPlacement)
                PaintTilesRandomly(positions, walkableTilemap, walkableTileBases);
            else
                PaintTilesWithProbabilities(positions, walkableTilemap, walkableTileBases, _walkableTilesProbabilities);
            Debug.Log($"Number of walkable tiles painted: {positions.Count}");
        }

        private static void PaintTilesRandomly(IEnumerable<Vector2Int> positions, Tilemap tilemap,
            List<TileBase> tileBases)
        {
            var rnd = new System.Random();
            foreach (var pos in positions)
            {
                var tilePos = tilemap.WorldToCell((Vector3Int)pos);
                tilemap.SetTile(tilePos, tileBases[rnd.Next(tileBases.Count)]);
            }
        }

        private static void PaintTilesWithProbabilities(IEnumerable<Vector2Int> positions, Tilemap tilemap,
            List<TileBase> tileBases, Dictionary<TileBase, float> probabilities)
        {
            var cumulativeProbabilities = new List<float>();
            var total = probabilities.Values.Sum();
            var rnd = new System.Random();

            foreach (var tileBase in tileBases)
            {
                if (!probabilities.TryGetValue(tileBase, out var p))
                {
                    Debug.LogError($"Probability for tile {tileBase.name} is not set.");
                    return;
                }

                cumulativeProbabilities.Add(p / total);
            }

            foreach (var pos in positions)
            {
                var tilePos = tilemap.WorldToCell((Vector3Int)pos);
                float randomValue = (float)rnd.NextDouble();
                float sum = 0f;
                for (int i = 0; i < tileBases.Count; i++)
                {
                    sum += cumulativeProbabilities[i];
                    if (randomValue <= sum)
                    {
                        tilemap.SetTile(tilePos, tileBases[i]);
                        break;
                    }
                }
            }
        }

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

            if (tile == null)
                return;

            foreach (var pos in tilesPositions)
            {
                // Conversión explícita: en lugar de (Vector3Int)pos usamos new Vector3Int(x, y, 0)
                Vector3Int tileCellPos = wallTilemap.WorldToCell(new Vector3Int(pos.x, pos.y, 0));
                wallTilemap.SetTile(tileCellPos, tile);
            }
        }


        public void PaintDoorTiles(IEnumerable<Vector2Int> tilesPositions)
        {
            foreach (var pos in tilesPositions)
            {
                var tilePos = doorTilemap.WorldToCell((Vector3Int)pos);
                doorTilemap.SetTile(tilePos, doorTileBase);
            }
        }

        #endregion

        #region Save & Load & Gestión de Colecciones

        public void ResetAllTiles()
        {
            walkableTilemap?.ClearAllTiles();
            wallTilemap?.ClearAllTiles();
            doorTilemap?.ClearAllTiles();
        }

        private static List<Generators.Models.SerializableTile> GetSerializableTiles(Tilemap tilemap,
            bool isDoor = false)
        {
            var list = new List<Generators.Models.SerializableTile>();
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                var tile = tilemap.GetTile(pos);
                if (tile == null) continue;
                var assetPath = AssetDatabase.GetAssetPath(tile);
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                list.Add(new Generators.Models.SerializableTile(pos, guid, isDoor));
            }

            return list;
        }

        public void SaveTilemap(string path)
        {
            var data = new Generators.Models.TilemapData(
                GetSerializableTiles(walkableTilemap),
                GetSerializableTiles(wallTilemap),
                GetSerializableTiles(doorTilemap, true)
            );
            var json = JsonUtility.ToJson(data);
            System.IO.File.WriteAllText(path, json);
        }

        public void LoadTilemap(string path, bool clearBeforeLoading = true, Vector3Int offset = default)
        {
            var json = System.IO.File.ReadAllText(path);
            var data = JsonUtility.FromJson<Generators.Models.TilemapData>(json);
            if (clearBeforeLoading)
                ResetAllTiles();
            foreach (var tile in data.walkableTiles)
            {
                var tileBase = GetTileBaseByGuid(tile.tileGUID);
                walkableTilemap.SetTile(tile.position + offset, tileBase);
            }

            foreach (var tile in data.wallTiles)
            {
                var tileBase = GetTileBaseByGuid(tile.tileGUID);
                wallTilemap.SetTile(tile.position + offset, tileBase);
            }

            foreach (var tile in data.doorTiles)
            {
                var tileBase = GetTileBaseByGuid(tile.tileGUID);
                doorTilemap.SetTile(tile.position + offset, tileBase);
            }
        }

        private static TileBase GetTileBaseByGuid(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
        }

        private static void ClearTileCollections(List<TileBase> tileBases, List<int> priorities,
            Dictionary<TileBase, float> probabilities)
        {
            tileBases?.Clear();
            priorities?.Clear();
            probabilities?.Clear();
        }

        public void RemoveAllWalkableTiles() =>
            ClearTileCollections(walkableTileBases, walkableTilesPriorities, _walkableTilesProbabilities);

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
                var relPath = "Assets" + file.Replace(Application.dataPath, "").Replace('\\', '/');
                var tile = AssetDatabase.LoadAssetAtPath<TileBase>(relPath);
                if (tile == null)
                    continue;
                tileBases.Add(tile);
                priorities.Add(0);
            }
        }

        public void SelectWalkableTilesFromFolder(string path)
        {
            SelectTilesFromFolder(walkableTileBases, walkableTilesPriorities, _walkableTilesProbabilities, path);
        }

        #endregion

        // Constructor auxiliar para instanciar programáticamente (si se requiere).
        public TilemapPainter(bool randomPlacement)
        {
            this.randomWalkableTilesPlacement = randomPlacement;
        }
    }
}