using Editor.Models;
using UnityEditor;
using UnityEngine;

namespace Editor.Controllers
{
    public class ActionsController
    {
        public bool _clearDungeon = true;

        internal void Generate()
        {
            if (GeneratorService.Instance.CurrentGenerator)
            {
                GeneratorService.Instance.CurrentGenerator.RunGeneration(_clearDungeon,
                    GeneratorService.Instance.CurrentGenerator.Origin);
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        /// <summary>
        /// Clears the dungeon.
        /// </summary>
        internal void ClearDungeon()
        {
            GeneratorService.Instance.CurrentGenerator?.ClearDungeon();
        }

        /// <summary>
        /// Saves the dungeon to a file.
        /// </summary>
        internal void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;

            GeneratorService.Instance.CurrentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Loads the dungeon from a file.
        /// </summary>
        internal void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                GeneratorService.Instance.CurrentGenerator.LoadDungeon(path);
            }
        }

        public void clearToggle(bool evtNewValue)
        {
            _clearDungeon = evtNewValue;

        }
    }
}