using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    public class StyleManager
    {
        private bool _showStyle = true;
        private readonly GeneratorSelection _generatorSelection;
        private Vector2 _floorScrollPosition;
        private SerializedObject _tilemapPainterObject;
        private readonly Dictionary<string, Vector2> _wallScrollPositions = new();

        public StyleManager(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        public void Draw()
        {
            _showStyle = EditorGUILayout.Foldout(_showStyle, "Style", true);
            if (_showStyle)
            {
                DrawTilemapPainterSettings();
            }
        }

        private void DrawTilemapPainterSettings()
        {
            if (!_generatorSelection.CurrentGenerator || !_generatorSelection.CurrentGenerator.TilemapPainter) return;

            _generatorSelection.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement = EditorGUILayout.Toggle(
                new GUIContent("Random Walkable Tiles Placement",
                    "Toggle to place walkable tiles randomly or based on probabilities"),
                _generatorSelection.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement);

            DrawTileGroupSettings(ref _floorScrollPosition, "walkableTileBases", "walkableTilesPriorities",
                "Add floor tile", true, 0);

            DrawWallTileSettings();
        }

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

                    var clearLabel = "Clear all " + (isWalkable ? "floor" : "wall") + " tiles";
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

                    var selectLabel = "Select " + (isWalkable ? "floor" : "wall") + " tiles from folder";
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

            scrollPosition = localScrollPosition;
        }

        private void DrawWallTileSettings()
        {
            if (!_generatorSelection.CurrentGenerator || !_generatorSelection.CurrentGenerator.TilemapPainter) return;

            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
            _tilemapPainterObject.Update();

            EditorGUILayoutExtensions.DrawSectionTitle("Wall Tile Settings");

            var wallFields = typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.FieldType == typeof(TileBase) && f.IsDefined(typeof(WallTileGroupAttribute), false));

            var groupedFields = wallFields.GroupBy(f =>
            {
                var attr = (WallTileGroupAttribute)f.GetCustomAttribute(typeof(WallTileGroupAttribute));
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
                EditorGUILayout.BeginHorizontal();
                foreach (var field in group)
                {
                    var wallProp = _tilemapPainterObject.FindProperty(field.Name);
                    if (wallProp != null)
                    {
                        string label = ObjectNames.NicifyVariableName(field.Name);
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

        private void DrawWallTilePreview(SerializedProperty tileProperty, string label, int controlID)
        {
            if (tileProperty == null)
            {
                Debug.LogError("tileProperty es null");
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
                tileProperty.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject() as TileBase;
            }
        }
    }
}