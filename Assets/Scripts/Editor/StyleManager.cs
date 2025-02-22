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
        # region Fields

        private bool _showStyle = true;
        private bool _showFloorTileSettings = true;
        private bool _showWallTileSettings = true;

        private Vector2 _walkableTilesScrollPosition;

        private readonly GeneratorSelection _generatorSelection;
        private SerializedObject _tilemapPainterObject;
        private readonly Dictionary<string, Vector2> _wallScrollPositions = new();

        # endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleManager"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public StyleManager(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        # region Drawing

        /// <summary>
        /// Draws the style interface.
        /// </summary>
        public void Draw()
        {
            _showStyle = EditorGUILayout.Foldout(_showStyle, "Style", true, Utils.GetSectionTitleStyle());
            if (!_showStyle) return;
            EditorGUILayout.Space();

            if (!_generatorSelection.CurrentGenerator ||
                !_generatorSelection.CurrentGenerator.TilemapPainter)
            {
                return;
            }

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


        /// <summary>
        /// Draws the wall tile settings.
        /// </summary>
        private void DrawWallTileSettings()
        {
            if (!_generatorSelection.CurrentGenerator ||
                !_generatorSelection.CurrentGenerator.TilemapPainter)
            {
                return;
            }

            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
            _tilemapPainterObject.Update();

            EditorGUILayoutExtensions.DrawSectionTitle("Wall Tile Settings");

            var wallFields = typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.FieldType == typeof(TileBase) && f.IsDefined(typeof(WallTileGroupAttribute), false));

            var groupedFields = wallFields.GroupBy(f =>
            {
                var attr = f.GetCustomAttribute<WallTileGroupAttribute>();
                return attr.GroupName;
            });

            foreach (var group in groupedFields)
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
                            DrawWallTilePreview(wallProp, label, controlID);
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            _tilemapPainterObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the preview for a tile base.
        /// </summary>
        /// <param name="tileBaseProperty">The tile base property.</param>
        /// <param name="label">The label for the tile base.</param>
        /// <param name="controlID">The control ID.</param>
        /// <param name="priorityProperty">The priority property.</param>
        /// <param name="index">The index of the tile base.</param>
        private void DrawTileBasePreview(SerializedProperty tileBaseProperty, string label, int controlID,
            SerializedProperty priorityProperty, int index)
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
                    _generatorSelection.CurrentGenerator.TilemapPainter.RemoveTileAtPosition(index);
                    _tilemapPainterObject.ApplyModifiedProperties();
                }

                GUILayout.FlexibleSpace();
            }

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

        /// <summary>
        /// Draws the preview for a wall tile.
        /// </summary>
        /// <param name="tileProperty">The tile property.</param>
        /// <param name="label">The label for the tile.</param>
        /// <param name="controlID">The control ID.</param>
        private void DrawWallTilePreview(SerializedProperty tileProperty, string label, int controlID)
        {
            if (tileProperty == null)
            {
                Debug.LogError("tileProperty is null");
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Height(20));
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    tileProperty.objectReferenceValue = null;
                    _tilemapPainterObject.ApplyModifiedProperties();
                }

                GUILayout.FlexibleSpace();
            }

            var tileBase = tileProperty.objectReferenceValue as TileBase;
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
            }
        }

        # endregion

        # region Draw walkable tiles

        /// <summary>
        /// Draws the settings for floor tiles in the Tilemap Painter.
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

            DrawWalkableTileGroupSettings("walkableTileBases", "walkableTilesPriorities", controlIdOffset: 0);
        }

        /// <summary>
        /// Draws the tile group settings.
        /// </summary>
        /// <param name="tileBasesPropName">The name of the tile bases property.</param>
        /// <param name="tilePrioritiesPropName">The name of the tile priorities property.</param>
        /// <param name="controlIdOffset">The control ID offset.</param>
        private void DrawWalkableTileGroupSettings(string tileBasesPropName,
            string tilePrioritiesPropName, int controlIdOffset)
        {
            var localScrollPosition = _walkableTilesScrollPosition;

            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
            var tileBasesProperty = _tilemapPainterObject.FindProperty(tileBasesPropName);
            var tilePrioritiesProperty = _tilemapPainterObject.FindProperty(tilePrioritiesPropName);

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
                        for (var i = 0; i < tileBasesProperty.arraySize; i++)
                        {
                            var tileBaseProperty = tileBasesProperty.GetArrayElementAtIndex(i);
                            var priorityProperty = tilePrioritiesProperty.GetArrayElementAtIndex(i);

                            using (new EditorGUILayout.VerticalScope())
                            {
                                DrawTileBasePreview(tileBaseProperty, $"Tile {i + 1}", i + controlIdOffset,
                                    priorityProperty, i);

                                AddPriorityToTilesUI(tilePrioritiesPropName, priorityProperty);
                            }
                        }
                    }
                }

                _tilemapPainterObject.ApplyModifiedProperties();
            }

            _walkableTilesScrollPosition = localScrollPosition;
        }

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
                priorityProperty.intValue = EditorGUILayout.IntField(priorityProperty.intValue,
                    GUILayout.Width(30));
                GUILayout.FlexibleSpace();
            }
        }

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

        private void AddSelectWalkableTilesFromFolder(string buttonLabel, bool isWalkable)
        {
            if (!GUILayout.Button(buttonLabel)) return;

            var path = EditorUtility.OpenFolderPanel("Select a folder", "", "");

            if (isWalkable)
            {
                _generatorSelection.CurrentGenerator.TilemapPainter.SelectWalkableTilesFromFolder(path);
            }

            _tilemapPainterObject.Update();
            _tilemapPainterObject.ApplyModifiedProperties();
        }

        private void AddClearTilesButton(string buttonLabel, bool isWalkable)
        {
            if (!GUILayout.Button(buttonLabel)) return;

            if (isWalkable)
                _generatorSelection.CurrentGenerator.TilemapPainter.RemoveAllWalkableTiles();
            else
                _generatorSelection.CurrentGenerator.TilemapPainter.RemoveAllWallTiles();

            _tilemapPainterObject.ApplyModifiedProperties();
        }

        # endregion

        # region Utils

        private void AddTileToUIButton(string buttonLabel, SerializedProperty tileBasesProperty,
            SerializedProperty tilePrioritiesProperty)
        {
            if (!GUILayout.Button(buttonLabel)) return;

            tileBasesProperty.InsertArrayElementAtIndex(tileBasesProperty.arraySize);
            tileBasesProperty.GetArrayElementAtIndex(tileBasesProperty.arraySize - 1).objectReferenceValue =
                null;
            tilePrioritiesProperty.InsertArrayElementAtIndex(tilePrioritiesProperty.arraySize);
            tilePrioritiesProperty.GetArrayElementAtIndex(tilePrioritiesProperty.arraySize - 1).intValue =
                0;
            _tilemapPainterObject.ApplyModifiedProperties();
        }

        # endregion
    }
}