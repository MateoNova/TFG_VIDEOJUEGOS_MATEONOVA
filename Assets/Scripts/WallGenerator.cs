using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum WallPosition
{
    Up,
    Down,
    Left,
    Right,
    TopLeft,
    BottomLeft,
    TopRight,
    BottomRight,
    TripleWallCornerExceptUp,
    TripleWallCornerExceptDown,
    TripleWallCornerExceptLeft,
    TripleWallCornerExceptRight,
    AllWallCorner
}

public enum SpecialWallPosition
{
    TripleWallCornerLeft,
    DownWall
}

/// <summary>
/// Class responsible for generating walls based on the positions of walkable tiles.
/// </summary>
public class WallGenerator : MonoBehaviour
{
    /// <summary>
    /// Generates walls based on the positions of walkable tiles.
    /// </summary>
    /// <param name="walkableTilesPositions">A set of positions of walkable tiles.</param>
    /// <param name="tilemapPainter">The TilemapRenderer used to render the wall tiles.</param>
    public static void GenerateWalls(HashSet<Vector2Int> walkableTilesPositions, TilemapPainter tilemapPainter)
    {
        var wallPositions = new Dictionary<WallPosition, HashSet<Vector2Int>>
        {
            { WallPosition.Up, GetUpWallsPositions(walkableTilesPositions) },
            { WallPosition.Down, GetDownWallsPositions(walkableTilesPositions) },
            { WallPosition.Left, GetLeftWallsPositions(walkableTilesPositions) },
            { WallPosition.Right, GetRightWallsPositions(walkableTilesPositions) },
            { WallPosition.TopLeft, GetTopLeftCornerPositions(walkableTilesPositions) },
            { WallPosition.BottomLeft, GetBottomLeftCornerPositions(walkableTilesPositions) },
            { WallPosition.TopRight, GetTopRightCornerPositions(walkableTilesPositions) },
            { WallPosition.BottomRight, GetBottomRightCornerPositions(walkableTilesPositions) },
            { WallPosition.TripleWallCornerExceptUp, new HashSet<Vector2Int>() },
            { WallPosition.TripleWallCornerExceptDown, new HashSet<Vector2Int>() },
            { WallPosition.TripleWallCornerExceptLeft, new HashSet<Vector2Int>() },
            { WallPosition.TripleWallCornerExceptRight, new HashSet<Vector2Int>() },
            { WallPosition.AllWallCorner, new HashSet<Vector2Int>() }
        };

        // 1) aplicar overrides
        ApplyWallOverrides(wallPositions, walkableTilesPositions);

        // 2) Genera los “specialWallPositions” como siempre
        var specialWallPositions =
            GetSpecialWallPositions(walkableTilesPositions, wallPositions.Values.SelectMany(x => x).ToHashSet());

        // 3) Pinta primero los muros “normales”
        foreach (var wallPosition in wallPositions)
        {
            tilemapPainter.PaintWallTiles(wallPosition.Value, wallPosition.Key);
        }

        // 4) Pinta los muros especiales
        foreach (var specialWallPosition in specialWallPositions)
        {
            tilemapPainter.PaintSpecialWallTiles(specialWallPosition.Value, specialWallPosition.Key);
        }
    }


    private static HashSet<Vector2Int> GetWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        var neighborPositions = new HashSet<Vector2Int>();

        foreach (var neighborPos in from position in floorPositions
                 from direction in Utils.Directions
                 select position + direction)
        {
            neighborPositions.Add(neighborPos);
        }

