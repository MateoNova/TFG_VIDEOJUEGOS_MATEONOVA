using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Manages the dungeon generation actions: generate, clear, save, and load.
    /// </summary>
    public class GenerationActions
    {
        # region Fields

        private readonly GeneratorSelection _generatorSelection;
        private bool _showGenerationActions = true;
        private bool _clearDungeon = true;

        # endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationActions"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public GenerationActions(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        # region Drawing

        /// <summary>
        /// Draws the generation actions interface.
        /// </summary>
        public void Draw()
        {
            _showGenerationActions = EditorGUILayout.Foldout(_showGenerationActions, "Generation Actions", true,
                Utils.GetSectionTitleStyle());

            if (_showGenerationActions) DrawDungeonActions();
        }

        /// <summary>
        /// Draws the dungeon actions.
        /// </summary>
        private void DrawDungeonActions()
        {
            _clearDungeon =
                EditorGUILayout.Toggle(
                    new GUIContent("Clear all tiles", "This will clear all tiles before generating the dungeon"),
                    _clearDungeon);
            EditorGUILayout.Space();

            DrawActionButton("Generate Dungeon", Generate);
            DrawActionButton("Clear Dungeon", ClearDungeon);
            DrawActionButton("Save Dungeon", SaveDungeon);
            DrawActionButton("Load Dungeon", LoadDungeon);
        }

        /// <summary>
        /// Draws an action button.
        /// </summary>
        /// <param name="label">The label of the button.</param>
        /// <param name="action">The action to be invoked when the button is clicked.</param>
        private static void DrawActionButton(string label, System.Action action)
        {
            if (GUILayout.Button(label))
            {
                action.Invoke();
            }
        }

        # endregion

        # region Actions

        /// <summary>
        /// Generates the dungeon.
        /// </summary>
        private void Generate()
        {
            if (_generatorSelection.CurrentGenerator)
            {
                _generatorSelection.CurrentGenerator.RunGeneration(_clearDungeon,
                    _generatorSelection.CurrentGenerator.Origin);
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        /// <summary>
        /// Clears the dungeon.
        /// </summary>
        private void ClearDungeon()
        {
            _generatorSelection.CurrentGenerator?.ClearDungeon();
        }

        /// <summary>
        /// Saves the dungeon to a file.
        /// </summary>
        private void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;

            _generatorSelection.CurrentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Loads the dungeon from a file.
        /// </summary>
        private void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _generatorSelection.CurrentGenerator.LoadDungeon(path);
            }
        }

        # endregion
    }
}