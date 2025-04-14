using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using GeneratorService = Models.Editor.GeneratorService;

namespace Controllers.Editor
{
    /// <summary>
    /// Controller responsible for managing style-related operations in the Unity Editor.
    /// </summary>
    public class StyleController
    {
        /// <summary>
        /// Checks if the current generator has a valid TilemapPainter.
        /// </summary>
        /// <returns>True if a valid TilemapPainter is present; otherwise, false.</returns>
        internal bool HasValidTilemapPainter()
        {
            return GeneratorService.Instance.CurrentGenerator != null &&
                   GeneratorService.Instance.CurrentGenerator.TilemapPainter != null;
        }

        /// <summary>
        /// Validates the current generator selection.
        /// </summary>
        /// <returns>True if the generator selection is valid; otherwise, false.</returns>
        internal bool IsGeneratorSelectionValid()
        {
            return GeneratorService.Instance.CurrentGenerator != null &&
                   GeneratorService.Instance.CurrentGenerator.TilemapPainter != null;
        }

        /// <summary>
        /// Retrieves and groups fields of the TilemapPainter class based on a specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to filter and group fields by.</typeparam>
        /// <param name="groupSelector">A function to determine the grouping key from the attribute.</param>
        /// <returns>A collection of grouped fields, where each group is identified by a string key.</returns>
        internal IEnumerable<IGrouping<string, FieldInfo>> GetGroupedFields<TAttribute>(
            Func<TAttribute, string> groupSelector)
            where TAttribute : Attribute
        {
            return typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) // Retrieve all fields.
                .Where(f => f.FieldType == typeof(TileBase) &&
                            f.IsDefined(typeof(TAttribute), false)) // Filter by type and attribute.
                .GroupBy(f => groupSelector(f.GetCustomAttribute<TAttribute>())); // Group by the provided selector.
        }

        /// <summary>
        /// Loads a TilesetPreset and assigns its sprites to the TilemapPainter wall fields.
        /// </summary>
        /// <param name="preset">The preset asset with the renamed sprites.</param>
        public void LoadPreset(TilesetPreset preset)
        {
            if (preset == null)
            {
                Debug.LogError("Preset is null.");
                return;
            }

            var generator = GeneratorService.Instance.CurrentGenerator;
            if (generator == null || generator.TilemapPainter == null)
            {
                Debug.LogError("Generator or its TilemapPainter is not available.");
                return;
            }

            var painter = generator.TilemapPainter;

            var wallMapping = new Dictionary<string, string>()
            {
                { "upWall", "Up" },
                { "downWall", "Down" },
                { "leftWall", "Left" },
                { "rightWall", "Right" },
                { "topLeftWall", "TopLeft" },
                { "topRightWall", "TopRight" },
                { "bottomLeftWall", "BottomLeft" },
                { "bottomRightWall", "BottomRight" },
                { "allCornersWall", "AllCorners" },
                { "topLeftInnerWall", "InnerTopLeft" },
                { "topRightInnerWall", "InnerTopRight" },
                { "bottomLeftInnerWall", "InnerBottomLeft" },
                { "bottomRightInnerWall", "InnerBottomRight" },
                { "tripleExceptUpWall", "TripleWallExceptUp" },
                { "tripleExcetDownWall", "TripleWallExceptDown" },
                { "tripleExceptLeftWall", "TripleWallExceptLeft" },
                { "tripleExceptRightWall", "TripleWallExceptRight" },
                { "tripleExceptLeftInnerWall", "TripleInnerWallExceptLeft" },
                { "tripleExceptRightInnerWall", "TripleInnerWallExceptRight" },
                { "aloneWall", "AloneWall" }
            };

            var painterType = painter.GetType();
            foreach (var (fieldName, expectedBaseName) in wallMapping)
            {
                var field = painterType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null)
                    continue;

                Tile foundTile = null;
                foreach (var tile in preset.tiles)
                {
                    var parts = tile.name.Split('_');
                    if (parts.Length >= 2)
                    {
                        var baseName = parts[1];
                        if (!baseName.Equals(expectedBaseName, StringComparison.OrdinalIgnoreCase)) continue;
                        foundTile = tile;
                        break;
                    }

                    if (!tile.name.Equals(expectedBaseName, StringComparison.OrdinalIgnoreCase)) continue;
                    foundTile = tile;
                    break;
                }

                if (foundTile != null)
                {
                    field.SetValue(painter, foundTile);
                }
            }

            EditorUtility.SetDirty(painter);
            AssetDatabase.SaveAssets();
        }
    }
}