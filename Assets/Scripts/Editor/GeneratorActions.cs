using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class GenerationActions
    {
        private readonly GeneratorSelection _generatorSelection;
        private bool _showGenerationActions = true;
        private bool _clearDungeon = true;

        public GenerationActions(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        public void Draw()
        {
            _showGenerationActions =
                EditorGUILayout.Foldout(_showGenerationActions, "Generation Actions", true,
                    Utils.getSectionTitleStyle());
            if (!_showGenerationActions) return;

            DrawDungeonActions();
        }

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

        private static void DrawActionButton(string label, System.Action action)
        {
            if (!GUILayout.Button(label)) return;
            action.Invoke();
        }

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

        private void ClearDungeon()
        {
            _generatorSelection.CurrentGenerator?.ClearDungeon();
        }

        private void SaveDungeon()
        {
            var path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
            if (string.IsNullOrEmpty(path))
                return;

            _generatorSelection.CurrentGenerator.SaveDungeon(path);
        }

        private void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _generatorSelection.CurrentGenerator.LoadDungeon(path);
            }
        }
    }
}