using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using Models;

#if UNITY_EDITOR
namespace Controllers.Editor
{
    /// <summary>
    /// Controller for processing sprites in the Unity Editor.
    /// Provides functionality for renaming sprites, creating presets, and generating tile palettes.
    /// </summary>
    public class SpriteProcessController
    {
        /// <summary>
        /// The size of each cell in pixels.
        /// </summary>
        private const int CellSize = 16;

        /// <summary>
        /// The pivot value for sprites, set to the center.
        /// </summary>
        private const float PivotValue = 0.5f;

        /// <summary>
        /// Renames sprites in the specified image by slicing it into cells and assigning names.
        /// </summary>
        /// <param name="imagePath">The path to the image to process.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public bool RenameSprites(string imagePath)
        {
            var importer = ConfigureTextureImporter(imagePath);
            if (importer == null) return false;

            var dataProvider = InitializeDataProvider(importer);
            if (dataProvider == null) return false;

            var rects = SliceAndNameSprites(imagePath);
            if (rects == null) return false;

            RegisterNameIdPairs(dataProvider, rects);
            ApplySlicing(dataProvider, rects);

            return true;
        }

        /// <summary>
        /// Configures the texture importer for slicing.
        /// </summary>
        /// <param name="imagePath">The path to the image to configure.</param>
        /// <returns>The configured <see cref="TextureImporter"/> or null if the importer is not found.</returns>
        private TextureImporter ConfigureTextureImporter(string imagePath)
        {
            var importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            if (importer == null) return null;

            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = CellSize;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            return importer;
        }

        /// <summary>
        /// Initializes the sprite editor data provider for the given texture importer.
        /// </summary>
        /// <param name="importer">The texture importer to initialize.</param>
        /// <returns>The initialized <see cref="ISpriteEditorDataProvider"/> or null if initialization fails.</returns>
        private ISpriteEditorDataProvider InitializeDataProvider(TextureImporter importer)
        {
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider?.InitSpriteEditorDataProvider();
            return dataProvider;
        }

        /// <summary>
        /// Slices the image into cells and assigns names to the resulting sprites.
        /// </summary>
        /// <param name="imagePath">The path to the image to slice.</param>
        /// <returns>A list of <see cref="SpriteRect"/> objects representing the sliced sprites.</returns>
        private List<SpriteRect> SliceAndNameSprites(string imagePath)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            if (texture == null) return null;

            var rects = new List<SpriteRect>();
            int texW = texture.width, texH = texture.height, idx = 0;

            for (var y = texH - CellSize; y >= 0; y -= CellSize)
            for (var x = 0; x < texW; x += CellSize)
            {
                if (IsCellBlank(texture, x, y)) continue;

                var baseName = idx < Utils.Utils.PredefinedTileNames.Count
                    ? Utils.Utils.PredefinedTileNames[idx]
                    : $"Floor {idx - Utils.Utils.PredefinedTileNames.Count + 1}";

                rects.Add(new SpriteRect
                {
                    name = $"{idx}_{baseName}",
                    spriteID = GUID.Generate(),
                    rect = new Rect(x, y, CellSize, CellSize),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(PivotValue, PivotValue)
                });
                idx++;
            }

            return rects;
        }

