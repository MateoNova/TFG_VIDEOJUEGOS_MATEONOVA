using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor.Controllers;
using Editor.Models;
using GeneralUtils;
using Generators.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class StyleView
    {
        private VisualElement _root;
        private readonly StyleController _controller = new();

        public VisualElement CreateUI()
        {
            // Se asume que el generador actual y su TilemapPainter están asignados
            _root = _root ?? StyleUtils.SimpleContainer();
            _root.Clear();

            Foldout styleFoldout = new Foldout { text = "Style", value = true };
            styleFoldout.Add(CreateFloorTileSettings());
            styleFoldout.Add(CreateWallTileSettings());
            _root.Add(styleFoldout);

            return _root;
        }

        private VisualElement CreateFloorTileSettings()
        {
            Foldout floorSettings = new Foldout { text = "Floor Tile Settings", value = true };
            floorSettings.Add(CreateRandomFloorPlacementToggle());
            floorSettings.Add(CreateWalkableOptionsButtons());
            floorSettings.Add(CreateWalkableTileGroupSettings());
            return floorSettings;
        }

        private VisualElement CreateWallTileSettings()
        {
            VisualElement container = new VisualElement();
            if (!_controller.HasValidTilemapPainter())
                return container;

            var groupedWallFields = _controller.GetGroupedFields<WallTileGroupAttribute>(attr => attr.GroupName);
            foreach (var group in groupedWallFields)
            {
                Foldout groupFoldout = CreateFoldoutForGroup(group);
                container.Add(groupFoldout);
            }

            return container;
        }

        private VisualElement CreateRandomFloorPlacementToggle()
        {
            VisualElement container = StyleUtils.HorizontalContainerCentered();
            container.Add(StyleUtils.LabelForToggle("Random Floor Placement?"));

            var currentGenerator = GeneratorService.Instance?.CurrentGenerator;
            if (currentGenerator?.TilemapPainter == null)
            {
                Debug.LogError(
                    "TilemapPainter is not initialized. Ensure the generator and its TilemapPainter are set.");
                return container; // Return an empty container or handle gracefully.
            }

            Toggle toggle = new Toggle
            {
                value = currentGenerator.TilemapPainter.randomWalkableTilesPlacement
            };
            toggle.RegisterValueChangedCallback(evt =>
            {
                currentGenerator.TilemapPainter.randomWalkableTilesPlacement = evt.newValue;
                EditorUtility.SetDirty(currentGenerator.TilemapPainter);
                RefreshUI();
            });
            container.Add(toggle);
            return container;
        }

        private VisualElement CreateWalkableOptionsButtons()
        {
            VisualElement container = StyleUtils.HorizontalContainerCentered();
            container.Add(CreateUIButton("Add Floor Tile", () =>
            {
                GeneratorService.Instance.CurrentGenerator.TilemapPainter.walkableTileBases.Add(null);
                GeneratorService.Instance.CurrentGenerator.TilemapPainter.walkableTilesPriorities.Add(0);
                RefreshUI();
            }));
            container.Add(CreateUIButton("Clear All Floor Tiles", () =>
            {
                GeneratorService.Instance.CurrentGenerator.TilemapPainter.RemoveAllWalkableTiles();
                RefreshUI();
            }));
            container.Add(CreateUIButton("Select Floor Tiles From Folder", () =>
            {
                string path = EditorUtility.OpenFolderPanel("Select a folder", "", "");
                GeneratorService.Instance.CurrentGenerator.TilemapPainter.SelectWalkableTilesFromFolder(path);
                AssetDatabase.Refresh();
                RefreshUI();
            }));
            return container;
        }

        private Button CreateUIButton(string text, System.Action onClick) =>
            new Button(onClick) { text = text };

        private VisualElement CreateWalkableTileGroupSettings()
        {
            VisualElement container = new VisualElement();

            var currentGenerator = GeneratorService.Instance?.CurrentGenerator;
            if (currentGenerator?.TilemapPainter == null || currentGenerator.TilemapPainter.walkableTileBases == null)
            {
                Debug.LogError(
                    "TilemapPainter or walkableTileBases is not initialized. Ensure all dependencies are set.");
                return container; // Return an empty container or handle gracefully.
            }

            var walkableTiles = currentGenerator.TilemapPainter.walkableTileBases;
            VisualElement horizontalContainer = StyleUtils.HorizontalContainerWrapped();

            for (int i = 0; i < walkableTiles.Count; i++)
            {
                VisualElement tileContainer = CreateTileContainerForWalkableTile(walkableTiles, i);
                horizontalContainer.Add(tileContainer);
            }

            container.Add(horizontalContainer);
            return container;
        }

        private VisualElement CreateTileContainerForWalkableTile(List<TileBase> tiles, int index)
        {
            VisualElement container = StyleUtils.TileContainer();
            Label label = GetLabelFromTile(tiles[index]);
            container.Add(label);
            IMGUIContainer imguiContainer = CreateIMGUIContainerForTile(tiles, index, label);
            imguiContainer.style.height = Utils.GetPreviewTileSize();
            container.Add(imguiContainer);
            // Se puede agregar UI para prioridad si fuera necesario.
            return container;
        }

        private static Label GetLabelFromTile(TileBase tile)
        {
            string labelText = tile != null
                ? Utils.AddSpacesToCamelCase(tile.name.Replace("floor", "", System.StringComparison.OrdinalIgnoreCase))
                : "No selected";
            return StyleUtils.LabelForTile(labelText);
        }

        private IMGUIContainer CreateIMGUIContainerForTile(List<TileBase> tiles, int index, Label label)
        {
            return new IMGUIContainer(() =>
            {
                TileBase currentTile = tiles[index];
                Texture preview = GetPreviewTexture(currentTile);
                int size = Utils.GetPreviewTileSize();
                if (GUILayout.Button(preview, GUILayout.Width(size), GUILayout.Height(size)))
                    EditorGUIUtility.ShowObjectPicker<TileBase>(currentTile, false, "", index);
                UpdateTileSelection(index, label);
            });
        }

        private static Texture GetPreviewTexture(TileBase tile)
        {
            if (tile == null)
                return EditorGUIUtility.IconContent(Utils.GetDefaultIconContent()).image;
            Texture preview = AssetPreview.GetAssetPreview(tile);
            return preview != null ? preview : EditorGUIUtility.ObjectContent(tile, typeof(TileBase)).image;
        }

        private void UpdateTileSelection(int index, Label label)
        {
            if (Event.current == null || Event.current.commandName != Utils.GetObjectSelectorUpdateCommand())
                return;
            if (EditorGUIUtility.GetObjectPickerControlID() != index)
                return;
            TileBase newTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;
            if (newTile == null) return;
            GeneratorService.Instance.CurrentGenerator.TilemapPainter.walkableTileBases[index] = newTile;
            label.text =
                Utils.AddSpacesToCamelCase(newTile.name.Replace("floor", "",
                    System.StringComparison.OrdinalIgnoreCase));
        }

        private Foldout CreateFoldoutForGroup(IEnumerable<System.Reflection.FieldInfo> group)
        {
            Foldout foldout = new Foldout
                { text = group.First().GetCustomAttribute<WallTileGroupAttribute>().GroupName, value = true };
            VisualElement horiz = StyleUtils.HorizontalContainerWrapped();
            foreach (var field in group)
                horiz.Add(CreateTileContainerForWallField(field));
            foldout.Add(horiz);
            return foldout;
        }

        private VisualElement CreateTileContainerForWallField(System.Reflection.FieldInfo field)
        {
            VisualElement container = StyleUtils.TileContainer();
            Label label = StyleUtils.LabelForTile(CleanWallLabel(field.Name));
            container.Add(label);
            IMGUIContainer imgui = CreateIMGUIContainerForWallField(field);
            imgui.style.height = 60;
            container.Add(imgui);
            return container;
        }

        private IMGUIContainer CreateIMGUIContainerForWallField(System.Reflection.FieldInfo field)
        {
            int controlID = field.Name.GetHashCode() & 0x7FFFFFFF;
            return new IMGUIContainer(() =>
            {
                var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
                if (painter == null) return;
                TileBase currentTile = field.GetValue(painter) as TileBase;
                Texture preview = GetPreviewTexture(currentTile);
                int size = Utils.GetPreviewTileSize();
                if (GUILayout.Button(preview, GUILayout.Width(size), GUILayout.Height(size)))
                    EditorGUIUtility.ShowObjectPicker<TileBase>(currentTile, false, "", controlID);
                UpdateWallTileSelection(field, controlID);
            });
        }

        private static void UpdateWallTileSelection(System.Reflection.FieldInfo field, int controlID)
        {
            if (Event.current == null || Event.current.commandName != Utils.GetObjectSelectorUpdateCommand())
                return;
            if (EditorGUIUtility.GetObjectPickerControlID() != controlID)
                return;
            TileBase newTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;
            if (newTile == null) return;
            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
            field.SetValue(painter, newTile);
        }

        private static string CleanWallLabel(string original)
        {
            string label = UnityEditor.ObjectNames.NicifyVariableName(original);
            label = label.Replace("wall", "", System.StringComparison.OrdinalIgnoreCase)
                .Replace("inner", "", System.StringComparison.OrdinalIgnoreCase)
                .Replace("triple", "", System.StringComparison.OrdinalIgnoreCase);
            return string.Join("\n", label.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));
        }

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