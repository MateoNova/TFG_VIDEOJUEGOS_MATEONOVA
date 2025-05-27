using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using Views.Attributes;

namespace Generators.GraphBased
{
    ///<summary>
    /// Represents a generator that uses a graph-based approach to create dungeons.
    /// This generator allows for the creation of complex dungeon layouts by connecting rooms
    /// and corridors based on a graph structure.
    /// </summary>
    [OpenGraphEditor]
    public class GraphBasedGenerator : BaseGenerator
    {
        #region Fields

        private GraphGeneratorView _graphView;

        [SerializeField, LocalizedLabel("ScalingFactorLabel", "GraphBasedGeneratorTable"),
         LocalizedTooltip("scalingFactor", "GraphBasedGeneratorTable")]
        private float scalingFactor = 0.05f;

        private readonly HashSet<Vector2Int> _occupiedDoorPositions = new();
        private readonly HashSet<Vector2Int> _allFloorPositions = new();
        private readonly HashSet<Vector2Int> _allWallPositions = new();
        private readonly List<Vector2Int> _allDoorsPositions = new();

        #endregion

        /// <summary>
        /// Runs the dungeon generation process using a graph-based approach.
        /// </summary>
        /// <param name="resetTilemap">Whether to reset the tilemap before generation.</param>
        /// <param name="startPoint">The starting point for generation (optional).</param>
        /// <returns>A set of positions representing the generated dungeon layout.</returns>
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

                if (!ComputeCorridor(src, tgt, roomDoors, out var corridor, out var d1, out var d2))
                    continue;

                tilemapPainter.PaintWalkableTiles(corridor);
                _allFloorPositions.UnionWith(corridor);

                var doorList = new List<Vector2Int> { d1, d2 };
                _allDoorsPositions.AddRange(doorList);
            }

            foreach (var door in _allDoorsPositions)
                _allWallPositions.Add(door);

            var wallPositions = _allFloorPositions.Except(_allDoorsPositions).ToHashSet();
            tilemapPainter.GenerateWalls(wallPositions, _allWallPositions);

            return null;
        }

        /// <summary>
        /// Computes a corridor between two rooms based on their door positions.
        /// </summary>
        /// <param name="src">The source room node.</param>
        /// <param name="tgt">The target room node.</param>
        /// <param name="roomDoors">A dictionary mapping room nodes to their door positions.</param>
        /// <param name="corridor">The resulting corridor positions.</param>
        /// <param name="door1">The first door position used in the corridor.</param>
        /// <param name="door2">The second door position used in the corridor.</param>
        /// <returns>True if a corridor was successfully computed, false otherwise.</returns>
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

                var filtered = path.Where(p => !_allWallPositions.Contains(p)).ToList();
                if (filtered.Count == 0)
                    continue;

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

        /// <summary>
        /// Paints the rooms in the dungeon based on the graph nodes.
        /// </summary>
        /// <returns>A dictionary mapping each room node to its door positions.</returns>
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

        /// <summary>
        /// Retrieves wall and floor positions from the JSON file at the specified path,
        /// applying the given offset.
        /// </summary>
        /// <param name="path">The path to the JSON file containing tilemap data.</param>
        /// <param name="offset">The offset to apply to the positions.</param>
        /// <remarks>
        /// This method reads the JSON file, deserializes it into a TilemapData object,
        /// and populates the _allWallPositions and _allFloorPositions sets
        /// with the positions of wall and floor tiles, respectively.
        /// </remarks>
        /// <returns>
        /// A list of Vector2Int positions representing the wall tiles,
        /// and a list of Vector2Int positions representing the floor tiles.
        /// </returns>
        private void GetWallPositions(string path, Vector2Int offset)
        {
            var json = System.IO.File.ReadAllText(path);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);
            foreach (var t in tilemapData.wallTiles)
                _allWallPositions.Add(new Vector2Int(t.position.x, t.position.y) + offset);
        }

        /// <summary>
        /// Retrieves floor positions from the JSON file at the specified path,
        /// applying the given offset.
        /// </summary>
        /// <param name="path">The path to the JSON file containing tilemap data.</param>
        /// <param name="offset">The offset to apply to the positions.</param>
        /// <remarks>
        /// This method reads the JSON file, deserializes it into a TilemapData object,
        /// and populates the _allFloorPositions set
        /// with the positions of walkable tiles.
        /// </remarks>
        /// <returns>
        /// A list of Vector2Int positions representing the walkable tiles.
        /// </returns>
        private void GetFloorPositions(string path, Vector2Int offset)
        {
            var json = System.IO.File.ReadAllText(path);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);
            foreach (var t in tilemapData.walkableTiles)
                _allFloorPositions.Add(new Vector2Int(t.position.x, t.position.y) + offset);
        }

        /// <summary>
        /// Retrieves door positions from the JSON file at the specified path,
        /// applying the given offset.
        /// </summary>
        /// <param name="path">The path to the JSON file containing tilemap data.</param>
        /// <param name="offset">The offset to apply to the positions.</param>
        /// <remarks>
        /// This method reads the JSON file, deserializes it into a TilemapData object,
        /// and returns a list of Vector2Int positions
        /// representing the door tiles,
        /// adjusted by the specified offset.
        /// </remarks>
        /// <returns>
        /// A list of Vector2Int positions representing the door tiles.
        /// </returns>
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

        /// <summary>
        /// Gets the endpoint of a corridor based on the door position.
        /// If no inward direction is found, it returns a position that is not a wall.
        /// </summary>
        /// <param name="door">The position of the door.</param>
        /// <returns>The calculated endpoint for the corridor.</returns>
        private Vector2Int GetCorridorEndpoint(Vector2Int door)
        {
            var inward = Vector2Int.zero;
            foreach (var dir in Utils.Utils.Directions)
            {
                if (!_allFloorPositions.Contains(door + dir)) continue;
                inward = dir;
                break;
            }

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
            for (var i = 1; i <= maxStep; i++)
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

        /// <summary>
        /// Opens the graph window for the GraphBasedGenerator.
        /// This method is called when the generator is initialized or when the user
        /// requests to open the graph window.
        /// </summary>
        public override void OpenGraphWindow()
        {
            GraphCustomWindow.ShowWindow();
        }

        #endregion
    }
}