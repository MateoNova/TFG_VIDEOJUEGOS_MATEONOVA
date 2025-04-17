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
            var painterType = painter.GetType();

            painter.walkableTileBases.Clear();
            painter.walkableTilesPriorities.Clear();

            foreach (var tile in preset.tiles)
            {
                var rawName = tile.name;
                var key = rawName.Contains("_")
                    ? rawName.Substring(rawName.IndexOf('_') + 1)
                    : rawName;

                if (Utils.Utils.PredefinedTileNames.Contains(key))
                {
                    var fieldName = char.ToLower(key[0]) + key[1..];
                    var field = painterType.GetField(
                        fieldName,
                        BindingFlags.Instance | BindingFlags.NonPublic
                    );
                    if (field == null)
                        Debug.LogWarning($"Field '{fieldName}' no encontrado en TilemapPainter.");
                    else
                        field.SetValue(painter, tile);
                }
                else if (key.StartsWith("Floor", StringComparison.OrdinalIgnoreCase))
                {
                    painter.walkableTileBases.Add(tile);
                    painter.walkableTilesPriorities.Add(1);
                }
            }

            EditorUtility.SetDirty(painter);
            AssetDatabase.SaveAssets();
        }
    }
}