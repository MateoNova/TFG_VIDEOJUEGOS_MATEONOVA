using System.Collections.Generic;
using UnityEngine;

namespace Generators
{
    public class CellularAutomataGenerator : BaseGenerator
    {
        [Header("Cellular Automata Settings")] [Tooltip("Width of the simulation grid")]
        public int width = 100;

        [Tooltip("Height of the simulation grid")]
        public int height = 100;

        [Range(0, 1f), Tooltip("Initial chance for a cell to be floor")]
        public float initialFillPercent = 0.45f;

        [Tooltip("Number of smoothing iterations")]
        public int smoothIterations = 5;

        [Tooltip("Birth limit: if a wall cell has more neighbors than this, it becomes floor")]
        public int birthLimit = 4;

        [Tooltip("Death limit: if a floor cell has fewer neighbors than this, it becomes wall")]
        public int deathLimit = 3;

        public override HashSet<Vector2Int> RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap)
                tilemapPainter.ResetAllTiles();

            var map = new int[width, height];
            InitializeMap(map);
            for (var i = 0; i < smoothIterations; i++)
                SmoothMap(map);

            // Collect floor positions
            var floorPositions = new HashSet<Vector2Int>();
            var offset = origin + startPoint - new Vector2Int(width / 2, height / 2);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (map[x, y] == 1)
                        floorPositions.Add(new Vector2Int(x, y) + offset);
                }
            }

            // Ensure connectivity
            EnsureConnectivity(floorPositions);

            // Paint floors
            //tilemapPainter.PaintWalkableTiles(floorPositions);
            return floorPositions;
        }

        private void InitializeMap(int[,] map)
        {
            var rng = new System.Random();
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = (rng.NextDouble() < initialFillPercent) ? 1 : 0;
        }

        private void SmoothMap(int[,] map)
        {
            var newMap = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighbors = CountFloorNeighbors(map, x, y);
                    if (map[x, y] == 1)
                        newMap[x, y] = (neighbors < deathLimit) ? 0 : 1;
                    else
                        newMap[x, y] = (neighbors > birthLimit) ? 1 : 0;
                }
            }

            System.Array.Copy(newMap, map, width * height);
        }

        private int CountFloorNeighbors(int[,] map, int gridX, int gridY)
        {
            int count = 0;
            for (int nx = gridX - 1; nx <= gridX + 1; nx++)
            {
                for (int ny = gridY - 1; ny <= gridY + 1; ny++)
                {
                    if (nx == gridX && ny == gridY) continue;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    {
                        // treat out-of-bounds as floor to encourage walls at edges
                        count++;
                    }
                    else if (map[nx, ny] == 1)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void EnsureConnectivity(HashSet<Vector2Int> floorPositions)
        {
            var visited = new HashSet<Vector2Int>();
            var regions = new List<HashSet<Vector2Int>>();

            foreach (var pos in floorPositions)
            {
                if (!visited.Contains(pos))
                {
                    var region = FloodFill(pos, floorPositions, visited);
                    regions.Add(region);
                }
            }

            if (regions.Count > 1)
            {
                ConnectRegions(regions, floorPositions);
            }
        }

        private HashSet<Vector2Int> FloodFill(Vector2Int start, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> visited)
        {
            var region = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current)) continue;

                region.Add(current);

                foreach (var direction in Utils.Utils.Directions)
                {
                    var neighbor = current + direction;
                    if (floorPositions.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return region;
        }

        private void ConnectRegions(List<HashSet<Vector2Int>> regions, HashSet<Vector2Int> floorPositions)
        {
            for (int i = 1; i < regions.Count; i++)
            {
                var regionA = regions[i - 1];
                var regionB = regions[i];

                var closestA = Vector2Int.zero;
                var closestB = Vector2Int.zero;
                var minDistance = float.MaxValue;

                foreach (var posA in regionA)
                {
                    foreach (var posB in regionB)
                    {
                        var distance = Vector2Int.Distance(posA, posB);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestA = posA;
                            closestB = posB;
                        }
                    }
                }

                CreateCorridor(closestA, closestB, floorPositions);
            }
        }

        private void CreateCorridor(Vector2Int start, Vector2Int end, HashSet<Vector2Int> floorPositions)
        {
            var current = start;

            while (current != end)
            {
                floorPositions.Add(current);

                if (current.x != end.x)
                {
                    current.x += current.x < end.x ? 1 : -1;
                }
                else if (current.y != end.y)
                {
                    current.y += current.y < end.y ? 1 : -1;
                }
            }
        }
    }
}