using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Editor
{
    public class StyleManager
    {
        #region Campos

        private bool _showStyle = true;
        private bool _showFloorTileSettings = true;
        private bool _showWallTileSettings = true;
        private readonly Dictionary<string, bool> _showWallTileGroupSettings = new();

        private Vector2 _walkableTilesScrollPosition;
        private readonly Dictionary<string, Vector2> _wallScrollPositions = new();

        private readonly GeneratorSelection _generatorSelection;
        private SerializedObject _tilemapPainterObject;

        private readonly Dictionary<string, int> _walkableTilePriorities = new();

        private readonly Dictionary<int, Action<TileBase>> _tilePickerCallbacks = new();
        private IMGUIContainer _imGuiContainer; // IMGUIContainer to capture Object Picker events

        private VisualElement _root;

        #endregion

        //todo mas adelante mirar pq el random no funciona pq no deja de mostar la priority
        public StyleManager(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        /// <summary>
        /// Crea y retorna el árbol de VisualElements para la UI.
        /// </summary>
        public VisualElement CreateUI()
        {
            if (_root == null)
            {
                _root = Utils.CreateContainer();
            }
            else
            {
                _root.Clear();
            }

            // Add occult IMGUIContainer to capture Object Picker events
            if (_imGuiContainer == null)
            {
                _imGuiContainer = new IMGUIContainer(OnIMGUI)
                {
                    style = { display = DisplayStyle.None }
                };
                _root.Add(_imGuiContainer);
            }

            // Foldout principal "Style"
            var styleFoldout = new Foldout { text = "Style", value = _showStyle };
            styleFoldout.RegisterValueChangedCallback(evt => _showStyle = evt.newValue);
            _root.Add(styleFoldout);

            if (!HasValidTilemapPainter())
            {
                styleFoldout.Add(new Label("Tilemap Painter no válido."));
                return _root;
            }

            // Section Floor Tile Settings
            styleFoldout.Add(CreateFloorTileSettingsUI());

            // Section Wall Tile Settings
            //styleFoldout.Add(CreateWallTileSettingsUI());

            return _root;
        }

        #region Floor Tile Settings UI

        private VisualElement CreateFloorTileSettingsUI()
        {
            var container = new VisualElement();

            // Foldout para Floor Tile Settings
            var floorFoldout =
                CreateSubsectionFoldout("Floor Tile Settings", ref _showFloorTileSettings, ref container);

            var tilemapPainter = _generatorSelection.CurrentGenerator.TilemapPainter;

            CreateRandomFloorPlacementOption(tilemapPainter, floorFoldout);

            CreateFloorTilesButtonActions(floorFoldout);

            // ScrollView con la lista de tiles (preview, label y prioridad)
            var scrollView = new ScrollView
            {
                style =
                {
                    height = Utils.GetWalkableDisplayHeightScrollView(tilemapPainter)
                }
            };
            _tilemapPainterObject = new SerializedObject(tilemapPainter);

            if (TryGetWalkableTileProperties(out var tileBasesProperty, out var tilePrioritiesProperty))
            {
                var tilesContainer = new VisualElement();
                // Recorre cada tile de la lista
                for (var i = 0; i < tileBasesProperty.arraySize; i++)
                {
                    var tileProp = tileBasesProperty.GetArrayElementAtIndex(i);
                    var priorityProp = tilePrioritiesProperty.GetArrayElementAtIndex(i);
                    var tileName = $"Tile {i + 1}";
                    if (tileProp.objectReferenceValue != null)
                    {
                        tileName = tileProp.objectReferenceValue.name;
                        _walkableTilePriorities.TryAdd(tileName, i);
                    }

                    // Contenedor vertical para cada entrada de tile
                    var tileEntry = new VisualElement
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column,
                            borderBottomWidth = 1,
                            borderBottomColor = new Color(0.7f, 0.7f, 0.7f),
                            paddingBottom = 2,
                            marginBottom = 2
                        }
                    };

                    // Label con el nombre del tile
                    var tileLabel = new Label(tileName);
                    tileEntry.Add(tileLabel);

                    // Botón de preview que actúa como selector
                    var previewButton = new Button(() => { ShowTilePicker(tileProp, i); });
                    if (tileProp.objectReferenceValue != null)
                    {
                        var tileBase = tileProp.objectReferenceValue as TileBase;
                        Texture previewTexture = AssetPreview.GetAssetPreview(tileBase) ??
                                                 AssetPreview.GetMiniThumbnail(tileBase);
                        if (previewTexture != null)
                        {
                            var tileImage = new Image();
                            tileImage.image = previewTexture;
                            tileImage.style.width = Utils.GetPreviewTileSize();
                            tileImage.style.height = Utils.GetPreviewTileSize();
                            previewButton.Clear();
                            previewButton.Add(tileImage);
                        }
                    }
                    else
                    {
                        previewButton.text = "Select Tile";
                    }

                    // Menú contextual para remover el tile
                    previewButton.AddManipulator(new ContextualMenuManipulator(evt =>
                    {
                        evt.menu.AppendAction("Remove tile", _ =>
                        {
                            RemoveTileAtIndex(i);
                            RefreshUI();
                        });
                    }));
                    tileEntry.Add(previewButton);

                    // Campo para editar la prioridad del tile
                    var priorityField = new IntegerField("Priority")
                    {
                        value = priorityProp.intValue
                    };
                    priorityField.RegisterValueChangedCallback(evt =>
                    {
                        priorityProp.intValue = evt.newValue;
                        _tilemapPainterObject.ApplyModifiedProperties();
                        RefreshUI();
                    });
                    tileEntry.Add(priorityField);

                    tilesContainer.Add(tileEntry);
                }

                scrollView.Add(tilesContainer);
            }

            floorFoldout.Add(scrollView);
            _tilemapPainterObject.ApplyModifiedProperties();

            return container;
        }

        private void CreateFloorTilesButtonActions(Foldout floorFoldout)
        {
            var buttonsRow = Utils.CreateButtonsRow();

            var addButton = new Button(() =>
            {
                AddTileToWalkable();
                RefreshUI();
            })
            {
                text = "Add Floor tile"
            };
            var clearButton = new Button(() =>
            {
                ClearWalkableTiles();
                RefreshUI();
            })
            {
                text = "Clear all floor tiles"
            };
            var selectButton = new Button(() =>
            {
                SelectWalkableTilesFromFolder();
                RefreshUI();
            })
            {
                text = "Select floor tiles from folder"
            };

            buttonsRow.Add(addButton);
            buttonsRow.Add(clearButton);
            buttonsRow.Add(selectButton);
            floorFoldout.Add(buttonsRow);
        }

        private void CreateRandomFloorPlacementOption(TilemapPainter tilemapPainter, Foldout floorFoldout)
        {
            var toggleContainer = Utils.CreateToggleContainer();
            var randomLabel = Utils.CreateLabelWithMinLength("¿Random floor placement?", 150);
            toggleContainer.Add(randomLabel);

            var randomToggle = Utils.CreateToggle(tilemapPainter.randomWalkableTilesPlacement);
            randomToggle.RegisterValueChangedCallback(evt =>
            {
                tilemapPainter.randomWalkableTilesPlacement = evt.newValue;
                EditorUtility.SetDirty(tilemapPainter);
                RefreshUI();
            });
            toggleContainer.Add(randomToggle);
            floorFoldout.Add(toggleContainer);
        }

        private Foldout CreateSubsectionFoldout(string foldoutText, ref bool showFoldout, ref VisualElement container)
        {
            var foldout = new Foldout { text = foldoutText, value = showFoldout };
            foldout.RegisterValueChangedCallback(evt => _showFloorTileSettings = evt.newValue);
            container.Add(foldout);
            return foldout;
        }

        #endregion

        #region Wall Tile Settings UI

        private VisualElement CreateWallTileSettingsUI()
        {
            VisualElement container = new VisualElement();

            // Foldout para Wall Tile Settings
            Foldout wallFoldout = new Foldout() { text = "Wall Tile Settings", value = _showWallTileSettings };
            wallFoldout.RegisterValueChangedCallback(evt => _showWallTileSettings = evt.newValue);
            container.Add(wallFoldout);

            // Fila con botón para limpiar wall tiles
            VisualElement wallButtonsRow = new VisualElement();
            wallButtonsRow.style.flexDirection = FlexDirection.Row;
            wallButtonsRow.style.marginBottom = 5;
            Button clearWallButton = new Button(() =>
            {
                ClearWallTiles();
                RefreshUI();
            })
            {
                text = "Clear all wall tiles"
            };
            wallButtonsRow.Add(clearWallButton);
            wallFoldout.Add(wallButtonsRow);

            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
            _tilemapPainterObject.Update();

            // Agrupación de campos de wall tiles usando reflection
            var groupedWallFields = GetGroupedFields<WallTileGroupAttribute>(attr => attr.GroupName);
            foreach (var group in groupedWallFields)
            {
                string groupName = group.Key;
                if (!_wallScrollPositions.ContainsKey(groupName))
                    _wallScrollPositions[groupName] = Vector2.zero;
                if (!_showWallTileGroupSettings.ContainsKey(groupName))
                    _showWallTileGroupSettings[groupName] = true;

                // Foldout para cada grupo de wall tiles
                Foldout groupFoldout = new Foldout()
                    { text = groupName, value = _showWallTileGroupSettings[groupName] };
                groupFoldout.RegisterValueChangedCallback(evt => _showWallTileGroupSettings[groupName] = evt.newValue);
                wallFoldout.Add(groupFoldout);

                if (_showWallTileGroupSettings[groupName])
                {
                    ScrollView groupScroll = new ScrollView();
                    groupScroll.style.height = Utils.GetWalllDisplayHeightScrollView();
                    // Contenedor horizontal para los botones de cada tile
                    VisualElement wallTilesContainer = new VisualElement();
                    wallTilesContainer.style.flexDirection = FlexDirection.Row;
                    wallTilesContainer.style.flexWrap = Wrap.Wrap;

                    foreach (var field in group)
                    {
                        SerializedProperty wallProp = _tilemapPainterObject.FindProperty(field.Name);
                        if (wallProp == null)
                            continue;

                        string label = ObjectNames.NicifyVariableName(field.Name)
                            .Replace("wall", "", StringComparison.OrdinalIgnoreCase)
                            .Replace("inner", "", StringComparison.OrdinalIgnoreCase)
                            .Replace("triple", "", StringComparison.OrdinalIgnoreCase);
                        var parts = label.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        label = string.Join("\n", parts);
                        int controlID = field.Name.GetHashCode() & 0x7FFFFFFF;

                        // Contenedor vertical para cada wall tile
                        VisualElement wallTileEntry = new VisualElement();
                        wallTileEntry.style.flexDirection = FlexDirection.Column;

                        Label wallLabel = new Label(label);
                        wallTileEntry.Add(wallLabel);

                        Button wallTileButton = new Button(() => { ShowTilePicker(wallProp, controlID); });
                        if (wallProp.objectReferenceValue != null)
                        {
                            TileBase tileBase = wallProp.objectReferenceValue as TileBase;
                            Texture previewTexture = AssetPreview.GetAssetPreview(tileBase) ??
                                                     AssetPreview.GetMiniThumbnail(tileBase);
                            if (previewTexture != null)
                            {
                                Image tileImage = new Image();
                                tileImage.image = previewTexture;
                                tileImage.style.width = Utils.GetPreviewTileSize();
                                tileImage.style.height = Utils.GetPreviewTileSize();
                                wallTileButton.Clear();
                                wallTileButton.Add(tileImage);
                            }
                        }
                        else
                        {
                            wallTileButton.text = "Select Tile";
                        }

                        wallTileButton.AddManipulator(new ContextualMenuManipulator(evt =>
                        {
                            evt.menu.AppendAction("Remove tile", action =>
                            {
                                wallProp.objectReferenceValue = null;
                                _tilemapPainterObject.ApplyModifiedProperties();
                                RefreshUI();
                            });
                        }));
                        wallTileEntry.Add(wallTileButton);
                        wallTilesContainer.Add(wallTileEntry);
                    }

                    groupScroll.Add(wallTilesContainer);
                    groupFoldout.Add(groupScroll);
                }
            }

            _tilemapPainterObject.ApplyModifiedProperties();
            return container;
        }

        #endregion

        #region Métodos de acción y Helpers

        private void AddTileToWalkable()
        {
            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
            if (!TryGetWalkableTileProperties(out var tileBasesProperty, out var tilePrioritiesProperty)) return;

            tileBasesProperty.InsertArrayElementAtIndex(tileBasesProperty.arraySize);
            tileBasesProperty.GetArrayElementAtIndex(tileBasesProperty.arraySize - 1).objectReferenceValue = null;
            tilePrioritiesProperty.InsertArrayElementAtIndex(tilePrioritiesProperty.arraySize);
            tilePrioritiesProperty.GetArrayElementAtIndex(tilePrioritiesProperty.arraySize - 1).intValue = 0;
            _tilemapPainterObject.ApplyModifiedProperties();
        }

        private void ClearWalkableTiles()
        {
            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
            if (TryGetWalkableTileProperties(out SerializedProperty tileBasesProperty,
                    out SerializedProperty tilePrioritiesProperty))
            {
                tileBasesProperty.ClearArray();
                tilePrioritiesProperty.ClearArray();
                _tilemapPainterObject.ApplyModifiedProperties();
            }
        }

        private void SelectWalkableTilesFromFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select a folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                _generatorSelection.CurrentGenerator.TilemapPainter.SelectWalkableTilesFromFolder(path);
            }

            _tilemapPainterObject.Update();
            _tilemapPainterObject.ApplyModifiedProperties();
        }

        private void ClearWallTiles()
        {
            _generatorSelection.CurrentGenerator.TilemapPainter.RemoveAllWallTiles();
        }

        /// <summary>
        /// Muestra el Object Picker y registra un callback para asignar el tile seleccionado.
        /// </summary>
        private void ShowTilePicker(SerializedProperty property, int controlID)
        {
            _tilePickerCallbacks[controlID] = (TileBase selectedTile) =>
            {
                _tilemapPainterObject.Update();
                property.objectReferenceValue = selectedTile;
                _tilemapPainterObject.ApplyModifiedProperties();
                RefreshUI();
            };
            TileBase currentTile = property.objectReferenceValue as TileBase;
            EditorGUIUtility.ShowObjectPicker<TileBase>(currentTile, false, "", controlID);
        }

        private void RemoveTileAtIndex(int index)
        {
            if (TryGetWalkableTileProperties(out SerializedProperty tileBasesProperty,
                    out SerializedProperty tilePrioritiesProperty))
            {
                tileBasesProperty.DeleteArrayElementAtIndex(index);
                tilePrioritiesProperty.DeleteArrayElementAtIndex(index);
                _tilemapPainterObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Invoca el callback de refresco para reconstruir la UI.
        /// </summary>
        private void RefreshUI()
        {
            if (_root == null)
            {
                _root = Utils.CreateContainer();
            }
            else
            {
                _root.Clear();
            }
        
            // Recreate the UI
            _root.MarkDirtyRepaint();
            CreateUI();
        }

        /// <summary>
        /// Método llamado desde el IMGUIContainer para capturar eventos del Object Picker.
        /// </summary>
        private void OnIMGUI()
        {
            if (Event.current == null) return;
            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                int controlID = EditorGUIUtility.GetObjectPickerControlID();
                if (_tilePickerCallbacks.TryGetValue(controlID, out Action<TileBase> callback))
                {
                    TileBase selectedTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;
                    callback(selectedTile);
                    _tilePickerCallbacks.Remove(controlID);
                }
            }
        }

        private static IEnumerable<IGrouping<string, FieldInfo>> GetGroupedFields<TAttribute>(
            Func<TAttribute, string> groupSelector)
            where TAttribute : Attribute
        {
            return typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.FieldType == typeof(TileBase) && f.IsDefined(typeof(TAttribute), false))
                .GroupBy(f => groupSelector(f.GetCustomAttribute<TAttribute>()));
        }

        private bool TryGetWalkableTileProperties(out SerializedProperty tileBasesProperty,
            out SerializedProperty tilePrioritiesProperty)
        {
            _tilemapPainterObject = new SerializedObject(_generatorSelection.CurrentGenerator.TilemapPainter);
            Type painterType = typeof(TilemapPainter);

            FieldInfo tileBasesField = painterType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.IsDefined(typeof(WalkableTileGroupAttribute), false) &&
                                     ((WalkableTileGroupAttribute)f.GetCustomAttribute(
                                         typeof(WalkableTileGroupAttribute), false)).IsTileBases);
            FieldInfo prioritiesField = painterType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.IsDefined(typeof(WalkableTileGroupAttribute), false) &&
                                     ((WalkableTileGroupAttribute)f.GetCustomAttribute(
                                         typeof(WalkableTileGroupAttribute), false)).IsTilePriorities);
            if (tileBasesField == null || prioritiesField == null)
            {
                Debug.LogError(
                    "Walkable tile fields not found. Ensure they are decorated with WalkableTileGroupAttribute.");
                tileBasesProperty = null;
                tilePrioritiesProperty = null;
                return false;
            }

            tileBasesProperty = _tilemapPainterObject.FindProperty(tileBasesField.Name);
            tilePrioritiesProperty = _tilemapPainterObject.FindProperty(prioritiesField.Name);
            return true;
        }

        private bool HasValidTilemapPainter()
        {
            return _generatorSelection.CurrentGenerator &&
                   _generatorSelection.CurrentGenerator.TilemapPainter;
        }

        #endregion
    }
}