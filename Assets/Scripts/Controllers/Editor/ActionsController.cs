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
        public bool ClearDungeonToggle { get; private set; } = true;


        public void Generate()
{
    var gen     = GeneratorService.Instance.CurrentGenerator;
    var painter = gen?.TilemapPainter as TilemapPainter;
    if (gen == null || painter == null) return;

    // 1) Obtener todas las posiciones caminables
    var allWalkables = gen.RunGeneration(true, gen.Origin).ToList();
    if (allWalkables.Count == 0) return;
    int total = allWalkables.Count;

    // 2) Limpiar el tilemap
    painter.ResetAllTiles();

    // 3) Recuperar presets + coverages
    var presets   = painter.GetAllPresets();
    var coverages = painter.GetPresetCoverages().Select(c => c / 100f).ToList();

    // 4) Filtrar biomas con coverage > 0
    var active = presets
        .Select((ps, i) => new { ps, cov = coverages[i], idx = i })
        .Where(x => x.cov > 0f)
        .ToList();
    if (active.Count == 0) return;

    // Reconstruir listas filtradas
    presets   = active.Select(x => x.ps).ToList();
    coverages = active.Select(x => x.cov).ToList();

    // 5) Elegir semillas aleatorias
    var rng   = new System.Random();
    var seeds = presets.Select(_ => allWalkables[rng.Next(total)]).ToList();

    // 6) Construir mapa Voronoi ponderado + domain warp
    var biomeMap    = new Dictionary<Vector2Int, int>(total);
    float noiseScale  = 0.1f, warpStrength = 2f;
    foreach (var pos in allWalkables)
    {
        float nx = pos.x * noiseScale;
        float ny = pos.y * noiseScale;
        float ox = (Mathf.PerlinNoise(nx, ny) - 0.5f) * warpStrength;
        float oy = (Mathf.PerlinNoise(nx + 100f, ny + 100f) - 0.5f) * warpStrength;
        Vector2 warped = new Vector2(pos.x + ox, pos.y + oy);

        int   best   = 0;
        float bestD  = float.MaxValue;
        for (int i = 0; i < seeds.Count; i++)
        {
            // penalizamos distancia por coverage
            float d = Vector2.SqrMagnitude(warped - (Vector2)seeds[i]) / coverages[i];
            if (d < bestD)
            {
                bestD = d;
                best  = i;
            }
        }
        biomeMap[pos] = best;
    }

    // 7) Agrupar por bioma y mostrar logs de cobertura real vs esperada
    var allSet  = new HashSet<Vector2Int>(allWalkables);
    var regions = biomeMap
        .GroupBy(kv => kv.Value)
        .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

    foreach (var kv in regions)
    {
        int idx    = kv.Key;
        var region = kv.Value;
        int count   = region.Count;
        float realPct = count / (float)total * 100f;
        float expPct  = coverages[idx] * 100f;

        Debug.Log(
            $"[CoverageCheck] Region {idx}: {count} tiles ({realPct:F2}%) — expected {expPct:F2}%"
        );

        // Pintar suelos
        painter.AddAndSelectPreset(presets[idx]);
        painter.PaintWalkableTiles(region);

        // Pintar muros de la región
        WallGenerator.GenerateWalls(
            new HashSet<Vector2Int>(region),
            painter,
            allSet
        );
    }
}



        public void ClearDungeon() => GeneratorService.Instance.CurrentGenerator?.ClearDungeon();

        public void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (!string.IsNullOrEmpty(path))
                GeneratorService.Instance.CurrentGenerator.SaveDungeon(path);
        }

        public void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
                GeneratorService.Instance.CurrentGenerator.LoadDungeon(path);
        }

        public void SetClearDungeon(bool newValue) => ClearDungeonToggle = newValue;
    }
}