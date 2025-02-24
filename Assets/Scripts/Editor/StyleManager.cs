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
        # region Fields

        private readonly GeneratorSelection _generatorSelection;

        private VisualElement _root;

        private List<TileBase> _walkableTileBases;
        private List<int> _walkableTilesPriorities;
        private bool _randomPlacement;

        # endregion

        # region General Methods

        public StyleManager(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        public VisualElement CreateUI()
        {
            _walkableTileBases = _generatorSelection.CurrentGenerator.TilemapPainter.walkableTileBases;
            _walkableTilesPriorities = _generatorSelection.CurrentGenerator.TilemapPainter.walkableTilesPriorities;
            _randomPlacement = _generatorSelection.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement;

            if (_root == null)
            {
                _root = Utils.CreateContainer();
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

            _root.MarkDirtyRepaint();
            CreateUI();
        }

        # endregion

        # region Walkable Tiles - Floor Tile Settings

        private VisualElement CreateFloorTileSettings()
        {
            var floorTileSettings = new Foldout { text = "Floor Tile Settings", value = true };
            floorTileSettings.Add(CreateRandomFloorPlacementToggle());

            if (!HasValidTilemapPainter()) return floorTileSettings;

            floorTileSettings.Add(CreateWalkableOptionsButtons());
            floorTileSettings.Add(CreateWalkableTileGroupSettings());
            return floorTileSettings;
        }

        private VisualElement CreateRandomFloorPlacementToggle()
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            var label = new Label("¿Random Floor Placement?")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginRight = 10
                }
            };
            container.Add(label);

            var toggle = new Toggle
            {
                value = _randomPlacement
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                _generatorSelection.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement = evt.newValue;
                EditorUtility.SetDirty(_generatorSelection.CurrentGenerator.TilemapPainter);
                RefreshUI();
            });
            container.Add(toggle);

            return container;
        }

        private VisualElement CreateWalkableOptionsButtons()
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

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

        private bool HasValidTilemapPainter()
        {
            return _generatorSelection.CurrentGenerator &&
                   _generatorSelection.CurrentGenerator.TilemapPainter;
        }

        # endregion

        # region Walkable Tiles - Tile Group Settings

        private VisualElement CreateWalkableTileGroupSettings()
        {
            var container = new VisualElement();

            if (!IsGeneratorSelectionValid())
            {
                Debug.LogError("Generator selection or its properties are not properly initialized.");
                return container;
            }

            var walkableTiles = _generatorSelection.CurrentGenerator.TilemapPainter.walkableTileBases;
            var horizontalContainer = Utils.CreateHorizontalContainer();

            for (var index = 0; index < walkableTiles.Count; index++)
            {
                var tileContainer = CreateTileContainer(walkableTiles, index);
                horizontalContainer.Add(tileContainer);
            }

            container.Add(horizontalContainer);
            return container;
        }

        private bool IsGeneratorSelectionValid()
        {
            return _generatorSelection != null &&
                   _generatorSelection.CurrentGenerator != null &&
                   _generatorSelection.CurrentGenerator.TilemapPainter != null;
        }


        private VisualElement CreateTileContainer(List<TileBase> walkableTiles, int index)
        {
            var walkableTile = walkableTiles[index];

            var tileContainer = Utils.CreateTileContainer();

            var label = GetLaberFromTile(walkableTile);
            tileContainer.Add(label);

            var imguiPreviewContainer = CreateIMGUIContainer(walkableTiles, index, label);
            imguiPreviewContainer.style.height = 60;
            tileContainer.Add(imguiPreviewContainer);

            if (_generatorSelection.CurrentGenerator.TilemapPainter.randomWalkableTilesPlacement) return tileContainer;

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
            if (currentTile == null) return EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image;

            var previewTexture = AssetPreview.GetAssetPreview(currentTile);
            if (previewTexture == null)
            {
                previewTexture = (Texture2D)EditorGUIUtility.ObjectContent(currentTile, typeof(TileBase)).image;
            }

            return previewTexture;
        }

        private void UpdateTileOnSelection(int currentIndex, Label label)
        {
            if (Event.current == null || Event.current.commandName != "ObjectSelectorUpdated") return;

            var pickerControlId = EditorGUIUtility.GetObjectPickerControlID();

            if (pickerControlId != currentIndex) return;

            var newTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;

            if (newTile == null) return;

            _generatorSelection.CurrentGenerator.TilemapPainter.walkableTileBases[currentIndex] = newTile;
            var newName = (newTile?.name).Replace("floor", "", StringComparison.OrdinalIgnoreCase);
            label.text = Utils.AddSpacesToCamelCase(newName);
        }

        private static Label GetLaberFromTile(TileBase walkableTile)
        {
            var labelText = walkableTile?.name ?? "No selected";
            labelText = labelText.Replace("floor", "", StringComparison.OrdinalIgnoreCase);
            labelText = Utils.AddSpacesToCamelCase(labelText);

            return Utils.CreateLabelForTile(labelText);
        }

        private VisualElement AddPriorityToTilesUI(int index)
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            if (_randomPlacement) return container;

            var label = new Label("Priority:")
            {
                style =
                {
                    width = 50,
                    marginRight = 5
                }
            };

            var intField = new IntegerField
            {
                value = _walkableTilesPriorities[index],
                style =
                {
                    width = 30
                }
            };

            intField.RegisterValueChangedCallback(evt =>
            {
                _walkableTilesPriorities[index] = evt.newValue;
                EditorUtility.SetDirty(_generatorSelection.CurrentGenerator.TilemapPainter);
            });

            container.Add(label);
            container.Add(intField);

            return container;
        }

        # endregion
        
        #region Wall Tiles - Wall Tile Settings
        
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
        
        private static IEnumerable<IGrouping<string, FieldInfo>> GetGroupedFields<TAttribute>(
            Func<TAttribute, string> groupSelector) where TAttribute : Attribute
        {
            return typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.FieldType == typeof(TileBase) && f.IsDefined(typeof(TAttribute), false))
                .GroupBy(f => groupSelector(f.GetCustomAttribute<TAttribute>()));
        }
        
        private Foldout CreateFoldoutForGroup(IGrouping<string, FieldInfo> group)
        {
            var foldout = new Foldout { text = group.Key, value = true };
            var horizontalContainer = CreateHorizontalContainer();
        
            foreach (var field in group)
            {
                var tileContainer = CreateTileContainerForField(field);
                horizontalContainer.Add(tileContainer);
            }
        
            foldout.Add(horizontalContainer);
            return foldout;
        }
        
        private VisualElement CreateHorizontalContainer()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap
                }
            };
        }
        
        private VisualElement CreateTileContainerForField(FieldInfo field)
        {
            var tileContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginRight = 10
                }
            };
        
            var label = CreateLabelForField(field);
            tileContainer.Add(label);
        
            var imguiContainer = CreateIMGUIContainerForField(field, label);
            imguiContainer.style.height = 60;
            tileContainer.Add(imguiContainer);
        
            return tileContainer;
        }
        
        private Label CreateLabelForField(FieldInfo field)
        {
            var labelText = CleanWallLabel(field.Name);
            return new Label(labelText)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 10,
                    marginBottom = 5
                }
            };
        }
        
        private IMGUIContainer CreateIMGUIContainerForField(FieldInfo field, Label label)
        {
            var controlID = field.Name.GetHashCode() & 0x7FFFFFFF;
        
            return new IMGUIContainer(() =>
            {
                var tilemapPainter = _generatorSelection.CurrentGenerator.TilemapPainter;
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
        
        private void UpdateTileOnSelection(FieldInfo field, int controlID)
        {
            if (Event.current == null || Event.current.commandName != "ObjectSelectorUpdated") return;
        
            var pickerControlID = EditorGUIUtility.GetObjectPickerControlID();
            if (pickerControlID != controlID) return;
        
            var newTile = EditorGUIUtility.GetObjectPickerObject() as TileBase;
            if (newTile == null) return;
        
            var tilemapPainter = _generatorSelection.CurrentGenerator.TilemapPainter;
            field.SetValue(tilemapPainter, newTile);
        }
        
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