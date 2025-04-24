using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEditor;
using UnityEngine;
using GeneratorService = Models.Editor.GeneratorService;

namespace Controllers.Editor
{
    /// <summary>
    /// Controller responsible for managing actions related to dungeon generation, clearing, saving, and loading.
    /// </summary>
    public class ActionsController
    {
        /// <summary>
        /// Indicates whether the dungeon should be cleared before generating a new one.
        /// </summary>
        public bool ClearDungeonToggle { get; private set; } = true;

        /// <summary>
        /// Generates a dungeon using the current generator and applies biome-based painting.
        /// </summary>
        public static void Generate()
        {
            var gen = GeneratorService.Instance.CurrentGenerator;
            var painter = gen?.TilemapPainter;
            if (gen == null || painter == null) return;

            var hashWalkables = gen.RunGeneration(true, gen.Origin);
            if (hashWalkables == null || hashWalkables.Count == 0)
            {
                return;
            }
            if (hashWalkables.Count == 0) return;
            
            var allWalkables = hashWalkables.ToList();

            painter.ResetAllTiles();

            var (presets, coverages) = GetActivePresetsAndCoverages(painter);
            if (presets.Count == 0) return;

            var seeds = GenerateRandomSeeds(presets, allWalkables);
            var biomeMap = BuildBiomeMap(allWalkables, seeds, coverages);

            PaintRegions(painter, presets, coverages, allWalkables, biomeMap);
        }

