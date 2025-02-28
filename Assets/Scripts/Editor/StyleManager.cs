using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeneralUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Editor
{
    /// <summary>
    /// Manages the styles for the generator selection.
    /// </summary>
    public class StyleManager
    {
        #region Fields

        private readonly GeneratorSelection _generatorSelection;

        private VisualElement _root;

        private List<TileBase> _walkableTileBases;
        private List<int> _walkableTilesPriorities;
        private bool _randomPlacement;

        #endregion

        #region General Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleManager"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public StyleManager(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        /// <summary>
        /// Creates the UI for the style manager.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
        public VisualElement CreateUI()
        {
            if (_generatorSelection.CurrentGenerator.TilemapPainter == null) return null;
            _walkableTileBases = _generatorSelection.CurrentGenerator.TilemapPainter.walkableTileBases;
            _walkableTilesPriorities = _generatorSelection.CurrentGenerator.TilemapPainter.walkableTilesPriorities;
            _randomPlacement = _generatorSelection.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement;

            if (_root == null)
            {
                _root = StyleUtils.SimpleContainer();
            }
            else
            {
                _root.Clear();
            }

            _root.Add(CreateStyleSection());

            return _root;
        }

        /// <summary>
        /// Creates the style section UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the style section UI elements.</returns>
        private VisualElement CreateStyleSection()
        {
            var styleSection = new Foldout { text = "Style", value = true };
            styleSection.Add(CreateFloorTileSettings());
            styleSection.Add(CreateWallTileSettings());
            return styleSection;
        }

        /// <summary>
        /// Refreshes the UI.
        /// </summary>
        private void RefreshUI()
        {
            if (_root == null)
            {
                _root = StyleUtils.SimpleContainer();
            }
            else
            {
                _root.Clear();
            }

            _root.MarkDirtyRepaint();
            CreateUI();
        }

        #endregion

        #region Walkable Tiles - Floor Tile Settings

        /// <summary>
        /// Creates the floor tile settings UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the floor tile settings UI elements.</returns>
        private VisualElement CreateFloorTileSettings()
        {
            var floorTileSettings = new Foldout { text = "Floor Tile Settings", value = true };
            floorTileSettings.Add(CreateRandomFloorPlacementToggle());

            if (!HasValidTilemapPainter()) return floorTileSettings;

            floorTileSettings.Add(CreateWalkableOptionsButtons());
            floorTileSettings.Add(CreateWalkableTileGroupSettings());
            return floorTileSettings;
        }

        /// <summary>
        /// Creates the random floor placement toggle UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the random floor placement toggle UI elements.</returns>
        private VisualElement CreateRandomFloorPlacementToggle()
        {
            var container = StyleUtils.HorizontalContainerCentered();
            var label = StyleUtils.LabelForToggle("¿Random Floor Placement?");
            container.Add(label);

            var toggle = new Toggle { value = _randomPlacement };

            toggle.RegisterValueChangedCallback(evt =>
            {
                _generatorSelection.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement = evt.newValue;
                EditorUtility.SetDirty(_generatorSelection.CurrentGenerator.TilemapPainter);
                RefreshUI();
            });
            container.Add(toggle);

            return container;
        }

        /// <summary>
        /// Creates the walkable options buttons UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the walkable options buttons UI elements.</returns>
        private VisualElement CreateWalkableOptionsButtons()
        {
            var container = StyleUtils.HorizontalContainerCentered();

            container.Add(CreateTileToUIButton("Add Floor tile"));
            container.Add(CreateClearTilesButton("Clear all floor tiles", true));
            container.Add(CreateSelectWalkableTilesFromFolderButton("Select floor tiles from folder", true));

            return container;
        }

        /// <summary>
        /// Creates a button to add a tile to the UI.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <returns>A <see cref="Button"/> to add a tile to the UI.</returns>
        private Button CreateTileToUIButton(string buttonText)
        {
            var button = new Button(() =>
            {
                _walkableTileBases.Add(null);
                _walkableTilesPriorities.Add(0);
                RefreshUI();
            })
            {
                text = buttonText
            };

            return button;
        }

        /// <summary>
        /// Creates a button to clear tiles.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <param name="isWalkable">Indicates whether the tiles are walkable.</param>
        /// <returns>A <see cref="Button"/> to clear tiles.</returns>
        private Button CreateClearTilesButton(string buttonText, bool isWalkable)
        {
            var button = new Button(() =>
            {
                if (isWalkable)
                    _generatorSelection.CurrentGenerator.TilemapPainter.RemoveAllWalkableTiles();
                else
                    _generatorSelection.CurrentGenerator.TilemapPainter.RemoveAllWallTiles();

                RefreshUI();
            })
            {
                text = buttonText
            };

            return button;
        }

        /// <summary>
        /// Creates a button to select walkable tiles from a folder.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <param name="isWalkable">Indicates whether the tiles are walkable.</param>
        /// <returns>A <see cref="Button"/> to select walkable tiles from a folder.</returns>
        private Button CreateSelectWalkableTilesFromFolderButton(string buttonText, bool isWalkable)
        {
            var button = new Button(() =>
            {
                var path = EditorUtility.OpenFolderPanel("Select a folder", "", "");
                if (isWalkable)
                {
                    _generatorSelection.CurrentGenerator.TilemapPainter.SelectWalkableTilesFromFolder(path);
                }

                AssetDatabase.Refresh();
                RefreshUI();
            })
            {
                text = buttonText
            };

            return button;
        }

        /// <summary>
        /// Checks if the tilemap painter is valid.
        /// </summary>
        /// <returns>True if the tilemap painter is valid, otherwise false.</returns>
        private bool HasValidTilemapPainter()
        {
            return _generatorSelection.CurrentGenerator &&
                   _generatorSelection.CurrentGenerator.TilemapPainter;
        }

        #endregion

        #region Walkable Tiles - Tile Group Settings

        /// <summary>
        /// Creates the walkable tile group settings UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the walkable tile group settings UI elements.</returns>
        private VisualElement CreateWalkableTileGroupSettings()
        {
            var container = new VisualElement();

            if (!IsGeneratorSelectionValid())
            {
                Debug.LogError("Generator selection or its properties are not properly initialized.");
                return container;
            }

            var walkableTiles = _generatorSelection.CurrentGenerator.TilemapPainter.walkableTileBases;
            var horizontalContainer = StyleUtils.HorizontalContainerWrapped();

            for (var index = 0; index < walkableTiles.Count; index++)
            {
                var tileContainer = CreateTileContainer(walkableTiles, index);
                horizontalContainer.Add(tileContainer);
            }

            container.Add(horizontalContainer);
            return container;
        }

        /// <summary>
        /// Checks if the generator selection is valid.
        /// </summary>
        /// <returns>True if the generator selection is valid, otherwise false.</returns>
        private bool IsGeneratorSelectionValid()
        {
            return _generatorSelection != null &&
                   _generatorSelection.CurrentGenerator != null &&
                   _generatorSelection.CurrentGenerator.TilemapPainter != null;
        }

        /// <summary>
        /// Creates a container for a tile.
        /// </summary>
        /// <param name="walkableTiles">The list of walkable tiles.</param>
        /// <param name="index">The index of the tile.</param>
        /// <returns>A <see cref="VisualElement"/> containing the tile container UI elements.</returns>
        private VisualElement CreateTileContainer(List<TileBase> walkableTiles, int index)
        {
            var walkableTile = walkableTiles[index];

            var tileContainer = StyleUtils.TileContainer();

            var label = GetLaberFromTile(walkableTile);
            tileContainer.Add(label);

            var imguiPreviewContainer = CreateIMGUIContainer(walkableTiles, index, label);
            imguiPreviewContainer.style.height = Utils.GemImGuiHeight();
            tileContainer.Add(imguiPreviewContainer);

            if (_generatorSelection.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement) return tileContainer;

            var priorityContainer = AddPriorityToTilesUI(index);
            tileContainer.Add(priorityContainer);

            return tileContainer;
        }

        /// <summary>
        /// Creates an IMGUI container for a tile.
        /// </summary>
        /// <param name="walkableTiles">The list of walkable tiles.</param>
        /// <param name="currentIndex">The index of the current tile.</param>
        /// <param name="label">The label for the tile.</param>
        /// <returns>An <see cref="IMGUIContainer"/> containing the IMGUI container UI elements.</returns>
        private IMGUIContainer CreateIMGUIContainer(List<TileBase> walkableTiles, int currentIndex, Label label)
        {
            return new IMGUIContainer(() =>
            {
                var currentTile = walkableTiles[currentIndex];
                var previewTexture = GetPreviewTexture(currentTile);

                var size = Utils.GetPreviewTileSize();
                if (GUILayout.Button(previewTexture, GUILayout.Width(size), GUILayout.Height(size)))
                {
                    EditorGUIUtility.ShowObjectPicker<TileBase>(currentTile, false, "", currentIndex);
                }

                UpdateTileOnSelection(currentIndex, label);
            });
        }

        /// <summary>
        /// Gets the preview texture for a tile.
        /// </summary>
        /// <param name="currentTile">The current tile.</param>
        /// <returns>The preview texture for the tile.</returns>
        private static Texture GetPreviewTexture(TileBase currentTile)
        {
            if (currentTile == null) return EditorGUIUtility.IconContent(Utils.GetDefaultIconContent()).image;

            var previewTexture = AssetPreview.GetAssetPreview(currentTile);
            if (previewTexture == null)
            {
                previewTexture = (Texture2D)EditorGUIUtility.ObjectContent(currentTile, typeof(TileBase)).image;
            }

            return previewTexture;
        }

        /// <summary>
        /// Updates the tile on selection.
        /// </summary>
        /// <param name="currentIndex">The index of the current tile.</param>
        /// <param name="label">The label for the tile.</param>
        private void UpdateTileOnSelection(int currentIndex, Label label)
        {
            if (Event.current == null || Event.current.commandName != Utils.GetObjectSelectorUpdateCommand()) return;

            var pickerControlId = EditorGUIUtility.GetObjectPickerControlID();

            if (pickerControlId != currentIndex) return;

            var newTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;

            if (newTile == null) return;

            _generatorSelection.CurrentGenerator.TilemapPainter.walkableTileBases[currentIndex] = newTile;
            var newName = newTile.name.Replace("floor", "", StringComparison.OrdinalIgnoreCase);
            label.text = Utils.AddSpacesToCamelCase(newName);
        }

        /// <summary>
        /// Gets the label for a tile.
        /// </summary>
        /// <param name="walkableTile">The walkable tile.</param>
        /// <returns>The label for the tile.</returns>
        private static Label GetLaberFromTile(TileBase walkableTile)
        {
            var labelText = walkableTile?.name ?? "No selected";
            labelText = labelText.Replace("floor", "", StringComparison.OrdinalIgnoreCase);
            labelText = Utils.AddSpacesToCamelCase(labelText);

            return StyleUtils.LabelForTile(labelText);
        }

        /// <summary>
        /// Adds priority to the tiles UI.
        /// </summary>
        /// <param name="index">The index of the tile.</param>
        /// <returns>A <see cref="VisualElement"/> containing the priority UI elements.</returns>
        private VisualElement AddPriorityToTilesUI(int index)
        {
            var container = StyleUtils.HorizontalContainerCentered();

            if (_randomPlacement) return container;

            var label = StyleUtils.LabelForIntField("Priority;");

            var intField = StyleUtils.SimpleIntField(_walkableTilesPriorities[index]);

            intField.RegisterValueChangedCallback(evt =>
            {
                _walkableTilesPriorities[index] = evt.newValue;
                EditorUtility.SetDirty(_generatorSelection.CurrentGenerator.TilemapPainter);
            });

            container.Add(label);
            container.Add(intField);

            return container;
        }

        #endregion

        #region Wall Tiles - Wall Tile Settings

        /// <summary>
        /// Creates the wall tile settings UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the wall tile settings UI elements.</returns>
        private VisualElement CreateWallTileSettings()
        {
            var container = new VisualElement();

            if (!HasValidTilemapPainter())
                return container;

            var groupedWallFields = GetGroupedFields<WallTileGroupAttribute>(attr => attr.GroupName);

            foreach (var group in groupedWallFields)
            {
                var foldout = CreateFoldoutForGroup(group);
                container.Add(foldout);
            }

            return container;
        }

        /// <summary>
        /// Gets the grouped fields by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="groupSelector">The group selector function.</param>
        /// <returns>An enumerable of grouped fields.</returns>
        private static IEnumerable<IGrouping<string, FieldInfo>> GetGroupedFields<TAttribute>(
            Func<TAttribute, string> groupSelector) where TAttribute : Attribute
        {
            return typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.FieldType == typeof(TileBase) && f.IsDefined(typeof(TAttribute), false))
                .GroupBy(f => groupSelector(f.GetCustomAttribute<TAttribute>()));
        }

        /// <summary>
        /// Creates a foldout for a group of fields.
        /// </summary>
        /// <param name="group">The group of fields.</param>
        /// <returns>A <see cref="Foldout"/> containing the foldout UI elements.</returns>
        private Foldout CreateFoldoutForGroup(IGrouping<string, FieldInfo> group)
        {
            var foldout = new Foldout { text = group.Key, value = true };
            var horizontalContainer = StyleUtils.HorizontalContainerWrapped();

            foreach (var field in group)
            {
                var tileContainer = CreateTileContainerForField(field);
                horizontalContainer.Add(tileContainer);
            }

            foldout.Add(horizontalContainer);
            return foldout;
        }

        /// <summary>
        /// Creates a container for a field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A <see cref="VisualElement"/> containing the field container UI elements.</returns>
        private VisualElement CreateTileContainerForField(FieldInfo field)
        {
            var tileContainer = StyleUtils.TileContainer();
            var label = CreateLabelForField(field);
            tileContainer.Add(label);

            var imguiContainer = CreateIMGUIContainerForField(field);
            imguiContainer.style.height = 60;
            tileContainer.Add(imguiContainer);

            return tileContainer;
        }

        /// <summary>
        /// Creates a label for a field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A <see cref="Label"/> containing the label UI elements.</returns>
        private static Label CreateLabelForField(FieldInfo field)
        {
            var labelText = CleanWallLabel(field.Name);
            return StyleUtils.LabelForTile(labelText);
        }

        /// <summary>
        /// Creates an IMGUI container for a field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>An <see cref="IMGUIContainer"/> containing the IMGUI container UI elements.</returns>
        private IMGUIContainer CreateIMGUIContainerForField(FieldInfo field)
        {
            var controlID = field.Name.GetHashCode() & 0x7FFFFFFF;

            return new IMGUIContainer(() =>
            {
                if (_generatorSelection == null) return;

                var tilemapPainter = _generatorSelection.CurrentGenerator.TilemapPainter;
                if (tilemapPainter == null) return;
                var currentTile = field.GetValue(tilemapPainter) as TileBase;
                var previewTexture = GetPreviewTexture(currentTile);

                var size = Utils.GetPreviewTileSize();
                if (GUILayout.Button(previewTexture, GUILayout.Width(size), GUILayout.Height(size)))
                {
                    EditorGUIUtility.ShowObjectPicker<TileBase>(currentTile, false, "", controlID);
                }

                UpdateTileOnSelection(field, controlID);
            });
        }

        /// <summary>
        /// Updates the tile on selection.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="controlID">The control ID.</param>
        private void UpdateTileOnSelection(FieldInfo field, int controlID)
        {
            if (Event.current == null || Event.current.commandName != Utils.GetObjectSelectorUpdateCommand()) return;

            var pickerControlID = EditorGUIUtility.GetObjectPickerControlID();
            if (pickerControlID != controlID) return;

            var newTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;
            if (newTile == null) return;

            var tilemapPainter = _generatorSelection.CurrentGenerator.TilemapPainter;
            field.SetValue(tilemapPainter, newTile);
        }

        /// <summary>
        /// Cleans the wall label.
        /// </summary>
        /// <param name="original">The original label.</param>
        /// <returns>The cleaned wall label.</returns>
        private static string CleanWallLabel(string original)
        {
            var label = ObjectNames.NicifyVariableName(original);
            label = label.Replace("wall", "", StringComparison.OrdinalIgnoreCase);
            label = label.Replace("inner", "", StringComparison.OrdinalIgnoreCase);
            label = label.Replace("triple", "", StringComparison.OrdinalIgnoreCase);
            var parts = label.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("\n", parts);
        }

        #endregion
    }
}