using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Controllers.Editor
{
    public class SpriteRenamerController
    {
        public bool RenameSprites(string imagePath)
        {
            var importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            if (importer == null || !importer.spriteImportMode.HasFlag(SpriteImportMode.Multiple))
                return false;

            var factory = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null)
                return false;
            provider.InitSpriteEditorDataProvider();

            var spriteRects = provider.GetSpriteRects();
            if (spriteRects == null || spriteRects.Length == 0)
                return false;

            var sortedRects = spriteRects
                .OrderByDescending(s => s.rect.y)
                .ThenBy(s => s.rect.x)
                .ToArray();
            for (var i = 0; i < sortedRects.Length; i++)
            {
                var baseName = i < Utils.Utils.PredefinedTileNames.Count
                    ? Utils.Utils.PredefinedTileNames[i]
                    : $"Floor {i - Utils.Utils.PredefinedTileNames.Count + 1}";
                sortedRects[i].name = $"{i}_{baseName}";
            }

            provider.SetSpriteRects(sortedRects);
            provider.Apply();

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh();
            return true;
        }

        public bool CreatePreset(string imagePath)
        {
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(imagePath);
            var sprites = subAssets.OfType<Sprite>().ToArray();
            if (sprites.Length == 0)
                return false;

            var sortedSprites = sprites
                .OrderByDescending(s => s.rect.y)
                .ThenBy(s => s.rect.x)
                .ToArray();

            var presetPath = EditorUtility.SaveFilePanelInProject(
                "Save Tileset Preset", "NewTilesetPreset", "asset",
                "Select a location to save the preset");
            if (string.IsNullOrEmpty(presetPath))
                return false;

            var presetDirectory = System.IO.Path.GetDirectoryName(presetPath);
            var tilesFolderName = System.IO.Path.GetFileNameWithoutExtension(presetPath).Replace("Preset", "Tiles");
            var tilesFolderPath = $"{presetDirectory}/{tilesFolderName}";
            if (!AssetDatabase.IsValidFolder(tilesFolderPath))
            {
                AssetDatabase.CreateFolder(presetDirectory, tilesFolderName);
            }

            var tileAssets = new List<Tile>();
            for (var i = 0; i < sortedSprites.Length; i++)
            {
                var sprite = sortedSprites[i];
                var tileInstance = ScriptableObject.CreateInstance<Tile>();
                tileInstance.sprite = sprite;

                var baseName = i < Utils.Utils.PredefinedTileNames.Count
                    ? Utils.Utils.PredefinedTileNames[i]
                    : $"Floor {i - Utils.Utils.PredefinedTileNames.Count + 1}";
                tileInstance.name = $"{baseName}";

                var tileAssetPath = $"{tilesFolderPath}/{tileInstance.name}.asset";
                AssetDatabase.CreateAsset(tileInstance, tileAssetPath);
                AssetDatabase.SaveAssets();
                tileAssets.Add(tileInstance);
            }

            var preset = ScriptableObject.CreateInstance<TilesetPreset>();
            preset.tiles = tileAssets.ToArray();

            AssetDatabase.CreateAsset(preset, presetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }
    }
}