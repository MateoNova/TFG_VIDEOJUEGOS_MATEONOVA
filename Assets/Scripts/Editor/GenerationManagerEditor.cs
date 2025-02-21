using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    /// <summary>
    /// Editor window for managing dungeon generation.
    /// </summary>
    public class GenerationManagerWindow : EditorWindow
    {
        #region Constants and Fields

        private InitializationManager _initializationManager = InitializationManager.Instance;
        private GeneratorSelection _generatorSelection = GeneratorSelection.Instance;
        private GeneratorSettings _generatorSettings = GeneratorSettings.Instance;
        private StyleManager _styleManager = StyleManager.Instance;


        private bool _showGenerationActions = true;


        /// <summary>
        /// Key for saved dungeon path in EditorPrefs.
        /// </summary>
        private const string SavedDungeonPathKey = "SavedDungeonPath";


        /// <summary>
        /// Flag to indicate whether to clear the dungeon before generation.
        /// </summary>
        private bool _clearDungeon = true;

        /// <summary>
        /// Flag to indicate whether the scene is initialized.
        /// </summary>
        private bool _isInitialized;


        /// <summary>
        /// Scroll position for the wall tile settings.
        /// </summary>
        private Vector2 _wallScrollPosition;

        #endregion


        private void InitScene()
        {
            _generatorSelection.RetrieveOrInitializeCachedGenerationManager();
            _generatorSelection.FindAllGenerators();
            _isInitialized = true;
            _generatorSelection.SelectGenerator(0);
        }

        private void ClearCachedData()
        {
            /*EditorPrefs.DeleteAll();

            if (_cachedGenerationManager)
            {
                DestroyImmediate(_cachedGenerationManager);
            }

            _cachedGenerationManager = null;
            _cachedPrefab = null;
            //_currentGenerator = null;
            _cachedGeneratorNames.Clear();
            //_generators.Clear();
            //_selectedGeneratorIndex = 0;
            _isInitialized = false;

            EditorApplication.delayCall += Repaint;*/
        }


        #region Initialization

        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            GetWindow<GenerationManagerWindow>("Generation Manager");
        }


        private void OnEnable()
        {
            InitScene();
        }

        #endregion

        #region GUI Drawing

        private Vector2 _globalScrollPosition;

        private void OnGUI()
        {
            _globalScrollPosition =
                EditorGUILayout.BeginScrollView(_globalScrollPosition, true, false, GUILayout.ExpandWidth(true));

            _initializationManager.Draw();

            _generatorSelection.Draw();

            /*if (_generators == null || _generators.Count == 0)
            {
                EditorGUILayout.HelpBox("No generators found in the scene.", MessageType.Warning);
            }
            else
            {*/

            _generatorSettings.Draw();

            _styleManager.Draw();


            _showGenerationActions = EditorGUILayout.Foldout(_showGenerationActions, "Generation Actions", true);
            if (_showGenerationActions)
            {
                EditorGUILayoutExtensions.DrawSectionTitle("Generation Actions");
                DrawDungeonActions();
            }
            //}

            EditorGUILayout.EndScrollView();
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

        #endregion

        #region Tile Group Drawing

        #endregion

        #region Dungeon Generation and Data Management

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

        #endregion
    }
}