        neighborPositions.ExceptWith(floorPositions);
        return neighborPositions;
    }

    public static HashSet<Vector2Int> GetUpWallsPositions(HashSet<Vector2Int> floorPositions) =>
        GetSpecificWallPositions(floorPositions, Vector2Int.up);

    public static HashSet<Vector2Int> GetDownWallsPositions(HashSet<Vector2Int> floorPositions) =>
        GetSpecificWallPositions(floorPositions, Vector2Int.down);

    public static HashSet<Vector2Int> GetLeftWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        var leftWallPositions = GetSpecificWallPositions(floorPositions, Vector2Int.left);
        // Special case: If a left wall has a wall to its left, it should be a down wall.
        var downWallPositions = new HashSet<Vector2Int>();

        foreach (var position in leftWallPositions)
        {
            var leftPos = position + Vector2Int.left;
            if (leftWallPositions.Contains(leftPos))
            {
                downWallPositions.Add(position);
            }
        }

        leftWallPositions.ExceptWith(downWallPositions);
        return leftWallPositions;
    }

    public static HashSet<Vector2Int> GetRightWallsPositions(HashSet<Vector2Int> floorPositions)
    {
        var rightWallPositions = GetSpecificWallPositions(floorPositions, Vector2Int.right);
        // Special case: If a right wall has a wall to its right, it should be a down wall.
        var downWallPositions = new HashSet<Vector2Int>();

        foreach (var position in rightWallPositions)
        {
            var rightPos = position + Vector2Int.right;
            if (rightWallPositions.Contains(rightPos))
            {
                downWallPositions.Add(position);
            }
        }

        rightWallPositions.ExceptWith(downWallPositions);
        return rightWallPositions;
    }

    private static HashSet<Vector2Int> GetSpecificWallPositions(HashSet<Vector2Int> floorPositions,
        Vector2Int direction)
    {
        var wallPositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            var neighborPos = position + direction;
            if (!floorPositions.Contains(neighborPos))
            {
                wallPositions.Add(neighborPos);
            }
        }

        return wallPositions;
    }

    public static HashSet<Vector2Int> GetTopLeftCornerPositions(HashSet<Vector2Int> floorPositions) =>
        GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.left);

    public static HashSet<Vector2Int> GetBottomLeftCornerPositions(HashSet<Vector2Int> floorPositions) =>
        GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.left);

    public static HashSet<Vector2Int> GetTopRightCornerPositions(HashSet<Vector2Int> floorPositions) =>
        GetCornerPositions(floorPositions, Vector2Int.up, Vector2Int.right);

    public static HashSet<Vector2Int> GetBottomRightCornerPositions(HashSet<Vector2Int> floorPositions) =>
        GetCornerPositions(floorPositions, Vector2Int.down, Vector2Int.right);

    private static HashSet<Vector2Int> GetCornerPositions(HashSet<Vector2Int> floorPositions, Vector2Int direction1,
        Vector2Int direction2)
    {
        var cornerPositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            var neighborPos1 = position + direction1;
            var neighborPos2 = position + direction2;
            var cornerPos = position + direction1 + direction2;

            if (!floorPositions.Contains(neighborPos1) && !floorPositions.Contains(neighborPos2) &&
                !floorPositions.Contains(cornerPos))
            {
                cornerPositions.Add(cornerPos);
            }
        }

        return cornerPositions;
    }

    public static Dictionary<SpecialWallPosition, HashSet<Vector2Int>> GetSpecialWallPositions(
        HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> wallPositions)
    {
        // Lista de casos especiales implementados
        List<ISpecialWallCase> specialCases = new List<ISpecialWallCase>
        {
            new TripleWallCornerLeftCase(),
            new DownWallCase(),
            // Agrega aquí nuevas implementaciones cuando sea necesario
        };

        // Inicializar el diccionario con los tipos de casos especiales
        var specialWallPositions = specialCases
            .Select(sc => sc.WallPosition)
            .Distinct()
            .ToDictionary(key => key, key => new HashSet<Vector2Int>());

        // Recorrer cada posición del piso y aplicar cada caso especial
        foreach (var position in floorPositions)
        {
            foreach (var specialCase in specialCases)
            {
                if (specialCase.IsMatch(position, floorPositions, wallPositions))
                {
                    specialWallPositions[specialCase.WallPosition].Add(position);
                }
            }
        }

        return specialWallPositions;
    }

    private static void ApplyWallOverrides(
        Dictionary<WallPosition, HashSet<Vector2Int>> wallPositionsByType,
        HashSet<Vector2Int> floorPositions
    )
    {
        // Unimos todos los muros en un solo set para comprobar “allWallPositions.Contains(...)”
        var allWallPositions = wallPositionsByType.Values.SelectMany(v => v).ToHashSet();

        // Lista de reglas de override que creamos arriba
        List<IWallOverrideCase> overrides = new List<IWallOverrideCase>
        {
            new LeftWallToTopRightCase(),
            new RightWallToTopLeftCase(),
            new LeftWallToBottomRightCase(),
            new RightWallToBottomLeftCase(),
            new RightWallToDownCase(),
            new LeftWallToDownCase(),
            new TopRightWallToTripleCornerExceptUp(),
            new TopLeftWallToTripleCornerCase(),
            new DownWallToTripleWallCornerExceptUp(),
            new DownWallToTripleWallCornerExceptDown(),
            new BottomRightWallToTripleWallCornerExceptDown(),
            new BottomLeftWallToTripleWallCornerExceptDown(),
            new BottomLeftWallToTripleWallCornerExceptLeft(),
            new BottomRightWallToTripleWallCornerExceptRight(),
            new BottomLeftWallToAllWallCorner()
        };

        // Para no modificar los sets mientras iteramos, guardamos los cambios y luego los aplicamos
        var changes = new List<(Vector2Int pos, WallPosition oldType, WallPosition newType)>();

        // Recorremos cada tipo (Up, Down, Left, Right, etc.)
        foreach (var wallType in wallPositionsByType.Keys.ToList())
        {
            // Recorremos cada posición que fue clasificada con ese tipo
            foreach (var pos in wallPositionsByType[wallType])
            {
                // Probamos cada override
                foreach (var overrideCase in overrides)
                {
                    if (overrideCase.IsMatch(pos, floorPositions, allWallPositions, wallType))
                    {
                        changes.Add((pos, wallType, overrideCase.OverrideWallPosition));
                        break; // si ya matcheó una regla, no revisamos más
                    }
                }
            }
        }

        // Aplicamos los cambios: quitamos la posición del set viejo y la agregamos al set nuevo
        foreach (var (pos, oldType, newType) in changes)
        {
            wallPositionsByType[oldType].Remove(pos);
            wallPositionsByType[newType].Add(pos);
        }
    }
}