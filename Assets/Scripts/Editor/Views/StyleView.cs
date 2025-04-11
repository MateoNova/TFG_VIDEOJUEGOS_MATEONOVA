using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor.Controllers;
using Editor.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Editor.Views
{
    /// <summary>
    /// Represents the view for managing style settings in the editor.
    /// Responsible for creating and managing the UI elements related to floor and wall tile styles.
    /// </summary>
    public class StyleView
    {
        /// <summary>
        /// The root container for the UI elements of the style view.
        /// </summary>
        private VisualElement _root;

        /// <summary>
        /// The controller responsible for handling style-related logic.
        /// </summary>
        private readonly StyleController _styleController = new StyleController();

        // Local data (updated based on the current TilemapPainter)
        private List<TileBase> _walkableTileBases;
        private List<int> _walkableTilesPriorities;
        private bool _randomPlacement;

        /// <summary>
        /// Creates the UI for the StyleView. Validates the existence of a generator and its TilemapPainter.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the style view UI.</returns>
        public VisualElement CreateUI()
        {
            if (GeneratorService.Instance.CurrentGenerator == null ||
                GeneratorService.Instance.CurrentGenerator.TilemapPainter == null)
            {
                Debug.LogError("The generator or its TilemapPainter is not properly initialized.");
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
        /// Creates the main style section.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the style section.</returns>
        private VisualElement CreateStyleSection()
        {
            var styleSection = new Foldout { text = "Style", value = true };
            styleSection.Add(CreateFloorTileSettings());
            styleSection.Add(CreateWallTileSettings());
            return styleSection;
        }

        #region Floor Tiles Section

        /// <summary>
        /// Creates the floor tile settings section.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the floor tile settings.</returns>
        private VisualElement CreateFloorTileSettings()
        {
            var floorTileSettings = new Foldout { text = "Floor Tile Settings", value = true };
            floorTileSettings.Add(CreateRandomFloorPlacementToggle());

            if (!_styleController.HasValidTilemapPainter())
                return floorTileSettings;

            floorTileSettings.Add(CreateWalkableOptionsButtons());
            floorTileSettings.Add(CreateWalkableTileGroupSettings());
            return floorTileSettings;
        }

        /// <summary>
        /// Creates a toggle for enabling or disabling random floor placement.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the toggle.</returns>
        private VisualElement CreateRandomFloorPlacementToggle()
        {
            var container = StyleUtils.HorizontalContainerCentered();
            container.Add(StyleUtils.LabelForToggle("Random Floor Placement?"));

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
        /// Creates buttons for managing walkable tile options.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the buttons.</returns>
        private VisualElement CreateWalkableOptionsButtons()
        {
            var container = StyleUtils.HorizontalContainerCentered();
            container.Add(CreateUIButton_AddTile("Add Floor Tile", () =>
            {
                _walkableTileBases.Add(null);
                _walkableTilesPriorities.Add(0);
                RefreshUI();
            }));
            container.Add(CreateUIButton_ClearTiles("Clear All Floor Tiles", true));
            container.Add(CreateUIButton_SelectTilesFromFolder("Select Floor Tiles from Folder", true));
            return container;
        }

        /// <summary>
        /// Creates a button for adding a tile.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <param name="onClick">The action to execute when the button is clicked.</param>
        /// <returns>A <see cref="Button"/> configured with the specified properties.</returns>
        private static Button CreateUIButton_AddTile(string buttonText, Action onClick)
        {
            return new Button(onClick) { text = buttonText };
        }

        /// <summary>
        /// Creates a button for clearing all tiles.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <param name="isWalkable">Indicates whether the button is for walkable tiles.</param>
        /// <returns>A <see cref="Button"/> configured with the specified properties.</returns>
        private Button CreateUIButton_ClearTiles(string buttonText, bool isWalkable)
        {
            return new Button(() =>
                {
                    if (isWalkable)
                        GeneratorService.Instance.CurrentGenerator.TilemapPainter.RemoveAllWalkableTiles();
                    else
                        GeneratorService.Instance.CurrentGenerator.TilemapPainter.RemoveAllWallTiles();

                    RefreshUI();
                })
                { text = buttonText };
        }

        /// <summary>
        /// Creates a button for selecting tiles from a folder.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <param name="isWalkable">Indicates whether the button is for walkable tiles.</param>
        /// <returns>A <see cref="Button"/> configured with the specified properties.</returns>
        private Button CreateUIButton_SelectTilesFromFolder(string buttonText, bool isWalkable)
        {
            return new Button(() =>
                {
                    var path = EditorUtility.OpenFolderPanel("Select a Folder", "", "");
                    if (isWalkable)
                    {
                        GeneratorService.Instance.CurrentGenerator.TilemapPainter.SelectWalkableTilesFromFolder(path);
                    }

                    AssetDatabase.Refresh();
                    RefreshUI();
                })
                { text = buttonText };
        }

        /// <summary>
        /// Creates the settings for walkable tile groups.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the settings.</returns>
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
        /// <param name="walkableTiles">The list of walkable tiles.</param>
        /// <param name="index">The index of the tile in the list.</param>
        /// <returns>A <see cref="VisualElement"/> containing the tile container.</returns>
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

            // Add the IMGUI container for selecting the tile
            var previewContainer = CreateIMGUIContainer_ForWalkableTile(walkableTiles, index, tileLabel);
            previewContainer.style.height = Utils.Utils.GetPreviewTileSize();
            tileContainer.Add(previewContainer);

            if (!GeneratorService.Instance.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement)
            {
                tileContainer.Add(CreatePriorityUI_ForTile(index));
            }

            return tileContainer;
        }

        /// <summary>
        /// Gets the label for a walkable tile.
        /// </summary>
        /// <param name="tile">The tile to get the label for.</param>
        /// <returns>A <see cref="Label"/> containing the tile's name.</returns>
        private static Label GetLabelForWalkableTile(TileBase tile)
        {
            var labelText = tile != null
                ? Utils.Utils.AddSpacesToCamelCase(tile.name.Replace("floor", "", StringComparison.OrdinalIgnoreCase))
                : "No selected";
            return StyleUtils.LabelForTile(labelText);
        }

        /// <summary>
        /// Creates an IMGUI container for a walkable tile.
        /// </summary>
        /// <param name="walkableTiles">The list of walkable tiles.</param>
        /// <param name="index">The index of the tile in the list.</param>
        /// <param name="label">The label associated with the tile.</param>
        /// <returns>An <see cref="IMGUIContainer"/> for the tile.</returns>
        private IMGUIContainer CreateIMGUIContainer_ForWalkableTile(List<TileBase> walkableTiles, int index,
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
        /// Creates the priority UI for a tile.
        /// </summary>
        /// <param name="index">The index of the tile in the list.</param>
        /// <returns>A <see cref="VisualElement"/> containing the priority UI.</returns>
        private VisualElement CreatePriorityUI_ForTile(int index)
        {
            var container = StyleUtils.HorizontalContainerCentered();
            container.Add(StyleUtils.LabelForIntField("Priority:"));
            var intField = StyleUtils.SimpleIntField(_walkableTilesPriorities[index]);
            intField.RegisterValueChangedCallback(evt =>
            {
                _walkableTilesPriorities[index] = evt.newValue;
                EditorUtility.SetDirty(GeneratorService.Instance.CurrentGenerator.TilemapPainter);
            });
            container.Add(intField);
            return container;
        }

        #endregion

        #region Wall Tiles Section

        /// <summary>
        /// Creates the wall tile settings section.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the wall tile settings.</returns>
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
        /// Creates a foldout for a group of wall tiles.
        /// </summary>
        /// <param name="group">The group of wall tiles.</param>
        /// <returns>A <see cref="Foldout"/> containing the group.</returns>
        private Foldout CreateFoldoutForGroup(IGrouping<string, FieldInfo> group)
        {
            var foldout = new Foldout { text = group.Key, value = true };
            var horizontalContainer = StyleUtils.HorizontalContainerWrapped();

            foreach (var field in group)
            {
                var tileContainer = CreateTileContainer_ForWallField(field);
                horizontalContainer.Add(tileContainer);
            }

            foldout.Add(horizontalContainer);
            return foldout;
        }

        /// <summary>
        /// Creates a container for a wall tile field.
        /// </summary>
        /// <param name="field">The field representing the wall tile.</param>
        /// <returns>A <see cref="VisualElement"/> containing the tile container.</returns>
        private VisualElement CreateTileContainer_ForWallField(FieldInfo field)
        {
            var container = StyleUtils.TileContainer();
            var label = CreateLabelForWallField(field);
            container.Add(label);
            var imguiContainer = CreateIMGUIContainer_ForWallField(field);
            imguiContainer.style.height = 60;
            container.Add(imguiContainer);
            return container;
        }

        /// <summary>
        /// Creates a label for a wall tile field.
        /// </summary>
        /// <param name="field">The field representing the wall tile.</param>
        /// <returns>A <see cref="Label"/> containing the field's name.</returns>
        private static Label CreateLabelForWallField(FieldInfo field)
        {
            var labelText = CleanWallLabel(field.Name);
            return StyleUtils.LabelForTile(labelText);
        }

        /// <summary>
        /// Creates an IMGUI container for a wall tile field.
        /// </summary>
        /// <param name="field">The field representing the wall tile.</param>
        /// <returns>An <see cref="IMGUIContainer"/> for the field.</returns>
        private static IMGUIContainer CreateIMGUIContainer_ForWallField(FieldInfo field)
        {
            // Generate a unique control ID based on the field name
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the preview texture for a tile.
        /// </summary>
        /// <param name="tile">The tile to get the preview for.</param>
        /// <returns>A <see cref="Texture"/> representing the tile's preview.</returns>
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
        /// Updates the selected walkable tile.
        /// </summary>
        /// <param name="index">The index of the tile in the list.</param>
        /// <param name="label">The label associated with the tile.</param>
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
        /// Updates the selected wall tile.
        /// </summary>
        /// <param name="field">The field representing the wall tile.</param>
        /// <param name="controlID">The control ID associated with the field.</param>
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
        /// Cleans the label for a wall tile by removing redundant words.
        /// </summary>
        /// <param name="original">The original label text.</param>
        /// <returns>The cleaned label text.</returns>
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
        /// Refreshes the entire UI.
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

        #endregion
    }
}