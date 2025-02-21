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

        private bool _showInitialization = true;
        private bool _showGeneratorSelection = true;
        private bool _showGeneratorSettings = true;
        private bool _showStyle = true;
        private bool _showGenerationActions = true;


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
        
          /// <summary>
    /// Cached Generation Manager GameObject.
    /// </summary>
    private GameObject _cachedGenerationManager;
    
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
    
    private void InitScene()
    {
        _cachedGenerationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
        FindAllGenerators();
        _isInitialized = true;
        SelectGenerator(0);
    }
    
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
    
    private static GameObject RetrieveCachedGenerationManager()
    {
        var cachedId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
        if (cachedId != -1)
        {
            return EditorUtility.InstanceIDToObject(cachedId) as GameObject;
        }

        return null;
    }


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

            _showInitialization = EditorGUILayout.Foldout(_showInitialization, "Initialization", true);
            if (_showInitialization)
            {
                EditorGUILayoutExtensions.DrawSectionTitle("Initialization");
                DrawButtons();
            }

            _showGeneratorSelection = EditorGUILayout.Foldout(_showGeneratorSelection, "Generator Selection", true);
            if (_showGeneratorSelection)
            {
                EditorGUILayoutExtensions.DrawSectionTitle("Generator Selection");
                DrawGeneratorSelection();
            }

            if (_generators == null || _generators.Count == 0)
            {
                EditorGUILayout.HelpBox("No generators found in the scene.", MessageType.Warning);
            }
            else
            {
                _showGeneratorSettings = EditorGUILayout.Foldout(_showGeneratorSettings, "Generator Settings", true);
                if (_showGeneratorSettings)
                {
                    EditorGUILayoutExtensions.DrawSectionTitle("Generator Settings");
                    DrawGeneratorSettings();
                }

                _showStyle = EditorGUILayout.Foldout(_showStyle, "Style", true);
                if (_showStyle)
                {
                    EditorGUILayoutExtensions.DrawSectionTitle("Style");
                    DrawTilemapPainterSettings();
                }

                _showGenerationActions = EditorGUILayout.Foldout(_showGenerationActions, "Generation Actions", true);
                if (_showGenerationActions)
                {
                    EditorGUILayoutExtensions.DrawSectionTitle("Generation Actions");
                    DrawDungeonActions();
                }
            }

            EditorGUILayout.EndScrollView();
        }


        private void DrawGeneratorSelection()
        {
            _selectedGeneratorIndex = EditorGUILayout.Popup("Select Generator", _selectedGeneratorIndex,
                _cachedGeneratorNames.ToArray());
            if (_isInitialized)
            {
                SelectGenerator(_selectedGeneratorIndex);
            }
        }

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


        private static bool ShouldDisplayField(SerializedObject serializedObject, string propertyName,
            System.Reflection.BindingFlags fieldBindingFlags = System.Reflection.BindingFlags.NonPublic,
            System.Reflection.BindingFlags conditionalFieldBindingFlags = System.Reflection.BindingFlags.NonPublic)
        {
            fieldBindingFlags |= System.Reflection.BindingFlags.Instance;
            conditionalFieldBindingFlags |= System.Reflection.BindingFlags.Instance;
            var targetObject = serializedObject.targetObject;
            var field = targetObject.GetType().GetField(propertyName, fieldBindingFlags);

            if (field == null) return true;
            var conditionalAttribute =
                (ConditionalFieldAttribute)Attribute.GetCustomAttribute(field, typeof(ConditionalFieldAttribute));

            if (conditionalAttribute == null) return true;
            var conditionField = targetObject.GetType()
                .GetField(conditionalAttribute.ConditionFieldName, conditionalFieldBindingFlags);

            if (conditionField == null) return true;
            var conditionValue = (bool)conditionField.GetValue(targetObject);

            return conditionValue;
        }


        private void DrawTilemapPainterSettings()
        {
            if (!_currentGenerator || !_currentGenerator.TilemapPainter) return;

            _currentGenerator.TilemapPainter.randomWalkableTilesPlacement = EditorGUILayout.Toggle(
                new GUIContent("Random Walkable Tiles Placement",
                    "Toggle to place walkable tiles randomly or based on probabilities"),
                _currentGenerator.TilemapPainter.randomWalkableTilesPlacement);

            DrawTileGroupSettings(ref _floorScrollPosition, "walkableTileBases", "walkableTilesPriorities",
                "Add floor tile", true, 0);
            //todo 
            /*DrawTileGroupSettings(ref _wallScrollPosition, "wallTileBases", "wallTilesPriorities",
                "Add wall tile", false, 1001);*/

            DrawWallTileSettings();
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
                        tileBasesProperty.GetArrayElementAtIndex(tileBasesProperty.arraySize - 1).objectReferenceValue =
                            null;
                        tilePrioritiesProperty.InsertArrayElementAtIndex(tilePrioritiesProperty.arraySize);
                        tilePrioritiesProperty.GetArrayElementAtIndex(tilePrioritiesProperty.arraySize - 1).intValue =
                            0;
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
                            _currentGenerator.TilemapPainter.RemoveAllWallTiles();
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
                    using var scrollScope =
                        new EditorGUILayout.ScrollViewScope(localScrollPosition, GUILayout.Height(125));
                    localScrollPosition = scrollScope.scrollPosition;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        for (var i = 0; i < tileBasesProperty.arraySize; i++)
                        {
                            var tileBaseProperty = tileBasesProperty.GetArrayElementAtIndex(i);
                            var priorityProperty = tilePrioritiesProperty.GetArrayElementAtIndex(i);

                            using (new EditorGUILayout.VerticalScope())
                            {
                                DrawTileBasePreview(tileBaseProperty, $"Tile {i + 1}", i + controlIdOffset,
                                    priorityProperty, i, isWalkable);

                                // Show and edit the priority
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (ShouldDisplayField(_tilemapPainterObject, tilePrioritiesPropName,
                                            conditionalFieldBindingFlags: System.Reflection.BindingFlags.Public))
                                        continue;

                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.LabelField("Priority:", GUILayout.Width(50));
                                    priorityProperty.intValue =
                                        EditorGUILayout.IntField(priorityProperty.intValue, GUILayout.Width(30));
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

        private void LoadDungeon()
        {
            var path = EditorUtility.OpenFilePanel("Load Dungeon", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                _currentGenerator.LoadDungeon(path);
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

            _currentGenerator.SaveDungeon(path);
        }


        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }


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

        #endregion

        private readonly Dictionary<string, Vector2> _wallScrollPositions = new Dictionary<string, Vector2>();

        private void DrawWallTileSettings()
        {
            if (!_currentGenerator || !_currentGenerator.TilemapPainter) return;

            // Actualiza el SerializedObject del TilemapPainter
            _tilemapPainterObject = new SerializedObject(_currentGenerator.TilemapPainter);
            _tilemapPainterObject.Update();

            EditorGUILayoutExtensions.DrawSectionTitle("Wall Tile Settings");

            // Obtenemos los fields del TilemapPainter de tipo TileBase que tengan el atributo WallTileGroupAttribute
            var wallFields = typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.FieldType == typeof(TileBase) && f.IsDefined(typeof(WallTileGroupAttribute), false));

            // Agrupamos por el valor del atributo
            var groupedFields = wallFields.GroupBy(f =>
            {
                var attr = (WallTileGroupAttribute)f.GetCustomAttribute(typeof(WallTileGroupAttribute));
                return attr.GroupName;
            });

            // Para cada grupo, mostramos una fila horizontal con scroll independiente
            foreach (var group in groupedFields)
            {
                string groupName = group.Key;

                // Inicializamos la scroll position para este grupo si no existe
                if (!_wallScrollPositions.ContainsKey(groupName))
                    _wallScrollPositions[groupName] = Vector2.zero;

                EditorGUILayout.LabelField(groupName, EditorStyles.boldLabel);
                _wallScrollPositions[groupName] =
                    EditorGUILayout.BeginScrollView(_wallScrollPositions[groupName], GUILayout.Height(100));
                EditorGUILayout.BeginHorizontal();
                foreach (var field in group)
                {
                    SerializedProperty wallProp = _tilemapPainterObject.FindProperty(field.Name);
                    if (wallProp != null)
                    {
                        // Utilizamos ObjectNames.NicifyVariableName para un label más amigable
                        string label = ObjectNames.NicifyVariableName(field.Name);
                        // Generamos un controlID único a partir del nombre del campo
                        int controlID = field.Name.GetHashCode() & 0x7FFFFFFF;
                        EditorGUILayout.BeginVertical();
                        DrawWallTilePreview(wallProp, label, controlID);
                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            }

            _tilemapPainterObject.ApplyModifiedProperties();
            Repaint();
        }

// Función auxiliar para dibujar la preview de cada wall tile
        private void DrawWallTilePreview(SerializedProperty tileProperty, string label, int controlID)
        {
            if (tileProperty == null)
            {
                Debug.LogError("tileProperty es null");
                return;
            }

            // Dibuja el título y el botón "X" para limpiar el tile
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Height(20));
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    // Al pulsar "X" se asigna null al campo del tile
                    tileProperty.objectReferenceValue = null;
                    _tilemapPainterObject.ApplyModifiedProperties();
                    Repaint();
                }

                GUILayout.FlexibleSpace();
            }

            // Muestra la preview del TileBase
            var tileBase = tileProperty.objectReferenceValue as TileBase;
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

            // Actualiza el tile seleccionado desde el Object Picker
            if (Event.current.commandName == "ObjectSelectorUpdated" &&
                EditorGUIUtility.GetObjectPickerControlID() == controlID)
            {
                tileProperty.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject() as TileBase;
            }
        }
    }
}