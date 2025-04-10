using UnityEditor;
using UnityEngine;
using Editor.Models;

namespace Editor.Controllers
{
    public class ActionsController
    {
        private bool _clearDungeon = true;
        public bool ClearDungeonToggle => _clearDungeon;

        public void Generate()
        {
            if (GeneratorService.Instance.CurrentGenerator != null)
            {
                GeneratorService.Instance.CurrentGenerator.RunGeneration(_clearDungeon,
                    GeneratorService.Instance.CurrentGenerator.Origin);
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        public void ClearDungeon()
        {
            GeneratorService.Instance.CurrentGenerator?.ClearDungeon();
        }

        public void SaveDungeon()
        {
            string path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;
            GeneratorService.Instance.CurrentGenerator.SaveDungeon(path);
        }

        public void LoadDungeon()
        {
            string path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                GeneratorService.Instance.CurrentGenerator.LoadDungeon(path);
            }
        }

        public void SetClearDungeon(bool newValue)
        {
            _clearDungeon = newValue;
        }
    }
}