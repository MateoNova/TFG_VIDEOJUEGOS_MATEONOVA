using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    /// <summary>
    /// A custom editor window for managing dungeon generation.
    /// </summary>
    public class GenerationManagerWindow : EditorWindow
    {
        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";
        private const string SavedDungeonPathKey = "SavedDungeonPath";

        private List<BaseGenerator> _generators = new();
        private int _selectedGeneratorIndex;
        private BaseGenerator _currentGenerator;
        private bool _clearDungeon = true;
        private bool _isInitialized;

        private List<string> _cachedGeneratorNames = new();
        private GameObject _cachedGenerationManager;
        private static GameObject _cachedPrefab;

        /// <summary>
        /// Shows the Generation Manager window.
        /// </summary>
        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            GetWindow<GenerationManagerWindow>("Generation Manager");
        }

        /// <summary>
        /// Initializes the scene when the window is enabled.
        /// </summary>
        private void OnEnable()
        {
            InitScene();
        }

        /// <summary>
        /// Initializes the scene by retrieving or instantiating the Generation Manager and finding all generators.
        /// </summary>
        private void InitScene()
        {
            _cachedGenerationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
            FindAllGenerators();
            _isInitialized = true;
            SelectGenerator(0);
        }

        /// <summary>
        /// Draws the GUI for the Generation Manager window.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayoutExtensions.DrawSectionTitle("Initialization");
            DrawButtons();

            EditorGUILayoutExtensions.DrawSectionTitle("Generator Selection");
            DrawGeneratorSelection();

            if (_generators == null || _generators.Count == 0)
            {
                EditorGUILayout.HelpBox("No generators found in the scene.", MessageType.Warning);
                return;
            }

            EditorGUILayoutExtensions.DrawSectionTitle("Generator Settings");
            DrawGeneratorSettings();

            EditorGUILayoutExtensions.DrawSectionTitle("Style");
            DrawTilemapPainterSettings();

            EditorGUILayoutExtensions.DrawSectionTitle("Generation Actions");
            DrawDungeonActions();
        }

        /// <summary>
        /// Draws the buttons for clearing cached data and initializing the scene.
        /// </summary>
        private void DrawButtons()
        {
            EditorGUILayoutExtensions.Horizontal(() =>
            {
                if (GUILayout.Button("Clear and delete"))
                {
                    ClearCachedData();
                }

                if (GUILayout.Button("Initialize Scene"))
                {
                    InitScene();
                }
            });
        }

        /// <summary>
        /// Draws the generator selection dropdown.
        /// </summary>
        private void DrawGeneratorSelection()
        {
            _selectedGeneratorIndex = EditorGUILayout.Popup("Select Generator", _selectedGeneratorIndex,
                _cachedGeneratorNames.ToArray());
            if (_isInitialized)
            {
                SelectGenerator(_selectedGeneratorIndex);
            }
        }

        /// <summary>
        /// Draws the settings for the selected generator.
        /// </summary>
        private void DrawGeneratorSettings()
        {
            if (!_currentGenerator) return;

            EditorGUILayoutExtensions.Vertical(() =>
            {
                var generatorObject = new SerializedObject(_currentGenerator);
                var property = generatorObject.GetIterator();
                property.NextVisible(true);

                while (property.NextVisible(false))
                {
                    var field = _currentGenerator.GetType().GetField(property.name,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        var conditionalAttribute =
                            (ConditionalFieldAttribute)Attribute.GetCustomAttribute(field,
                                typeof(ConditionalFieldAttribute));
                        if (conditionalAttribute != null)
                        {
                            var conditionField = _currentGenerator.GetType()
                                .GetField(conditionalAttribute.ConditionFieldName,
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (conditionField != null)
                            {
                                var conditionValue = (bool)conditionField.GetValue(_currentGenerator);
                                if (!conditionValue)
                                {
                                    continue;
                                }
                            }
                        }
                    }

                    EditorGUILayout.PropertyField(property, true);
                }

                generatorObject.ApplyModifiedProperties();
            }, "box");
        }

        /// <summary>
        /// Draws the settings for the TilemapPainter.
        /// </summary>
        private void DrawTilemapPainterSettings()
        {
            if (!_currentGenerator || !_currentGenerator.getTilemapPainter()) return;

            EditorGUILayoutExtensions.Horizontal(() =>
            {
                var painterObject = new SerializedObject(_currentGenerator.getTilemapPainter());
                var walkableTileBaseProperty = painterObject.FindProperty("walkableTileBase");
                var wallTileBaseProperty = painterObject.FindProperty("wallTileBase");

                DrawTileBasePreview(walkableTileBaseProperty, "Walkable Tile Base", 1);
                GUILayout.Space(10); // Add space between the previews
                DrawTileBasePreview(wallTileBaseProperty, "Wall Tile Base", 2);

                painterObject.ApplyModifiedProperties();
            });
        }

        /// <summary>
        /// Draws a preview of the TileBase with a label and allows selection.
        /// </summary>
        /// <param name="tileBaseProperty">The SerializedProperty of the TileBase.</param>
        /// <param name="label">The label for the TileBase.</param>
        /// <param name="controlID">The control ID for the object picker.</param>
        private static void DrawTileBasePreview(SerializedProperty tileBaseProperty, string label, int controlID)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(64));
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(64), GUILayout.Height(20));
            var tileBase = tileBaseProperty.objectReferenceValue as TileBase;
            if (tileBase)
            {
                var previewTexture = AssetPreview.GetAssetPreview(tileBase);
                if (previewTexture)
                {
                    if (GUILayout.Button(previewTexture, GUILayout.Width(64), GUILayout.Height(64)))
                    {
                        EditorGUIUtility.ShowObjectPicker<TileBase>(tileBase, false, "", controlID);
                    }

                    if (Event.current.commandName == "ObjectSelectorUpdated" &&
                        EditorGUIUtility.GetObjectPickerControlID() == controlID)
                    {
                        tileBaseProperty.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject() as TileBase;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Select Tile", GUILayout.Width(64), GUILayout.Height(64)))
                {
                    EditorGUIUtility.ShowObjectPicker<TileBase>(null, false, "", controlID);
                }

                if (Event.current.commandName == "ObjectSelectorUpdated" &&
                    EditorGUIUtility.GetObjectPickerControlID() == controlID)
                {
                    tileBaseProperty.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject() as TileBase;
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the buttons for dungeon generation actions.
        /// </summary>
        private void DrawDungeonActions()
        {
            _clearDungeon =
                EditorGUILayout.Toggle(
                    new GUIContent("Clear all tiles", "This will clear all tiles before generating the dungeon"),
                    _clearDungeon);
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Dungeon"))
            {
                Generate();
            }

            if (GUILayout.Button("Clear Dungeon"))
            {
                _currentGenerator.ClearDungeon();
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

        /// <summary>
        /// Loads a dungeon from a file.
        /// </summary>
        private void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _currentGenerator.LoadDungeon(path);
            }
        }

        /// <summary>
        /// Saves the current dungeon to a file.
        /// </summary>
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

            if (string.IsNullOrEmpty(path)) return;

            if (System.IO.File.Exists(path))
            {
                var overwrite = EditorUtility.DisplayDialog("Overwrite Confirmation",
                    "The file already exists. Do you want to overwrite it?", "Yes", "No");

                if (!overwrite)
                {
                    return;
                }
            }

            _currentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Finds all generators in the scene and caches their names.
        /// </summary>
        private void FindAllGenerators()
        {
            if (_cachedGenerationManager)
            {
                _generators =
                    new List<BaseGenerator>(_cachedGenerationManager.GetComponentsInChildren<BaseGenerator>());
                _cachedGeneratorNames = GetGeneratorNames();
                if (_cachedGeneratorNames.Count > 0)
                {
                    EditorPrefs.SetString(CachedGeneratorNamesKey, string.Join(",", _cachedGeneratorNames));
                }
            }
            else
            {
                Debug.LogWarning("GenerationManager not found in the scene.");
                ClearGeneratorLists();
            }
        }

        /// <summary>
        /// Clears the cached generator lists.
        /// </summary>
        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        /// <summary>
        /// Runs the generation process for the selected generator.
        /// </summary>
        private void Generate()
        {
            if (_currentGenerator)
            {
                _currentGenerator.RunGeneration(_clearDungeon, _currentGenerator.getOrigin());
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        /// <summary>
        /// Selects a generator by index.
        /// </summary>
        /// <param name="index">The index of the generator to select.</param>
        private void SelectGenerator(int index)
        {
            if (index >= 0 && index < _generators.Count)
            {
                _selectedGeneratorIndex = index;
                _currentGenerator = _generators[_selectedGeneratorIndex];
                Repaint();
            }
            else
            {
                Debug.LogWarning("Invalid generator index.");
            }
        }

        /// <summary>
        /// Gets the names of all generators in the scene.
        /// </summary>
        /// <returns>A list of generator names.</returns>
        private List<string> GetGeneratorNames()
        {
            var names = new List<string>();
            foreach (var generator in _generators)
            {
                if (generator)
                {
                    names.Add(generator.name);
                }
                else
                {
                    Debug.LogWarning("Generator is null.");
                }
            }

            return names;
        }

        /// <summary>
        /// Retrieves the cached Generation Manager from the editor preferences.
        /// </summary>
        /// <returns>The cached Generation Manager GameObject, or null if not found.</returns>
        private static GameObject RetrieveCachedGenerationManager()
        {
            var cachedGenerationManagerId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
            if (cachedGenerationManagerId != -1)
            {
                return EditorUtility.InstanceIDToObject(cachedGenerationManagerId) as GameObject;
            }

            return null;
        }

        /// <summary>
        /// Instantiates the Generation Manager prefab.
        /// </summary>
        /// <returns>The instantiated Generation Manager GameObject.</returns>
        private GameObject InstantiateGenerationManager()
        {
            if (!_cachedPrefab)
            {
                _cachedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            if (!_cachedPrefab)
            {
                return null;
            }

            _cachedGenerationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, _cachedGenerationManager.GetInstanceID());
            return _cachedGenerationManager;
        }

        /// <summary>
        /// Clears all cached data and resets the Generation Manager.
        /// </summary>
        private void ClearCachedData()
        {
            EditorPrefs.DeleteAll();

            DestroyImmediate(_cachedGenerationManager);
            _cachedGenerationManager = null;
            _cachedPrefab = null;
            _currentGenerator = null;
            _cachedGeneratorNames.Clear();
            _generators.Clear();
            _selectedGeneratorIndex = 0;
            _isInitialized = false;

            EditorApplication.delayCall += Repaint;
        }
    }
}