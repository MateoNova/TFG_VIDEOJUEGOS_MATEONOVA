using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomWalkGeneration
{
    /// <summary>
    /// Generates a random walk dungeon and renders it on a tilemap.
    /// </summary>
    public class RandomWalkGenerator : BaseGenerator
    {
        #region Serialized Fields

        [SerializeField, Tooltip("Settings for the random walk algorithm.")]
        private RandomWalkRoomsSettings randomWalkRoomsSettings;

        [SerializeField,
         Tooltip("Flag to determine if corridors should be generated and calcule potential rooms positions.")]
        private bool generateCorridors;

        [ConditionalField("generateCorridors"), SerializeField,
         Tooltip(
             "Length of each corridor. Smaller values create shorter corridors, resulting in a more compact dungeon layout. Larger values create longer corridors, resulting in a more spread-out dungeon layout.")]
        private int corridorLength = 10;

        [ConditionalField("generateCorridors"), SerializeField,
         Tooltip(
             "Number of corridors to generate. Smaller values create fewer corridors, resulting in fewer connections between rooms. Larger values create more corridors, resulting in more connections between rooms.")]
        private int corridorCount = 5;

        [ConditionalField("generateCorridors"), SerializeField, Range(0f, 1f),
         Tooltip(
             "Percentage of potential room positions to convert into rooms. Smaller values create fewer rooms, resulting in a more sparse dungeon layout. Larger values create more rooms, resulting in a more dense dungeon layout.")]
        private float roomPercentage = 0.8f;

        [ConditionalField("generateCorridors"), SerializeField, Range(1f, 4f),
         Tooltip(
             "Width of the corridors. Smaller values create narrower corridors, resulting in tighter passageways. Larger values create wider corridors, resulting in more spacious passageways.")]
        private int corridorWidth = 1;

        #endregion

        #region Generation Methods

        public void CORRRREEEE()
        {
            RunGeneration();
        }

        /// <summary>
        /// Runs the dungeon generation process.
        /// </summary>
        /// <param name="resetTilemap">If true, resets the tilemap before generation.</param>
        /// <param name="startPoint">The starting point for the generation.</param>
        public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap)
                tilemapPainter.ResetAllTiles();

            HashSet<Vector2Int> walkableTilesPositions = new();

            if (generateCorridors)
            {
                GenerateCorridorsAndRooms(walkableTilesPositions);
            }
            else
            {
                walkableTilesPositions = GenerateWalkableArea(startPoint);
            }

            tilemapPainter.PaintWalkableTiles(walkableTilesPositions);
            WallGenerator.GenerateWalls(walkableTilesPositions, tilemapPainter);
        }

        #endregion

        #region Corridor and Room Generation

        /// <summary>
        /// Generates corridors and rooms.
        /// </summary>
        /// <param name="walkableTilesPositions">The set of walkable tile positions.</param>
        private void GenerateCorridorsAndRooms(HashSet<Vector2Int> walkableTilesPositions)
        {
            // Set to store positions that are potential candidates for rooms.
            HashSet<Vector2Int> potentialRoomPositions = new();
            CreateCorridors(walkableTilesPositions, potentialRoomPositions);

            if (!(roomPercentage > 0)) return;

            // Create rooms from potential room positions.
            var roomsPositions = CreateRooms(potentialRoomPositions);

            // Find dead ends in the corridor layout.
            var deadEnds = FindAllDeadEnds(walkableTilesPositions);

            // Create additional rooms at dead ends that don't already belong to a room.
            CreateRoomsAtDeadEnds(deadEnds, roomsPositions);

            // Merge room floor positions with the main walkable tiles.
            walkableTilesPositions.UnionWith(roomsPositions);
        }

        /// <summary>
        /// Creates corridors by performing random walks.
        /// </summary>
        /// <param name="floorPositions">Set of floor positions to be filled with corridors.</param>
        /// <param name="roomsPotentialPositions">Set of potential room positions to be updated.</param>
        private void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> roomsPotentialPositions)
        {
            var currentPos = origin;
            roomsPotentialPositions.Add(currentPos);

            for (var i = 0; i < corridorCount; i++)
            {
                var corridor = RandomWalkCorridor(currentPos, corridorLength);
                currentPos = corridor.Last();
                roomsPotentialPositions.Add(currentPos);
                floorPositions.UnionWith(corridor);
            }
        }

        /// <summary>
        /// Creates rooms from potential room positions.
        /// </summary>
        /// <param name="potentialRoomPositions">Set of potential room positions.</param>
        /// <returns>Set of room positions.</returns>
        private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
        {
            HashSet<Vector2Int> roomsPositions = new();
            var roomToCreateCount = (int)(potentialRoomPositions.Count * roomPercentage);

            // Shuffle and select a subset of potential room positions.
            var shuffledPositions = potentialRoomPositions
                .OrderBy(_ => System.Guid.NewGuid())
                .Take(roomToCreateCount)
                .ToList();

            foreach (var roomArea in shuffledPositions.Select(GenerateWalkableArea))
            {
                roomsPositions.UnionWith(roomArea);
            }

            return roomsPositions;
        }

        /// <summary>
        /// Finds all dead ends in the given floor positions.
        /// </summary>
        /// <param name="floorPositions">Set of floor positions.</param>
        /// <returns>List of dead end positions.</returns>
        private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
        {
            return floorPositions
                .Where(pos =>
                {
                    // Count the number of directions with continuous floor tiles (based on corridor width).
                    var validDirections = Utils.Directions.Count(direction =>
                        Enumerable.Range(1, corridorWidth)
                            .All(offset => floorPositions.Contains(pos + direction * offset))
                    );
                    return validDirections == 1;
                })
                .ToList();
        }

        /// <summary>
        /// Creates rooms at dead end positions that are not already part of a room.
        /// </summary>
        /// <param name="deadEnds">List of dead end positions.</param>
        /// <param name="roomFloors">Set of room floor positions.</param>
        private void CreateRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
        {
            foreach (var roomArea in from deadEnd in deadEnds
                     where !roomFloors.Contains(deadEnd)
                     select GenerateWalkableArea(deadEnd))
            {
                roomFloors.UnionWith(roomArea);
            }
        }

        #endregion

        #region Corridor Generation

        /// <summary>
        /// Generates a random walk corridor.
        /// </summary>
        /// <param name="startPos">Starting position of the corridor.</param>
        /// <param name="length">Length of the corridor.</param>
        /// <returns>List of positions forming the corridor.</returns>
        private List<Vector2Int> RandomWalkCorridor(Vector2Int startPos, int length)
        {
            var currentPos = startPos;
            var direction = Utils.GetRandomCardinalDirection();
            List<Vector2Int> path = new() { startPos };

            for (var i = 0; i < length; i++)
            {
                currentPos += direction;
                path.Add(currentPos);

                // Expand corridor width by adding perpendicular positions.
                for (var w = 1; w < corridorWidth; w++)
                {
                    path.Add(currentPos + Utils.GetPerpendicularDirection(direction) * w);
                }
            }

            return path;
        }

        #endregion

        #region Random Walk Generation

        /// <summary>
        /// Generates a walkable area using the random walk algorithm.
        /// </summary>
        /// <param name="startPos">Starting position for the walkable area.</param>
        /// <returns>Set of walkable tile positions.</returns>
        private HashSet<Vector2Int> GenerateWalkableArea(Vector2Int startPos = default)
        {
            var currentPos = startPos;
            HashSet<Vector2Int> tiles = new();

            for (var i = 0; i < randomWalkRoomsSettings.walkIterations; i++)
            {
                var path = RandomWalkAlgorithm(currentPos, randomWalkRoomsSettings.stepsPerIteration);
                tiles.UnionWith(path);

                if (randomWalkRoomsSettings.randomizeStartPos && tiles.Count > 0)
                {
                    // Selecciona aleatoriamente un nuevo punto de inicio de entre los tiles ya visitados.
                    currentPos = tiles.ElementAt(Random.Range(0, tiles.Count));
                }
            }

            return tiles;
        }

        /// <summary>
        /// Performs the random walk algorithm.
        /// </summary>
        /// <param name="origin">Starting position for the random walk.</param>
        /// <param name="steps">Number of steps to take.</param>
        /// <returns>Set of positions visited during the random walk.</returns>
        private static HashSet<Vector2Int> RandomWalkAlgorithm(Vector2Int origin, int steps)
        {
            HashSet<Vector2Int> path = new() { origin };
            var currentPos = origin;

            for (var i = 0; i < steps; i++)
            {
                currentPos += Utils.GetRandomCardinalDirection();
                path.Add(currentPos);
            }

            return path;
        }

        #endregion
    }
}