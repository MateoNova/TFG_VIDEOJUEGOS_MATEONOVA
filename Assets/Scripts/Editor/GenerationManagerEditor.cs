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
        
        private readonly Dictionary<string, Vector2> _wallScrollPositions = new Dictionary<string, Vector2>();
        private InitializationManager _initializationManager = InitializationManager.Instance;
        private GeneratorSelection _generatorSelection = GeneratorSelection.Instance;
        private GeneratorSettings _generatorSettings = GeneratorSettings.Instance;


        private bool _showStyle = true;
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
            //}

            EditorGUILayout.EndScrollView();
        }


        


        


        private void DrawTilemapPainterSettings()
        {
            if (!_generatorSelection._currentGenerator || !_generatorSelection._currentGenerator.TilemapPainter) return;

            _generatorSelection._currentGenerator.TilemapPainter.randomWalkableTilesPlacement = EditorGUILayout.Toggle(
                new GUIContent("Random Walkable Tiles Placement",
                    "Toggle to place walkable tiles randomly or based on probabilities"),
                _generatorSelection._currentGenerator.TilemapPainter.randomWalkableTilesPlacement);

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

        private void DrawTileGroupSettings(ref Vector2 scrollPosition, string tileBasesPropName,
            string tilePrioritiesPropName, string addTileButtonLabel, bool isWalkable, int controlIdOffset)
        {
            _tilemapPainterObject = new SerializedObject(_generatorSelection._currentGenerator.TilemapPainter);
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
                            _generatorSelection._currentGenerator.TilemapPainter.RemoveAllWalkableTiles();
                        }
                        else
                        {
                            _generatorSelection._currentGenerator.TilemapPainter.RemoveAllWallTiles();
                        }

                        _tilemapPainterObject.ApplyModifiedProperties();
                        Repaint();
                    }

                    var selectLabel = "Select " + (isWalkable ? "floor" : "wall") + " tiles from folder";
                    if (GUILayout.Button(selectLabel))
                    {
                        var path = EditorUtility.OpenFolderPanel("Select a folder", "", "");
                        _generatorSelection._currentGenerator.TilemapPainter.SelectFromFolder(isWalkable, path);
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
                                    if (Utils.ShouldDisplayField(_tilemapPainterObject, tilePrioritiesPropName,
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
                    _generatorSelection._currentGenerator.TilemapPainter.RemoveTileAtPosition(index, isWalkable);
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
            if (_generatorSelection._currentGenerator)
            {
                _generatorSelection._currentGenerator.RunGeneration(_clearDungeon, _generatorSelection._currentGenerator.Origin);
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

        #region Generation Manager and Cached Data

        #endregion

        

        private void DrawWallTileSettings()
        {
            if (!_generatorSelection._currentGenerator || !_generatorSelection._currentGenerator.TilemapPainter) return;

            // Actualiza el SerializedObject del TilemapPainter
            _tilemapPainterObject = new SerializedObject(_generatorSelection._currentGenerator.TilemapPainter);
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