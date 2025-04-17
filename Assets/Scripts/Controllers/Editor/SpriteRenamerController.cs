using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Models;
using UnityEditor.U2D.Sprites;

#if UNITY_EDITOR
namespace Controllers.Editor
{
    /// <summary>
    /// Controller that configures the importer, applies grid slicing while ignoring empty cells,
    /// and automatically renames sprites using PredefinedTileNames.
    /// </summary>
    public class SpriteRenamerController
    {
        private const int CellSize = 16; // Size of each cell in pixels for slicing.
        private const float PivotValue = 0.5f; // Default pivot value for sprites.

        /// <summary>
        /// Configures the importer, performs slicing (ignoring empty tiles), and renames sprites in metadata.
        /// </summary>
        /// <param name="imagePath">Path to the image to process.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool RenameSprites(string imagePath)
        {
            var importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            if (importer == null) return false;

            // 1) Initial configuration of the TextureImporter.
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = CellSize;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            // 2) Prepare the Sprite Editor Data Provider.
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            // 3) Generate SpriteRects with unique GUIDs.
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            int texW = texture.width, texH = texture.height;
            var spriteRects = new List<SpriteRect>();
            int idx = 0;

            // Iterate through the texture grid and create SpriteRects for non-empty cells.
            for (int y = texH - CellSize; y >= 0; y -= CellSize)
            for (int x = 0; x < texW; x += CellSize)
            {
                if (IsCellBlank(texture, x, y)) continue; // Skip empty cells.
                var name = idx < Utils.Utils.PredefinedTileNames.Count
                    ? Utils.Utils.PredefinedTileNames[idx]
                    : $"Floor {idx - Utils.Utils.PredefinedTileNames.Count + 1}";
                spriteRects.Add(new SpriteRect
                {
                    name = $"{idx}_{name}",
                    spriteID = GUID.Generate(), // Generate a unique GUID for the sprite.
                    rect = new Rect(x, y, CellSize, CellSize),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(PivotValue, PivotValue),
                });
                idx++;
            }

            // 4) Register name↔ID pairs (Unity 2021.2+).
            var nameFileIdProv = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var pairs = nameFileIdProv.GetNameFileIdPairs().ToList();
            pairs.AddRange(spriteRects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            nameFileIdProv.SetNameFileIdPairs(pairs);

            // 5) Assign the SpriteRects and apply changes.
            dataProvider.SetSpriteRects(spriteRects.ToArray());
            dataProvider.Apply();

            // 6) Reimport the asset to apply changes.
            var ai = dataProvider.targetObject as AssetImporter;
            ai.SaveAndReimport();
            AssetDatabase.Refresh();

            return true;
        }

        /// <summary>
        /// Checks if all pixels in the specified region are transparent using GetPixels.
        /// </summary>
        /// <param name="tex">The texture to check.</param>
        /// <param name="x">The x-coordinate of the region.</param>
        /// <param name="y">The y-coordinate of the region.</param>
        /// <returns>True if the region is blank, false otherwise.</returns>
        private bool IsCellBlank(Texture2D tex, int x, int y)
        {
            try
            {
                var pixels = tex.GetPixels(x, y, CellSize, CellSize);
                return pixels.All(c => c.a == 0f); // Check if all pixels are fully transparent.
            }
            catch
            {
                // If reading fails, assume the cell is not blank.
                return false;
            }
        }

        /// <summary>
        /// Creates a tileset preset after slicing and renaming.
        /// </summary>
        /// <param name="imagePath">Path to the image to process.</param>
        /// <returns>True if the preset was created successfully, false otherwise.</returns>
        public bool CreatePreset(string imagePath)
        {
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(imagePath);
            var sprites = subAssets.OfType<Sprite>().ToArray();
            if (sprites.Length == 0) return false;

            // Sort sprites by their position in the texture.
            var sorted = sprites
                .OrderByDescending(s => s.rect.y)
                .ThenBy(s => s.rect.x)
                .ToArray();

            // Prompt the user to select a save location for the preset.
            var presetPath = EditorUtility.SaveFilePanelInProject(
                "Save Tileset Preset", "NewTilesetPreset", "asset",
                "Select a location to save the preset"
            );
            if (string.IsNullOrEmpty(presetPath)) return false;

            // Create a folder for the tiles if it doesn't exist.
            var dir = System.IO.Path.GetDirectoryName(presetPath);
            var tilesFolder = System.IO.Path.GetFileNameWithoutExtension(presetPath).Replace("Preset", "Tiles");
            var folderPath = $"{dir}/{tilesFolder}";
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(dir, tilesFolder);

            // Create Tile assets for each sprite.
            var tiles = new List<Tile>();
            for (int i = 0; i < sorted.Length; i++)
            {
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sorted[i];
                tile.name = sorted[i].name;
                var assetPath = $"{folderPath}/{sorted[i].name}.asset";
                AssetDatabase.CreateAsset(tile, assetPath);
                tiles.Add(tile);
            }

            // Create and save the tileset preset.
            var preset = ScriptableObject.CreateInstance<TilesetPreset>();
            preset.tiles = tiles.ToArray();
            AssetDatabase.CreateAsset(preset, presetPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }
    }
}
#endif