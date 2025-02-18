using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
///  Class responsible for generating walls based on the positions of walkable tiles.
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
        var wallPositions = GetWallsPositions(walkableTilesPositions, Utils.Directions);
        
        var cornerPositions = GetWallsPositions(walkableTilesPositions, Utils.DiagonalDirections);
        CreateBasicWalls(tilemapPainter, wallPositions, walkableTilesPositions);
        CreateCornerWalls(tilemapPainter, cornerPositions, walkableTilesPositions);
    }

    private static void CreateCornerWalls(TilemapPainter tilemapPainter, HashSet<Vector2Int> cornerPositions, HashSet<Vector2Int> walkableTilesPositions)
    {
        foreach (var pos in cornerPositions)
        {
            var neighborBinaryType = "";
            foreach (var direction in Utils.eightDirectionsList)
            {
                var neighborPos = pos + direction;
                if (walkableTilesPositions.Contains(neighborPos))
                {
                    neighborBinaryType += "1";
                }
                else
                {
                    neighborBinaryType += "0";
                }
            }
            
            tilemapPainter.PaintSingleCornerWall(pos, neighborBinaryType);
        }
    }

    private static void CreateBasicWalls(TilemapPainter tilemapPainter, HashSet<Vector2Int> wallPositions, HashSet<Vector2Int> walkableTilesPositions)
    {
        foreach (var position in wallPositions)
        {
            var neighborBinaryType = "";
            foreach (var direction in Utils.Directions)
            {
                var neighborPos = position + direction;
                if (walkableTilesPositions.Contains(neighborPos))
                {
                    neighborBinaryType += "1";
                }
                else
                {
                    neighborBinaryType += "0";
                }
            }
            
            tilemapPainter.PaintSingleBasicWall(position, neighborBinaryType);
        }
    }

    /// <summary>
    /// Gets the positions of walls based on the positions of floor tiles.
    /// </summary>
    /// <param name="floorPositions">A set of positions of floor tiles.</param>
    /// <returns>A set of positions of wall tiles.</returns>
    private static HashSet<Vector2Int> GetWallsPositions(HashSet<Vector2Int> floorPositions, Vector2Int[] directions)
    {
        var neighborPositions = new HashSet<Vector2Int>();

        // Iterate through each floor position and its neighboring positions
        foreach (var neighborPos in from position in floorPositions
                 from direction in directions
                 select position + direction)
        {
            neighborPositions.Add(neighborPos);
        }

        // Remove positions that are already occupied by floor tiles
        neighborPositions.ExceptWith(floorPositions);
        return neighborPositions;
    }
}