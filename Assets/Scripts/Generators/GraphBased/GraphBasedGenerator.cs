using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using Views.Attributes;

namespace Generators.GraphBased
{
    /// <summary>
    /// GraphBasedGenerator is a generator that creates a dungeon layout based on a graph structure.
    /// It uses a tilemap to paint rooms and corridors, and it handles door placements.
    /// </summary>
    [OpenGraphEditor]
    public class GraphBasedGenerator : BaseGenerator
    {
        #region Fields

        /// <summary>
        /// Reference to the GraphGeneratorView.
        /// </summary>
        private GraphGeneratorView _graphView;

        /// <summary>
        /// Scaling factor for adjusting positions.
        /// </summary>
        [SerializeField, LocalizedTooltip("scalingFactor", "GraphBasedGeneratorTable")] private float scalingFactor = 0.05f;

        /// <summary>
        /// Set of occupied door positions.
        /// </summary>
        private readonly HashSet<Vector2Int> _occupiedDoorPositions = new();

        /// <summary>
        /// Set of all floor positions.
        /// </summary>
        private readonly HashSet<Vector2Int> _allFloorPositions = new();

        /// <summary>
        /// Set of all wall positions.
        /// </summary>
        private readonly HashSet<Vector2Int> _allWallPositions = new();

        /// <summary>
        /// List of all door positions.
        /// </summary>
        private readonly List<Vector2Int> _allDoorsPositions = new();

        #endregion

        #region Generation Methods

        /// <summary>
        /// Runs the generation process.
        /// </summary>
        /// <param name="resetTilemap">If true, resets the tilemap before generation.</param>
        /// <param name="startPoint">The starting point for generation.</param>
        public override HashSet<Vector2Int> RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap)
            {
                tilemapPainter.ResetAllTiles();
            }

            _occupiedDoorPositions.Clear();
            _allFloorPositions.Clear();
            _allWallPositions.Clear();
            _allDoorsPositions.Clear();

            _graphView = GraphCustomWindow.GetGraphGeneratorView();

            if (_graphView == null)
            {
                Debug.LogError("GraphGeneratorView not found.");
                return null;
            }

            var roomDoors = PaintRooms();

            // Generate corridors between rooms
            foreach (var edge in _graphView.edges)
            {
                if (edge is null) continue;

                var outputPort = edge.output;
                var inputPort = edge.input;
                if (outputPort == null || inputPort == null) continue;

                if (outputPort.node is not GraphNode sourceNode || inputPort.node is not GraphNode targetNode)
                    continue;

                var room1Doors = roomDoors[sourceNode];
                var room2Doors = roomDoors[targetNode];
                GenerateCorridor(room1Doors, room2Doors);
            }

