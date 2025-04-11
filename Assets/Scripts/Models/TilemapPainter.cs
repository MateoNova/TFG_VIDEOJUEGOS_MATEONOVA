using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        // Constructor se mantiene para compatibilidad, aunque en Unity se suele usar Awake o Start.
        public TilemapPainter(bool randomPlacement)
        {
            randomWalkableTilesPlacement = randomPlacement;
        }

        #region Walkable Tiles

        [SerializeField] internal Tilemap walkableTilemap;
        [SerializeField] public List<TileBase> walkableTileBases = new();
        [SerializeField] public List<int> walkableTilesPriorities = new();
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

        #region Initialization Helpers

        private void InitializeWalkableTilesProbabilities()
        {
            _walkableTilesProbabilities = new Dictionary<TileBase, float>();
            int totalPriority = walkableTilesPriorities.Sum();
            for (var i = 0; i < walkableTileBases.Count; i++)
            {
                float prob = (i < walkableTilesPriorities.Count && totalPriority != 0)
                    ? (float)walkableTilesPriorities[i] / totalPriority
                    : 0f;
                if (i >= walkableTilesPriorities.Count)
                    Debug.LogWarning($"No priority defined for tile at index {i}. Defaulting to 0.");
                _walkableTilesProbabilities[walkableTileBases[i]] = prob;
            }
        }

        /// <summary>
        /// Pre-calcula la conversión de posiciones en mundo a posiciones de celda para optimizar el proceso de pintado.
        /// </summary>
        private List<(Vector2Int worldPos, Vector3Int cellPos)> GetCellPositions(IEnumerable<Vector2Int> positions,
            Tilemap tilemap)
        {
            var list = new List<(Vector2Int, Vector3Int)>();
            foreach (var pos in positions)
            {
                // Convertir cada posición de forma única
                Vector3Int cellPos = tilemap.WorldToCell(new Vector3Int(pos.x, pos.y, 0));
                list.Add((pos, cellPos));
            }

            return list;
        }

        #endregion

        #region Painting Tiles

        public void PaintWalkableTiles(IEnumerable<Vector2Int> tilePositions)
        {
            InitializeWalkableTilesProbabilities();
            List<Vector2Int> positions = tilePositions.ToList();
            var cellPositions = GetCellPositions(positions, walkableTilemap);

            if (randomWalkableTilesPlacement)
                PaintTilesRandomly(cellPositions);
            else
                PaintTilesWithProbabilities(cellPositions);

            Debug.Log($"Number of walkable tiles painted: {positions.Count}");
        }

        private void PaintTilesRandomly(List<(Vector2Int worldPos, Vector3Int cellPos)> cellPositions)
        {
            System.Random rnd = new System.Random();
            foreach (var (_, cellPos) in cellPositions)
            {
                // Selecciona un tile aleatorio
                var tile = walkableTileBases[rnd.Next(walkableTileBases.Count)];
                walkableTilemap.SetTile(cellPos, tile);
            }
        }

        private void PaintTilesWithProbabilities(List<(Vector2Int worldPos, Vector3Int cellPos)> cellPositions)
        {
            // Precalcular probabilidades acumuladas (una única vez)
            float total = _walkableTilesProbabilities.Values.Sum();
            List<(TileBase tile, float cumulative)> cumulativeList = new List<(TileBase, float)>();
            float accumulator = 0f;
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

            System.Random rnd = new System.Random();
            foreach (var (_, cellPos) in cellPositions)
            {
                float randomValue = (float)rnd.NextDouble();
                foreach (var (tile, cumulative) in cumulativeList)
                {
                    if (randomValue <= cumulative)
                    {
                        walkableTilemap.SetTile(cellPos, tile);
                        break;
                    }
                }
            }
        }

        public void PaintWallTiles(IEnumerable<Vector2Int> tilePositions, Utils.Utils.WallPosition wallPosition)
        {
            TileBase tile = wallPosition switch
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
                Debug.LogWarning($"No se ha definido tile para la posición de muro: {wallPosition}");
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
                doorTilemap.SetTile(cellPos, doorTileBase);
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

        #region (Opcional) Selección de Tiles desde Carpeta

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
            SelectTilesFromFolder(walkableTileBases, walkableTilesPriorities, _walkableTilesProbabilities, path);
        }

        #endregion
        
        #region Tile Collections Clearing

        /// <summary>
        /// Limpia las colecciones de tiles walkable.
        /// </summary>
        public void RemoveAllWalkableTiles()
        {
            // Limpia las colecciones para tiles walkable
            walkableTileBases.Clear();
            walkableTilesPriorities.Clear();
            _walkableTilesProbabilities.Clear();
    
            // Si también deseas limpiar el tilemap visual:
            walkableTilemap?.ClearAllTiles();
        }

        /// <summary>
        /// Reinicia (o elimina) la asignación de tiles de paredes.
        /// </summary>
        public void RemoveAllWallTiles()
        {
            // Se pone a null las referencias de los tiles de pared para que se pueda reinicializar si se
            // desea, además se borra el tilemap en caso de ser necesario.
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
    
            // Limpia visualmente el tilemap de paredes.
            wallTilemap?.ClearAllTiles();
        }

        #endregion

    }
}