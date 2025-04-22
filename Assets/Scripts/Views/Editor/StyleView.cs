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
        private readonly List<float> _presetCoverage = new();

        public StyleView() => LoadPresetsFromEditorPrefs();

        public VisualElement CreateUI()
        {
            var gen = GeneratorService.Instance.CurrentGenerator;
            if (gen == null) return new VisualElement();

            _root ??= StyleUtils.SimpleContainer();
            _root.Clear();

            // 1) Carga sección general y botones de preset
            _root.Add(CreateStyleSection());

            // 2) Si hay más de un preset, mostrar cobertura de biomas
            if (_loadedPresets.Count > 1)
                _root.Add(CreateBiomeCoverageFolder());

            // 3) Para cada preset, forzar selección y dibujar subsección
            var painter = gen.TilemapPainter as TilemapPainter;
            for (int idx = 0; idx < _loadedPresets.Count; idx++)
            {
                var preset = _loadedPresets[idx];
                painter.AddAndSelectPreset(preset);
                _root.Add(CreatePresetSubsection(preset, idx));
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

                    _loadedPresets.Add(preset);
                    _presetCoverage.Add(100f / _loadedPresets.Count);
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

        private VisualElement CreateBiomeCoverageFolder()
        {
            var fold = StyleUtils.ModernFoldout("");
            fold.SetLocalizedText("BiomeCoverage", "StyleTable");

            // Sincronizar cobertura
            while (_presetCoverage.Count < _loadedPresets.Count)
                _presetCoverage.Add(100f / _loadedPresets.Count);
            while (_presetCoverage.Count > _loadedPresets.Count)
                _presetCoverage.RemoveAt(_presetCoverage.Count - 1);

            for (int i = 0; i < _loadedPresets.Count; i++)
            {
                var row = StyleUtils.HorizontalContainerCentered();
                var label = new Label(_loadedPresets[i].name) { style = { width = 100 } };
                row.Add(label);
                var field = new FloatField { value = _presetCoverage[i], style = { width = 50 } };
                field.RegisterValueChangedCallback(evt =>
                {
                    _presetCoverage[i] = Mathf.Clamp(evt.newValue, 0f, 100f);
                    EditorUtility.SetDirty(_loadedPresets[i]);
                });
                row.Add(field);
                fold.Add(row);
            }

            return fold;
        }

        private VisualElement CreatePresetSubsection(TilesetPreset preset, int presetIdx)
        {
            var section = StyleUtils.ModernSubFoldout(preset.name);
            section.AddManipulator(new ContextualMenuManipulator(evt =>
                evt.menu.AppendAction("Eliminar preset", _ => RemovePreset(preset))
            ));

            section.Add(CreateFloorTileSettings(preset, presetIdx));
            section.Add(CreateWallTileSettings(preset, presetIdx));

            return section;
        }

        private void RemovePreset(TilesetPreset preset)
        {
            int idx = _loadedPresets.IndexOf(preset);
            if (idx >= 0)
            {
                _loadedPresets.RemoveAt(idx);
                _presetCoverage.RemoveAt(idx);
            }

            EditorPrefs.SetString(LoadedPresetsKey,
                string.Join(";", _loadedPresets.Select(AssetDatabase.GetAssetPath)));

            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter as TilemapPainter;
            painter.RemovePreset(preset);

            RefreshUI();
        }

        // ─── Floor Settings ─────────────────────────────────────────────────

        private VisualElement CreateFloorTileSettings(TilesetPreset preset, int presetIdx)
        {
            // 1) Asegurarse de que priorities tenga la misma longitud que walkableTileBases
            SyncWalkableLists(preset);

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

            // Tile previews + priority
            var previewRow = StyleUtils.HorizontalContainerWrapped();
            for (int i = 0; i < walkables.Count; i++)
                previewRow.Add(CreateWalkableTileControl(preset, presetIdx, i));
            fe.Add(previewRow);

            return fe;
        }

        private void SyncWalkableLists(TilesetPreset preset)
        {
            var w = preset.walkableTileBases;
            var p = preset.walkableTilesPriorities;
            // añadir prioridades faltantes
            while (p.Count < w.Count) p.Add(1);
            // recortar excesos
            while (p.Count > w.Count) p.RemoveAt(p.Count - 1);
            EditorUtility.SetDirty(preset);
        }

        private VisualElement CreateWalkableTileControl(TilesetPreset preset, int presetIdx, int tileIdx)
        {
            var key = presetIdx * 1000 + tileIdx;
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
                            label.text = Utils.Utils.AddSpacesToCamelCase(
                                nt.name.Replace("floor", "", StringComparison.OrdinalIgnoreCase));
                        }
                    }
                })
                { style = { height = Utils.Utils.GetPreviewTileSize() } });

            // Priority field
            if (!isRandom)
            {
                var row = StyleUtils.HorizontalContainerCentered();
                var pflbl = StyleUtils.LabelForIntField("");
                pflbl.SetLocalizedText("Priority", "StyleTable");
                row.Add(pflbl);

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

        private static Label GetLabelForWalkableTile(TileBase tile)
        {
            var txt = tile != null
                ? Utils.Utils.AddSpacesToCamelCase(tile.name)
                : "No selected";
            var lbl = StyleUtils.LabelForTile(txt);
            if (tile == null) lbl.SetLocalizedText("NoSelected", "StyleTable");
            return lbl;
        }

        private static Texture GetPreviewTexture(TileBase tile)
        {
            if (tile == null)
                return EditorGUIUtility.IconContent(Utils.Utils.GetDefaultIconContent()).image;
            var pr = AssetPreview.GetAssetPreview(tile);
            return pr ?? (Texture2D)EditorGUIUtility.ObjectContent(tile, typeof(TileBase)).image;
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
            var controlID = presetIdx * 1000 + field.Name.GetHashCode();
            var cont = StyleUtils.TileContainer();
            var lbl = StyleUtils.LabelForTile(ObjectNames.NicifyVariableName(field.Name)
                .Replace("Wall", "", StringComparison.OrdinalIgnoreCase));
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

        private void RefreshUI()
        {
            _root.Clear();
            _root.Add(CreateStyleSection());
            if (_loadedPresets.Count > 1)
                _root.Add(CreateBiomeCoverageFolder());
            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter as TilemapPainter;
            for (int idx = 0; idx < _loadedPresets.Count; idx++)
            {
                var p = _loadedPresets[idx];
                painter.AddAndSelectPreset(p);
                _root.Add(CreatePresetSubsection(p, idx));
            }

            _root.MarkDirtyRepaint();
        }

        private void LoadPresetsFromEditorPrefs()
        {
            _loadedPresets.Clear();
            _presetCoverage.Clear();
            if (!EditorPrefs.HasKey(LoadedPresetsKey)) return;

            foreach (var path in EditorPrefs.GetString(LoadedPresetsKey).Split(';'))
            {
                var preset = AssetDatabase.LoadAssetAtPath<TilesetPreset>(path);
                if (preset != null)
                {
                    _loadedPresets.Add(preset);
                    _presetCoverage.Add(100f / Mathf.Max(1, _loadedPresets.Count));
                }
            }
        }
    }
}

#endif