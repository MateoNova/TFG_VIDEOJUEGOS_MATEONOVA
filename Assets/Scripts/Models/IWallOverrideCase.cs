using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    /// <summary>
    /// Interface representing an override rule for reclassifying walls.
    /// </summary>
    public interface IWallOverrideCase
    {
        /// <summary>
        /// Determines if the override condition is met given the original wall position.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="floorPositions">The set of floor positions.</param>
        /// <param name="allWallPositions">The set of all wall positions.</param>
        /// <param name="originalWallPosition">The original wall position (Left, Right, etc.).</param>
        /// <returns>True if the override condition is met, otherwise false.</returns>
        bool IsMatch(
            Vector2Int position,
            HashSet<Vector2Int> floorPositions,
            HashSet<Vector2Int> allWallPositions,
            Utils.Utils.WallPosition originalWallPosition
        );

        /// <summary>
        /// Gets the new wall position to assign if the override condition is met.
        /// </summary>
        Utils.Utils.WallPosition OverrideWallPosition { get; }
    }
}