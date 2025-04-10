using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor.Models;
using GeneralUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace Editor.Controllers
{
    public class StyleController
    {
        #region Fields

        #endregion

        #region General Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="StyleManager"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>


        /// <summary>
        /// Creates the style section UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the style section UI elements.</returns>


        /// <summary>
        /// Refreshes the UI.
        /// </summary>

        #endregion

        #region Walkable Tiles - Floor Tile Settings

        /// <summary>
        /// Creates the random floor placement toggle UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the random floor placement toggle UI elements.</returns>
        /// <summary>
        /// Creates the walkable options buttons UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the walkable options buttons UI elements.</returns>
        /// <summary>
        /// Creates a button to add a tile to the UI.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <returns>A <see cref="Button"/> to add a tile to the UI.</returns>
        /// <summary>
        /// Creates a button to clear tiles.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <param name="isWalkable">Indicates whether the tiles are walkable.</param>
        /// <returns>A <see cref="Button"/> to clear tiles.</returns>
        /// <summary>
        /// Creates a button to select walkable tiles from a folder.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        /// <param name="isWalkable">Indicates whether the tiles are walkable.</param>
        /// <returns>A <see cref="Button"/> to select walkable tiles from a folder.</returns>
        /// <summary>
        /// Checks if the tilemap painter is valid.
        /// </summary>
        /// <returns>True if the tilemap painter is valid, otherwise false.</returns>
        internal bool HasValidTilemapPainter()
        {
            return GeneratorService.Instance.CurrentGenerator &&
                   GeneratorService.Instance.CurrentGenerator.TilemapPainter;
        }

        #endregion

        #region Walkable Tiles - Tile Group Settings

        /// <summary>
        /// Creates the walkable tile group settings UI.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the walkable tile group settings UI elements.</returns>
        /// <summary>
        /// Checks if the generator selection is valid.
        /// </summary>
        /// <returns>True if the generator selection is valid, otherwise false.</returns>
        internal bool IsGeneratorSelectionValid()
        {
            return GeneratorService.Instance.CurrentGenerator != null &&
                   GeneratorService.Instance.CurrentGenerator.TilemapPainter != null;
        }

        #endregion

        #region Wall Tiles - Wall Tile Settings

        /// <summary>
        /// Gets the grouped fields by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="groupSelector">The group selector function.</param>
        /// <returns>An enumerable of grouped fields.</returns>
        internal IEnumerable<IGrouping<string, FieldInfo>> GetGroupedFields<TAttribute>(
            Func<TAttribute, string> groupSelector) where TAttribute : Attribute
        {
            return typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.FieldType == typeof(TileBase) && f.IsDefined(typeof(TAttribute), false))
                .GroupBy(f => groupSelector(f.GetCustomAttribute<TAttribute>()));
        }

        #endregion
    }
}