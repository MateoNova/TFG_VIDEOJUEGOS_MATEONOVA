using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using Views.Attributes;

namespace Generators.GraphBased
{
    [OpenGraphEditor]
    public class GraphBasedGenerator : BaseGenerator
    {
        #region Fields

        private GraphGeneratorView _graphView;

        [SerializeField, LocalizedTooltip("scalingFactor", "GraphBasedGeneratorTable")]
        private float scalingFactor = 0.05f;

        private readonly HashSet<Vector2Int> _occupiedDoorPositions = new();
        private readonly HashSet<Vector2Int> _allFloorPositions = new();
        private readonly HashSet<Vector2Int> _allWallPositions = new();
        private readonly List<Vector2Int> _allDoorsPositions = new();

        #endregion

        public override HashSet<Vector2Int> RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap)
                tilemapPainter.ResetAllTiles();

            _occupiedDoorPositions.Clear();
            _allFloorPositions.Clear();
            _allWallPositions.Clear();
            _allDoorsPositions.Clear();

            _graphView = GraphCustomWindow.GetGraphGeneratorView();
            if (_graphView == null)
            {
                Debug.LogError("GraphGeneratorView not found.");
                return new HashSet<Vector2Int>();
            }

            var roomDoors = PaintRooms();

            foreach (var edge in _graphView.edges)
            {
                if (edge?.output?.node is not GraphNode src ||
                    edge.input?.node is not GraphNode tgt)
                    continue;

                // obtenemos el corredor *y* los 2 endpoints de puerta
                if (!ComputeCorridor(src, tgt, roomDoors, out var corridor, out var d1, out var d2))
                    continue;

                // ➤ 0) Eliminamos las puertas del listado para que no las pisemos:
                //corridor.RemoveAll(p => p == d1 || p == d2);

                // ➤ 1) Pintar el suelo del corredor (sin las puertas)
                tilemapPainter.PaintWalkableTiles(corridor);
                _allFloorPositions.UnionWith(corridor);

                // ➤ 2) Pintar de nuevo las puertas en sus posiciones originales
                var doorList = new List<Vector2Int> { d1, d2 };
                //tilemapPainter.PaintDoorTiles(doorList);
                _allDoorsPositions.AddRange(doorList);
            }

            // Marcar puertas como muros internos
            foreach (var door in _allDoorsPositions)
                _allWallPositions.Add(door);

            // Convert IEnumerable<Vector2Int> to HashSet<Vector2Int>
            var wallPositions = _allFloorPositions.Except(_allDoorsPositions).ToHashSet();
            WallGenerator.GenerateWalls(wallPositions, tilemapPainter, _allWallPositions);

