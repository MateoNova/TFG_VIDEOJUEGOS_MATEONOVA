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
            // 1) Configure importer for slicing
            var importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            if (importer == null) return false;
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = CellSize;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            // 2) Prepare Data Provider
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            // 3) Slice and name sprites
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

            // 4) Register name↔ID pairs
            var nameProv = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var pairs = nameProv.GetNameFileIdPairs().ToList();
            pairs.AddRange(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            nameProv.SetNameFileIdPairs(pairs);

            // 5) Apply slicing
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
            catch
            {
                return false;
            }
        }

        public bool CreatePreset(string imagePath)
        {
            var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(imagePath).OfType<Sprite>();
            var sprites = subs.ToArray();
            if (sprites.Length == 0) return false;

            var sorted = sprites
                .OrderByDescending(s => s.rect.y)
                .ThenBy(s => s.rect.x)
                .ToArray();

            var presetPath = EditorUtility.SaveFilePanelInProject(
                "Save Tileset Preset", "NewTilesetPreset", "asset", "Select a location to save the preset");
            if (string.IsNullOrEmpty(presetPath)) return false;

            var dir = Path.GetDirectoryName(presetPath);
            var presetName = Path.GetFileNameWithoutExtension(presetPath);
            var tilesFolder = presetName.Replace("Preset", "Tiles");
            var folderPath = Path.Combine(dir, tilesFolder);
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(dir, tilesFolder);

            var tiles = new List<Tile>();
            foreach (var spr in sorted)
            {
                var t = ScriptableObject.CreateInstance<Tile>();
                t.sprite = spr;
                var p = Path.Combine(folderPath, spr.name + ".asset");
                AssetDatabase.CreateAsset(t, p);
                tiles.Add(t);
            }

            var preset = ScriptableObject.CreateInstance<TilesetPreset>();
            preset.tiles = tiles.ToArray();
            AssetDatabase.CreateAsset(preset, presetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CreateTilePalette(imagePath, folderPath, presetName);
            return true;
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

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            int texW = texture.width, texH = texture.height;
            var root = PrefabUtility.LoadPrefabContents(palettePath);
            var tilemap = root.GetComponentInChildren<Tilemap>();

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

            PrefabUtility.SaveAsPrefabAsset(root, palettePath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// AssetPostprocessor para forzar colliders distintos según el nombre de la tile.
    /// </summary>
    internal class CustomPhysicsShapePostprocessor : AssetPostprocessor
    {
        // Grupos de nombres tal como están en Utils.Utils.PredefinedTileNames
        static readonly HashSet<string> LeftEdgeAt60 = new()
        {
            "TopLeftWall", "TripleExceptLeftWall", "BottomLeftWall",
            "TopLeftInnerWall", "TripleExceptLeftInnerWall", "BottomLeftInnerWall",
            "RightWall"
        };

        static readonly HashSet<string> RightEdgeAt60 = new()
        {
            "TopRightWall", "TripleExceptRightWall", "BottomRightWall",
            "TopRightInnerWall", "TripleExceptRightInnerWall", "BottomRightInnerWall",
            "LeftWall"
        };

        // Subconjunto dentro de LeftEdgeAt60 que en realidad deben empezar al 40%
        static readonly HashSet<string> SpecialLeft40 = new()
        {
            "TripleExceptLeftInnerWall",
            "RightWall",
            "TopLeftInnerWall",
            "TripleExceptLeftWall"
        };

        private const string AloneWallName = "AloneWall";

        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0) return;

            foreach (var sprite in sprites)
            {
                // Extraer el nombre real (sin índice “0_”)
                var baseName = sprite.name.Split('_').Last();

                // Saltar si no es tile de muro
                if (!LeftEdgeAt60.Contains(baseName)
                    && !RightEdgeAt60.Contains(baseName)
                    && baseName != AloneWallName)
                    continue;

                var w = sprite.rect.width;
                var h = sprite.rect.height;

                // Calcular porcentajes de inicio/fin en X
                float leftPct, rightPct;
                if (baseName == AloneWallName)
                {
                    // 60% centrado
                    var span = 0.4f;
                    leftPct = (1f - span) * 0.5f; 
                    rightPct = leftPct + span; 
                }
                else if (RightEdgeAt60.Contains(baseName))
                {
                    leftPct = 0f;
                    rightPct = 0.7f;
                }
                else // es del grupo LeftEdgeAt60
                {
                    leftPct = 0.3f;
                    rightPct = 1.0f;
                }

                // Convertir a coordenadas de píxel dentro de sprite.rect
                float x0 = w * leftPct;
                float x1 = w * rightPct;

                // Crear rectángulo de colisión (un polígono en cuatro vértices)
                var shape = new Vector2[]
                {
                    new(x0, 0f),
                    new(x1, 0f),
                    new(x1, h),
                    new(x0, h),
                };

                // Sobrescribir la forma original
                sprite.OverridePhysicsShape(new List<Vector2[]> { shape });
            }
        }
    }
}
#endif

