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
        private VisualElement _root;
        private readonly StyleController _styleController = new();
        private readonly List<TilesetPreset> _loadedPresets = new();

        public StyleView() => LoadPresetsFromEditorPrefs();

        public VisualElement CreateUI()
        {
            var gen = GeneratorService.Instance.CurrentGenerator;
            if (gen == null || gen.TilemapPainter == null)
                return new VisualElement();

            _root ??= StyleUtils.SimpleContainer();
            _root.Clear();

            // Load‐preset button
            _root.Add(CreateStyleSection());

            // One subsection per‐preset
            var painter = gen.TilemapPainter as TilemapPainter;
            for (int presetIdx = 0; presetIdx < _loadedPresets.Count; presetIdx++)
            {
                var preset = _loadedPresets[presetIdx];
                painter.AddAndSelectPreset(preset);
                _root.Add(CreatePresetSubsection(preset, presetIdx));
            }

            return _root;
        }

        private VisualElement CreateStyleSection()
        {
            var section = StyleUtils.ModernFoldout("");
            section.SetLocalizedText("Style", "StyleTable");
            section.Add(CreatePresetSettings());
            return section;
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

                    // Register preset
                    _loadedPresets.Add(preset);
                    EditorPrefs.SetString(LoadedPresetsKey,
                        string.Join(";", _loadedPresets.Select(AssetDatabase.GetAssetPath)));

                    RefreshUI();
                    EditorUtility.DisplayDialog("Success", $"Preset '{preset.name}' loaded.", "OK");
                })
                { text = "Load Preset" };
            loadBtn.SetLocalizedText("loadPreset", "StyleTable");
            container.Add(loadBtn);
            return container;
        }

        private VisualElement CreatePresetSubsection(TilesetPreset preset, int presetIdx)
        {
            // Ensure painter is pointing at this preset
            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter as TilemapPainter;
            painter.AddAndSelectPreset(preset);

            var section = StyleUtils.ModernSubFoldout(preset.name);
            section.AddManipulator(new ContextualMenuManipulator(evt =>
                evt.menu.AppendAction("Eliminar preset", _ => RemovePreset(preset))
            ));

            // Floor settings
            section.Add(CreateFloorTileSettings(preset, presetIdx));
            // Wall settings
            section.Add(CreateWallTileSettings(preset, presetIdx));

            return section;
        }

        private void RemovePreset(TilesetPreset preset)
        {
            _loadedPresets.Remove(preset);
            EditorPrefs.SetString(LoadedPresetsKey,
                string.Join(";", _loadedPresets.Select(AssetDatabase.GetAssetPath)));

            // Also remove from painter
            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter as TilemapPainter;
            painter.RemovePreset(preset);

            RefreshUI();
        }

        // ─── Floor Settings ─────────────────────────────────────────────────

        private VisualElement CreateFloorTileSettings(TilesetPreset preset, int presetIdx)
        {
            var walkables = preset.walkableTileBases;
            var priorities = preset.walkableTilesPriorities;

            var fe = StyleUtils.ModernSubFoldout("");
            fe.SetLocalizedText("FloorTileSettings", "StyleTable");

            // Random toggle
            var toggleRow = StyleUtils.HorizontalContainerCentered();
            var lbl = StyleUtils.LabelForToggle("");
            lbl.SetLocalizedText("RandomFloorPlacement", "StyleTable");
            toggleRow.Add(lbl);

            var tog = new Toggle { value = preset.randomWalkableTilesPlacement };
            tog.RegisterValueChangedCallback(evt =>
            {
                preset.randomWalkableTilesPlacement = evt.newValue;
                EditorUtility.SetDirty(preset);
                RefreshUI();
            });
            toggleRow.Add(tog);
            fe.Add(toggleRow);

            if (walkables.Count == 0 && !preset.randomWalkableTilesPlacement)
            {
                // allow adding
            }

            // Add / Clear buttons
            var btnRow = StyleUtils.HorizontalContainerCentered();
            var addBtn = new Button(() =>
            {
                walkables.Add(null);
                priorities.Add(0);
                EditorUtility.SetDirty(preset);
                RefreshUI();
            }) { text = "Add Floor Tile" };
            addBtn.SetLocalizedText("AddFloorTile", "StyleTable");
            btnRow.Add(addBtn);

            var clearBtn = new Button(() =>
            {
                walkables.Clear();
                priorities.Clear();
                EditorUtility.SetDirty(preset);
                RefreshUI();
            }) { text = "Clear All Floor Tiles" };
            clearBtn.SetLocalizedText("ClearAllFloorTiles", "StyleTable");
            btnRow.Add(clearBtn);

            fe.Add(btnRow);

            // Tile previews
            var previewRow = StyleUtils.HorizontalContainerWrapped();
            for (int i = 0; i < walkables.Count; i++)
                previewRow.Add(CreateWalkableTileControl(preset, presetIdx, i));
            fe.Add(previewRow);

            return fe;
        }
        
        private VisualElement CreateWalkableTileControl(TilesetPreset preset, int presetIdx, int tileIdx)
        {
            var key = presetIdx * 1000 + tileIdx; // unique control ID
            var walkables = preset.walkableTileBases;
            var priorities = preset.walkableTilesPriorities;
            var isRandom = preset.randomWalkableTilesPlacement;

            var cont = StyleUtils.TileContainer();
            var label = GetLabelForWalkableTile(walkables[tileIdx]);
            cont.Add(label);

            // IMGUI preview + picker
            cont.Add(new IMGUIContainer(() =>
            {
                var curr = walkables[tileIdx];
                var tex = GetPreviewTexture(curr);
                var size = Utils.Utils.GetPreviewTileSize();

                if (GUILayout.Button(tex, GUILayout.Width(size), GUILayout.Height(size)))
                    EditorGUIUtility.ShowObjectPicker<TileBase>(curr, false, "", key);

                if (Event.current?.commandName == Utils.Utils.GetObjectSelectorUpdateCommand() &&
                    EditorGUIUtility.GetObjectPickerControlID() == key)
                {
                    var nt = EditorGUIUtility.GetObjectPickerObject() as TileBase;
                    if (nt != null)
                    {
                        walkables[tileIdx] = nt;
                        EditorUtility.SetDirty(preset);
                        label.text = Utils.Utils.AddSpacesToCamelCase(nt.name.Replace("floor", "", StringComparison.OrdinalIgnoreCase));
                    }
                }
            })
            { style = { height = Utils.Utils.GetPreviewTileSize() } });

            // Priority field
            if (!isRandom)
            {
                var row = StyleUtils.HorizontalContainerCentered();
                var plabel = StyleUtils.LabelForIntField("");
                plabel.SetLocalizedText("Priority", "StyleTable");
                row.Add(plabel);

                var intF = StyleUtils.SimpleIntField(priorities[tileIdx]);
                intF.RegisterValueChangedCallback(evt =>
                {
                    priorities[tileIdx] = evt.newValue;
                    EditorUtility.SetDirty(preset);
                });
                row.Add(intF);
                cont.Add(row);
            }

            return cont;
        }

        private VisualElement CreateRandomFloorPlacementToggle(TilesetPreset preset)
        {
            var c = StyleUtils.HorizontalContainerCentered();
            var lbl = StyleUtils.LabelForToggle("");
            lbl.SetLocalizedText("RandomFloorPlacement", "StyleTable");
            c.Add(lbl);

            var toggle = new Toggle { value = preset.randomWalkableTilesPlacement };
            toggle.RegisterValueChangedCallback(evt =>
            {
                preset.randomWalkableTilesPlacement = evt.newValue;
                EditorUtility.SetDirty(preset);
                RefreshUI();
            });
            c.Add(toggle);
            return c;
        }

        private VisualElement CreateWalkableOptionsButtons(
            TilesetPreset preset,
            IList<TileBase> walkables,
            IList<int> priorities)
        {
            var c = StyleUtils.HorizontalContainerCentered();
            // Add
            c.Add(new Button(() =>
            {
                walkables.Add(null);
                priorities.Add(0);
                EditorUtility.SetDirty(preset);
                RefreshUI();
            }) { text = "Add Floor Tile" }.Let(b => b.SetLocalizedText("AddFloorTile", "StyleTable")));
            // Clear
            c.Add(new Button(() =>
            {
                walkables.Clear();
                priorities.Clear();
                EditorUtility.SetDirty(preset);
                RefreshUI();
            }) { text = "Clear All Floor Tiles" }.Let(b => b.SetLocalizedText("ClearAllFloorTiles", "StyleTable")));
            return c;
        }

        private VisualElement CreateWalkableTileGroupSettings(
            TilesetPreset preset,
            List<TileBase> walkables,
            List<int> priorities,
            bool random)
        {
            var container = new VisualElement();
            var hc = StyleUtils.HorizontalContainerWrapped();
            for (int i = 0; i < walkables.Count; i++)
                hc.Add(CreateTileContainerForWalkableTile(preset, walkables, priorities, i));
            container.Add(hc);
            return container;
        }

        private VisualElement CreateTileContainerForWalkableTile(
            TilesetPreset preset,
            List<TileBase> walkables,
            List<int> priorities,
            int index)
        {
            var tileCont = StyleUtils.TileContainer();
            var lbl = GetLabelForWalkableTile(walkables[index]);
            tileCont.Add(lbl);

            // Preview + Picker
            tileCont.Add(new IMGUIContainer(() =>
                {
                    var current = walkables[index];
                    var tex = GetPreviewTexture(current);
                    var size = Utils.Utils.GetPreviewTileSize();

                    if (GUILayout.Button(tex, GUILayout.Width(size), GUILayout.Height(size)))
                        EditorGUIUtility.ShowObjectPicker<TileBase>(current, false, "", index);

                    if (Event.current?.commandName == Utils.Utils.GetObjectSelectorUpdateCommand() &&
                        EditorGUIUtility.GetObjectPickerControlID() == index)
                    {
                        var nt = EditorGUIUtility.GetObjectPickerObject() as TileBase;
                        if (nt != null)
                        {
                            walkables[index] = nt;
                            EditorUtility.SetDirty(preset);
                            lbl.text = Utils.Utils.AddSpacesToCamelCase(
                                nt.name.Replace("floor", "", StringComparison.OrdinalIgnoreCase));
                        }
                    }
                })
                { style = { height = Utils.Utils.GetPreviewTileSize() } });

            // Priority (si no random)
            if (!preset.randomWalkableTilesPlacement)
            {
                var row = StyleUtils.HorizontalContainerCentered();
                var pflbl = StyleUtils.LabelForIntField("");
                pflbl.SetLocalizedText("Priority", "StyleTable");
                row.Add(pflbl);

                var field = StyleUtils.SimpleIntField(priorities[index]);
                field.RegisterValueChangedCallback(evt =>
                {
                    priorities[index] = evt.newValue;
                    EditorUtility.SetDirty(preset);
                });
                row.Add(field);
                tileCont.Add(row);
            }

            return tileCont;
        }

        private static Label GetLabelForWalkableTile(TileBase tile)
        {
            var txt = tile != null
                ? Utils.Utils.AddSpacesToCamelCase(tile.name)
                : "No selected";
            var lbl = StyleUtils.LabelForTile(txt);
            if (tile == null) lbl.SetLocalizedText("NoSelected", "StyleTable");
            return lbl;
        }

        // ─── Wall Settings ─────────────────────────────────────────────────

        private VisualElement CreateWallTileSettings(TilesetPreset preset, int presetIdx)
        {
            var container = new VisualElement();
            var groups = _styleController
                .GetGroupedFields<WallTileGroupAttribute>(a => a.GroupName);

            foreach (var group in groups)
            {
                var fold = StyleUtils.ModernSubFoldout("");
                fold.SetLocalizedText(group.Key, "StyleTable");

                var row = StyleUtils.HorizontalContainerWrapped();
                foreach (var field in group)
                    row.Add(CreateWallTileControl(preset, presetIdx, field));
                fold.Add(row);
                container.Add(fold);
            }

            return container;
        }
        
        private VisualElement CreateWallTileControl(TilesetPreset preset, int presetIdx, FieldInfo field)
        {
            var controlID = (presetIdx * 1000) + field.Name.GetHashCode(); 
            var cont = StyleUtils.TileContainer();
            var lbl = StyleUtils.LabelForTile(ObjectNames.NicifyVariableName(field.Name).Replace("Wall", ""));
            lbl.SetLocalizedText(field.Name, "StyleTable");
            cont.Add(lbl);

            cont.Add(new IMGUIContainer(() =>
                {
                    var curr = field.GetValue(preset) as TileBase;
                    var tex = GetPreviewTexture(curr);
                    var size = Utils.Utils.GetPreviewTileSize();

                    if (GUILayout.Button(tex, GUILayout.Width(size), GUILayout.Height(size)))
                        EditorGUIUtility.ShowObjectPicker<TileBase>(curr, false, "", controlID);

                    if (Event.current?.commandName == Utils.Utils.GetObjectSelectorUpdateCommand() &&
                        EditorGUIUtility.GetObjectPickerControlID() == controlID)
                    {
                        var nt = EditorGUIUtility.GetObjectPickerObject() as TileBase;
                        if (nt != null)
                        {
                            field.SetValue(preset, nt);
                            EditorUtility.SetDirty(preset);
                            lbl.text = Utils.Utils.AddSpacesToCamelCase(nt.name);
                        }
                    }
                })
                { style = { height = Utils.Utils.GetPreviewTileSize() } });

            return cont;
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

            c.Add(new IMGUIContainer(() =>
                {
                    var current = field.GetValue(preset) as TileBase;
                    var tex = GetPreviewTexture(current);
                    var size = Utils.Utils.GetPreviewTileSize();

                    if (GUILayout.Button(tex, GUILayout.Width(size), GUILayout.Height(size)))
                        EditorGUIUtility.ShowObjectPicker<TileBase>(current, false, "", field.Name.GetHashCode());

                    if (Event.current?.commandName == Utils.Utils.GetObjectSelectorUpdateCommand() &&
                        EditorGUIUtility.GetObjectPickerControlID() == field.Name.GetHashCode())
                    {
                        var nt = EditorGUIUtility.GetObjectPickerObject() as TileBase;
                        if (nt != null)
                        {
                            field.SetValue(preset, nt);
                            EditorUtility.SetDirty(preset);
                            lbl.text = Utils.Utils.AddSpacesToCamelCase(nt.name);
                        }
                    }
                })
                { style = { height = Utils.Utils.GetPreviewTileSize() } });

            return c;
        }

        private static Label CreateLabelForWallField(FieldInfo field)
        {
            var txt = ObjectNames.NicifyVariableName(field.Name)
                .Replace("Wall", "", StringComparison.OrdinalIgnoreCase);
            var lbl = StyleUtils.LabelForTile(txt);
            lbl.SetLocalizedText(field.Name, "StyleTable");
            return lbl;
        }

        private static Texture GetPreviewTexture(TileBase tile)
        {
            if (tile == null)
                return EditorGUIUtility.IconContent(Utils.Utils.GetDefaultIconContent()).image;
            var pr = AssetPreview.GetAssetPreview(tile);
            return pr ?? (Texture2D)EditorGUIUtility.ObjectContent(tile, typeof(TileBase)).image;
        }

        private void RefreshUI()
        {
            _root.Clear();
            _root.Add(CreateStyleSection());
            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter as TilemapPainter;
            for (int i = 0; i < _loadedPresets.Count; i++)
            {
                var p = _loadedPresets[i];
                painter.AddAndSelectPreset(p);
                _root.Add(CreatePresetSubsection(p, i));
            }
            _root.MarkDirtyRepaint();
        }

        private void LoadPresetsFromEditorPrefs()
        {
            _loadedPresets.Clear();
            if (!EditorPrefs.HasKey(LoadedPresetsKey)) return;
            foreach (var path in EditorPrefs.GetString(LoadedPresetsKey).Split(';'))
            {
                var preset = AssetDatabase.LoadAssetAtPath<TilesetPreset>(path);
                if (preset != null)
                    _loadedPresets.Add(preset);
            }
        }
    }

    // Helper extension to fluent-set text and return the button
    static class UIExtensions
    {
        public static T Let<T>(this T obj, Action<T> fn)
        {
            fn(obj);
            return obj;
        }
    }
}

#endif