            return new HashSet<Vector2Int>(_allFloorPositions);
        }

        /// <summary>
        /// Extrae la lógica de A* y filtra muros, devolviendo el corredor
        /// y los dos endpoints de puerta que se han usado.
        /// </summary>
        private bool ComputeCorridor(
            GraphNode src,
            GraphNode tgt,
            Dictionary<GraphNode, List<Vector2Int>> roomDoors,
            out List<Vector2Int> corridor,
            out Vector2Int door1,
            out Vector2Int door2)
        {
            var srcDoors = roomDoors[src];
            var tgtDoors = roomDoors[tgt];

            // 1) Construimos pares ordenados por distancia
            var pairs = (from d1 in srcDoors
                from d2 in tgtDoors
                let dist = Vector2Int.Distance(d1, d2)
                orderby dist
                select (d1, d2)).ToList();

            foreach (var (d1, d2) in pairs)
            {
                if (_occupiedDoorPositions.Contains(d1) ||
                    _occupiedDoorPositions.Contains(d2))
                    continue;

                var start = GetCorridorEndpoint(d1);
                var end = GetCorridorEndpoint(d2);

                var path = Pathfinding.FindPath(start, end, tilemapPainter);
                if (path == null || path.Count == 0)
                    continue;

                // 2) Filtrar sólo tiles de suelo (sin pisar muros existentes)
                var filtered = path.Where(p => !_allWallPositions.Contains(p)).ToList();
                if (filtered.Count == 0)
                    continue;

                // 3) Marcamos ocupadas esas puertas y salimos
                _occupiedDoorPositions.Add(d1);
                _occupiedDoorPositions.Add(d2);

                corridor = filtered;
                door1 = d1;
                door2 = d2;
                return true;
            }

            corridor = null;
            door1 = door2 = Vector2Int.zero;
            Debug.LogWarning("No se pudo generar un pasillo entre las salas.");
            return false;
        }


        #region Room Painting

        private Dictionary<GraphNode, List<Vector2Int>> PaintRooms()
        {
            var roomDoors = new Dictionary<GraphNode, List<Vector2Int>>();

            foreach (var element in _graphView.nodes)
            {
                if (element is not GraphNode graphNode) continue;

                var pos = graphNode.GetPosition();
                var world = new Vector2(pos.x, -pos.y) * scalingFactor;
                var gridPos = new Vector2Int(
                    Mathf.RoundToInt(world.x),
                    Mathf.RoundToInt(world.y)
                );

                var pm = new TilemapPersistenceManager(
                    tilemapPainter.walkableTilemap,
                    tilemapPainter.wallTilemap,
                    tilemapPainter.doorTilemap
                );

                pm.LoadTilemap(
                    graphNode.JsonFilePath,
                    tilemapPainter,
                    clearBeforeLoading: false,
                    offset: new Vector3Int(gridPos.x, gridPos.y, 0)
                );

                // Acumular puertas, pisos y muros de la sala
                var doors = GetDoorPositions(graphNode.JsonFilePath, gridPos);
                foreach (var d in doors)
                    _allDoorsPositions.Add(d);

                GetFloorPositions(graphNode.JsonFilePath, gridPos);
                GetWallPositions(graphNode.JsonFilePath, gridPos);

                roomDoors[graphNode] = doors;
            }

            return roomDoors;
        }

        #endregion

        #region Position Retrieval Helpers

        private void GetWallPositions(string path, Vector2Int offset)
        {
            var json = System.IO.File.ReadAllText(path);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);
            foreach (var t in tilemapData.wallTiles)
                _allWallPositions.Add(new Vector2Int(t.position.x, t.position.y) + offset);
        }

        private void GetFloorPositions(string path, Vector2Int offset)
        {
            var json = System.IO.File.ReadAllText(path);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);
            foreach (var t in tilemapData.walkableTiles)
                _allFloorPositions.Add(new Vector2Int(t.position.x, t.position.y) + offset);
        }

        private static List<Vector2Int> GetDoorPositions(string path, Vector2Int offset)
        {
            var json = System.IO.File.ReadAllText(path);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);
            return tilemapData.doorTiles
                .Select(d => new Vector2Int(d.position.x, d.position.y) + offset)
                .ToList();
        }

        #endregion

        #region Corridor Endpoint Calculation

        private Vector2Int GetCorridorEndpoint(Vector2Int door)
        {
            // Hacia dentro: vecino que sea floor
            Vector2Int inward = Vector2Int.zero;
            foreach (var dir in Utils.Utils.Directions)
            {
                if (_allFloorPositions.Contains(door + dir))
                {
                    inward = dir;
                    break;
                }
            }

            // Si no hay, tomar cualquiera no-muro
            if (inward == Vector2Int.zero)
            {
                foreach (var dir in Utils.Utils.Directions)
                {
                    var cand = door + dir;
                    if (!_allWallPositions.Contains(cand))
                        return cand;
                }

                return door;
            }

            // Hacia afuera
            var outward = -inward;
            const int maxStep = 3;
            for (int i = 1; i <= maxStep; i++)
            {
                var cand = door + outward * i;
                if (!_allFloorPositions.Contains(cand) &&
                    !_allWallPositions.Contains(cand))
                    return cand;
            }

            return door + outward;
        }

        #endregion

        #region Utility Methods

        public override void OpenGraphWindow()
        {
            GraphCustomWindow.ShowWindow();
        }

        #endregion
    }
}