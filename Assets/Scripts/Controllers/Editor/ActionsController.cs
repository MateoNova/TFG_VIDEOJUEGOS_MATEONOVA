using System.Collections.Generic;
using System.Linq;
using Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using GeneratorService = Models.Editor.GeneratorService;

namespace Controllers.Editor
{
    /// <summary>
    /// Provides methods for generating, clearing, saving, and loading dungeons,
    /// as well as managing the state of the dungeon generation process.
    /// </summary>
    public class ActionsController
    {
        public static bool ClearDungeonToggle { get; private set; } = true;

        /// <summary>
        /// Generates a new dungeon layout, assigns biome regions using a parallelized Burst job,
        /// paints the floors and unified walls according to biome assignments, and manages profiling.
        /// </summary>
        /// <remarks>
        /// The method performs the following steps:
        /// 1. Retrieves the current dungeon generator and its tilemap painter.
        /// 2. Runs the dungeon generation algorithm to obtain walkable tile positions.
        /// 3. Retrieves active biome presets and their coverage percentages.
        /// 4. Generates random seed positions for each biome preset.
        /// 5. Uses a Burst-compiled parallel job to assign each walkable tile to the closest biome seed,
        ///    applying domain warping and coverage weighting.
        /// 6. Builds a biome map from the job results.
        /// 7. Paints the floor tiles for each biome region.
        /// 8. Generates and paints unified wall tiles based on the biome map.
        /// Profiling samples are used to measure performance of key steps.
        /// </remarks>
        public static void Generate()
        {
            var marker = new ProfilerMarker("ActionsController.Generate");
            using (marker.Auto())
            {
                var gen = GeneratorService.Instance.CurrentGenerator;
                var painter = gen?.TilemapPainter;
                if (gen == null || painter == null) return;

                var hashWalkables = gen.RunGeneration(ClearDungeonToggle, gen.Origin);
                if (hashWalkables == null || hashWalkables.Count == 0) return;
                var allWalkables = hashWalkables.ToList();

                Profiler.BeginSample($"WalkableTiles: {allWalkables.Count}");
                Profiler.EndSample();

                var (presets, coverages) = GetActivePresetsAndCoverages(painter);
                if (presets.Count == 0) return;

                Profiler.BeginSample($"BiomeRegions: {presets.Count}");
                Profiler.EndSample();

                var seeds = GenerateRandomSeeds(presets, allWalkables);

                var tempCount = allWalkables.Count;
                var posArr = new NativeArray<int2>(tempCount, Allocator.TempJob);
                for (var i = 0; i < tempCount; i++) posArr[i] = new int2(allWalkables[i].x, allWalkables[i].y);
                var seedArr = new NativeArray<int2>(seeds.Count, Allocator.TempJob);
                for (var i = 0; i < seeds.Count; i++) seedArr[i] = new int2(seeds[i].x, seeds[i].y);
                var covArr = new NativeArray<float>(coverages.Count, Allocator.TempJob);
                for (var i = 0; i < coverages.Count; i++) covArr[i] = coverages[i];
                var outArr = new NativeArray<int>(tempCount, Allocator.TempJob);

                var job = new BuildBiomeMapJob
                {
                    Positions = posArr,
                    Seeds = seedArr,
                    Coverages = covArr,
                    OutIndices = outArr
                };
                var handle = job.Schedule(tempCount, 64);
                handle.Complete();

                var biomeMap = new Dictionary<Vector2Int, int>(tempCount);
                for (var i = 0; i < tempCount; i++)
                    biomeMap[new Vector2Int(posArr[i].x, posArr[i].y)] = outArr[i];

                posArr.Dispose();
                seedArr.Dispose();
                covArr.Dispose();
                outArr.Dispose();

                PaintFloors(painter, presets, coverages, allWalkables, biomeMap);
                GenerateUnifiedWalls(painter, presets, biomeMap);
            }
        }

        /// <summary>
        /// Paints the floor tiles for each biome region on the tilemap using the corresponding preset.
        /// </summary>
        /// <param name="painter">The TilemapPainter used to paint tiles.</param>
        /// <param name="presets">List of TilesetPresets, one for each biome.</param>
        /// <param name="coverages">List of expected coverage percentages for each biome.</param>
        /// <param name="allWalkables">List of all walkable tile positions.</param>
        /// <param name="biomeMap">
        /// Dictionary mapping each walkable tile position to its assigned biome index.
        /// </param>
        /// <remarks>
        /// This method groups all walkable tiles by their biome, logs the coverage for each region,
        /// selects the appropriate preset, and paints the floor tiles for each biome region.
        /// </remarks>
        private static void PaintFloors(
            TilemapPainter painter,
            List<TilesetPreset> presets,
            List<float> coverages,
            List<Vector2Int> allWalkables,
            Dictionary<Vector2Int, int> biomeMap)
        {
            var regions = biomeMap
                .GroupBy(kv => kv.Value)
                .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

            foreach (var (biomeIdx, tiles) in regions)
            {
                LogCoverage(biomeIdx, tiles.Count, allWalkables.Count, coverages[biomeIdx]);
                painter.AddAndSelectPreset(presets[biomeIdx]);
                painter.PaintWalkableTiles(tiles);
            }
        }

