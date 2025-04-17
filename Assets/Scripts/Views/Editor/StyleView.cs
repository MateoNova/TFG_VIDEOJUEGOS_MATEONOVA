using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Views.Attributes;
using GeneratorService = Models.Editor.GeneratorService;
using StyleController = Controllers.Editor.StyleController;
using StyleUtils = Utils.StyleUtils;
using Utils;

#if UNITY_EDITOR

namespace Views.Editor
{
    public class StyleView
    {
        /// <summary>
        /// The controller responsible for handling style-related logic.
        /// </summary>
        private VisualElement _root;

        /// <summary>
        /// The controller responsible for handling style-related logic.
        /// </summary>
        private readonly StyleController _styleController = new();

        /// <summary>
        /// The container for the UI elements of the style view.
        /// </summary>
        private List<TileBase> _walkableTileBases;

        /// <summary>
        /// The priorities for the walkable tiles.
        /// </summary>
        private List<int> _walkableTilesPriorities;

        /// <summary>
        /// Indicates whether the walkable tiles should be placed randomly.
        /// </summary>
        private bool _randomPlacement;

        /// <summary>
        /// Creates the UI for the style view.
        /// </summary>
        public VisualElement CreateUI()
        {
            if (GeneratorService.Instance.CurrentGenerator == null ||
                GeneratorService.Instance.CurrentGenerator.TilemapPainter == null)
            {
                return new VisualElement();
            }

            var tilemapPainter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
            _walkableTileBases = tilemapPainter.walkableTileBases;
            _walkableTilesPriorities = tilemapPainter.walkableTilesPriorities;
            _randomPlacement = tilemapPainter.randomWalkableTilesPlacement;

            _root ??= StyleUtils.SimpleContainer();
            _root.Clear();
            _root.Add(CreateStyleSection());

            return _root;
        }

        /// <summary>
        /// Creates the style section UI element.
        /// </summary>
        private VisualElement CreateStyleSection()
        {
            var styleSection = StyleUtils.ModernFoldout("");
            styleSection.SetLocalizedText("Style", "StyleTable");
            styleSection.Add(CreatePresetSettings());
            styleSection.Add(CreateFloorTileSettings());
            styleSection.Add(CreateWallTileSettings());
            return styleSection;
        }

        /// <summary>
        /// Creates the preset settings UI element.
        /// </summary>
        private VisualElement CreatePresetSettings()
        {
            var loadPresetButton = new Button(() =>
            {
                var presetPath = EditorUtility.OpenFilePanel("Select Preset", "Assets", "asset");
                if (string.IsNullOrEmpty(presetPath) || !presetPath.StartsWith(Application.dataPath)) return;

                presetPath = "Assets" + presetPath.Substring(Application.dataPath.Length);
                var preset = AssetDatabase.LoadAssetAtPath<TilesetPreset>(presetPath);

                if (preset != null)
                {
                    _styleController.LoadPreset(preset);
                    EditorUtility.DisplayDialog("Success", "Preset loaded successfully.", "OK");
                    RefreshUI();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to load preset.", "OK");
                }
            })
            {
                text = "Load Preset"
            };
            loadPresetButton.SetLocalizedText("loadPreset", "StyleTable");
            return loadPresetButton;
        }

        /// <summary>
        /// Creates the floor tile settings UI element.
        /// </summary>
        private VisualElement CreateFloorTileSettings()
        {
            var floorTileSettings = StyleUtils.ModernSubFoldout("");
            floorTileSettings.SetLocalizedText("FloorTileSettings", "StyleTable");
            floorTileSettings.Add(CreateRandomFloorPlacementToggle());

            if (!_styleController.HasValidTilemapPainter())
                return floorTileSettings;

            floorTileSettings.Add(CreateWalkableOptionsButtons());
            floorTileSettings.Add(CreateWalkableTileGroupSettings());
            return floorTileSettings;
        }

