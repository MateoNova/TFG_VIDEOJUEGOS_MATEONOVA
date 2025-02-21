using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class GenerationActions
    {
        # region Singleton

        private static GenerationActions _instance;

        public static GenerationActions Instance
        {
            get { return _instance ??= new GenerationActions(); }
        }

        # endregion

        # region Constants

        private const string SectionTitle = "Generation Actions";
        private const string GenerateButtonLabel = "Generate Dungeon";
        private const string ClearButtonLabel = "Clear Dungeon";
        private const string SaveButtonLabel = "Save Dungeon";
        private const string LoadButtonLabel = "Load Dungeon";
        private const string ClearButtonText = "Clear all tiles";
        private const string ClearDungeonTooltip = "This will clear all tiles before generating the dungeon";

        # endregion

        # region Fields

        private readonly GeneratorSelection _generatorSelection = GeneratorSelection.Instance;

        /// <summary>
        /// Flag to indicate whether to show the generation actions.
        /// </summary>
        private bool _showGenerationActions = true;

        /// <summary>
        /// Flag to indicate whether to clear the dungeon before generation.
        /// </summary>
        private bool _clearDungeon = true;

        # endregion

        #region Drawing

        /// <summary>
        /// Draws the generation actions UI in the editor.
        /// </summary>
        public void Draw()
        {
            // Draws a foldout with a custom style to show or hide the generation actions.
            _showGenerationActions =
                EditorGUILayout.Foldout(_showGenerationActions, SectionTitle, true, Utils.getSectionTitleStyle());

            // If the foldout is not expanded, return early.
            if (!_showGenerationActions) return;

            // Draw the dungeon actions UI.
            DrawDungeonActions();
        }

        /// <summary>
        /// Draws the dungeon actions buttons and toggle in the editor.
        /// </summary>
        private void DrawDungeonActions()
        {
            // Draws a toggle to clear the dungeon before generation.
            _clearDungeon = EditorGUILayout.Toggle(new GUIContent(ClearButtonText, ClearDungeonTooltip), _clearDungeon);
            EditorGUILayout.Space();

            // Draws buttons for generating, clearing, saving, and loading the dungeon.
            DrawActionButton(GenerateButtonLabel, Generate);
            DrawActionButton(ClearButtonLabel, ClearDungeon);
            DrawActionButton(SaveButtonLabel, SaveDungeon);
            DrawActionButton(LoadButtonLabel, LoadDungeon);
        }

        /// <summary>
        /// Draws a button and invokes the specified action when the button is clicked.
        /// </summary>
        /// <param name="label">The label of the button.</param>
        /// <param name="action">The action to invoke when the button is clicked.</param>
        private static void DrawActionButton(string label, System.Action action)
        {
            // If the button is clicked, invoke the action.
            if (!GUILayout.Button(label)) return;
            action.Invoke();
        }

        #endregion

        #region Actions

        /// <summary>
        /// Generates the dungeon using the selected generator.
        /// </summary>
        private void Generate()
        {
            if (_generatorSelection._currentGenerator)
            {
                _generatorSelection._currentGenerator.RunGeneration(_clearDungeon,
                    _generatorSelection._currentGenerator.Origin);
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        /// <summary>
        /// Clears the dungeon using the selected generator.
        /// </summary>
        private void ClearDungeon()
        {
            _generatorSelection._currentGenerator?.ClearDungeon();
        }

        /// <summary>
        /// Saves the current dungeon to a specified file path.
        /// </summary>
        private void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;

            _generatorSelection._currentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Loads a dungeon from a specified file path.
        /// </summary>
        private void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _generatorSelection._currentGenerator.LoadDungeon(path);
            }
        }

        #endregion
    }
}