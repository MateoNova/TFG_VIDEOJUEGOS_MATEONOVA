using Editor.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Controllers
{
    public class ActionsController
    {
        public bool _clearDungeon = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationActions"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
     
        


        /// <summary>
        /// Adds the action buttons to the UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the action buttons.</returns>
        

        /// <summary>
        /// Adds the clear toggle to the UI.
        /// </summary>
        /// <param name="actionsFoldout">The foldout to add the toggle to.</param>
        

        /// <summary>
        /// Generates the dungeon.
        /// </summary>
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