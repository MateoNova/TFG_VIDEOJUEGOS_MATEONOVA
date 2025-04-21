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
        /// Generates a dungeon using the currently selected generator.
        /// </summary>
        public void Generate()
        {
            var gen = GeneratorService.Instance.CurrentGenerator;
            var painter = gen?.TilemapPainter;
            if (gen == null || painter == null) return;

            // 1) Genera posiciones...
            var positions = gen.RunGeneration(true, gen.Origin);

            // 2) Limpia el mapa
            painter.ResetAllTiles();

            // 3) Obtén presets y coverages
            var presets = painter.GetAllPresets();
            var coverages = painter.GetPresetCoverages();

            // Si por algún motivo no coinciden, rebalancea
            if (coverages.Count != presets.Count)
            {
                Debug.LogWarning("Coverages y presets fuera de sync, rebalanceando...");
                painter.RebalanceCoverages();
                coverages = painter.GetPresetCoverages();
            }

            // 4) Reparte según % de cobertura
            int total = positions.Count;
            int painted = 0;
            var list = positions.ToList();

            for (int i = 0; i < presets.Count; i++)
            {
                int count = Mathf.RoundToInt(total * (coverages[i] / 100f));
                var sub = list.GetRange(painted, Mathf.Min(count, list.Count - painted));

                painter.AddAndSelectPreset(presets[i]);
                painter.PaintWalkableTiles(sub);

                painted += sub.Count;
            }

            // 5) Muro final
            WallGenerator.GenerateWalls(positions, painter);
        }


        /// <summary>
        /// Clears the current dungeon using the selected generator.
        /// </summary>
        public void ClearDungeon()
        {
            // Clears the dungeon if a generator is available.
            GeneratorService.Instance.CurrentGenerator?.ClearDungeon();
        }

        /// <summary>
        /// Saves the current dungeon to a JSON file.
        /// </summary>
        public void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;

            GeneratorService.Instance.CurrentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Loads a dungeon from a JSON file.
        /// </summary>
        public void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                GeneratorService.Instance.CurrentGenerator.LoadDungeon(path);
            }
        }

        /// <summary>
        /// Sets the value of the ClearDungeonToggle property.
        /// </summary>
        /// <param name="newValue">The new value for the ClearDungeonToggle property.</param>
        public void SetClearDungeon(bool newValue)
        {
            ClearDungeonToggle = newValue;
        }
    }
}