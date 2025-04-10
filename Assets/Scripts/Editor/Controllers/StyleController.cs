using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor.Models;
using UnityEngine.Tilemaps;

namespace Editor.Controllers
{
    public class StyleController
    {
        /// <summary>
        /// Verifica si el TilemapPainter del generador actual es válido.
        /// </summary>
        internal bool HasValidTilemapPainter()
        {
            return GeneratorService.Instance.CurrentGenerator != null &&
                   GeneratorService.Instance.CurrentGenerator.TilemapPainter != null;
        }

        /// <summary>
        /// Verifica si la selección del generador y su TilemapPainter están inicializados.
        /// </summary>
        internal bool IsGeneratorSelectionValid()
        {
            return GeneratorService.Instance.CurrentGenerator != null &&
                   GeneratorService.Instance.CurrentGenerator.TilemapPainter != null;
        }

        /// <summary>
        /// Obtiene los campos (tipo TileBase) decorados con el atributo TAttribute y los agrupa por el nombre del grupo.
        /// </summary>
        internal IEnumerable<IGrouping<string, FieldInfo>> GetGroupedFields<TAttribute>(
            Func<TAttribute, string> groupSelector)
            where TAttribute : Attribute
        {
            return typeof(TilemapPainter)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.FieldType == typeof(TileBase) && f.IsDefined(typeof(TAttribute), false))
                .GroupBy(f => groupSelector(f.GetCustomAttribute<TAttribute>()));
        }
    }
}