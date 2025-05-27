using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views.Attributes;

namespace Generators
{
    /// <summary>
    /// Generates a dungeon-like structure using cellular automata.
    /// This generator creates a grid of cells that are either filled or empty,
    /// then smooths the grid using rules for birth and death of cells.
    /// It can optionally ensure connectivity between regions
    /// using flood fill and corridor creation.
    /// </summary>
    public class CellularAutomataGenerator : BaseGenerator
    {
        [SerializeField, LocalizedLabel("WidthLabel", "CellularAutomataGeneratorTable"),
         LocalizedTooltip("WidthTooltip", "CellularAutomataGeneratorTable")]
        private int width = 100;

        [SerializeField, LocalizedLabel("HeightLabel", "CellularAutomataGeneratorTable"),
         LocalizedTooltip("HeightTooltip", "CellularAutomataGeneratorTable")]
        private int height = 100;

        [SerializeField, LocalizedLabel("InitialFillPercentLabel", "CellularAutomataGeneratorTable"),
         LocalizedTooltip("InitialFillPercentTooltip", "CellularAutomataGeneratorTable"), Range(0.4f, 0.5f)]
        private float initialFillPercent = 0.45f;

        [SerializeField, LocalizedLabel("SmoothIterationsLabel", "CellularAutomataGeneratorTable"),
         LocalizedTooltip("SmoothIterationsTooltip", "CellularAutomataGeneratorTable")]
        private int smoothIterations = 5;

        [SerializeField, LocalizedLabel("BirthLimitLabel", "CellularAutomataGeneratorTable"),
         LocalizedTooltip("BirthLimitTooltip", "CellularAutomataGeneratorTable"),
         Range(4f, 4.5f)]
        private float birthLimit = 4;

        [SerializeField, LocalizedLabel("DeathLimitLabel", "CellularAutomataGeneratorTable"),
         LocalizedTooltip("DeathLimitTooltip", "CellularAutomataGeneratorTable"),
         Range(2.5f, 3f)]
        private float deathLimit = 3;

        [SerializeField, Condition("floodFill"), LocalizedLabel("FloodFillLabel", "CellularAutomataGeneratorTable"),
         LocalizedTooltip("FloodFillTooltip", "CellularAutomataGeneratorTable")]
        private bool floodFill;

        [SerializeField, ConditionalField("floodFill"),
         LocalizedLabel("MaxCorridorDistanceLabel", "CellularAutomataGeneratorTable"),
         LocalizedTooltip("MaxCorridorDistanceTooltip", "CellularAutomataGeneratorTable"),
         Range(3, 100)]
        private int maxCorridorDistance = 20;

        [SerializeField, ConditionalField("floodFill"),
         LocalizedLabel("CorridorWidthLabel", "CellularAutomataGeneratorTable"),
         LocalizedTooltip("CorridorWidthTooltip", "CellularAutomataGeneratorTable"), Range(1, 4)]
        private int corridorWidth = 1;

        /// <summary>
        /// Runs the cellular automata generation algorithm.
        /// Initializes a grid with random filled and empty cells,
        /// smooths the grid based on birth and death rules,
        /// and optionally ensures connectivity
        /// by creating corridors between regions.
        /// </summary>
        /// <param name="resetTilemap">Whether to reset the tilemap before generation.</param>
        /// <param name="startPoint">The starting point for the generation, used to offset the origin.</param>
        /// <returns>A set of positions representing the generated floor tiles.</returns>
        public override HashSet<Vector2Int> RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap)
                tilemapPainter.ResetAllTiles();

            var map = new int[width, height];
            InitializeMap(map);
            for (var i = 0; i < smoothIterations; i++)
                SmoothMap(map, width, height);

            var floorPositions = new HashSet<Vector2Int>();
            var offset = origin + startPoint - new Vector2Int(width / 2, height / 2);

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                if (map[x, y] == 1)
                    floorPositions.Add(new Vector2Int(x, y) + offset);

            if (floodFill)
                EnsureConnectivity(floorPositions, map);