        /// <summary>
        /// Generates and paints wall tiles around walkable regions, unifying wall appearance across biome boundaries.
        /// </summary>
        /// <param name="painter">The <see cref="TilemapPainter"/> used to paint wall tiles.</param>
        /// <param name="presets">A list of <see cref="TilesetPreset"/>s, one for each biome.</param>
        /// <param name="biomeMap">
        /// A dictionary mapping each walkable tile position to its assigned biome index.
        /// </param>
        /// <remarks>
        /// This method:
        /// <list type="number">
        /// <item>Finds all positions adjacent (including diagonals) to walkable tiles that are not themselves walkable.</item>
        /// <item>For each such position, counts the influence (adjacency) of each biome.</item>
        /// <item>Assigns the wall to the biome with the most influence at that position.</item>
        /// <item>Determines the wall's visual type based on neighboring walkable tiles.</item>
        /// <item>Paints the wall tile using the appropriate biome preset and wall type.</item>
        /// </list>
        /// </remarks>
        private static void GenerateUnifiedWalls(
            TilemapPainter painter,
            List<TilesetPreset> presets,
            Dictionary<Vector2Int, int> biomeMap)
        {
            var dirs = new[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                new(1, 1), new(-1, 1), new(1, -1), new(-1, -1)
            };

            var influence = new Dictionary<Vector2Int, Dictionary<int, int>>();
            foreach (var (pos, biome) in biomeMap)
            {
                foreach (var dir in dirs)
                {
                    var nb = pos + dir;
                    if (biomeMap.ContainsKey(nb)) continue;

                    if (!influence.ContainsKey(nb))
                        influence[nb] = new Dictionary<int, int>();
                    if (!influence[nb].ContainsKey(biome))
                        influence[nb][biome] = 0;
                    influence[nb][biome]++;
                }
            }

            Profiler.BeginSample($"WallTiles: {influence.Count}");
            Profiler.EndSample();

            foreach (var (wallPos, value) in influence)
            {
                var chosenBiome = value
                    .OrderByDescending(x => x.Value)
                    .First()
                    .Key;

                var neighborBits = GetNeighborBits(wallPos, chosenBiome, biomeMap);
                var wallPosition = Utils.Utils.DetermineWallPosition(neighborBits);

                painter.AddAndSelectPreset(presets[chosenBiome]);
                painter.PaintWallTiles(new[] { wallPos }, wallPosition);
            }
        }

        /// <summary>
        /// Returns an array of booleans indicating the presence of neighboring tiles belonging to the specified biome.
        /// </summary>
        /// <param name="pos">The position to check neighbors around.</param>
        /// <param name="biome">The biome index to match neighbors against.</param>
        /// <param name="biomeMap">A dictionary mapping tile positions to their biome indices.</param>
        /// <returns>
        /// A boolean array of length 8, where each element represents the presence of a neighboring tile
        /// in the specified biome at the following directions:
        /// 0: North, 1: North-East, 2: East, 3: South-East, 4: South, 5: South-West, 6: West, 7: North-West.
        /// </returns>
        private static bool[] GetNeighborBits(
            Vector2Int pos,
            int biome,
            Dictionary<Vector2Int, int> biomeMap)
        {
            var bits = new bool[8];

            bits[0] = Has(0, 1); // N
            bits[1] = Has(1, 1); // NE
            bits[2] = Has(1, 0); // E
            bits[3] = Has(1, -1); // SE
            bits[4] = Has(0, -1); // S
            bits[5] = Has(-1, -1); // SW
            bits[6] = Has(-1, 0); // W
            bits[7] = Has(-1, 1); // NW
            return bits;

            bool Has(int dx, int dy) =>
                biomeMap.TryGetValue(new Vector2Int(pos.x + dx, pos.y + dy), out var b) && b == biome;
        }

