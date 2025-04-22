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
            var gen = GeneratorService.Instance.CurrentGenerator;
            var painter = gen?.TilemapPainter;
            if (gen == null || painter == null) return;

            // 1) Run generation -> all walkable positions
            var allList = gen.RunGeneration(true, gen.Origin).ToList();
            int totalCount = allList.Count;
            if (totalCount == 0) return;

            // 2) Reset map
            painter.ResetAllTiles();

            // 3) Presets & coverages
            var presets = painter.GetAllPresets();
            var coverages = painter.GetPresetCoverages();
            if (presets.Count != coverages.Count)
            {
                Debug.LogWarning("Presets/coverages mismatch; rebalancing.");
                painter.RebalanceCoverages();
                coverages = painter.GetPresetCoverages();
            }

            // 4) Compute exact counts per bioma
            var counts = new int[presets.Count];
            int sumSoFar = 0;
            for (int i = 0; i < presets.Count; i++)
            {
                if (i == presets.Count - 1)
                    counts[i] = totalCount - sumSoFar;
                else
                {
                    counts[i] = Mathf.RoundToInt(totalCount * (coverages[i] / 100f));
                    sumSoFar += counts[i];
                }
            }

            // 5) Flood-fill por bioma
            var unassigned = new HashSet<Vector2Int>(allList);
            var rng = new System.Random();

            for (int i = 0; i < presets.Count; i++)
            {
                int need = counts[i];
                if (need <= 0) continue;

                // 5.1) Escoger semilla aleatoria
                var seed = unassigned.ElementAt(rng.Next(unassigned.Count));
                var region = new HashSet<Vector2Int>();
                var queue = new Queue<Vector2Int>();
                queue.Enqueue(seed);

                // 5.2) BFS hasta need o agotarse vecinos
                while (queue.Count > 0 && region.Count < need)
                {
                    var pos = queue.Dequeue();
                    if (!unassigned.Remove(pos))
                        continue;
                    region.Add(pos);

                    // enqueue vecinos 4‑direcciones
                    foreach (var dir in Utils.Utils.Directions)
                    {
                        var nb = pos + dir;
                        if (unassigned.Contains(nb) && !region.Contains(nb))
                            queue.Enqueue(nb);
                    }
                }

                // 5.3) Si faltan, rellenar aleatoriamente
                while (region.Count < need && unassigned.Count > 0)
                {
                    var extra = unassigned.ElementAt(rng.Next(unassigned.Count));
                    unassigned.Remove(extra);
                    region.Add(extra);
                }

                // 5.4) Pintar suelo y paredes para esta región
                painter.AddAndSelectPreset(presets[i]);
                painter.PaintWalkableTiles(region);
                WallGenerator.GenerateWalls(region, painter);
            }
        }

        public void ClearDungeon()
        {
            GeneratorService.Instance.CurrentGenerator?.ClearDungeon();
        }

        public void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path)) return;
            GeneratorService.Instance.CurrentGenerator.SaveDungeon(path);
        }

        public void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (string.IsNullOrEmpty(path)) return;
            GeneratorService.Instance.CurrentGenerator.LoadDungeon(path);
        }

        public void SetClearDungeon(bool newValue)
        {
            ClearDungeonToggle = newValue;
        }
    }
}