            return floorPositions;
        }

        /// <summary>
        /// Initializes the map with random filled and empty cells.
        /// </summary>
        /// <param name="map">The 2D array representing the map.</param>
        private void InitializeMap(int[,] map)
        {
            var rng = new System.Random();
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                map[x, y] = (rng.NextDouble() < initialFillPercent) ? 1 : 0;
        }

        /// <summary>
        /// Smooths the map using cellular automata rules.
        /// </summary>
        /// <param name="map">The 2D array representing the map.</param>
        /// <param name="w">The width of the map.</param>
        /// <param name="h">The height of the map.</param>
        private void SmoothMap(int[,] map, int w, int h)
        {
            var newMap = new int[w, h];
            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                var neighbors = CountFloorNeighbors(map, x, y, w, h);
                newMap[x, y] = map[x, y] == 1
                    ? (neighbors < deathLimit ? 0 : 1)
                    : (neighbors > birthLimit ? 1 : 0);
            }

            System.Array.Copy(newMap, map, w * h);
        }

        /// <summary>
        /// Counts the number of floor neighbors around a given cell.
        /// Includes out-of-bounds cells as neighbors.
        /// </summary>
        private static int CountFloorNeighbors(int[,] map, int gridX, int gridY, int w, int h)
        {
            var count = 0;
            for (var nx = gridX - 1; nx <= gridX + 1; nx++)
            for (var ny = gridY - 1; ny <= gridY + 1; ny++)
            {
                if (nx == gridX && ny == gridY) continue;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    count++;
                else if (map[nx, ny] == 1)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Ensures connectivity between regions of the map
        /// by creating corridors.
        /// </summary>
        /// <param name="floorPositions">The set of floor positions to connect.</param>
        /// <param name="map">The 2D array representing the map.</param>
        private void EnsureConnectivity(HashSet<Vector2Int> floorPositions, int[,] map)
        {
            var visited = new HashSet<Vector2Int>();

            var regions = (from pos in floorPositions
                where !visited.Contains(pos)
                select FloodFill(pos, floorPositions, visited)).ToList();

            if (regions.Count <= 1)
                return;

            var n = regions.Count;
            var parent = Enumerable.Range(0, n).ToArray();
            System.Func<int, int> find = null;
            find = x => parent[x] == x ? x : parent[x] = find(parent[x]);

            var pairs = new List<(int a, int b, float dist, Vector2Int A, Vector2Int B)>();

            for (var i = 0; i < n; i++)
            for (var j = i + 1; j < n; j++)
            {
                var minD = float.MaxValue;
                Vector2Int cA = Vector2Int.zero, cB = Vector2Int.zero;
                foreach (var a in regions[i])
                foreach (var b in regions[j])
                {
                    var d = Vector2Int.Distance(a, b);
                    if (!(d < minD)) continue;
                    minD = d;
                    cA = a;
                    cB = b;
                }

                if (minD <= maxCorridorDistance)
                    pairs.Add((i, j, minD, cA, cB));
            }

            pairs.Sort((p, q) => p.dist.CompareTo(q.dist));

            foreach (var (i, j, _, a, b) in pairs)
            {
                if (find(i) == find(j)) continue;
                var corridor = CreateCorridor(a, b, floorPositions);
                SmoothCorridor(corridor, map);
                Unite(i, j);
            }

            return;

            void Unite(int a, int b)
            {
                parent[find(a)] = find(b);
            }
        }

        /// <summary>
        /// Flood fills the map starting from a given position,
        /// collecting all connected floor positions
        /// and marking them as visited.
        /// </summary>
        /// <param name="start">The starting position for the flood fill.</param>
        /// <param name="floorPositions">The set of floor positions to check against.</param>
        /// <param name="visited">The set of visited positions to avoid reprocessing.</param>
        /// <returns>A set of positions representing the connected region.</returns>
        private static HashSet<Vector2Int> FloodFill(Vector2Int start, HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> visited)
        {
            var region = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!visited.Add(cur)) continue;
                region.Add(cur);
                foreach (var dir in Utils.Utils.Directions)
                {
                    var n = cur + dir;
                    if (floorPositions.Contains(n) && !visited.Contains(n))
                        queue.Enqueue(n);
                }
            }

            return region;
        }

        /// <summary>
        /// Creates a corridor between two points,
        /// ensuring the corridor is wide enough
        /// and adds the corridor positions to the floor positions set.
        /// </summary>
        /// <param name="start">The starting position of the corridor.</param>
        /// <param name="end">The ending position of the corridor.</param>
        /// <param name="floorPositions">The set of floor positions to add the corridor to.</param>
        /// <returns>A list of positions representing the corridor path.</returns>
        private List<Vector2Int> CreateCorridor(Vector2Int start, Vector2Int end, HashSet<Vector2Int> floorPositions)
        {
            var path = new List<Vector2Int>();
            var current = start;
            var rng = new System.Random();
            var lastDir = Vector2Int.zero;

            while (current != end)
            {
                path.Add(current);
                floorPositions.Add(current);

                // Determine direction
                var moveX = current.x != end.x && (current.y == end.y || rng.Next(2) == 0);
                Vector2Int dir;
                dir = moveX
                    ? new Vector2Int((int)Mathf.Sign(end.x - current.x), 0)
                    : new Vector2Int(0, (int)Mathf.Sign(end.y - current.y));

                lastDir = dir != Vector2Int.zero ? dir : lastDir;

                for (var w = 1; w < corridorWidth; w++)
                {
                    var perp = new Vector2Int(-lastDir.y, lastDir.x);
                    floorPositions.Add(current + perp * w);
                    floorPositions.Add(current - perp * w);
                }

                current += dir;
            }

            path.Add(end);
            floorPositions.Add(end);

            for (var w = 1; w < corridorWidth; w++)
            {
                var perp = new Vector2Int(-lastDir.y, lastDir.x);
                floorPositions.Add(end + perp * w);
                floorPositions.Add(end - perp * w);
            }

            return path;
        }

        /// <summary>
        /// Smooths the corridor by applying cellular automata rules
        /// to the corridor area.
        /// </summary>
        /// <param name="corridor">The list of positions representing the corridor.</param>
        /// <param name="map">The 2D array representing the map.</param>
        private void SmoothCorridor(List<Vector2Int> corridor, int[,] map)
        {
            var minX = corridor.Min(p => p.x) - 2;
            var maxX = corridor.Max(p => p.x) + 2;
            var minY = corridor.Min(p => p.y) - 2;
            var maxY = corridor.Max(p => p.y) + 2;
            
            minX = Mathf.Clamp(minX, 0, width - 1);
            minY = Mathf.Clamp(minY, 0, height - 1);
            maxX = Mathf.Clamp(maxX, 0, width - 1);
            maxY = Mathf.Clamp(maxY, 0, height - 1);
            
            int w = maxX - minX + 1, h = maxY - minY + 1;
            var sub = new int[w, h];

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
                sub[x, y] = map[minX + x, minY + y];

            for (var i = 0; i < 3; i++)
                SmoothMap(sub, w, h);

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
                if (sub[x, y] == 1)
                    map[minX + x, minY + y] = 1;
        }
    }
}