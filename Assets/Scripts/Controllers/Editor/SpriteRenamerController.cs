using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEditor;
using UnityEngine;

namespace Controllers.Editor
{
    public class SpriteRenamerController
    {
        private readonly List<string> _predefinedNames = new()
        {
            "TopLeft", "TripleWallExceptUp", "TopRight", "InnerTopLeft", "InnerTopRight",
            "Right", "Up", "TripleWallExceptLeft", "AllCorners", "TripleWallExceptRight",
            "TripleInnerWallExceptLeft", "TripleInnerWallExceptRight", "Left", "BottomLeft",
            "TripleWallExceptDown", "BottomRight", "InnerBottomLeft", "InnerBottomRight",
            "AloneWall", "Down"
        };

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
            for (int i = 0; i < sortedRects.Length; i++)
            {
                var baseName = i < _predefinedNames.Count
                    ? _predefinedNames[i]
                    : $"Floor {i - _predefinedNames.Count + 1}";
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

        /// <summary>
        /// Crea un preset a partir de los sprite slices ya renombrados en la imagen.
        /// </summary>
        /// <param name="imagePath">Ruta de la imagen con los sprites renombrados.</param>
        /// <returns>True si se creó exitosamente el preset; false en caso contrario.</returns>
        public bool CreatePreset(string imagePath)
        {
            Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(imagePath);
            Sprite[] sprites = subAssets.OfType<Sprite>().ToArray();
            if (sprites.Length == 0)
                return false;

            var sortedSprites = sprites.OrderByDescending(s => s.rect.y)
                .ThenBy(s => s.rect.x)
                .ToArray();

            TilesetPreset preset = ScriptableObject.CreateInstance<TilesetPreset>();
            preset.sprites = sortedSprites;

            string presetPath = EditorUtility.SaveFilePanelInProject("Save Tileset Preset", "NewTilesetPreset", "asset",
                "Select a location to save the preset");
            if (string.IsNullOrEmpty(presetPath))
                return false;

            AssetDatabase.CreateAsset(preset, presetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }
    }
}