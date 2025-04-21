using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using Models;

#if UNITY_EDITOR
namespace Controllers.Editor
{
    public class SpriteRenamerController
    {
        private const int CellSize = 16;
        private const float PivotValue = 0.5f;

        public bool RenameSprites(string imagePath)
        {
            var importer = ConfigureTextureImporter(imagePath);
            if (importer == null) return false;

            var dataProvider = InitializeDataProvider(importer);
            if (dataProvider == null) return false;

            var rects = SliceAndNameSprites(imagePath);
            if (rects == null) return false;

            RegisterSpriteNames(dataProvider, rects);
            ApplySlicing(dataProvider);

            return true;
        }

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

        private ISpriteEditorDataProvider InitializeDataProvider(TextureImporter importer)
        {
            var factory = new SpriteDataProviderFactories();
            factory.Init();

            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider?.InitSpriteEditorDataProvider();

            return dataProvider;
        }

        private List<SpriteRect> SliceAndNameSprites(string imagePath)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            if (texture == null) return null;

            var rects = new List<SpriteRect>();
            int texW = texture.width, texH = texture.height, idx = 0;

            for (int y = texH - CellSize; y >= 0; y -= CellSize)
            {
                for (int x = 0; x < texW; x += CellSize)
                {
                    if (IsCellBlank(texture, x, y)) continue;

                    string baseName = idx < Utils.Utils.PredefinedTileNames.Count
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
            }

            return rects;
        }

        private void RegisterSpriteNames(ISpriteEditorDataProvider dataProvider, List<SpriteRect> rects)
        {
            var nameProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var pairs = nameProvider.GetNameFileIdPairs().ToList();

            pairs.AddRange(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            nameProvider.SetNameFileIdPairs(pairs);
        }

        private void ApplySlicing(ISpriteEditorDataProvider dataProvider)
        {
            dataProvider.Apply();
            (dataProvider.targetObject as AssetImporter)?.SaveAndReimport();
            AssetDatabase.Refresh();
        }

        private bool IsCellBlank(Texture2D texture, int x, int y)
        {
            try
            {
                var pixels = texture.GetPixels(x, y, CellSize, CellSize);
                return pixels.All(c => c.a == 0f);
            }
            catch
            {
                return false;
            }
        }

        public bool CreatePreset(string imagePath)
        {
            var sprites = LoadAndSortSprites(imagePath);
            if (sprites == null || sprites.Length == 0) return false;

            var presetPath = GetPresetSavePath();
            if (string.IsNullOrEmpty(presetPath)) return false;

            var folderPath = CreateTilesFolder(presetPath);
            var tiles = CreateTiles(sprites, folderPath);

            CreateTilesetPreset(presetPath, tiles);
            CreateTilePalette(imagePath, folderPath, Path.GetFileNameWithoutExtension(presetPath));

            return true;
        }

        private Sprite[] LoadAndSortSprites(string imagePath)
        {
            var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(imagePath).OfType<Sprite>();
            return subs
                .OrderByDescending(s => s.rect.y)
                .ThenBy(s => s.rect.x)
                .ToArray();
        }

        private string GetPresetSavePath()
        {
            return EditorUtility.SaveFilePanelInProject(
                "Save Tileset Preset", "NewTilesetPreset", "asset", "Select a location to save the preset");
        }

        private string CreateTilesFolder(string presetPath)
        {
            var dir = Path.GetDirectoryName(presetPath);
            var presetName = Path.GetFileNameWithoutExtension(presetPath);
            var tilesFolder = presetName.Replace("Preset", "Tiles");
            var folderPath = Path.Combine(dir, tilesFolder);

            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(dir, tilesFolder);

            return folderPath;
        }

        private List<Tile> CreateTiles(Sprite[] sprites, string folderPath)
        {
            var tiles = new List<Tile>();

            foreach (var sprite in sprites)
            {
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;

                var path = Path.Combine(folderPath, sprite.name + ".asset");
                AssetDatabase.CreateAsset(tile, path);
                tiles.Add(tile);
            }

            return tiles;
        }

        private void CreateTilesetPreset(string presetPath, List<Tile> tiles)
        {
            var preset = ScriptableObject.CreateInstance<TilesetPreset>();
            preset.tiles = tiles.ToArray();

            AssetDatabase.CreateAsset(preset, presetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

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

            var tiles = LoadTilesFromFolder(tilesFolderPath);
            AssignTilesToTilemap(tiles, tilemap, imagePath);

            PrefabUtility.SaveAsPrefabAsset(root, palettePath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private Tile[] LoadTilesFromFolder(string folderPath)
        {
            return AssetDatabase
                .FindAssets("t:Tile", new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => AssetDatabase.LoadAssetAtPath<Tile>(p))
                .Where(t => t != null)
                .ToArray();
        }

        private void AssignTilesToTilemap(Tile[] tiles, Tilemap tilemap, string imagePath)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            int texH = texture.height;

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