        /// <summary>
        /// Creates a toggle for random floor placement.
        /// </summary>
        private VisualElement CreateRandomFloorPlacementToggle()
        {
            var container = StyleUtils.HorizontalContainerCentered();
            var toggleLabel = StyleUtils.LabelForToggle("");
            toggleLabel.SetLocalizedText("RandomFloorPlacement", "StyleTable");
            container.Add(toggleLabel);

            var toggle = new Toggle { value = _randomPlacement };
            toggle.RegisterValueChangedCallback(evt =>
            {
                GeneratorService.Instance.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement = evt.newValue;
                EditorUtility.SetDirty(GeneratorService.Instance.CurrentGenerator.TilemapPainter);
                RefreshUI();
            });
            container.Add(toggle);
            return container;
        }

        /// <summary>
        /// Creates buttons for walkable tile options.
        /// </summary>
        private VisualElement CreateWalkableOptionsButtons()
        {
            var container = StyleUtils.HorizontalContainerCentered();
            container.Add(CreateUIButtonForAddTile("AddFloorTile", () =>
            {
                _walkableTileBases.Add(null);
                _walkableTilesPriorities.Add(0);
                RefreshUI();
            }));
            container.Add(CreateUIButtonForClearTiles("ClearAllFloorTiles", true));
            container.Add(CreateUIButtonForSelectTilesFromFolder("SelectFloorTilesFromFolder", true));
            return container;
        }

        /// <summary>
        /// Creates a button for adding a tile.
        /// </summary>
        private static Button CreateUIButtonForAddTile(string key, Action onClick)
        {
            var button = new Button(onClick);
            button.SetLocalizedText(key, "StyleTable");
            return button;
        }

        /// <summary>
        /// Creates a button for clearing tiles.
        /// </summary>
        private Button CreateUIButtonForClearTiles(string key, bool isWalkable)
        {
            var button = new Button(() =>
            {
                if (isWalkable)
                    GeneratorService.Instance.CurrentGenerator.TilemapPainter.RemoveAllWalkableTiles();
                else
                    GeneratorService.Instance.CurrentGenerator.TilemapPainter.RemoveAllWallTiles();

                RefreshUI();
            });
            button.SetLocalizedText(key, "StyleTable");
            return button;
        }

        /// <summary>
        /// Creates a button for selecting tiles from a folder.
        /// </summary>
        private Button CreateUIButtonForSelectTilesFromFolder(string key, bool isWalkable)
        {
            var button = new Button(() =>
            {
                var path = EditorUtility.OpenFolderPanel("Select a Folder", "", "");
                if (isWalkable)
                {
                    GeneratorService.Instance.CurrentGenerator.TilemapPainter.SelectWalkableTilesFromFolder(path);
                }

                AssetDatabase.Refresh();
                RefreshUI();
            });
            button.SetLocalizedText(key, "StyleTable");
            return button;
        }

        /// <summary>
        /// Creates the walkable tile group settings UI element.
        /// </summary>
        private VisualElement CreateWalkableTileGroupSettings()
        {
            var container = new VisualElement();

            if (!_styleController.IsGeneratorSelectionValid())
            {
                Debug.LogError("The generator selection or its properties are not properly initialized.");
                return container;
            }

            var walkableTiles = GeneratorService.Instance.CurrentGenerator.TilemapPainter.walkableTileBases;
            var horizontalContainer = StyleUtils.HorizontalContainerWrapped();

            for (var index = 0; index < walkableTiles.Count; index++)
            {
                var tileContainer = CreateTileContainerForWalkableTile(walkableTiles, index);
                horizontalContainer.Add(tileContainer);
            }

            container.Add(horizontalContainer);
            return container;
        }