        /// <summary>
        /// Registers name and ID pairs for the sliced sprites.
        /// </summary>
        /// <param name="dataProvider">The sprite editor data provider.</param>
        /// <param name="rects">The list of <see cref="SpriteRect"/> objects to register.</param>
        private void RegisterNameIdPairs(ISpriteEditorDataProvider dataProvider, List<SpriteRect> rects)
        {
            var nameProv = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var pairs = nameProv.GetNameFileIdPairs().ToList();
            pairs.AddRange(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            nameProv.SetNameFileIdPairs(pairs);
        }

        /// <summary>
        /// Applies the slicing operation to the sprite editor data provider.
        /// </summary>
        /// <param name="dataProvider">The sprite editor data provider.</param>
        /// <param name="rects">The list of <see cref="SpriteRect"/> objects to apply.</param>
        private void ApplySlicing(ISpriteEditorDataProvider dataProvider, List<SpriteRect> rects)
        {
            dataProvider.SetSpriteRects(rects.ToArray());
            dataProvider.Apply();
            (dataProvider.targetObject as AssetImporter)?.SaveAndReimport();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Checks if a cell in the texture is blank (fully transparent).
        /// </summary>
        /// <param name="tex">The texture to check.</param>
        /// <param name="x">The x-coordinate of the cell.</param>
        /// <param name="y">The y-coordinate of the cell.</param>
        /// <returns>True if the cell is blank, false otherwise.</returns>
        private bool IsCellBlank(Texture2D tex, int x, int y)
        {
            try
            {
                var px = tex.GetPixels(x, y, CellSize, CellSize);
                return px.All(c => c.a == 0f);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a tileset preset from the sprites in the specified image.
        /// </summary>
        /// <param name="imagePath">The path to the image containing the sprites.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
        public bool CreatePreset(string imagePath)
        {
            var sprites = LoadSprites(imagePath);
            if (sprites.Length == 0) return false;

            var presetPath = GetPresetPath();
            if (string.IsNullOrEmpty(presetPath)) return false;

            var folderPath = PrepareTilesFolder(presetPath);
            var preset = CreateTilesetPreset();

            AssignSpritesToPreset(sprites, preset, folderPath);
            SavePreset(preset, presetPath);

            CreateTilePalette(imagePath, folderPath, Path.GetFileNameWithoutExtension(presetPath));
            return true;
        }

        /// <summary>
        /// Loads all sprites from the specified image.
        /// </summary>
        /// <param name="imagePath">The path to the image to load sprites from.</param>
        /// <returns>An array of <see cref="Sprite"/> objects.</returns>
        private Sprite[] LoadSprites(string imagePath)
        {
            return AssetDatabase.LoadAllAssetRepresentationsAtPath(imagePath)
                .OfType<Sprite>()
                .OrderByDescending(s => s.rect.y)
                .ThenBy(s => s.rect.x)
                .ToArray();
        }

        /// <summary>
        /// Prompts the user to select a path to save the tileset preset.
        /// </summary>
        /// <returns>The selected path, or null if the user cancels the operation.</returns>
        private string GetPresetPath()
        {
            return EditorUtility.SaveFilePanelInProject(
                "Save Tileset Preset", "NewTilesetPreset", "asset",
                "Select a location to save the preset"
            );
        }

        /// <summary>
        /// Prepares a folder for storing tile assets.
        /// </summary>
        /// <param name="presetPath">The path to the tileset preset.</param>
        /// <returns>The path to the prepared folder.</returns>
        private string PrepareTilesFolder(string presetPath)
        {
            var dir = Path.GetDirectoryName(presetPath);
            var presetName = Path.GetFileNameWithoutExtension(presetPath);
            var tilesFolder = presetName.Replace("Preset", "Tiles");
            if (dir != null)
            {
                var folderPath = Path.Combine(dir, tilesFolder);

                if (!AssetDatabase.IsValidFolder(folderPath))
                    AssetDatabase.CreateFolder(dir, tilesFolder);

                return folderPath;
            }
            Debug.LogError($"[CreatePreset] Invalid directory for preset path: {presetPath}");
            return null;
        }

        /// <summary>
        /// Creates a new tileset preset.
        /// </summary>
        /// <returns>A new <see cref="TilesetPreset"/> instance.</returns>
        private TilesetPreset CreateTilesetPreset()
        {
            var preset = ScriptableObject.CreateInstance<TilesetPreset>();
            preset.walkableTileBases = new List<TileBase>();
            preset.walkableTilesPriorities = new List<int>();
            preset.randomWalkableTilesPlacement = false;
            return preset;
        }

        /// <summary>
        /// Assigns sprites to the tileset preset and creates tile assets.
        /// </summary>
        /// <param name="sprites">The sprites to assign.</param>
        /// <param name="preset">The tileset preset to assign to.</param>
        /// <param name="folderPath">The folder to store tile assets in.</param>
        private void AssignSpritesToPreset(Sprite[] sprites, TilesetPreset preset, string folderPath)
        {
            foreach (var spr in sprites)
            {
                var tile = CreateTileAsset(spr, folderPath);
                AssignTileToPreset(spr, tile, preset);
            }
        }

        /// <summary>
        /// Creates a tile asset for the given sprite.
        /// </summary>
        /// <param name="sprite">The sprite to create a tile for.</param>
        /// <param name="folderPath">The folder to store the tile asset in.</param>
        /// <returns>The created <see cref="Tile"/> asset.</returns>
        private Tile CreateTileAsset(Sprite sprite, string folderPath)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            var tileAssetPath = Path.Combine(folderPath, sprite.name + ".asset");
            AssetDatabase.CreateAsset(tile, tileAssetPath);
            return tile;
        }

        /// <summary>
        /// Assigns a tile to the appropriate field in the tileset preset based on its name.
        /// </summary>
        /// <param name="sprite">The sprite associated with the tile.</param>
        /// <param name="tile">The tile to assign.</param>
        /// <param name="preset">The tileset preset to assign to.</param>
        private void AssignTileToPreset(Sprite sprite, Tile tile, TilesetPreset preset)
        {
            var key = ExtractKeyFromSpriteName(sprite.name);

            if (key.Equals("DoorClosed", StringComparison.OrdinalIgnoreCase))
            {
                preset.doorClosed = tile;
                return;
            }

            if (key.Equals("DoorOpen", StringComparison.OrdinalIgnoreCase))
            {
                preset.doorOpen = tile;
                return;
            }

            if (key.StartsWith("Floor", StringComparison.OrdinalIgnoreCase))
            {
                preset.walkableTileBases.Add(tile);
                preset.walkableTilesPriorities.Add(1);
            }
            else if (key.EndsWith("Wall", StringComparison.OrdinalIgnoreCase))
            {
                AssignWallTileToPreset(key, tile, preset);
            }
            else
            {
                Debug.LogWarning($"[CreatePreset] Sprite '{sprite.name}' does not match any known pattern.");
            }
        }

        /// <summary>
        /// Extracts the key from a sprite name.
        /// </summary>
        /// <param name="spriteName">The name of the sprite.</param>
        /// <returns>The extracted key.</returns>
        private string ExtractKeyFromSpriteName(string spriteName)
        {
            var parts = spriteName.Split('_');
            return parts.Length > 1 ? parts[1] : parts[0];
        }

        /// <summary>
        /// Assigns a wall tile to the appropriate field in the tileset preset.
        /// </summary>
        /// <param name="key">The key identifying the wall type.</param>
        /// <param name="tile">The tile to assign.</param>
        /// <param name="preset">The tileset preset to assign to.</param>
        private void AssignWallTileToPreset(string key, Tile tile, TilesetPreset preset)
        {
            var field = typeof(TilesetPreset)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(f =>
                    f.FieldType == typeof(TileBase) &&
                    f.Name.Equals(
                        char.ToLowerInvariant(key[0]) + key[1..],
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (field != null)
            {
                field.SetValue(preset, tile);
            }
            else
            {
                Debug.LogWarning($"[CreatePreset] No field found for wall '{key}' in {preset.name}");
            }
        }

        /// <summary>
        /// Saves the tileset preset to the specified path.
        /// </summary>
        /// <param name="preset">The tileset preset to save.</param>
        /// <param name="presetPath">The path to save the preset to.</param>
        private void SavePreset(TilesetPreset preset, string presetPath)
        {
            AssetDatabase.CreateAsset(preset, presetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Creates a tile palette for the tileset.
        /// </summary>
        /// <param name="imagePath">The path to the image containing the tiles.</param>
        /// <param name="tilesFolderPath">The folder containing the tile assets.</param>
        /// <param name="presetName">The name of the tileset preset.</param>
        private void CreateTilePalette(string imagePath, string tilesFolderPath, string presetName)
        {
            var paletteName = presetName.Replace("Preset", "TilePalette");
            GridPaletteUtility.CreateNewPalette(
                tilesFolderPath, paletteName,
                GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Automatic,
                Vector3.one, GridLayout.CellSwizzle.XYZ
            );

            var palettePath = Path.Combine(tilesFolderPath, paletteName + ".prefab");
            var root = PrefabUtility.LoadPrefabContents(palettePath);
            var tilemap = root.GetComponentInChildren<Tilemap>();

            AssignTilesToPalette(imagePath, tilesFolderPath, tilemap);

            PrefabUtility.SaveAsPrefabAsset(root, palettePath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Assigns tiles to the tile palette.
        /// </summary>
        /// <param name="imagePath">The path to the image containing the tiles.</param>
        /// <param name="tilesFolderPath">The folder containing the tile assets.</param>
        /// <param name="tilemap">The tilemap to assign tiles to.</param>
        private void AssignTilesToPalette(string imagePath, string tilesFolderPath, Tilemap tilemap)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            var texH = texture.height;

            var tiles = AssetDatabase
                .FindAssets("t:Tile", new[] { tilesFolderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => AssetDatabase.LoadAssetAtPath<Tile>(p))
                .Where(t => t != null)
                .ToArray();

            foreach (var tile in tiles)
            {
                var rect = tile.sprite.rect;
                var xIdx = (int)(rect.x / CellSize);
                var yIdx = (int)((texH - CellSize - rect.y) / CellSize);
                tilemap.SetTile(new Vector3Int(xIdx, -yIdx, 0), tile);
            }
        }
    }
}
#endif