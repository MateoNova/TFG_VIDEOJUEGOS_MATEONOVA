using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public static class Pathfinding
    {
        // Helper class for A*
        private class Node
        {
            public Vector2Int Position;
            public float G; // cost from start
            public float F; // g + h
            public Node Parent;
        }

        /// <summary>
        /// Implements A* to find a path from start to end, avoiding cells with tiles (walls, doors, etc.).
        /// Start and end cells are allowed to have doors.
        /// </summary>
        /// <param name="start">The starting position.</param>
        /// <param name="end">The ending position.</param>
        /// <param name="painter">The TilemapPainter used to check walkability.</param>
        /// <returns>A list of positions representing the path.</returns>
        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, TilemapPainter painter)
        {
            var openList = new List<Node>();
            var closedSet = new HashSet<Vector2Int>();

            var startNode = new Node { Position = start, G = 0, F = Heuristic(start, end), Parent = null };
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                // Select the node with the lowest f
                openList.Sort((a, b) => a.F.CompareTo(b.F));
                var current = openList[0];

                if (current.Position == end)
                {
                    return ReconstructPath(current);
                }

                openList.Remove(current);
                closedSet.Add(current.Position);

                // Neighbors in 4 directions
                foreach (var dir in new Vector2Int[]
                         {
                             new(0, 1),
                             new(0, -1),
                             new(1, 0),
                             new(-1, 0)
                         })
                {
                    var neighborPos = current.Position + dir;
                    if (closedSet.Contains(neighborPos))
                        continue;

                    if (!IsWalkable(neighborPos, painter, start, end))
                        continue;

                    var tentativeG = current.G + 1;
                    var neighbor = openList.Find(n => n.Position == neighborPos);
                    if (neighbor == null)
                    {
                        neighbor = new Node
                        {
                            Position = neighborPos,
                            G = tentativeG,
                            F = tentativeG + Heuristic(neighborPos, end),
                            Parent = current
                        };
                        openList.Add(neighbor);
                    }
                    else if (tentativeG < neighbor.G)
                    {
                        neighbor.G = tentativeG;
                        neighbor.F = tentativeG + Heuristic(neighborPos, end);
                        neighbor.Parent = current;
                    }
                }
            }

            // If no path is found, return an empty list.
            return new List<Vector2Int>();
        }

        /// <summary>
        /// Manhattan distance heuristic.
        /// </summary>
        /// <param name="a">The starting position.</param>
        /// <param name="b">The ending position.</param>
        /// <returns>The Manhattan distance between the two positions.</returns>
        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// Reconstructs the path from the end node to the start.
        /// </summary>
        /// <param name="endNode">The end node.</param>
        /// <returns>A list of positions representing the path.</returns>
        private static List<Vector2Int> ReconstructPath(Node endNode)
        {
            var path = new List<Vector2Int>();
            var current = endNode;
            while (current != null)
            {
                path.Add(current.Position);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Determines if the cell at pos is walkable. Cells with tiles in walkableTilemap, wallTilemap, or doorTilemap are considered obstacles, except for the start and end cells.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <param name="painter">The TilemapPainter used to check walkability.</param>
        /// <param name="start">The starting position.</param>
        /// <param name="end">The ending position.</param>
        /// <returns>True if the position is walkable, false otherwise.</returns>
        private static bool IsWalkable(Vector2Int pos, TilemapPainter painter, Vector2Int start, Vector2Int end)
        {
            var cellPos = new Vector3Int(pos.x, pos.y, 0);
            // If the position is the start or end, allow it even if it has a door
            var isEndpoint = pos == start || pos == end;

            var walkable = painter.walkableTilemap.GetTile(cellPos) == null &&
                           painter.wallTilemap.GetTile(cellPos) == null;
            if (!isEndpoint)
            {
                walkable = walkable && painter.doorTilemap.GetTile(cellPos) == null;
            }

            return walkable;
        }
    }
}