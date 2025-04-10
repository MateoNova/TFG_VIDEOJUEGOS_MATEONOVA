using UnityEditor;
using UnityEngine;
using Editor.Models;

namespace Editor.Controllers
{
    public class ActionsController
    {
        public bool ClearDungeonToggle { get; private set; } = true;

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

        public static void ClearDungeon()
        {
            GeneratorService.Instance.CurrentGenerator?.ClearDungeon();
        }

        public void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;
            GeneratorService.Instance.CurrentGenerator.SaveDungeon(path);
        }

        public void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                GeneratorService.Instance.CurrentGenerator.LoadDungeon(path);
            }
        }

        public void SetClearDungeon(bool newValue)
        {
            ClearDungeonToggle = newValue;
        }
    }
}