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
        private readonly GeneratorSelection _generatorSelection;
        private Vector2 _floorScrollPosition;
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
            DrawTilemapPainterSettings();
        }

        /// <summary>
        /// Draws the Tilemap Painter settings.
        /// </summary>
        private void DrawTilemapPainterSettings()
        {
            if (!_generatorSelection.CurrentGenerator ||
                !_generatorSelection.CurrentGenerator.TilemapPainter)
            {
                return;
            }

            var tilemapPainter = _generatorSelection.CurrentGenerator.TilemapPainter;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("¿Random Placement?", Utils.GetOptionStyle(), GUILayout.ExpandWidth(false));
            tilemapPainter.randomWalkableTilesPlacement = EditorGUILayout.Toggle(tilemapPainter.randomWalkableTilesPlacement, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            
            DrawTileGroupSettings(ref _floorScrollPosition, "walkableTileBases", "walkableTilesPriorities",
                "Add floor tile", isWalkable: true, controlIdOffset: 0);

            DrawWallTileSettings();
        }

        /// <summary>
        /// Draws the tile group settings.
        /// </summary>
        /// <param name="scrollPosition">The scroll position.</param>
        /// <param name="tileBasesPropName">The name of the tile bases property.</param>
        /// <param name="tilePrioritiesPropName">The name of the tile priorities property.</param>
        /// <param name="addTileButtonLabel">The label for the add tile button.</param>
        /// <param name="isWalkable">Indicates if the tiles are walkable.</param>
        /// <param name="controlIdOffset">The control ID offset.</param>
        private void DrawTileGroupSettings(ref Vector2 scrollPosition, string tileBasesPropName,
            string tilePrioritiesPropName, string addTileButtonLabel, bool isWalkable, int controlIdOffset)
        {
            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
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
                    }

                    var clearLabel = $"Clear all {(isWalkable ? "floor" : "wall")} tiles";
                    if (GUILayout.Button(clearLabel))
                    {
                        if (isWalkable)
                        {
                            _generatorSelection.CurrentGenerator.TilemapPainter.RemoveAllWalkableTiles();
                        }
                        else
                        {
                            _generatorSelection.CurrentGenerator.TilemapPainter.RemoveAllWallTiles();
                        }

                        _tilemapPainterObject.ApplyModifiedProperties();
                    }

                    var selectLabel = $"Select {(isWalkable ? "floor" : "wall")} tiles from folder";
                    if (GUILayout.Button(selectLabel))
                    {
                        var path = EditorUtility.OpenFolderPanel("Select a folder", "", "");
                        _generatorSelection.CurrentGenerator.TilemapPainter.SelectFromFolder(isWalkable, path);
                        _tilemapPainterObject.Update();
                        _tilemapPainterObject.ApplyModifiedProperties();
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

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (Utils.ShouldDisplayField(_tilemapPainterObject, tilePrioritiesPropName,
                                            conditionalFieldBindingFlags: BindingFlags.Public))
                                    {
                                        continue;
                                    }

                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.LabelField("Priority:", GUILayout.Width(50));
                                    priorityProperty.intValue = EditorGUILayout.IntField(priorityProperty.intValue,
                                        GUILayout.Width(30));
                                    GUILayout.FlexibleSpace();
                                }
                            }
                        }
                    }
                }

                _tilemapPainterObject.ApplyModifiedProperties();
            }

            scrollPosition = localScrollPosition;
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
        /// <param name="isWalkable">Indicates if the tile is walkable.</param>
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
                    _generatorSelection.CurrentGenerator.TilemapPainter.RemoveTileAtPosition(index, isWalkable);
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
    }
}