        /// <summary>
        /// Retrieves the list of active tileset presets and their normalized coverage values from the given painter.
        /// </summary>
        /// <param name="painter">The <see cref="TilemapPainter"/> from which to obtain presets and coverages.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description>A list of active <see cref="TilesetPreset"/>s (those with coverage &gt; 0).</description></item>
        /// <item><description>A list of corresponding normalized coverage values (as floats between 0 and 1).</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// If the number of presets and coverages do not match, coverages are evenly distributed among all presets.
        /// Only presets with a positive coverage value are considered active and included in the result.
        /// </remarks>
        private static (List<TilesetPreset> presets, List<float> coverages) GetActivePresetsAndCoverages(
            TilemapPainter painter)
        {
            var presets = painter.GetAllPresets();
            var coverages = painter.GetPresetCoverages().Select(c => c / 100f).ToList();
            if (presets.Count != coverages.Count)
            {
                var count = presets.Count;
                coverages = Enumerable.Repeat(count > 0 ? 1f / count : 0f, count).ToList();
            }

            var active = presets
                .Select((ps, i) => new { ps, cov = coverages[i] })
                .Where(x => x.cov > 0f)
                .ToList();
            return (active.Select(x => x.ps).ToList(), active.Select(x => x.cov).ToList());
        }

        /// <summary>
        /// Generates a list of random seed positions from the available walkable tiles,
        /// with one seed for each tileset preset.
        /// </summary>
        /// <param name="presets">The list of <see cref="TilesetPreset"/>s, one for each biome or region.</param>
        /// <param name="allWalkables">The list of all walkable tile positions to choose seeds from.</param>
        /// <returns>
        /// A list of <see cref="Vector2Int"/> positions, each randomly selected from <paramref name="allWalkables"/>,
        /// with the number of seeds equal to the number of presets.
        /// </returns>
        private static List<Vector2Int> GenerateRandomSeeds(
            List<TilesetPreset> presets,
            List<Vector2Int> allWalkables)
        {
            var rng = new System.Random();
            return presets.Select(_ => allWalkables[rng.Next(allWalkables.Count)]).ToList();
        }


        /// <summary>
        /// Logs the actual and expected coverage percentage for a biome region.
        /// </summary>
        /// <param name="idx">The index of the biome region.</param>
        /// <param name="regionCount">The number of tiles assigned to this region.</param>
        /// <param name="total">The total number of walkable tiles.</param>
        /// <param name="expectedCoverage">The expected coverage for this region (as a value between 0 and 1).</param>
        private static void LogCoverage(int idx, int regionCount, int total, float expectedCoverage)
        {
            var realPct = regionCount / (float)total * 100f;
            var expPct = expectedCoverage * 100f;
            Debug.Log($"[CoverageCheck] Region {idx}: {regionCount} tiles ({realPct:F2}%) — expected {expPct:F2}%");
        }

        /// <summary>
        /// Clears the current dungeon by invoking the <c>ClearDungeon</c> method on the active generator,
        /// if one is available.
        /// </summary>
        public static void ClearDungeon() => GeneratorService.Instance.CurrentGenerator?.ClearDungeon();

        /// <summary>
        /// Opens a file save dialog to let the user specify a location and filename,
        /// then saves the current dungeon to the selected JSON file if a valid path is provided.
        /// </summary>
        public static void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (!string.IsNullOrEmpty(path))
                GeneratorService.Instance.CurrentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Opens a file dialog to let the user select a JSON file to load a dungeon from,
        /// then loads the dungeon using the current generator if a valid path is provided.
        /// </summary>
        public static void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
                GeneratorService.Instance.CurrentGenerator.LoadDungeon(path);
        }

        /// <summary>
        /// Sets the value of the ClearDungeon toggle.
        /// </summary>
        public static void SetClearDungeon(bool newValue) => ClearDungeonToggle = newValue;
    }

    [BurstCompile]
    internal struct BuildBiomeMapJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int2> Positions;
        [ReadOnly] public NativeArray<int2> Seeds;
        [ReadOnly] public NativeArray<float> Coverages;
        public float NoiseScale;
        public float WarpStrength;
        public NativeArray<int> OutIndices;

        public void Execute(int index)
        {
            var pos = Positions[index];
            var warped = ApplyDomainWarp(pos, NoiseScale, WarpStrength);
            var best = FindClosestSeed(warped, Seeds, Coverages);
            OutIndices[index] = best;
        }

        private static float2 ApplyDomainWarp(int2 pos, float noiseScale, float warpStrength)
        {
            var nx = pos.x * noiseScale;
            var ny = pos.y * noiseScale;
            var ox = (Mathf.PerlinNoise(nx, ny) - 0.5f) * warpStrength;
            var oy = (Mathf.PerlinNoise(nx + 100f, ny + 100f) - 0.5f) * warpStrength;
            return new float2(pos.x + ox, pos.y + oy);
        }

        private static int FindClosestSeed(float2 warped, NativeArray<int2> seeds, NativeArray<float> coverages)
        {
            var best = 0;
            var bestD = float.MaxValue;
            for (var i = 0; i < seeds.Length; i++)
            {
                var seed = new float2(seeds[i].x, seeds[i].y);
                var d = math.distancesq(warped, seed) / coverages[i];
                if (!(d < bestD)) continue;
                bestD = d;
                best = i;
            }

            return best;
        }
    }
}