using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor.Models;
using Generators.Models;
using UnityEngine.Tilemaps;

namespace Editor.Controllers
{
    public class StyleController
    {
        internal bool HasValidTilemapPainter()
        {
            return GeneratorService.Instance.CurrentGenerator != null &&
                   GeneratorService.Instance.CurrentGenerator.TilemapPainter != null;
        }

        internal bool IsGeneratorSelectionValid()
        {
            return GeneratorService.Instance.CurrentGenerator != null &&
                   GeneratorService.Instance.CurrentGenerator.TilemapPainter != null;
        }

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