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
    /// Manages the style configuration interface, including the Tilemap Painter settings.
    /// </summary>
    public class StyleManager
    {
        #region Fields

        private bool _showStyle = true;
        private bool _showFloorTileSettings = true;
        private bool _showWallTileSettings = true;

        private Vector2 _walkableTilesScrollPosition;
        private readonly Dictionary<string, Vector2> _wallScrollPositions = new();

        private readonly GeneratorSelection _generatorSelection;
        private SerializedObject _tilemapPainterObject;

        private readonly Dictionary<string, int> _walkableTilePriorities = new();

        #endregion

        public StyleManager(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        /// <summary>
        /// Draws the style interface.
        /// </summary>
        public void Draw()
        {
            _showStyle = EditorGUILayout.Foldout(_showStyle, "Style", true, Utils.GetSectionTitleStyle());
            if (!_showStyle) return;
            EditorGUILayout.Space();

            if (!HasValidTilemapPainter()) return;

            _showFloorTileSettings = EditorGUILayout.Foldout(_showFloorTileSettings, "Floor Tile Settings", true,
                Utils.GetSubSectionTitleStyle());
            if (_showFloorTileSettings)
            {
                EditorGUILayout.Space();
                DrawFloorTileSettings();
            }

            _showWallTileSettings = EditorGUILayout.Foldout(_showWallTileSettings, "Wall Tile Settings", true);
            if (_showWallTileSettings)
            {
                DrawWallTileSettings();
            }
        }


        #region Walkable Tiles drawing

        /// <summary>
        /// Draws the floor tile settings including the random placement toggle and the tile group settings.
        /// </summary>
        private void DrawFloorTileSettings()
        {
            var tilemapPainter = _generatorSelection.CurrentGenerator.TilemapPainter;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("¿Random Floor Placement?", Utils.GetOptionStyle(), GUILayout.ExpandWidth(false));
                tilemapPainter.randomWalkableTilesPlacement =
                    EditorGUILayout.Toggle(tilemapPainter.randomWalkableTilesPlacement);
            }

            EditorGUILayout.Space();
            DrawWalkableTileGroupSettings();
        }

        /// <summary>
        /// Draws the walkable tile group settings using reflection.
        /// </summary>
        private void DrawWalkableTileGroupSettings()
        {
            if (!HasValidTilemapPainter()) return;

            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);

            if (!TryGetWalkableTileProperties(out var tileBasesProperty, out var tilePrioritiesProperty))
                return;

            var localScrollPosition = _walkableTilesScrollPosition;
            using (new EditorGUILayout.VerticalScope())
            {
                AddWalkableOptionsButtons(tileBasesProperty, tilePrioritiesProperty);

                if (tileBasesProperty.arraySize > 0)
                {
                    using var scrollScope =
                        new EditorGUILayout.ScrollViewScope(localScrollPosition, GUILayout.Height(125));
                    localScrollPosition = scrollScope.scrollPosition;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        List<int> indicesToRemove = new List<int>();

                        for (var i = 0; i < tileBasesProperty.arraySize; i++)
                        {
                            var tileBaseProperty = tileBasesProperty.GetArrayElementAtIndex(i);
                            var priorityProperty = tilePrioritiesProperty.GetArrayElementAtIndex(i);

                            var name = $"Tile {i + 1}";
                            if (tileBaseProperty.objectReferenceValue != null)
                            {
                                name = tileBaseProperty.objectReferenceValue.name;
                                _walkableTilePriorities.TryAdd(name, i);
                            }

                            using (new EditorGUILayout.VerticalScope())
                            {
                                var i1 = i;
                                DrawTilePreview(tileBaseProperty, name, i,
                                    () => { indicesToRemove.Add(i1); });

                                AddPriorityToTilesUI(tilePrioritiesProperty.name, priorityProperty);
                            }
                        }

                        foreach (var index in indicesToRemove.OrderByDescending(i => i))
                        {
                            tileBasesProperty.DeleteArrayElementAtIndex(index);
                            tilePrioritiesProperty.DeleteArrayElementAtIndex(index);
                        }
                    }
                }

                _tilemapPainterObject.ApplyModifiedProperties();
            }

            _walkableTilesScrollPosition = localScrollPosition;
        }

        #endregion

        #region Wall Tiles Drawing

        /// <summary>
        /// Draws the wall tile settings using reflection and grouping by wall tile group attribute.
        /// </summary>
        private void DrawWallTileSettings()
        {
            if (!HasValidTilemapPainter()) return;

            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
            _tilemapPainterObject.Update();

            EditorGUILayoutExtensions.DrawSectionTitle("Wall Tile Settings");

            // Se agrupan los campos decorados con WallTileGroupAttribute por su GroupName.
            var groupedWallFields = GetGroupedFields<WallTileGroupAttribute>(attr => attr.GroupName);

            foreach (var group in groupedWallFields)
            {
                var groupName = group.Key;

                if (!_wallScrollPositions.ContainsKey(groupName))
                    _wallScrollPositions[groupName] = Vector2.zero;

                EditorGUILayout.LabelField(groupName, EditorStyles.boldLabel);
                _wallScrollPositions[groupName] =
                    EditorGUILayout.BeginScrollView(_wallScrollPositions[groupName], GUILayout.Height(100));

                using (new EditorGUILayout.HorizontalScope())
                {
                    foreach (var field in group)
                    {
                        var wallProp = _tilemapPainterObject.FindProperty(field.Name);
                        if (wallProp == null) continue;

                        var label = ObjectNames.NicifyVariableName(field.Name);
                        var controlID = field.Name.GetHashCode() & 0x7FFFFFFF;

                        using (new EditorGUILayout.VerticalScope())
                        {
                            // Para walls, la acción de remover es asignar null.
                            DrawTilePreview(wallProp, label, controlID,
                                () => { wallProp.objectReferenceValue = null; });
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            _tilemapPainterObject.ApplyModifiedProperties();
        }

        #endregion


        #region Common UI Helpers

        /// <summary>
        /// Draws a tile preview with a removal button and tile selection.
        /// </summary>
        /// <param name="tileProperty">The serialized tile property.</param>
        /// <param name="label">Label for the tile.</param>
        /// <param name="controlID">Control identifier.</param>
        /// <param name="onRemove">Callback action on removal.</param>
        private void DrawTilePreview(SerializedProperty tileProperty, string label, int controlID, Action onRemove)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Height(20));
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    onRemove?.Invoke();
                    _tilemapPainterObject.ApplyModifiedProperties();
                }

                GUILayout.FlexibleSpace();
            }

            TileBase tileBase = tileProperty.objectReferenceValue as TileBase;
            if (tileBase != null)
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
                tileProperty.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject() as TileBase;
                _tilemapPainterObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws the priority UI for a tile.
        /// </summary>
        private void AddPriorityToTilesUI(string tilePrioritiesPropName, SerializedProperty priorityProperty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (Utils.ShouldDisplayField(_tilemapPainterObject, tilePrioritiesPropName,
                        conditionalFieldBindingFlags: BindingFlags.Public))
                {
                    return;
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Priority:", GUILayout.Width(50));
                priorityProperty.intValue = EditorGUILayout.IntField(priorityProperty.intValue, GUILayout.Width(30));
                GUILayout.FlexibleSpace();
            }
        }

        /// <summary>
        /// Adds buttons for tile operations (add, clear, select from folder) for walkable tiles.
        /// </summary>
        private void AddWalkableOptionsButtons(SerializedProperty tileBasesProperty,
            SerializedProperty tilePrioritiesProperty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                AddTileToUIButton("Add Floor tile", tileBasesProperty, tilePrioritiesProperty);
                AddClearTilesButton("Clear all floor tiles", true);
                AddSelectWalkableTilesFromFolder("Select floor tiles from folder", true);
            }
        }

        /// <summary>
        /// Adds a button for selecting tiles from a folder.
        /// </summary>
        private void AddSelectWalkableTilesFromFolder(string buttonLabel, bool isWalkable)
        {
            if (!GUILayout.Button(buttonLabel)) return;

            string path = EditorUtility.OpenFolderPanel("Select a folder", "", "");
            if (isWalkable)
            {
                _generatorSelection.CurrentGenerator.TilemapPainter.SelectWalkableTilesFromFolder(path);
            }

            _tilemapPainterObject.Update();
            _tilemapPainterObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Adds a button for clearing tiles.
        /// </summary>
        private void AddClearTilesButton(string buttonLabel, bool isWalkable)
        {
            if (!GUILayout.Button(buttonLabel)) return;

            if (isWalkable)
                _generatorSelection.CurrentGenerator.TilemapPainter.RemoveAllWalkableTiles();
            else
                _generatorSelection.CurrentGenerator.TilemapPainter.RemoveAllWallTiles();

            _tilemapPainterObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Adds a button to add a tile to the list.
        /// </summary>
        private void AddTileToUIButton(string buttonLabel, SerializedProperty tileBasesProperty,
            SerializedProperty tilePrioritiesProperty)
        {
            if (!GUILayout.Button(buttonLabel)) return;

            tileBasesProperty.InsertArrayElementAtIndex(tileBasesProperty.arraySize);
            tileBasesProperty.GetArrayElementAtIndex(tileBasesProperty.arraySize - 1).objectReferenceValue = null;
            tilePrioritiesProperty.InsertArrayElementAtIndex(tilePrioritiesProperty.arraySize);
            tilePrioritiesProperty.GetArrayElementAtIndex(tilePrioritiesProperty.arraySize - 1).intValue = 0;
            _tilemapPainterObject.ApplyModifiedProperties();
        }

        #endregion

        #region Reflection Helpers

        /// <summary>
        /// Retrieves grouped fields from TilemapPainter decorated with a specific attribute.
        /// </summary>
        private static IEnumerable<IGrouping<string, FieldInfo>> GetGroupedFields<TAttribute>(
            Func<TAttribute, string> groupSelector) where TAttribute : Attribute
        {
            return typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.FieldType == typeof(TileBase) && f.IsDefined(typeof(TAttribute), false))
                .GroupBy(f => groupSelector(f.GetCustomAttribute<TAttribute>()));
        }

        /// <summary>
        /// Retrieves walkable tile properties (bases and priorities) using reflection.
        /// </summary>
        private bool TryGetWalkableTileProperties(out SerializedProperty tileBasesProperty,
            out SerializedProperty tilePrioritiesProperty)
        {
            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
            var painterType = typeof(TilemapPainter);

            var tileBasesField = painterType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.IsDefined(typeof(WalkableTileGroupAttribute), false) &&
                                     ((WalkableTileGroupAttribute)f.GetCustomAttribute(
                                         typeof(WalkableTileGroupAttribute), false)).IsTileBases);

            var prioritiesField = painterType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.IsDefined(typeof(WalkableTileGroupAttribute), false) &&
                                     ((WalkableTileGroupAttribute)f.GetCustomAttribute(
                                         typeof(WalkableTileGroupAttribute), false)).IsTilePriorities);

            if (tileBasesField == null || prioritiesField == null)
            {
                Debug.LogError(
                    "Walkable tile fields not found. Ensure they are decorated with WalkableTileGroupAttribute.");
                tileBasesProperty = null;
                tilePrioritiesProperty = null;
                return false;
            }

            tileBasesProperty = _tilemapPainterObject.FindProperty(tileBasesField.Name);
            tilePrioritiesProperty = _tilemapPainterObject.FindProperty(prioritiesField.Name);
            return true;
        }

        /// <summary>
        /// Checks if the current generator and its TilemapPainter are valid.
        /// </summary>
        private bool HasValidTilemapPainter()
        {
            return _generatorSelection.CurrentGenerator &&
                   _generatorSelection.CurrentGenerator.TilemapPainter;
        }

        #endregion
    }
}