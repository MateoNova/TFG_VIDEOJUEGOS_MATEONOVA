using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interfaz que representa una “regla de override” para reclasificar muros.
/// </summary>
public interface IWallOverrideCase
{
    /// <summary>
    /// Dado el wallPosition original (Left, Right, etc.), decide si se cumple la condición de override.
    /// </summary>
    bool IsMatch(
        Vector2Int position,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> allWallPositions,
        WallPosition originalWallPosition
    );

    /// <summary>
    /// A qué WallPosition se debe cambiar si se cumple la condición.
    /// </summary>
    WallPosition OverrideWallPosition { get; }
}