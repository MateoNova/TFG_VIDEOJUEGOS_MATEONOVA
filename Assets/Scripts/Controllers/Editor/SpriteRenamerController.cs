using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;

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

            var factory = new SpriteDataProviderFactories();
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
    }
}