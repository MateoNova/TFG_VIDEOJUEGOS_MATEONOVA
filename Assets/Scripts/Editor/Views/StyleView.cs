using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor.Controllers;
using Editor.Models;
using GeneralUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class StyleView
    {
        private VisualElement _root;
        private readonly StyleController _styleController = new();

        private List<TileBase> _walkableTileBases;
        private List<int> _walkableTilesPriorities;
        private bool _randomPlacement;

        /// <summary>
        /// Creates the UI for the style manager.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
        public VisualElement CreateUI()
        {
            if (GeneratorService.Instance.CurrentGenerator == null)
            {
                Debug.LogError("Current generator is null. Ensure the generator is properly initialized.");
                return null;
            }

            if (GeneratorService.Instance.CurrentGenerator.TilemapPainter == null)
            {
                Debug.LogError("TilemapPainter is null. Ensure the TilemapPainter is properly assigned.");
                return null;
            }

            if (GeneratorService.Instance.CurrentGenerator.TilemapPainter == null) return null;
            _walkableTileBases = GeneratorService.Instance.CurrentGenerator.TilemapPainter.walkableTileBases;
            _walkableTilesPriorities =
                GeneratorService.Instance.CurrentGenerator.TilemapPainter.walkableTilesPriorities;
            _randomPlacement = GeneratorService.Instance.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement;

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

        private VisualElement CreateStyleSection()
        {
            var styleSection = new Foldout { text = "Style", value = true };
            styleSection.Add(CreateFloorTileSettings());
            styleSection.Add(CreateWallTileSettings());
            return styleSection;
        }

        /// <summary>
        /// Creates the wall tile settings UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the wall tile settings UI elements.</returns>
        private VisualElement CreateWallTileSettings()
        {
            var container = new VisualElement();

            if (!_styleController.HasValidTilemapPainter())
                return container;

            var groupedWallFields = _styleController.GetGroupedFields<WallTileGroupAttribute>(attr => attr.GroupName);

            foreach (var group in groupedWallFields)
            {
                var foldout = CreateFoldoutForGroup(group);
                container.Add(foldout);
            }

            return container;
        }

        /// <summary>
        /// Creates the floor tile settings UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the floor tile settings UI elements.</returns>
        private VisualElement CreateFloorTileSettings()
        {
            var floorTileSettings = new Foldout { text = "Floor Tile Settings", value = true };
            floorTileSettings.Add(CreateRandomFloorPlacementToggle());

            if (!_styleController.HasValidTilemapPainter()) return floorTileSettings;

            floorTileSettings.Add(CreateWalkableOptionsButtons());
            floorTileSettings.Add(CreateWalkableTileGroupSettings());
            return floorTileSettings;
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
                if (GeneratorService.Instance.CurrentGenerator == null) return;

                var tilemapPainter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
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

            var tilemapPainter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
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


        private VisualElement CreateRandomFloorPlacementToggle()
        {
            var container = StyleUtils.HorizontalContainerCentered();
            var label = StyleUtils.LabelForToggle("¿Random Floor Placement?");
            container.Add(label);

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

        private VisualElement CreateWalkableOptionsButtons()
        {
            var container = StyleUtils.HorizontalContainerCentered();

            container.Add(CreateTileToUIButton("Add Floor tile"));
            container.Add(CreateClearTilesButton("Clear all floor tiles", true));
            container.Add(CreateSelectWalkableTilesFromFolderButton("Select floor tiles from folder", true));

            return container;
        }

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

        private Button CreateClearTilesButton(string buttonText, bool isWalkable)
        {
            var button = new Button(() =>
            {
                if (isWalkable)
                    GeneratorService.Instance.CurrentGenerator.TilemapPainter.RemoveAllWalkableTiles();
                else
                    GeneratorService.Instance.CurrentGenerator.TilemapPainter.RemoveAllWallTiles();

                RefreshUI();
            })
            {
                text = buttonText
            };

            return button;
        }

        private Button CreateSelectWalkableTilesFromFolderButton(string buttonText, bool isWalkable)
        {
            var button = new Button(() =>
            {
                var path = EditorUtility.OpenFolderPanel("Select a folder", "", "");
                if (isWalkable)
                {
                    GeneratorService.Instance.CurrentGenerator.TilemapPainter.SelectWalkableTilesFromFolder(path);
                }

                AssetDatabase.Refresh();
                RefreshUI();
            })
            {
                text = buttonText
            };

            return button;
        }

        private VisualElement CreateWalkableTileGroupSettings()
        {
            var container = new VisualElement();

            if (!_styleController.IsGeneratorSelectionValid())
            {
                Debug.LogError("Generator selection or its properties are not properly initialized.");
                return container;
            }

            var walkableTiles = GeneratorService.Instance.CurrentGenerator.TilemapPainter.walkableTileBases;
            var horizontalContainer = StyleUtils.HorizontalContainerWrapped();

            for (var index = 0; index < walkableTiles.Count; index++)
            {
                var tileContainer = CreateTileContainer(walkableTiles, index);
                horizontalContainer.Add(tileContainer);
            }

            container.Add(horizontalContainer);
            return container;
        }

        private VisualElement CreateTileContainer(List<TileBase> walkableTiles, int index)
        {
            var walkableTile = walkableTiles[index];

            var tileContainer = StyleUtils.TileContainer();

            var label = GetLaberFromTile(walkableTile);
            label.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Delete", _ =>
                {
                    walkableTiles.RemoveAt(index);
                    _walkableTilesPriorities.RemoveAt(index);
                    RefreshUI();
                });
            }));
            tileContainer.Add(label);

            var imguiPreviewContainer = CreateIMGUIContainer(walkableTiles, index, label);
            imguiPreviewContainer.style.height = Utils.GetPreviewTileSize();
            tileContainer.Add(imguiPreviewContainer);

            if (GeneratorService.Instance.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement)
                return tileContainer;

            var priorityContainer = AddPriorityToTilesUI(index);
            tileContainer.Add(priorityContainer);

            return tileContainer;
        }

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

        private void UpdateTileOnSelection(int currentIndex, Label label)
        {
            if (Event.current == null || Event.current.commandName != Utils.GetObjectSelectorUpdateCommand()) return;

            var pickerControlId = EditorGUIUtility.GetObjectPickerControlID();

            if (pickerControlId != currentIndex) return;

            var newTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;

            if (newTile == null) return;

            GeneratorService.Instance.CurrentGenerator.TilemapPainter.walkableTileBases[currentIndex] = newTile;
            var newName = newTile.name.Replace("floor", "", StringComparison.OrdinalIgnoreCase);
            label.text = Utils.AddSpacesToCamelCase(newName);
        }

        private static Label GetLaberFromTile(TileBase walkableTile)
        {
            var labelText = walkableTile?.name ?? "No selected";
            labelText = labelText.Replace("floor", "", StringComparison.OrdinalIgnoreCase);
            labelText = Utils.AddSpacesToCamelCase(labelText);

            return StyleUtils.LabelForTile(labelText);
        }

        private VisualElement AddPriorityToTilesUI(int index)
        {
            var container = StyleUtils.HorizontalContainerCentered();

            if (_randomPlacement) return container;

            var label = StyleUtils.LabelForIntField("Priority;");

            var intField = StyleUtils.SimpleIntField(_walkableTilesPriorities[index]);

            intField.RegisterValueChangedCallback(evt =>
            {
                _walkableTilesPriorities[index] = evt.newValue;
                EditorUtility.SetDirty(GeneratorService.Instance.CurrentGenerator.TilemapPainter);
            });

            container.Add(label);
            container.Add(intField);

            return container;
        }

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
    }
}