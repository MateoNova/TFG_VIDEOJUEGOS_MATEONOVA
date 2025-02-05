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
        [SerializeField] private RandomWalkSettings randomWalkSettings;

        /// <summary>
        /// Runs the dungeon generation algorithm and renders the walkable area.
        /// </summary>
        protected override void RunGeneration()
        { 
            var floorPositions = GenerateWalkableArea();
            tilemapRenderer.RenderWalkableTiles(floorPositions);
        }

        /// <summary>
        /// Generates the walkable area using the random walk algorithm.
        /// </summary>
        /// <returns>A HashSet of Vector2Int representing the positions of the walkable tiles.</returns>
        private HashSet<Vector2Int> GenerateWalkableArea()
        {
            var currentPos = origin;
            var tiles = new HashSet<Vector2Int>();

            for (var i = 0; i < randomWalkSettings.walkIterations; i++)
            {
                var path = RandomWalkAlgorithm(currentPos, randomWalkSettings.stepsPerIteration);
                tiles.UnionWith(path);
                if (randomWalkSettings.randomizeStartPos && tiles.Count > 0)
                {
                    currentPos = tiles.ElementAt(Random.Range(0, tiles.Count));
                }
            }
            return tiles;
        }
    
        /// <summary>
        /// Performs a random walk starting from the given origin for a specified number of steps.
        /// </summary>
        /// <param name="origin">The starting position of the random walk.</param>
        /// <param name="steps">The number of steps to take in the random walk.</param>
        /// <returns>A HashSet of Vector2Int representing the path taken by the random walk.</returns>
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