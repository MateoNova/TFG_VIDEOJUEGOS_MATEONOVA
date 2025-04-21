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
        /// Retrieves and groups fields of the TilemapPainter class based on a specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to filter and group fields by.</typeparam>
        /// <param name="groupSelector">A function to determine the grouping key from the attribute.</param>
        /// <returns>A collection of grouped fields, where each group is identified by a string key.</returns>
        internal IEnumerable<IGrouping<string, FieldInfo>> GetGroupedFields<TAttribute>(
            Func<TAttribute, string> groupSelector)
            where TAttribute : Attribute
        {
            return typeof(TilesetPreset)
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
            if (preset == null) { Debug.LogError("Preset is null."); return; }
            var gen = GeneratorService.Instance.CurrentGenerator;
            if (gen?.TilemapPainter == null) { Debug.LogError("TilemapPainter not available."); return; }

            var painter = gen.TilemapPainter;

            // 1) Registra y selecciona el preset
            painter.AddAndSelectPreset(preset); 

            // 2) Limpia las colecciones de walkables (ahora protegido)
            //painter.ClearWalkableTileBases();
            //painter.ClearWalkableTilesPriorities();

            // 3) Asigna cada tile desde preset.tiles
            foreach (var tile in preset.tiles)
            {
                var key = tile.name.Contains("_") 
                    ? tile.name.Substring(tile.name.IndexOf('_') + 1) 
                    : tile.name;

                if (Utils.Utils.PredefinedTileNames.Contains(key))
                {
                    var fieldName = char.ToLower(key[0]) + key[1..];
                    var field = painter.GetType().GetField(
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
                    painter.AddTileWalkableTileBases(tile);
                }
            }

            EditorUtility.SetDirty(painter);
            AssetDatabase.SaveAssets();
        }

    }
}