using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Key for cached generator names in EditorPrefs.
        /// </summary>
        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";

        /// <summary>
        /// Key for cached generation manager ID in EditorPrefs.
        /// </summary>
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";

        /// <summary>
        /// Path to the Generation Manager prefab.
        /// </summary>
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";

        /// <summary>
        /// Key for saved dungeon path in EditorPrefs.
        /// </summary>
        private const string SavedDungeonPathKey = "SavedDungeonPath";

        /// <summary>
        /// List of all generators in the scene.
        /// </summary>
        private List<BaseGenerator> _generators = new();

        /// <summary>
        /// Index of the selected generator.
        /// </summary>
        private int _selectedGeneratorIndex;

        /// <summary>
        /// Currently selected generator.
        /// </summary>
        private BaseGenerator _currentGenerator;

        /// <summary>
        /// Flag to indicate whether to clear the dungeon before generation.
        /// </summary>
        private bool _clearDungeon = true;

        /// <summary>
        /// Flag to indicate whether the scene is initialized.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Cached generator names.
        /// </summary>
        private List<string> _cachedGeneratorNames = new();

        /// <summary>
        /// Cached Generation Manager GameObject.
        /// </summary>
        private GameObject _cachedGenerationManager;

        /// <summary>
        /// Cached prefab of the Generation Manager.
        /// </summary>
        private static GameObject _cachedPrefab;

        /// <summary>
        /// Serialized object for the Tilemap Painter.
        /// </summary>
        private SerializedObject _tilemapPainterObject;

        /// <summary>
        /// Scroll position for the floor tile settings.
        /// </summary>
        private Vector2 _floorScrollPosition;

        /// <summary>
        /// Scroll position for the wall tile settings.
        /// </summary>
        private Vector2 _wallScrollPosition;

        #endregion

        #region Initialization

        /// <summary>
        /// Shows the Generation Manager window.
        /// </summary>
        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            GetWindow<GenerationManagerWindow>("Generation Manager");
        }

        /// <summary>
        /// Called when the window is enabled.
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

        #endregion

        #region GUI Drawing

        /// <summary>
        /// Draws the GUI for the window.
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
        /// Draws the buttons for initialization and clearing cached data.
        /// </summary>
        private void DrawButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear and delete"))
                {
                    ClearCachedData();
                }

                if (GUILayout.Button("Initialize Scene"))
                {
                    InitScene();
                }
            }
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

            using (new EditorGUILayout.VerticalScope("box"))
            {
                SerializedObject generatorObject = new(_currentGenerator);
                var property = generatorObject.GetIterator();
                property.NextVisible(true);

                while (property.NextVisible(false))
                {
                    if (ShouldDisplayField(generatorObject, property.name))
                    {
                        EditorGUILayout.PropertyField(property, true);
                    }
                }

                generatorObject.ApplyModifiedProperties();
            }
        }


        /// <summary>
        /// Determines whether a field should be displayed based on the presence and value of a ConditionalFieldAttribute.
        /// </summary>
        /// <param name="serializedObject">The serialized object containing the field.</param>
        /// <param name="propertyName">The name of the property to check.</param>
        /// <returns>True if the field should be displayed; otherwise, false.</returns>
        private static bool ShouldDisplayField(SerializedObject serializedObject, string propertyName, 
            System.Reflection.BindingFlags fieldBindingFlags = System.Reflection.BindingFlags.NonPublic, 
            System.Reflection.BindingFlags conditionalFieldBindingFlags = System.Reflection.BindingFlags.NonPublic)
        {
            fieldBindingFlags |= System.Reflection.BindingFlags.Instance;
            conditionalFieldBindingFlags |= System.Reflection.BindingFlags.Instance;
            var targetObject = serializedObject.targetObject;
            var field = targetObject.GetType().GetField(propertyName, fieldBindingFlags );
        
            if (field == null) return true;
            var conditionalAttribute = (ConditionalFieldAttribute)Attribute.GetCustomAttribute(field, typeof(ConditionalFieldAttribute));
        
            if (conditionalAttribute == null) return true;
            var conditionField = targetObject.GetType().GetField(conditionalAttribute.ConditionFieldName, conditionalFieldBindingFlags);
        
            if (conditionField == null) return true;
            var conditionValue = (bool)conditionField.GetValue(targetObject);
            
            return conditionValue;
        }

        /// <summary>
        /// Draws the settings for the Tilemap Painter (floor and wall tiles).
        /// </summary>
        private void DrawTilemapPainterSettings()
        {
            if (!_currentGenerator || !_currentGenerator.TilemapPainter) return;

            _currentGenerator.TilemapPainter.randomWalkableTilesPlacement = EditorGUILayout.Toggle(
                new GUIContent("Random Walkable Tiles Placement",
                    "Toggle to place walkable tiles randomly or based on probabilities"),
                _currentGenerator.TilemapPainter.randomWalkableTilesPlacement);

            DrawTileGroupSettings(ref _floorScrollPosition, "walkableTileBases", "walkableTilesPriorities",
                "Add floor tile", true, 0);
            DrawTileGroupSettings(ref _wallScrollPosition, "wallTileBases", "wallTilesPriorities",
                "Add wall tile", false, 1001);
        }

        /// <summary>
        /// Draws the actions for generating, clearing, saving, and loading the dungeon.
        /// </summary>
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
                _currentGenerator?.ClearDungeon();
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

        /// <summary>
        /// Draws the tile group settings (for floor or wall tiles) including buttons and previews.
        /// </summary>
        /// <param name="scrollPosition">Scroll position (passed by reference).</param>
        /// <param name="tileBasesPropName">Serialized property name for the tile bases.</param>
        /// <param name="tilePrioritiesPropName">Serialized property name for the tile priorities.</param>
        /// <param name="addTileButtonLabel">Label for the add tile button.</param>
        /// <param name="isWalkable">Determines if the group corresponds to floor (true) or wall (false) tiles.</param>
        /// <param name="controlIdOffset">Offset for the object picker control ID.</param>
        private void DrawTileGroupSettings(ref Vector2 scrollPosition, string tileBasesPropName,
            string tilePrioritiesPropName, string addTileButtonLabel, bool isWalkable, int controlIdOffset)
        {
            _tilemapPainterObject = new SerializedObject(_currentGenerator.TilemapPainter);
            var tileBasesProperty = _tilemapPainterObject.FindProperty(tileBasesPropName);
            var tilePrioritiesProperty = _tilemapPainterObject.FindProperty(tilePrioritiesPropName);
        
            var localScrollPosition = scrollPosition;
        
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(addTileButtonLabel))
                    {
                        tileBasesProperty.InsertArrayElementAtIndex(tileBasesProperty.arraySize);
                        tileBasesProperty.GetArrayElementAtIndex(tileBasesProperty.arraySize - 1).objectReferenceValue = null;
                        tilePrioritiesProperty.InsertArrayElementAtIndex(tilePrioritiesProperty.arraySize);
                        tilePrioritiesProperty.GetArrayElementAtIndex(tilePrioritiesProperty.arraySize - 1).intValue = 0;
                        _tilemapPainterObject.ApplyModifiedProperties();
                        Repaint();
                    }
        
                    var clearLabel = "Clear all " + (isWalkable ? "floor" : "wall") + " tiles";
                    if (GUILayout.Button(clearLabel))
                    {
                        if (isWalkable)
                        {
                            _currentGenerator.TilemapPainter.RemoveAllWalkableTiles();
                        }
                        else
                        {
                            //todo 
                            //_currentGenerator.TilemapPainter.RemoveAllWallTiles();
                        }
                        _tilemapPainterObject.ApplyModifiedProperties();
                        Repaint();
                    }
        
                    var selectLabel = "Select " + (isWalkable ? "floor" : "wall") + " tiles from folder";
                    if (GUILayout.Button(selectLabel))
                    {
                        var path = EditorUtility.OpenFolderPanel("Select a folder", "", "");
                        _currentGenerator.TilemapPainter.SelectFromFolder(isWalkable, path);
                        _tilemapPainterObject.Update();
                        _tilemapPainterObject.ApplyModifiedProperties();
                        Repaint();
                    }
                }
        
                if (tileBasesProperty.arraySize > 0)
                {
                    using var scrollScope = new EditorGUILayout.ScrollViewScope(localScrollPosition, GUILayout.Height(125));
                    localScrollPosition = scrollScope.scrollPosition;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        for (var i = 0; i < tileBasesProperty.arraySize; i++)
                        {
                            var tileBaseProperty = tileBasesProperty.GetArrayElementAtIndex(i);
                            var priorityProperty = tilePrioritiesProperty.GetArrayElementAtIndex(i);
        
                            using (new EditorGUILayout.VerticalScope())
                            {
                                DrawTileBasePreview(tileBaseProperty, $"Tile {i + 1}", i + controlIdOffset, priorityProperty, i, isWalkable);
        
                                // Show and edit the priority
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (ShouldDisplayField(_tilemapPainterObject, tilePrioritiesPropName,
                                            conditionalFieldBindingFlags: System.Reflection.BindingFlags.Public))
                                        continue;
                                    
                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.LabelField("Priority:", GUILayout.Width(50));
                                    priorityProperty.intValue = EditorGUILayout.IntField(priorityProperty.intValue, GUILayout.Width(30));
                                    GUILayout.FlexibleSpace();
                                }
                            }
                        }
                    }
                }
        
                _tilemapPainterObject.ApplyModifiedProperties();
            }
        
            scrollPosition = localScrollPosition; // Assign back to the ref parameter
            Repaint();
        }

        /// <summary>
        /// Draws the preview for a TileBase.
        /// </summary>
        /// <param name="tileBaseProperty">The serialized property of the TileBase.</param>
        /// <param name="label">The label for the TileBase.</param>
        /// <param name="controlID">The control ID for the object picker.</param>
        /// <param name="priorityProperty">The serialized property for the tile priority.</param>
        /// <param name="index">The index of the tile in the array.</param>
        /// <param name="isWalkable">Determines if the tile is floor (true) or wall (false).</param>
        private void DrawTileBasePreview(SerializedProperty tileBaseProperty, string label, int controlID,
            SerializedProperty priorityProperty, int index, bool isWalkable)
        {
            if (tileBaseProperty == null || priorityProperty == null)
            {
                Debug.LogError("tileBaseProperty or priorityProperty is null");
                return;
            }


            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Height(20));

                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    _currentGenerator.TilemapPainter.RemoveTileAtPosition(index, isWalkable);
                    _tilemapPainterObject.ApplyModifiedProperties();
                    Repaint();
                }

                GUILayout.FlexibleSpace();
            }

            // Show the preview of the TileBase
            var tileBase = tileBaseProperty.objectReferenceValue as TileBase;
            if (tileBase)
            {
                Texture previewTexture = AssetPreview.GetAssetPreview(tileBase);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(previewTexture, GUILayout.Width(64), GUILayout.Height(64)))
                    {
                        EditorGUIUtility.ShowObjectPicker<TileBase>(tileBase, false, "", controlID);
                    }

                    GUILayout.FlexibleSpace();
                }
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Select Tile", GUILayout.Width(64), GUILayout.Height(64)))
                    {
                        EditorGUIUtility.ShowObjectPicker<TileBase>(null, false, "", controlID);
                    }

                    GUILayout.FlexibleSpace();
                }
            }

            if (Event.current.commandName == "ObjectSelectorUpdated" &&
                EditorGUIUtility.GetObjectPickerControlID() == controlID)
            {
                tileBaseProperty.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject() as TileBase;
            }
        }

        #endregion

        #region Dungeon Generation and Data Management

        /// <summary>
        /// Generates the dungeon using the selected generator.
        /// </summary>
        private void Generate()
        {
            if (_currentGenerator)
            {
                _currentGenerator.RunGeneration(_clearDungeon, _currentGenerator.Origin);
            }
            else
            {
                Debug.LogWarning("No generator selected.");
            }
        }

        /// <summary>
        /// Loads the dungeon from a file.
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
        /// Saves the dungeon to a file.
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

            _currentGenerator.SaveDungeon(path);
        }

        /// <summary>
        /// Finds all generators in the scene.
        /// </summary>
        private void FindAllGenerators()
        {
            if (_cachedGenerationManager != null)
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
        /// Clears the generator lists.
        /// </summary>
        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        /// <summary>
        /// Selects a generator by index.
        /// </summary>
        /// <param name="index">Index of the generator to select.</param>
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
        /// Gets the names of all generators.
        /// </summary>
        /// <returns>List of generator names.</returns>
        private List<string> GetGeneratorNames()
        {
            List<string> names = new();
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

        #endregion

        #region Generation Manager and Cached Data

        /// <summary>
        /// Retrieves the cached Generation Manager from the EditorPrefs.
        /// </summary>
        /// <returns>Cached Generation Manager GameObject.</returns>
        private static GameObject RetrieveCachedGenerationManager()
        {
            var cachedId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
            if (cachedId != -1)
            {
                return EditorUtility.InstanceIDToObject(cachedId) as GameObject;
            }

            return null;
        }

        /// <summary>
        /// Instantiates the Generation Manager prefab.
        /// </summary>
        /// <returns>Instantiated Generation Manager GameObject.</returns>
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
        /// Clears all cached data and resets the state.
        /// </summary>
        private void ClearCachedData()
        {
            EditorPrefs.DeleteAll();

            if (_cachedGenerationManager)
            {
                DestroyImmediate(_cachedGenerationManager);
            }

            _cachedGenerationManager = null;
            _cachedPrefab = null;
            _currentGenerator = null;
            _cachedGeneratorNames.Clear();
            _generators.Clear();
            _selectedGeneratorIndex = 0;
            _isInitialized = false;

            EditorApplication.delayCall += Repaint;
        }

        #endregion
    }
}