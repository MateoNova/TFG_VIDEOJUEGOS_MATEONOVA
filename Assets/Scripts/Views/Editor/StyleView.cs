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
        private const string LoadedPresetsKey = "StyleView_LoadedPresets";

        public StyleView() => LoadPresetsFromEditorPrefs();

        private VisualElement _root;
        private readonly StyleController _styleController = new();
        private List<TileBase> _walkableTileBases;
        private List<int> _walkableTilesPriorities;
        private bool _randomPlacement;

        private readonly List<TilesetPreset> _loadedPresets = new();

        public VisualElement CreateUI()
        {
            var gen = GeneratorService.Instance.CurrentGenerator;
            if (gen == null || gen.TilemapPainter == null)
                return new VisualElement();

            _root ??= StyleUtils.SimpleContainer();
            _root.Clear();

            // 1) Botón principal para cargar presets
            _root.Add(CreateStyleSection());

            // 2) Una subsección por cada preset, seleccionándolo primero
            var painter = gen.TilemapPainter as TilemapPainter;
            foreach (var preset in _loadedPresets)
            {
                painter.AddAndSelectPreset(preset);
                _root.Add(CreatePresetSubsection(preset));
            }

            return _root;
        }

        private VisualElement CreateStyleSection()
        {
            var styleSection = StyleUtils.ModernFoldout("");
            styleSection.SetLocalizedText("Style", "StyleTable");
            styleSection.Add(CreatePresetSettings());
            return styleSection;
        }

        private VisualElement CreatePresetSettings()
        {
            var container = new VisualElement();
            var loadBtn = new Button(() =>
                {
                    var path = EditorUtility.OpenFilePanel("Select Preset", "Assets", "asset");
                    if (string.IsNullOrEmpty(path) || !path.StartsWith(Application.dataPath)) return;
                    path = "Assets" + path.Substring(Application.dataPath.Length);

                    var preset = AssetDatabase.LoadAssetAtPath<TilesetPreset>(path);
                    if (preset == null)
                    {
                        EditorUtility.DisplayDialog("Error", "Failed to load preset.", "OK");
                        return;
                    }

                    // cargar en painter y en la lista
                    _styleController.LoadPreset(preset);
                    _loadedPresets.Add(preset);
                    SavePresetsToEditorPrefs();

                    RefreshUI();
                    EditorUtility.DisplayDialog("Success", $"Preset '{preset.name}' loaded.", "OK");
                })
                { text = "Load Preset" };
            loadBtn.SetLocalizedText("loadPreset", "StyleTable");
            container.Add(loadBtn);
            return container;
        }

        private VisualElement CreatePresetSubsection(TilesetPreset preset)
        {
            // Reconstruimos las listas de este preset
            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter as TilemapPainter;
            _walkableTileBases = painter.GetWalkableTileBases();
            _walkableTilesPriorities = painter.GetWalkableTilesPriorities();
            _randomPlacement = painter.GetRandomWalkableTilesPlacement();

            var section = StyleUtils.ModernSubFoldout(preset.name);
            section.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Eliminar preset", _ => RemovePreset(preset));
            }));

            section.Add(CreateFloorTileSettings());
            section.Add(CreateWallTileSettings(preset));
            return section;
        }

        private void RemovePreset(TilesetPreset preset)
        {
            _loadedPresets.Remove(preset);
            SavePresetsToEditorPrefs();
            // también quitamos del painter
            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter as TilemapPainter;
            painter.RemovePreset(preset);
            RefreshUI();
        }

        // —————————————————————————————————————————
        //  Floor settings (idéntico a tu código)
        // —————————————————————————————————————————

        private VisualElement CreateFloorTileSettings()
        {
            var fe = StyleUtils.ModernSubFoldout("");
            fe.SetLocalizedText("FloorTileSettings", "StyleTable");
            fe.Add(CreateRandomFloorPlacementToggle());

            if (!_styleController.HasValidTilemapPainter())
                return fe;

            fe.Add(CreateWalkableOptionsButtons());
            fe.Add(CreateWalkableTileGroupSettings());
            return fe;
        }

        private VisualElement CreateRandomFloorPlacementToggle()
        {
            var c = StyleUtils.HorizontalContainerCentered();
            var l = StyleUtils.LabelForToggle("");
            l.SetLocalizedText("RandomFloorPlacement", "StyleTable");
            c.Add(l);

            var t = new Toggle { value = _randomPlacement };
            t.RegisterValueChangedCallback(evt =>
            {
                var p = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
                p.SetRandomWalkableTilesPlacement(evt.newValue);
                EditorUtility.SetDirty(p);
                RefreshUI();
            });
            c.Add(t);
            return c;
        }

        private VisualElement CreateWalkableOptionsButtons()
        {
            var c = StyleUtils.HorizontalContainerCentered();
            c.Add(CreateUIButtonForAddTile("AddFloorTile", () =>
            {
                _walkableTileBases.Add(null);
                _walkableTilesPriorities.Add(0);
                RefreshUI();
            }));
            c.Add(CreateUIButtonForClearTiles("ClearAllFloorTiles", true));
            c.Add(CreateUIButtonForSelectTilesFromFolder("SelectFloorTilesFromFolder", true));
            return c;
        }

        private static Button CreateUIButtonForAddTile(string key, Action onClick)
        {
            var b = new Button(onClick);
            b.SetLocalizedText(key, "StyleTable");
            return b;
        }

        private Button CreateUIButtonForClearTiles(string key, bool isWalkable)
        {
            var b = new Button(() =>
            {
                var p = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
                if (isWalkable) p.RemoveAllWalkableTiles();
                else p.RemoveAllWallTiles();
                RefreshUI();
            });
            b.SetLocalizedText(key, "StyleTable");
            return b;
        }

        private Button CreateUIButtonForSelectTilesFromFolder(string key, bool isWalkable)
        {
            var b = new Button(() =>
            {
                var path = EditorUtility.OpenFolderPanel("Select a Folder", "", "");
                if (isWalkable)
                    GeneratorService.Instance.CurrentGenerator.TilemapPainter
                        .SelectWalkableTilesFromFolder(path);
                AssetDatabase.Refresh();
                RefreshUI();
            });
            b.SetLocalizedText(key, "StyleTable");
            return b;
        }

        private VisualElement CreateWalkableTileGroupSettings()
        {
            var container = new VisualElement();
            if (!_styleController.IsGeneratorSelectionValid())
                return container;

            var walkables = GeneratorService.Instance.CurrentGenerator.TilemapPainter.GetWalkableTileBases();
            var hc = StyleUtils.HorizontalContainerWrapped();
            for (int i = 0; i < walkables.Count; i++)
                hc.Add(CreateTileContainerForWalkableTile(walkables, i));
            container.Add(hc);
            return container;
        }

        private VisualElement CreateTileContainerForWalkableTile(List<TileBase> walkableTiles, int index)
        {
            var tileCont = StyleUtils.TileContainer();
            var lbl = GetLabelForWalkableTile(walkableTiles[index]);

            lbl.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Delete", _ =>
                {
                    walkableTiles.RemoveAt(index);
                    _walkableTilesPriorities.RemoveAt(index);
                    RefreshUI();
                });
            }));
            tileCont.Add(lbl);

            var prev = CreateIMGUIContainerForWalkableTile(walkableTiles, index, lbl);
            prev.style.height = Utils.Utils.GetPreviewTileSize();
            tileCont.Add(prev);

            if (!GeneratorService.Instance.CurrentGenerator.TilemapPainter.GetRandomWalkableTilesPlacement())
                tileCont.Add(CreatePriorityUIForTile(index));

            return tileCont;
        }

        private static Label GetLabelForWalkableTile(TileBase tile)
        {
            var text = tile != null
                ? Utils.Utils.AddSpacesToCamelCase(tile.name)
                : "No selected";
            var l = StyleUtils.LabelForTile(text);
            if (tile == null) l.SetLocalizedText("NoSelected", "StyleTable");
            return l;
        }

        private IMGUIContainer CreateIMGUIContainerForWalkableTile(
            List<TileBase> walkableTiles, int index, Label label)
        {
            return new IMGUIContainer(() =>
            {
                var curr = walkableTiles[index];
                var previewTex = GetPreviewTexture(curr);
                var size = Utils.Utils.GetPreviewTileSize();

                if (GUILayout.Button(previewTex, GUILayout.Width(size), GUILayout.Height(size)))
                    EditorGUIUtility.ShowObjectPicker<TileBase>(curr, false, "", index);

                UpdateWalkableTileOnSelection(index, label);
            });
        }

        private VisualElement CreatePriorityUIForTile(int index)
        {
            var c = StyleUtils.HorizontalContainerCentered();
            var lbl = StyleUtils.LabelForIntField("");
            lbl.SetLocalizedText("Priority", "StyleTable");
            c.Add(lbl);

            var f = StyleUtils.SimpleIntField(_walkableTilesPriorities[index]);
            f.RegisterValueChangedCallback(evt =>
            {
                _walkableTilesPriorities[index] = evt.newValue;
                EditorUtility.SetDirty(GeneratorService.Instance.CurrentGenerator.TilemapPainter);
            });
            c.Add(f);
            return c;
        }

        // —————————————————————————————————————————
        //  Wall settings, **usando el preset capturado**  
        // —————————————————————————————————————————

        private VisualElement CreateWallTileSettings(TilesetPreset preset)
        {
            var container = new VisualElement();
            if (!_styleController.HasValidTilemapPainter())
                return container;

            var groups = _styleController.GetGroupedFields<WallTileGroupAttribute>(a => a.GroupName);
            foreach (var g in groups)
                container.Add(CreateFoldoutForGroup(g, preset));
            return container;
        }

        private Foldout CreateFoldoutForGroup(IGrouping<string, FieldInfo> group, TilesetPreset preset)
        {
            var f = StyleUtils.ModernSubFoldout("");
            f.SetLocalizedText(group.Key, "StyleTable");
            var hc = StyleUtils.HorizontalContainerWrapped();
            foreach (var field in group)
                hc.Add(CreateTileContainerForWallField(field, preset));
            f.Add(hc);
            return f;
        }

        private VisualElement CreateTileContainerForWallField(FieldInfo field, TilesetPreset preset)
        {
            var c = StyleUtils.TileContainer();
            var lbl = CreateLabelForWallField(field);
            c.Add(lbl);

            var imgc = CreateIMGUIContainerForWallField(field, preset);
            imgc.style.height = Utils.Utils.GetPreviewTileSize();
            c.Add(imgc);
            return c;
        }

        private static Label CreateLabelForWallField(FieldInfo field)
        {
            var txt = CleanWallLabel(field.Name);
            var l = StyleUtils.LabelForTile(txt);
            l.SetLocalizedText(field.Name, "StyleTable");
            return l;
        }

        private static IMGUIContainer CreateIMGUIContainerForWallField(FieldInfo field, TilesetPreset preset)
        {
            var controlID = field.Name.GetHashCode() & 0x7FFFFFFF;
            return new IMGUIContainer(() =>
            {
                // **usamos directamente el preset**, no painter.GetCurrentTilesetPreset()
                var currentTile = field.GetValue(preset) as TileBase;
                var previewTex = GetPreviewTexture(currentTile);
                var size = Utils.Utils.GetPreviewTileSize();

                if (GUILayout.Button(previewTex, GUILayout.Width(size), GUILayout.Height(size)))
                    EditorGUIUtility.ShowObjectPicker<TileBase>(currentTile, false, "", controlID);

                // actualización al elegir desde el ObjectPicker
                if (Event.current?.commandName == Utils.Utils.GetObjectSelectorUpdateCommand() &&
                    EditorGUIUtility.GetObjectPickerControlID() == controlID)
                {
                    var nt = EditorGUIUtility.GetObjectPickerObject() as TileBase;
                    if (nt != null)
                    {
                        field.SetValue(preset, nt);
                        Debug.Log($"Campo '{field.Name}' updated → {nt.name}");
                    }
                }
            });
        }

        // —————————————————————————————————————————
        //  Helpers, Refresh, Save/Load prefs...
        // —————————————————————————————————————————

        private static void UpdateWalkableTileOnSelection(int index, Label label)
        {
            if (Event.current?.commandName != Utils.Utils.GetObjectSelectorUpdateCommand()) return;
            if (EditorGUIUtility.GetObjectPickerControlID() != index) return;
            var nt = EditorGUIUtility.GetObjectPickerObject() as TileBase;
            if (nt == null) return;
            GeneratorService.Instance.CurrentGenerator.TilemapPainter.SetWalkableTileBases(index, nt);
            label.text = Utils.Utils.AddSpacesToCamelCase(
                nt.name.Replace("floor", "", StringComparison.OrdinalIgnoreCase)
            );
        }

        private static Texture GetPreviewTexture(TileBase tile)
        {
            if (tile == null)
                return EditorGUIUtility.IconContent(Utils.Utils.GetDefaultIconContent()).image;
            var pr = AssetPreview.GetAssetPreview(tile);
            return pr ?? (Texture2D)EditorGUIUtility.ObjectContent(tile, typeof(TileBase)).image;
        }

        private static string CleanWallLabel(string original)
        {
            var label = ObjectNames.NicifyVariableName(original)
                .Replace("wall", "", StringComparison.OrdinalIgnoreCase)
                .Replace("inner", "", StringComparison.OrdinalIgnoreCase)
                .Replace("triple", "", StringComparison.OrdinalIgnoreCase);
            return string.Join("\n", label.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void RefreshUI()
        {
            if (_root == null) _root = StyleUtils.SimpleContainer();
            else _root.Clear();

            _root.Add(CreateStyleSection());
            foreach (var p in _loadedPresets)
                _root.Add(CreatePresetSubsection(p));
            _root.MarkDirtyRepaint();
        }

        private void SavePresetsToEditorPrefs()
        {
            var paths = _loadedPresets.Select(AssetDatabase.GetAssetPath).ToArray();
            EditorPrefs.SetString(LoadedPresetsKey, string.Join(";", paths));
        }

        private void LoadPresetsFromEditorPrefs()
        {
            _loadedPresets.Clear();
            if (!EditorPrefs.HasKey(LoadedPresetsKey)) return;
            foreach (var path in EditorPrefs.GetString(LoadedPresetsKey).Split(';'))
            {
                var preset = AssetDatabase.LoadAssetAtPath<TilesetPreset>(path);
                if (preset != null) _loadedPresets.Add(preset);
            }
        }
    }
}

#endif