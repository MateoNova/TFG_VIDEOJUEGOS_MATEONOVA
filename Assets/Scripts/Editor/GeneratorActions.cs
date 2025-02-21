using Editor;
using UnityEditor;
using UnityEngine;

public class GenerationActions
{
    
    private GeneratorSelection _generatorSelection = GeneratorSelection.Instance;

    private bool _showGenerationActions = true;

    private static GenerationActions _instance;
    public static GenerationActions Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GenerationActions();
            }

            return _instance;
        }
    }
    
    /// <summary>
    /// Flag to indicate whether to clear the dungeon before generation.
    /// </summary>
    private bool _clearDungeon = true;
    
    /// <summary>
    /// Key for saved dungeon path in EditorPrefs.
    /// </summary>
    private const string SavedDungeonPathKey = "SavedDungeonPath";

    public void Draw()
    {
        _showGenerationActions = EditorGUILayout.Foldout(_showGenerationActions, "Generation Actions", true);
        if (_showGenerationActions)
        {
            EditorGUILayoutExtensions.DrawSectionTitle("Generation Actions");
            DrawDungeonActions();
        }
    }
    
    private void DrawDungeonActions()
    {
        _clearDungeon = EditorGUILayout.Toggle(
            new GUIContent("Clear all tiles", "This will clear all tiles before generating the dungeon"),
            _clearDungeon);
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Dungeon"))
        {
            Generate();
        }

        if (GUILayout.Button("Clear Dungeon"))
        {
            _generatorSelection._currentGenerator?.ClearDungeon();
        }

        if (GUILayout.Button("Save Dungeon"))
        {
            SaveDungeon();
        }

        if (GUILayout.Button("Load Dungeon"))
        {
            LoadDungeon();
        }
    }
    
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
    
            private void LoadDungeon()
            {
                var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    _generatorSelection._currentGenerator.LoadDungeon(path);
                }
            }
    
    
            private void SaveDungeon()
            {
                var path = EditorPrefs.GetString(SavedDungeonPathKey, string.Empty);
                if (string.IsNullOrEmpty(path))
                {
                    path = EditorUtility.SaveFilePanel("Save Dungeon", "", "Dungeon.json", "json");
                    if (!string.IsNullOrEmpty(path))
                    {
                        EditorPrefs.SetString(SavedDungeonPathKey, path);
                    }
                }
    
                if (string.IsNullOrEmpty(path))
                    return;
    
                if (System.IO.File.Exists(path))
                {
                    var overwrite = EditorUtility.DisplayDialog("Overwrite Confirmation",
                        "The file already exists. Do you want to overwrite it?", "Yes", "No");
    
                    if (!overwrite)
                    {
                        return;
                    }
                }
    
                _generatorSelection._currentGenerator.SaveDungeon(path);
            }
}