        /// <summary>
        /// Filters presets and coverages to include only those with coverage greater than 0.
        /// </summary>
        /// <param name="painter">The tilemap painter used for retrieving presets and coverages.</param>
        /// <returns>A tuple containing the filtered presets and their corresponding coverages.</returns>
        private static (List<TilesetPreset> presets, List<float> coverages) GetActivePresetsAndCoverages(TilemapPainter painter)
        {
            var presets = painter.GetAllPresets();
            var coverages = painter.GetPresetCoverages().Select(c => c / 100f).ToList();
        
            // Ensure both lists are synchronized in size
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
        /// Generates random seed positions for each preset.
        /// </summary>
        /// <param name="presets">The list of presets.</param>
        /// <param name="allWalkables">The list of all walkable positions.</param>
        /// <returns>A list of seed positions.</returns>
        private static List<Vector2Int> GenerateRandomSeeds(List<TilesetPreset> presets, List<Vector2Int> allWalkables)
        {
            var rng = new System.Random();
            return presets.Select(_ => allWalkables[rng.Next(allWalkables.Count)]).ToList();
        }

        /// <summary>
        /// Builds a biome map by assigning each walkable position to the closest seed based on coverage.
        /// </summary>
        /// <param name="allWalkables">The list of all walkable positions.</param>
        /// <param name="seeds">The list of seed positions.</param>
        /// <param name="coverages">The list of coverage values for each preset.</param>
        /// <returns>A dictionary mapping each position to its assigned biome index.</returns>
        private static Dictionary<Vector2Int, int> BuildBiomeMap(
            List<Vector2Int> allWalkables,
            List<Vector2Int> seeds,
            List<float> coverages)
        {
            var biomeMap = new Dictionary<Vector2Int, int>(allWalkables.Count);
            const float noiseScale = 0.1f;
            const float warpStrength = 2f;

            foreach (var pos in allWalkables)
            {
                var warped = ApplyDomainWarp(pos, noiseScale, warpStrength);
                var best = FindClosestSeed(warped, seeds, coverages);
                biomeMap[pos] = best;
            }

            return biomeMap;
        }

        /// <summary>
        /// Applies domain warping to a position using Perlin noise.
        /// </summary>
        /// <param name="pos">The original position.</param>
        /// <param name="noiseScale">The scale of the noise.</param>
        /// <param name="warpStrength">The strength of the warp effect.</param>
        /// <returns>The warped position.</returns>
        private static Vector2 ApplyDomainWarp(Vector2Int pos, float noiseScale, float warpStrength)
        {
            var nx = pos.x * noiseScale;
            var ny = pos.y * noiseScale;
            var ox = (Mathf.PerlinNoise(nx, ny) - 0.5f) * warpStrength;
            var oy = (Mathf.PerlinNoise(nx + 100f, ny + 100f) - 0.5f) * warpStrength;
            return new Vector2(pos.x + ox, pos.y + oy);
        }

        /// <summary>
        /// Finds the closest seed to a given position, considering coverage values.
        /// </summary>
        /// <param name="warped">The warped position.</param>
        /// <param name="seeds">The list of seed positions.</param>
        /// <param name="coverages">The list of coverage values for each preset.</param>
        /// <returns>The index of the closest seed.</returns>
        private static int FindClosestSeed(Vector2 warped, List<Vector2Int> seeds, List<float> coverages)
        {
            var best = 0;
            var bestD = float.MaxValue;

            for (var i = 0; i < seeds.Count; i++)
            {
                var d = Vector2.SqrMagnitude(warped - seeds[i]) / coverages[i];

                if (!(d < bestD)) continue;
                bestD = d;
                best = i;
            }

            return best;
        }

        /// <summary>
        /// Paints regions on the tilemap based on the biome map and logs coverage information.
        /// </summary>
        /// <param name="painter">The tilemap painter used for painting.</param>
        /// <param name="presets">The list of presets.</param>
        /// <param name="coverages">The list of coverage values for each preset.</param>
        /// <param name="allWalkables">The list of all walkable positions.</param>
        /// <param name="biomeMap">The biome map mapping positions to biome indices.</param>
        private static void PaintRegions(
            TilemapPainter painter,
            List<TilesetPreset> presets,
            List<float> coverages,
            List<Vector2Int> allWalkables,
            Dictionary<Vector2Int, int> biomeMap)
        {
            var allSet = new HashSet<Vector2Int>(allWalkables);
            var regions = biomeMap
                .GroupBy(kv => kv.Value)
                .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

            foreach (var (key, value) in regions)
            {
                LogCoverage(key, value.Count, allWalkables.Count, coverages[key]);

                painter.AddAndSelectPreset(presets[key]);
                painter.PaintWalkableTiles(value);

                WallGenerator.GenerateWalls(new HashSet<Vector2Int>(value), painter, allSet);
            }
        }

        /// <summary>
        /// Logs the coverage information for a specific biome region.
        /// </summary>
        /// <param name="idx">The index of the biome region.</param>
        /// <param name="regionCount">The number of tiles in the region.</param>
        /// <param name="total">The total number of walkable tiles.</param>
        /// <param name="expectedCoverage">The expected coverage percentage for the biome.</param>
        private static void LogCoverage(int idx, int regionCount, int total, float expectedCoverage)
        {
            var realPct = regionCount / (float)total * 100f;
            var expPct = expectedCoverage * 100f;

            Debug.Log($"[CoverageCheck] Region {idx}: {regionCount} tiles ({realPct:F2}%) — expected {expPct:F2}%");
        }

        /// <summary>
        /// Clears the current dungeon by resetting all tiles.
        /// </summary>
        public static void ClearDungeon() => GeneratorService.Instance.CurrentGenerator?.ClearDungeon();

        /// <summary>
        /// Saves the current dungeon to a JSON file.
        /// </summary>
        public static void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (!string.IsNullOrEmpty(path))
                GeneratorService.Instance.CurrentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Loads a dungeon from a JSON file.
        /// </summary>
        public static void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
                GeneratorService.Instance.CurrentGenerator.LoadDungeon(path);
        }

        /// <summary>
        /// Sets the value of the ClearDungeonToggle property.
        /// </summary>
        /// <param name="newValue">The new value for the toggle.</param>
        public void SetClearDungeon(bool newValue) => ClearDungeonToggle = newValue;
    }
}