        /// <summary>
        /// Creates a container for a walkable tile.
        /// </summary>
        private VisualElement CreateTileContainerForWalkableTile(List<TileBase> walkableTiles, int index)
        {
            var tile = walkableTiles[index];
            var tileContainer = StyleUtils.TileContainer();
            var tileLabel = GetLabelForWalkableTile(tile);

            tileLabel.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Delete", _ =>
                {
                    walkableTiles.RemoveAt(index);
                    _walkableTilesPriorities.RemoveAt(index);
                    RefreshUI();
                });
            }));
            tileContainer.Add(tileLabel);

            var previewContainer = CreateIMGUIContainerForWalkableTile(walkableTiles, index, tileLabel);
            previewContainer.style.height = Utils.Utils.GetPreviewTileSize();
            tileContainer.Add(previewContainer);

            if (!GeneratorService.Instance.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement)
            {
                tileContainer.Add(CreatePriorityUIForTile(index));
            }

            return tileContainer;
        }

        /// <summary>
        /// Creates a label for a walkable tile.
        /// </summary>
        private static Label GetLabelForWalkableTile(TileBase tile)
        {
            var labelText = tile != null
                ? Utils.Utils.AddSpacesToCamelCase(tile.name)
                : "No selected";
            var label = StyleUtils.LabelForTile(labelText);

            if (tile == null) label.SetLocalizedText("NoSelected", "StyleTable");

            return label;
        }

        /// <summary>
        /// Creates an IMGUI container for a walkable tile.
        /// </summary>
        private IMGUIContainer CreateIMGUIContainerForWalkableTile(List<TileBase> walkableTiles, int index,
            Label label)
        {
            return new IMGUIContainer(() =>
            {
                var currentTile = walkableTiles[index];
                var previewTexture = GetPreviewTexture(currentTile);
                var size = Utils.Utils.GetPreviewTileSize();

                if (GUILayout.Button(previewTexture, GUILayout.Width(size), GUILayout.Height(size)))
                {
                    EditorGUIUtility.ShowObjectPicker<TileBase>(currentTile, false, "", index);
                }

                UpdateWalkableTileOnSelection(index, label);
            });
        }

        /// <summary>
        /// Creates a UI element for setting the priority of a walkable tile.
        /// </summary>
        private VisualElement CreatePriorityUIForTile(int index)
        {
            var container = StyleUtils.HorizontalContainerCentered();
            var label = StyleUtils.LabelForIntField("");
            label.SetLocalizedText("Priority", "StyleTable");
            container.Add(label);

            var intField = StyleUtils.SimpleIntField(_walkableTilesPriorities[index]);
            intField.RegisterValueChangedCallback(evt =>
            {
                _walkableTilesPriorities[index] = evt.newValue;
                EditorUtility.SetDirty(GeneratorService.Instance.CurrentGenerator.TilemapPainter);
            });
            container.Add(intField);
            return container;
        }

        /// <summary>
        /// Creates the wall tile settings UI element.
        /// </summary>
        private VisualElement CreateWallTileSettings()
        {
            var container = new VisualElement();
            if (!_styleController.HasValidTilemapPainter())
                return container;

            var groupedWallFields =
                _styleController.GetGroupedFields<WallTileGroupAttribute>(attr => attr.GroupName);
            foreach (var group in groupedWallFields)
            {
                var groupFoldout = CreateFoldoutForGroup(group);
                container.Add(groupFoldout);
            }

            return container;
        }

        /// <summary>
        /// Creates a foldout for a group of wall fields.
        /// </summary>
        private Foldout CreateFoldoutForGroup(IGrouping<string, FieldInfo> group)
        {
            var foldout = StyleUtils.ModernSubFoldout("");
            foldout.SetLocalizedText(group.Key, "StyleTable");
            var horizontalContainer = StyleUtils.HorizontalContainerWrapped();

            foreach (var field in group)
            {
                var tileContainer = CreateTileContainerForWallField(field);
                horizontalContainer.Add(tileContainer);
            }

            foldout.Add(horizontalContainer);
            return foldout;
        }

        /// <summary>
        /// Creates a container for a wall field tile.
        /// </summary>
        private VisualElement CreateTileContainerForWallField(FieldInfo field)
        {
            var container = StyleUtils.TileContainer();
            var label = CreateLabelForWallField(field);
            container.Add(label);
            var imguiContainer = CreateIMGUIContainerForWallField(field);
            imguiContainer.style.height = 60;
            container.Add(imguiContainer);
            return container;
        }

        /// <summary>
        /// Creates a label for a wall field tile.
        /// </summary>
        private static Label CreateLabelForWallField(FieldInfo field)
        {
            var labelText = CleanWallLabel(field.Name);
            var label = StyleUtils.LabelForTile(labelText);
            label.SetLocalizedText(field.Name, "StyleTable");
            return label;
        }

        /// <summary>
        /// Creates an IMGUI container for a wall field tile.
        /// </summary>
        private static IMGUIContainer CreateIMGUIContainerForWallField(FieldInfo field)
        {
            var controlID = field.Name.GetHashCode() & 0x7FFFFFFF;
            return new IMGUIContainer(() =>
            {
                var tilemapPainter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;

                if (tilemapPainter == null)
                    return;

                var currentTile = field.GetValue(tilemapPainter) as TileBase;
                var previewTexture = GetPreviewTexture(currentTile);
                var size = Utils.Utils.GetPreviewTileSize();

                if (GUILayout.Button(previewTexture, GUILayout.Width(size), GUILayout.Height(size)))
                {
                    EditorGUIUtility.ShowObjectPicker<TileBase>(currentTile, false, "", controlID);
                }

                UpdateWallTileOnSelection(field, controlID);
            });
        }

        /// <summary>
        /// Gets the preview texture for a tile.
        /// </summary>
        private static Texture GetPreviewTexture(TileBase tile)
        {
            if (tile == null)
            {
                return EditorGUIUtility.IconContent(Utils.Utils.GetDefaultIconContent()).image;
            }

            Texture preview = AssetPreview.GetAssetPreview(tile);

            if (preview == null)
            {
                preview = EditorGUIUtility.ObjectContent(tile, typeof(TileBase)).image;
            }

            return preview;
        }

        /// <summary>
        /// Updates the walkable tile on selection.
        /// </summary>
        private static void UpdateWalkableTileOnSelection(int index, Label label)
        {
            if (Event.current == null || Event.current.commandName != Utils.Utils.GetObjectSelectorUpdateCommand())
                return;

            if (EditorGUIUtility.GetObjectPickerControlID() != index)
                return;

            var newTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;

            if (newTile == null) return;

            GeneratorService.Instance.CurrentGenerator.TilemapPainter.walkableTileBases[index] = newTile;
            label.text =
                Utils.Utils.AddSpacesToCamelCase(newTile.name.Replace("floor", "", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Updates the wall tile on selection.
        /// </summary>
        private static void UpdateWallTileOnSelection(FieldInfo field, int controlID)
        {
            if (Event.current == null || Event.current.commandName != Utils.Utils.GetObjectSelectorUpdateCommand())
                return;
            if (EditorGUIUtility.GetObjectPickerControlID() != controlID)
                return;

            var newTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;

            if (newTile == null) return;

            var tilemapPainter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
            field.SetValue(tilemapPainter, newTile);
        }

        /// <summary>
        /// Cleans the wall label by removing specific keywords and formatting it.
        /// </summary>
        private static string CleanWallLabel(string original)
        {
            var label = ObjectNames.NicifyVariableName(original);
            label = label.Replace("wall", "", StringComparison.OrdinalIgnoreCase)
                .Replace("inner", "", StringComparison.OrdinalIgnoreCase)
                .Replace("triple", "", StringComparison.OrdinalIgnoreCase);

            var parts = label.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return string.Join("\n", parts);
        }

        /// <summary>
        /// Refreshes the UI by clearing and recreating it.
        /// </summary>
        private void RefreshUI()
        {
            if (_root == null)
                _root = StyleUtils.SimpleContainer();
            else
                _root.Clear();
            _root.MarkDirtyRepaint();
            CreateUI();
        }
    }
}

#endif