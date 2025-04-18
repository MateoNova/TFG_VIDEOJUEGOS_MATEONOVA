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
            var importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            if (importer == null) return false;

            // Configure importer for slicing
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = CellSize;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            // Prepare Data Provider
            var factory = new SpriteDataProviderFactories(); factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            // Slice and name sprites
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            int texW = texture.width, texH = texture.height;
            var rects = new List<SpriteRect>();
            int idx = 0;
            for (int y = texH - CellSize; y >= 0; y -= CellSize)
            for (int x = 0; x < texW; x += CellSize)
            {
                if (IsCellBlank(texture, x, y)) continue;
                string baseName = idx < Utils.Utils.PredefinedTileNames.Count
                    ? Utils.Utils.PredefinedTileNames[idx]
                    : $"Floor {idx - Utils.Utils.PredefinedTileNames.Count + 1}";
                rects.Add(new SpriteRect {
                    name     = $"{idx}_{baseName}",
                    spriteID = GUID.Generate(),
                    rect     = new Rect(x, y, CellSize, CellSize),
                    alignment= (int)SpriteAlignment.Center,
                    pivot    = new Vector2(PivotValue, PivotValue)
                });
                idx++;
            }

            // Register name↔ID pairs
            var nameProv = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var pairs = nameProv.GetNameFileIdPairs().ToList();
            pairs.AddRange(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            nameProv.SetNameFileIdPairs(pairs);

            // Apply slicing
            dataProvider.SetSpriteRects(rects.ToArray());
            dataProvider.Apply();
            (dataProvider.targetObject as AssetImporter).SaveAndReimport();
            AssetDatabase.Refresh();

            return true;
        }

        private bool IsCellBlank(Texture2D tex, int x, int y)
        {
            try
            {
                var px = tex.GetPixels(x, y, CellSize, CellSize);
                return px.All(c => c.a == 0f);
            }
            catch { return false; }
        }

        public bool CreatePreset(string imagePath)
        {
            var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(imagePath).OfType<Sprite>();
            var sprites = subs.ToArray();
            if (sprites.Length == 0) return false;

            // Sort by sheet position
            var sorted = sprites
                .OrderByDescending(s => s.rect.y)
                .ThenBy(s => s.rect.x)
                .ToArray();

            // Prompt for preset asset
            var presetPath = EditorUtility.SaveFilePanelInProject(
                "Save Tileset Preset", "NewTilesetPreset", "asset", "Location"
            );
            if (string.IsNullOrEmpty(presetPath)) return false;

            var dir = Path.GetDirectoryName(presetPath);
            var presetName = Path.GetFileNameWithoutExtension(presetPath);
            var tilesFolder = presetName.Replace("Preset", "Tiles");
            var folderPath = Path.Combine(dir, tilesFolder);
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(dir, tilesFolder);

            // Create Tile assets in original order
            var tiles = new List<Tile>();
            foreach (var spr in sorted)
            {
                var t = ScriptableObject.CreateInstance<Tile>();
                t.sprite = spr;
                var p = Path.Combine(folderPath, spr.name + ".asset");
                AssetDatabase.CreateAsset(t, p);
                tiles.Add(t);
            }

            // Save TilesetPreset
            var preset = ScriptableObject.CreateInstance<TilesetPreset>();
            preset.tiles = tiles.ToArray();
            AssetDatabase.CreateAsset(preset, presetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Create palette preserving sheet layout
            CreateTilePalette(imagePath, folderPath, presetName);
            return true;
        }

        private void CreateTilePalette(string imagePath, string tilesFolderPath, string presetName)
        {
            // palette name
            var paletteName = presetName.Replace("Preset", "TilePalette");
            // create palette prefab
            GridPaletteUtility.CreateNewPalette(
                tilesFolderPath, paletteName,
                GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Automatic,
                Vector3.one, GridLayout.CellSwizzle.XYZ
            );
            var palettePath = Path.Combine(tilesFolderPath, paletteName + ".prefab");

            // load sprite sheet to compute grid
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            int texW = texture.width, texH = texture.height;

            // open prefab for editing
            var root = PrefabUtility.LoadPrefabContents(palettePath);
            var tilemap = root.GetComponentInChildren<Tilemap>();

            // load tiles (order matters by sprite rect)
            var tiles = AssetDatabase
                .FindAssets("t:Tile", new[] { tilesFolderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => AssetDatabase.LoadAssetAtPath<Tile>(p))
                .Where(t => t != null)
                .ToArray();

            // place each tile at its original sheet grid coordinate
            foreach (var tile in tiles)
            {
                var rect = tile.sprite.rect;
                int xIdx = (int)(rect.x / CellSize);
                int yIdx = (int)((texH - CellSize - rect.y) / CellSize);
                tilemap.SetTile(new Vector3Int(xIdx, -yIdx, 0), tile);
            }

            // save and unload
            PrefabUtility.SaveAsPrefabAsset(root, palettePath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