            // Add doors to all wall positions
            foreach (var door in _allDoorsPositions)
            {
                _allWallPositions.Add(door);
            }
            //todo arreglar, tengo q extraer lo del tielmappainter paint y wall geenrator para q eso se haga fuera.
            WallGenerator.GenerateWalls(_allFloorPositions, tilemapPainter, _allWallPositions);
            return null;
        }

        #endregion

        #region Room Painting

        /// <summary>
        /// Paints rooms on the tilemap.
        /// </summary>
        /// <returns>A dictionary mapping each GraphNode to its list of door positions.</returns>
        private Dictionary<GraphNode, List<Vector2Int>> PaintRooms()
        {
            var roomDoors = new Dictionary<GraphNode, List<Vector2Int>>();
            foreach (var node in _graphView.nodes)
            {
                if (node is not GraphNode graphNode) continue;

                var pos = graphNode.GetPosition();
                var originalPos = new Vector2(pos.x, -pos.y);
                var adjustedPos = originalPos * scalingFactor;
                var gridPos = new Vector2Int(Mathf.RoundToInt(adjustedPos.x), Mathf.RoundToInt(adjustedPos.y));

                // Paint the room on the Tilemap
                var persistenceManager = new TilemapPersistenceManager(
                    tilemapPainter.walkableTilemap,
                    tilemapPainter.wallTilemap,
                    tilemapPainter.doorTilemap
                );
                persistenceManager.LoadTilemap(
                    graphNode.JsonFilePath,
                    tilemapPainter,
                    clearBeforeLoading: false,
                    offset: new Vector3Int(gridPos.x, gridPos.y, 0)
                );

                // Get door and floor positions
                var doors = GetDoorPositions(graphNode.JsonFilePath, gridPos);
                foreach (var door in doors)
                {
                    _allDoorsPositions.Add(door);
                }

                GetFloorPositions(graphNode.JsonFilePath, gridPos);
                GetWallPositions(graphNode.JsonFilePath, gridPos);
                roomDoors[graphNode] = doors;
            }

            return roomDoors;
        }

        #endregion

        #region Position Retrieval

        /// <summary>
        /// Gets wall positions from the JSON file.
        /// </summary>
        /// <param name="graphNodeJsonFilePath">The path to the JSON file.</param>
        /// <param name="gridPos">The grid position offset.</param>
        private void GetWallPositions(string graphNodeJsonFilePath, Vector2Int gridPos)
        {
            var json = System.IO.File.ReadAllText(graphNodeJsonFilePath);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);

            foreach (var tile in tilemapData.wallTiles)
            {
                _allWallPositions.Add(new Vector2Int(tile.position.x, tile.position.y) + gridPos);
            }
        }

        /// <summary>
        /// Gets floor positions from the JSON file.
        /// </summary>
        /// <param name="jsonFilePath">The path to the JSON file.</param>
        /// <param name="offset">The grid position offset.</param>
        private void GetFloorPositions(string jsonFilePath, Vector2Int offset)
        {
            var json = System.IO.File.ReadAllText(jsonFilePath);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);

            foreach (var tile in tilemapData.walkableTiles)
            {
                _allFloorPositions.Add(new Vector2Int(tile.position.x, tile.position.y) + offset);
            }
        }

        /// <summary>
        /// Gets door positions from the JSON file.
        /// </summary>
        /// <param name="jsonFilePath">The path to the JSON file.</param>
        /// <param name="offset">The grid position offset.</param>
        /// <returns>A list of door positions.</returns>
        private static List<Vector2Int> GetDoorPositions(string jsonFilePath, Vector2Int offset)
        {
            var json = System.IO.File.ReadAllText(jsonFilePath);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);

            return tilemapData.doorTiles.Select(tile => new Vector2Int(tile.position.x, tile.position.y) + offset)
                .ToList();
        }

        #endregion

        #region Corridor Generation

        /// <summary>
        /// Generates a corridor between the doors of two rooms.
        /// </summary>
        /// <param name="room1Doors">List of door positions in the first room.</param>
        /// <param name="room2Doors">List of door positions in the second room.</param>
        private void GenerateCorridor(List<Vector2Int> room1Doors, List<Vector2Int> room2Doors)
        {
            // Create a list of door pairs sorted by distance
            var candidatePairs = (from door1 in room1Doors
                from door2 in room2Doors
                let distance = Vector2Int.Distance(door1, door2)
                select (door1, door2, distance)).ToList();

            candidatePairs.Sort((a, b) => a.distance.CompareTo(b.distance));

            // Try each pair to see if a corridor can be created with A*
            foreach (var candidate in candidatePairs)
            {
                if (_occupiedDoorPositions.Contains(candidate.door1) ||
                    _occupiedDoorPositions.Contains(candidate.door2))
                    continue;

                var corridorPath = Pathfinding.FindPath(candidate.door1, candidate.door2, tilemapPainter);
                if (corridorPath == null || corridorPath.Count == 0)
                    continue;

                _occupiedDoorPositions.Add(candidate.door1);
                _occupiedDoorPositions.Add(candidate.door2);

                tilemapPainter.PaintWalkableTiles(corridorPath);

                foreach (var pos in corridorPath)
                {
                    _allFloorPositions.Add(pos);
                }

                // Remove the tile next to each door
                corridorPath.Remove(candidate.door1 + new Vector2Int(1, 0));
                corridorPath.Remove(candidate.door1 + new Vector2Int(-1, 0));

                tilemapPainter.PaintDoorTiles(new List<Vector2Int> { candidate.door1, candidate.door2 });
                return;
            }

            Debug.LogWarning("Could not generate a corridor between the rooms with the available doors.");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Opens the graph window.
        /// </summary>
        public override void OpenGraphWindow()
        {
            GraphCustomWindow.ShowWindow();
        }

        #endregion
    }
}