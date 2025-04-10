using UnityEditor;
using UnityEngine;

namespace Editor.Controllers
{
    /// <summary>
    /// Controller for managing generation actions.
    /// </summary>
    public class ActionsController
    {
        private readonly GeneratorSelection _generatorSelection;
        public bool ClearDungeonState { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionsController"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public ActionsController(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        /// <summary>
        /// Generates the dungeon.
        /// </summary>
        public void Generate()
        {
            if (_generatorSelection.CurrentGenerator)
            {
                _generatorSelection.CurrentGenerator.RunGeneration(ClearDungeonState, _generatorSelection.CurrentGenerator.Origin);
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        /// <summary>
        /// Clears the dungeon.
        /// </summary>
        public void ClearDungeon()
        {
            _generatorSelection.CurrentGenerator?.ClearDungeon();
        }

        /// <summary>
        /// Saves the dungeon to a file.
        /// </summary>
        public void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;

            _generatorSelection.CurrentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Loads the dungeon from a file.
        /// </summary>
        public void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _generatorSelection.CurrentGenerator.LoadDungeon(path);
            }
        }
    }
}