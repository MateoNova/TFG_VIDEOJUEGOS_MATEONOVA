using System.Collections.Generic;
using UnityEngine;

public interface ISpecialWallCase
{
    SpecialWallPosition WallPosition { get; }
    /// <summary>
    /// Determina si en la posición dada se cumple el caso especial.
    /// </summary>
    bool IsMatch(Vector2Int position, HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> wallPositions);
}