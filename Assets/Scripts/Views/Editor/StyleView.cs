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
        /// Key used to store and retrieve loaded presets from EditorPrefs.
        /// </summary>
        private const string LoadedPresetsKey = "StyleView_LoadedPresets";

        /// <summary>
        /// Root visual element for the UI.
        /// </summary>
        private VisualElement _root;

        /// <summary>
        /// Controller for managing style-related operations.
        /// </summary>
        private readonly StyleController _styleController = new();

        /// <summary>
        /// List of loaded tileset presets.
        /// </summary>
        private readonly List<TilesetPreset> _loadedPresets = new();

        /// <summary>
        /// List of coverage percentages for each preset.
        /// </summary>
        private readonly List<float> _presetCoverage = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleView"/> class and loads presets from EditorPrefs.
        /// </summary>
        public StyleView() => LoadPresetsFromEditorPrefs();

        /// <summary>
        /// Creates the UI for the Style View.
        /// </summary>
        /// <returns>The root visual element containing the UI.</returns>
        public VisualElement CreateUI()
        {
            var gen = GeneratorService.Instance.CurrentGenerator;
            if (gen == null) return new VisualElement();

            _root ??= StyleUtils.SimpleContainer();
            _root.Clear();

            var styleSection = CreateStyleSection();
            _root.Add(styleSection);

            if (_loadedPresets.Count > 1)
                styleSection.Add(CreateBiomeCoverageFolder());

            var painter = gen.TilemapPainter;
            for (var idx = 0; idx < _loadedPresets.Count; idx++)
            {
                var preset = _loadedPresets[idx];
                painter.AddAndSelectPreset(preset);
                styleSection.Add(CreatePresetSubsection(preset, idx));
            }

            return _root;
        }

        /// <summary>
        /// Creates the main style section of the UI.
        /// </summary>
        /// <returns>A visual element representing the style section.</returns>
        private VisualElement CreateStyleSection()
        {
            var section = StyleUtils.ModernFoldout(string.Empty);
            section.SetLocalizedText(LocalizationKeysHelper.StyleFoldout, LocalizationKeysHelper.StyleTable);
            section.Add(CreatePresetSettings());
            return section;
        }

        /// <summary>
        /// Creates the preset settings UI, including a button to load presets.
        /// </summary>
        /// <returns>A visual element containing the preset settings.</returns>
        private VisualElement CreatePresetSettings()
        {
            var container = new VisualElement();
            var loadBtn = new Button(() =>
            {
                var path = EditorUtility.OpenFilePanel("Select Preset", "Assets", "asset");
                if (string.IsNullOrEmpty(path) || !path.StartsWith(Application.dataPath)) return;
                path = "Assets" + path[Application.dataPath.Length..];

                var preset = AssetDatabase.LoadAssetAtPath<TilesetPreset>(path);
                if (preset == null)
                {
                    var text = LocalizationUIHelper.SetLocalizedText(LocalizationKeysHelper.StyleLoadPresetError,
                        LocalizationKeysHelper.StyleTable);
                    EditorUtility.DisplayDialog("Error", text, "OK");
                    return;
                }

                _loadedPresets.Add(preset);
                _presetCoverage.Add(100f / _loadedPresets.Count);
                EditorPrefs.SetString(LoadedPresetsKey,
                    string.Join(";", _loadedPresets.Select(AssetDatabase.GetAssetPath)));

                RefreshUI();
            });
            loadBtn.SetLocalizedText(LocalizationKeysHelper.StyleLoadPreset, LocalizationKeysHelper.StyleTable);
            container.Add(loadBtn);
            return container;
        }

        /// <summary>
        /// Creates a foldout for managing biome coverage settings.
        /// </summary>
        /// <returns>A visual element representing the biome coverage folder.</returns>
        private VisualElement CreateBiomeCoverageFolder()
        {
            var fold = StyleUtils.ModernFoldout(string.Empty);
            fold.SetLocalizedText(LocalizationKeysHelper.StyleBiomeCoverage, LocalizationKeysHelper.StyleTable);

            // Ensure _presetCoverage and _loadedPresets have the same length
            while (_presetCoverage.Count < _loadedPresets.Count)
                _presetCoverage.Add(100f / _loadedPresets.Count);
            while (_presetCoverage.Count > _loadedPresets.Count)
                _presetCoverage.RemoveAt(_presetCoverage.Count - 1);

            // Update the painter with the full collection
            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
            painter.SetPresetCoverages(_presetCoverage);

            // Build each row
            for (var i = 0; i < _loadedPresets.Count; i++)
            {
                var idx = i;

                var row = StyleUtils.HorizontalContainerCentered();

                var label = StyleUtils.LabelForIntField(_loadedPresets[idx].name);
                row.Add(label);

                var field = new FloatField { value = _presetCoverage[idx], style = { width = 50 } };
                field.RegisterValueChangedCallback(evt =>
                {
                    _presetCoverage[idx] = Mathf.Clamp(evt.newValue, 0f, 100f);
                    painter.SetPresetCoverages(_presetCoverage);
                    EditorUtility.SetDirty(_loadedPresets[idx]);
                });
                row.Add(field);

                fold.Add(row);
            }

            return fold;
        }

        /// <summary>
        /// Creates a subsection for a specific preset, including floor and wall tile settings.
        /// </summary>
        /// <param name="preset">The tileset preset to create the subsection for.</param>
        /// <param name="presetIdx">The index of the preset in the list.</param>
        /// <returns>A visual element representing the preset subsection.</returns>
        private VisualElement CreatePresetSubsection(TilesetPreset preset, int presetIdx)
        {
            var section = StyleUtils.ModernSubFoldout(preset.name);

            section.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    var text = LocalizationUIHelper.SetLocalizedText(LocalizationKeysHelper.StyleDeletePreset,
                        LocalizationKeysHelper.StyleTable);
                    evt.menu.AppendAction(text, _ => RemovePreset(preset));
                }
            ));

            section.Add(CreateFloorTileSettings(preset, presetIdx));
            section.Add(CreateWallTileSettings(preset, presetIdx));

            return section;
        }

        /// <summary>
        /// Removes a preset from the list and updates the UI.
        /// </summary>
        /// <param name="preset">The preset to remove.</param>
        private void RemovePreset(TilesetPreset preset)
        {
            var idx = _loadedPresets.IndexOf(preset);
            if (idx >= 0)
            {
                _loadedPresets.RemoveAt(idx);
                _presetCoverage.RemoveAt(idx);
            }

            EditorPrefs.SetString(LoadedPresetsKey,
                string.Join(";", _loadedPresets.Select(AssetDatabase.GetAssetPath)));

            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
            painter.RemovePreset(preset);

            RefreshUI();
        }

        /// <summary>
        /// Creates the floor tile settings UI for a specific preset.
        /// </summary>
        /// <param name="preset">The tileset preset to configure.</param>
        /// <param name="presetIdx">The index of the preset in the list.</param>
        /// <returns>A visual element representing the floor tile settings.</returns>
        private VisualElement CreateFloorTileSettings(TilesetPreset preset, int presetIdx)
        {
            SyncWalkableLists(preset);

            var walkables = preset.walkableTileBases;
            var priorities = preset.walkableTilesPriorities;

            var fe = StyleUtils.ModernSubFoldout(string.Empty);
            fe.SetLocalizedText(LocalizationKeysHelper.StyleFloorSettings, LocalizationKeysHelper.StyleTable);

            var toggleRow = StyleUtils.HorizontalContainerCentered();
            var lbl = StyleUtils.LabelForToggle(string.Empty);
            lbl.SetLocalizedText(LocalizationKeysHelper.StyleRandomPlacement, LocalizationKeysHelper.StyleTable);
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

            var btnRow = StyleUtils.HorizontalContainerCentered();
            var addBtn = new Button(() =>
            {
                walkables.Add(null);
                priorities.Add(0);
                EditorUtility.SetDirty(preset);
                RefreshUI();
            });
            addBtn.SetLocalizedText(LocalizationKeysHelper.StyleAddFloorTile, LocalizationKeysHelper.StyleTable);
            btnRow.Add(addBtn);

            var clearBtn = new Button(() =>
            {
                walkables.Clear();
                priorities.Clear();
                EditorUtility.SetDirty(preset);
                RefreshUI();
            });
            clearBtn.SetLocalizedText(LocalizationKeysHelper.StyleClearFloorTiles, LocalizationKeysHelper.StyleTable);
            btnRow.Add(clearBtn);

            fe.Add(btnRow);

            var previewRow = StyleUtils.HorizontalContainerWrapped();
            for (var i = 0; i < walkables.Count; i++)
                previewRow.Add(CreateWalkableTileControl(preset, presetIdx, i));
            fe.Add(previewRow);

            return fe;
        }

        /// <summary>
        /// Synchronizes the walkable tile lists in the given preset by ensuring that the priorities list
        /// matches the length of the walkable tiles list. Adds default priorities or removes excess priorities as needed.
        /// </summary>
        /// <param name="preset">The tileset preset to synchronize.</param>
        private void SyncWalkableLists(TilesetPreset preset)
        {
            var w = preset.walkableTileBases; 
            var p = preset.walkableTilesPriorities; 
            while (p.Count < w.Count) p.Add(1); // Add default priority values if priorities list is shorter
            while (p.Count > w.Count) p.RemoveAt(p.Count - 1); // Remove excess priorities if priorities list is longer
            EditorUtility.SetDirty(preset); 
        }

        /// <summary>
        /// Creates a visual element for controlling a specific walkable tile in the given preset.
        /// Includes a label, a tile preview button, and optionally a priority field if random placement is disabled.
        /// </summary>
        /// <param name="preset">The tileset preset containing the walkable tile.</param>
        /// <param name="presetIdx">The index of the preset in the list.</param>
        /// <param name="tileIdx">The index of the walkable tile in the preset.</param>
        /// <returns>A visual element representing the walkable tile control.</returns>
        private VisualElement CreateWalkableTileControl(TilesetPreset preset, int presetIdx, int tileIdx)
        {
            var key = presetIdx * 1000 + tileIdx; // Unique key for the tile control
            var walkables = preset.walkableTileBases; 
            var priorities = preset.walkableTilesPriorities; 
            var isRandom = preset.randomWalkableTilesPlacement; 

            var cont = StyleUtils.TileContainer(); 
            var label = GetLabelForWalkableTile(walkables[tileIdx]); 
            
            label.RegisterCallback<MouseDownEvent>(evt =>
            {
                var text = LocalizationUIHelper.SetLocalizedText(LocalizationKeysHelper.StyleDeleteTile,
                    LocalizationKeysHelper.StyleTable);
                if (evt.button != (int)MouseButton.RightMouse) return;
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(text), false, () =>
                {
                    walkables.RemoveAt(tileIdx);
                    if (tileIdx < priorities.Count)
                        priorities.RemoveAt(tileIdx);

                    EditorUtility.SetDirty(preset);
                    RefreshUI();
                });
                menu.ShowAsContext();
                evt.StopPropagation();
            });
        
            cont.Add(label);

            // Add a button for selecting a tile with a preview image
            cont.Add(new IMGUIContainer(() =>
                {
                    var curr = walkables[tileIdx];
                    var tex = GetPreviewTexture(curr); 
                    var size = Utils.Utils.GetPreviewTileSize();

                    if (GUILayout.Button(tex, GUILayout.Width(size), GUILayout.Height(size)))
                        EditorGUIUtility.ShowObjectPicker<TileBase>(curr, false, string.Empty, key);

                    if (Event.current?.commandName != Utils.Utils.GetObjectSelectorUpdateCommand() ||
                        EditorGUIUtility.GetObjectPickerControlID() != key) return;

                    var nt = EditorGUIUtility.GetObjectPickerObject() as TileBase;

                    if (nt == null) return;
                    walkables[tileIdx] = nt;
                    EditorUtility.SetDirty(preset);
                    label.text = Utils.Utils.AddSpacesToCamelCase(
                        nt.name.Replace("floor", string.Empty, StringComparison.OrdinalIgnoreCase));
                })
                { style = { height = Utils.Utils.GetPreviewTileSize() } });

            if (isRandom) return cont;

            // Add a priority field for the tile
            var row = StyleUtils.HorizontalContainerCentered();
            var pflbl = StyleUtils.LabelForIntField(string.Empty);
            pflbl.SetLocalizedText(LocalizationKeysHelper.StylePriority, LocalizationKeysHelper.StyleTable);
            row.Add(pflbl);

            var intF = StyleUtils.SimpleIntField(priorities[tileIdx]);
            intF.RegisterValueChangedCallback(evt =>
            {
                priorities[tileIdx] = evt.newValue;
                EditorUtility.SetDirty(preset);
            });
            row.Add(intF);
            cont.Add(row);

            return cont;
        }

        /// <summary>
        /// Generates a label for a walkable tile. If the tile is null, a default "No selected" label is returned.
        /// </summary>
        /// <param name="tile">The tile to generate the label for.</param>
        /// <returns>A label element for the tile.</returns>
        private static Label GetLabelForWalkableTile(TileBase tile)
        {
            var txt = tile != null
                ? Utils.Utils.AddSpacesToCamelCase(tile.name)
                : "No selected";
            var lbl = StyleUtils.LabelForTile(txt);
            if (tile == null)
                lbl.SetLocalizedText(LocalizationKeysHelper.StyleNoSelected, LocalizationKeysHelper.StyleTable);
            return lbl;
        }

        /// <summary>
        /// Retrieves the preview texture for a given tile. If the tile is null, a default icon is returned.
        /// </summary>
        /// <param name="tile">The tile to retrieve the preview texture for.</param>
        /// <returns>The preview texture for the tile.</returns>
        private static Texture GetPreviewTexture(TileBase tile)
        {
            if (tile == null)
                return EditorGUIUtility.IconContent(Utils.Utils.GetDefaultIconContent())
                    .image;
            var pr = AssetPreview.GetAssetPreview(tile);
            return pr ??
                   (Texture2D)EditorGUIUtility.ObjectContent(tile, typeof(TileBase))
                       .image;
        }

        /// <summary>
        /// Creates the wall tile settings UI for a specific preset. Groups wall tiles by their attributes
        /// and generates controls for each group.
        /// </summary>
        /// <param name="preset">The tileset preset to configure.</param>
        /// <param name="presetIdx">The index of the preset in the list.</param>
        /// <returns>A visual element representing the wall tile settings.</returns>
        private VisualElement CreateWallTileSettings(TilesetPreset preset, int presetIdx)
        {
            var container = new VisualElement();
            var groups = _styleController
                .GetGroupedFields<
                    WallTileGroupAttribute>(a => a.GroupName);

            foreach (var group in groups)
            {
                var fold = StyleUtils.ModernSubFoldout(string.Empty);
                fold.SetLocalizedText(group.Key, LocalizationKeysHelper.StyleTable);

                var row = StyleUtils.HorizontalContainerWrapped();
                foreach (var field in group)
                    row.Add(CreateWallTileControl(preset, presetIdx,
                        field)); 
                fold.Add(row);
                container.Add(fold);
            }

            return container;
        }

        /// <summary>
        /// Creates a control for a specific wall tile field in the given preset.
        /// Includes a label and a tile preview button for selecting a tile.
        /// </summary>
        /// <param name="preset">The tileset preset containing the wall tile field.</param>
        /// <param name="presetIdx">The index of the preset in the list.</param>
        /// <param name="field">The field representing the wall tile.</param>
        /// <returns>A visual element representing the wall tile control.</returns>
        private VisualElement CreateWallTileControl(TilesetPreset preset, int presetIdx, FieldInfo field)
        {
            var controlID = presetIdx * 1000 + field.Name.GetHashCode(); 
            var cont = StyleUtils.TileContainer();
            var lbl = StyleUtils.LabelForTile(ObjectNames.NicifyVariableName(field.Name)
                .Replace("Wall", string.Empty, StringComparison.OrdinalIgnoreCase));
            lbl.SetLocalizedText(field.Name, LocalizationKeysHelper.StyleTable);
            cont.Add(lbl);

            cont.Add(new IMGUIContainer(() =>
                {
                    var curr = field.GetValue(preset) as TileBase;
                    var tex = GetPreviewTexture(curr);
                    var size = Utils.Utils.GetPreviewTileSize();

                    if (GUILayout.Button(tex, GUILayout.Width(size), GUILayout.Height(size)))
                        EditorGUIUtility.ShowObjectPicker<TileBase>(curr, false, string.Empty, controlID);

                    if (Event.current?.commandName != Utils.Utils.GetObjectSelectorUpdateCommand() ||
                        EditorGUIUtility.GetObjectPickerControlID() != controlID) return;

                    var nt = EditorGUIUtility.GetObjectPickerObject() as TileBase;

                    if (nt == null) return;
                    field.SetValue(preset, nt);
                    EditorUtility.SetDirty(preset);
                })
                { style = { height = Utils.Utils.GetPreviewTileSize() } });

            return cont;
        }

        /// <summary>
        /// Refreshes the UI by clearing and rebuilding it.
        /// </summary>
        private void RefreshUI()
        {
            _root.Clear();

            var styleSection = CreateStyleSection();
            _root.Add(styleSection);

            if (_loadedPresets.Count > 1)
                styleSection.Add(CreateBiomeCoverageFolder());

            var painter = GeneratorService.Instance.CurrentGenerator.TilemapPainter;
            for (var idx = 0; idx < _loadedPresets.Count; idx++)
            {
                var preset = _loadedPresets[idx];
                painter.AddAndSelectPreset(preset);
                styleSection.Add(CreatePresetSubsection(preset, idx));
            }

            _root.MarkDirtyRepaint();
        }

        /// <summary>
        /// Loads presets from EditorPrefs and initializes the preset coverage list.
        /// </summary>
        private void LoadPresetsFromEditorPrefs()
        {
            _loadedPresets.Clear();
            _presetCoverage.Clear();
            if (!EditorPrefs.HasKey(LoadedPresetsKey)) return;

            foreach (var path in EditorPrefs.GetString(LoadedPresetsKey).Split(';'))
            {
                var preset = AssetDatabase.LoadAssetAtPath<TilesetPreset>(path);

                if (preset == null) continue;
                _loadedPresets.Add(preset);
                _presetCoverage.Add(100f / Mathf.Max(1, _loadedPresets.Count));
            }
        }
    }
}

#endif