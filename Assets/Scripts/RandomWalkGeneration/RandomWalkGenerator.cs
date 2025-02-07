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
        /// <summary>
        /// Settings for the random walk algorithm.
        /// </summary>
        /// <remarks>
        /// These settings control the behavior of the random walk algorithm, such as the number of iterations and steps per iteration.
        /// </remarks>
        [SerializeField, Tooltip("Settings for the random walk algorithm.")]
        private RandomWalkRoomsSettings randomWalkRoomsSettings;

        /// <summary>
        /// Flag to determine if corridors should be generated.
        /// </summary>
        /// <remarks>
        /// If true, the generator will create corridors and calculate potential room positions.
        /// </remarks>
        [SerializeField, Tooltip("Flag to determine if corridors should be generated and calcule potential rooms positions.")]
        private bool generateCorridors;

        /// <summary>
        /// Length of each corridor.
        /// </summary>
        /// <remarks>
        /// A smaller value will create shorter corridors, resulting in a more compact dungeon layout.
        /// A larger value will create longer corridors, resulting in a more spread-out dungeon layout.
        /// </remarks>
        [ConditionalField("generateCorridors"), SerializeField,
         Tooltip(
             "Length of each corridor. Smaller values create shorter corridors, resulting in a more compact dungeon layout. Larger values create longer corridors, resulting in a more spread-out dungeon layout.")]
        private int corridorLength = 10;

        /// <summary>
        /// Number of corridors to generate.
        /// </summary>
        /// <remarks>
        /// A smaller value will create fewer corridors, resulting in fewer connections between rooms.
        /// A larger value will create more corridors, resulting in more connections between rooms.
        /// </remarks>
        [ConditionalField("generateCorridors"), SerializeField,
         Tooltip(
             "Number of corridors to generate. Smaller values create fewer corridors, resulting in fewer connections between rooms. Larger values create more corridors, resulting in more connections between rooms.")]
        private int corridorCount = 5;

        /// <summary>
        /// Percentage of potential room positions to convert into rooms.
        /// </summary>
        /// <remarks>
        /// A smaller value will create fewer rooms, resulting in a more sparse dungeon layout.
        /// A larger value will create more rooms, resulting in a more dense dungeon layout.
        /// </remarks>
        [ConditionalField("generateCorridors"), SerializeField, Range(0f, 1f),
         Tooltip(
             "Percentage of potential room positions to convert into rooms. Smaller values create fewer rooms, resulting in a more sparse dungeon layout. Larger values create more rooms, resulting in a more dense dungeon layout.")]
        private float roomPercentage = 0.8f;

        /// <summary>
        /// Width of the corridors.
        /// </summary>
        /// <remarks>
        /// A smaller value will create narrower corridors, resulting in tighter passageways.
        /// A larger value will create wider corridors, resulting in more spacious passageways.
        /// </remarks>
        [ConditionalField("generateCorridors"), SerializeField, Range(1f, 4f),
         Tooltip(
             "Width of the corridors. Smaller values create narrower corridors, resulting in tighter passageways. Larger values create wider corridors, resulting in more spacious passageways.")]
        private int corridorWidth = 1;

        /// <summary>
        /// Runs the dungeon generation process.
        /// </summary>
        /// <param name="resetTilemap">If true, resets the tilemap before generation.</param>
        /// <param name="startPoint">The starting point for the generation.</param>
        public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap) tilemapPainter.ResetAllTiles();

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

        /// <summary>
        /// Generates corridors and rooms.
        /// </summary>
        /// <param name="walkableTilesPositions">The set of walkable tile positions.</param>
        private void GenerateCorridorsAndRooms(HashSet<Vector2Int> walkableTilesPositions)
        {
            HashSet<Vector2Int> potentialRoomPositions = new();
            CreateCorridors(walkableTilesPositions, potentialRoomPositions);

            if (!(roomPercentage > 0)) return;

            var roomsPositions = CreateRooms(potentialRoomPositions);
            var deadEnds = FindAllDeadEnds(walkableTilesPositions);
            CreateRoomsAtDeadEnds(deadEnds, roomsPositions);

            walkableTilesPositions.UnionWith(roomsPositions);
        }

        /// <summary>
        /// Creates rooms at dead ends.
        /// </summary>
        /// <param name="deadEnds">List of dead end positions.</param>
        /// <param name="roomFloors">Set of room floor positions.</param>
        private void CreateRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
        {
            foreach (var room in from deadEnd in deadEnds
                     where !roomFloors.Contains(deadEnd)
                     select GenerateWalkableArea(deadEnd))
            {
                roomFloors.UnionWith(room);
            }
        }

        /// <summary>
        /// Finds all dead ends in the floor positions.
        /// </summary>
        /// <param name="floorPositions">Set of floor positions.</param>
        /// <returns>List of dead end positions.</returns>
        private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
        {
            return floorPositions
                .Where(pos =>
                {
                    var validDirections = Utils.Directions.Count(direction =>
                        Enumerable.Range(1, corridorWidth).All(offset =>
                            floorPositions.Contains(pos + direction * offset)
                        )
                    );
                    return validDirections == 1;
                })
                .ToList();
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

            var roomToCreate = potentialRoomPositions.OrderBy(_ => System.Guid.NewGuid()).Take(roomToCreateCount)
                .ToList();

            foreach (var room in roomToCreate.Select(GenerateWalkableArea))
            {
                roomsPositions.UnionWith(room);
            }

            return roomsPositions;
        }

        /// <summary>
        /// Creates corridors.
        /// </summary>
        /// <param name="floorPositions">Set of floor positions.</param>
        /// <param name="roomsPotentialPositions">Set of potential room positions.</param>
        private void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> roomsPotentialPositions)
        {
            var currentPos = origin;
            roomsPotentialPositions.Add(currentPos);

            for (var i = 0; i < corridorCount; i++)
            {
                var corridor = RandomWalkCorridor(currentPos, corridorLength);
                currentPos = corridor[^1];
                roomsPotentialPositions.Add(currentPos);
                floorPositions.UnionWith(corridor);
            }
        }

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

                // Add width to the corridor
                for (var w = 1; w < corridorWidth; w++)
                {
                    path.Add(currentPos + Utils.GetPerpendicularDirection(direction) * w);
                }
            }

            return path;
        }

        /// <summary>
        /// Generates a walkable area using the random walk algorithm.
        /// </summary>
        /// <param name="startPos">Starting position for the walkable area.</param>
        /// <returns>Set of walkable tile positions.</returns>
        protected HashSet<Vector2Int> GenerateWalkableArea(Vector2Int startPos = default)
        {
            var currentPos = startPos;
            var tiles = new HashSet<Vector2Int>();

            for (var i = 0; i < randomWalkRoomsSettings.walkIterations; i++)
            {
                var path = RandomWalkAlgorithm(currentPos, randomWalkRoomsSettings.stepsPerIteration);
                tiles.UnionWith(path);
                if (randomWalkRoomsSettings.randomizeStartPos && tiles.Count > 0)
                {
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
            var path = new HashSet<Vector2Int> { origin };
            var currentPos = origin;

            for (var i = 0; i < steps; i++)
            {
                currentPos += Utils.GetRandomCardinalDirection();
                path.Add(currentPos);
            }

            return path;
        }
    }
}