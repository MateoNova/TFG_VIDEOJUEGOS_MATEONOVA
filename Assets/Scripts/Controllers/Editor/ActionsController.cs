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
            if (GeneratorService.Instance.CurrentGenerator != null)
            {
                GeneratorService.Instance.CurrentGenerator.RunGeneration(ClearDungeonToggle,
                    GeneratorService.Instance.CurrentGenerator.Origin);
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        /// <summary>
        /// Clears the current dungeon using the selected generator.
        /// </summary>
        public static void ClearDungeon()
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