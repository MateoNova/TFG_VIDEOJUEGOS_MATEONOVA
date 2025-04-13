using System.Collections.Generic;
using UnityEngine;
using WallGenerator = Models.WallGenerator;

namespace Generators
{
    /// <summary>
    /// Generates a dungeon using the Binary Space Partitioning (BSP) algorithm.
    /// </summary>
    public class BspGenerator : BaseGenerator
    {
        #region Serialized Fields

        [SerializeField, Tooltip("Minimum size of the rooms.")]
        private int minRoomSize = 5;

        [SerializeField, Tooltip("Maximum size of the rooms.")]
        private int maxRoomSize = 20;

        [SerializeField, Tooltip("Maximum number of iterations for splitting the space.")]
        private int maxIterations = 5;

        [SerializeField, Tooltip("Aspect ratio threshold for deciding split direction.")]
        private float aspectProportion = 1.5f;

        [SerializeField, Range(0f, 4f), Tooltip("Width of the corridors.")]
        private int corridorWidth = 1;

        #endregion

        #region Generation Process

        /// <summary>
        /// Runs the generation process.
        /// </summary>
        /// <param name="resetTilemap">If true, resets the tilemap before generation.</param>
        /// <param name="startPoint">The starting point for the generation.</param>
        public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap)
            {
                tilemapPainter.ResetAllTiles();
            }

            // Create the root node for the BSP tree.
            var rootNode = new BspNode(new RectInt(startPoint.x, startPoint.y, maxRoomSize * 2, maxRoomSize * 2));
            SplitNode(rootNode, maxIterations);

            // Collect rooms from the BSP tree.
            var rooms = new List<RectInt>();
            CollectRooms(rootNode, rooms);

            // Fill walkable tiles from all rooms.
            var walkableTiles = new HashSet<Vector2Int>();
            foreach (var room in rooms)
            {
                for (var x = room.xMin; x < room.xMax; x++)
                {
                    for (var y = room.yMin; y < room.yMax; y++)
                    {
                        walkableTiles.Add(new Vector2Int(x, y));
                    }
                }
            }

            // Create corridors connecting rooms.
            CreateCorridors(rootNode, walkableTiles, rooms);

            // Render the dungeon.
            tilemapPainter.PaintWalkableTiles(walkableTiles);
            WallGenerator.GenerateWalls(walkableTiles, tilemapPainter);
        }

        #endregion

        #region BSP Tree Generation

        /// <summary>
        /// Recursively splits a node into smaller nodes.
        /// </summary>
        /// <param name="node">The node to split.</param>
        /// <param name="iterations">The number of iterations left for splitting.</param>
        private void SplitNode(BspNode node, int iterations)
        {
            if (iterations <= 0 || node.Width <= minRoomSize * 2 || node.Height <= minRoomSize * 2)
                return;

            var splitHorizontally = ShouldSplitHorizontally(node);
            var splitPos = splitHorizontally
                ? Random.Range(minRoomSize, node.Height - minRoomSize)
                : Random.Range(minRoomSize, node.Width - minRoomSize);

            if (splitHorizontally)
            {
                node.Left = new BspNode(new RectInt(node.Rect.xMin, node.Rect.yMin, node.Width, splitPos));
                node.Right = new BspNode(new RectInt(node.Rect.xMin, node.Rect.yMin + splitPos, node.Width,
                    node.Height - splitPos));
            }
            else
            {
                node.Left = new BspNode(new RectInt(node.Rect.xMin, node.Rect.yMin, splitPos, node.Height));
                node.Right = new BspNode(new RectInt(node.Rect.xMin + splitPos, node.Rect.yMin, node.Width - splitPos,
                    node.Height));
            }

            SplitNode(node.Left, iterations - 1);
            SplitNode(node.Right, iterations - 1);
        }

        /// <summary>
        /// Determines whether to split the node horizontally.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <returns>True if the node should be split horizontally, otherwise false.</returns>
        private bool ShouldSplitHorizontally(BspNode node)
        {
            // Convert dimensions to float for proper division.
            float width = node.Width;
            float height = node.Height;

            if (width > height && (width / height) >= aspectProportion)
                return false;
            if (height > width && (height / width) >= aspectProportion)
                return true;

            return Random.value > 0.5f;
        }

        /// <summary>
        /// Collects rooms from the BSP tree.
        /// </summary>
        /// <param name="node">The node to collect rooms from.</param>
        /// <param name="rooms">The list to store the collected rooms.</param>
        private void CollectRooms(BspNode node, List<RectInt> rooms)
        {
            if (node == null)
                return;

            // If the node is a leaf, create a room within it.
            if (node.Left == null && node.Right == null)
            {
                var roomWidth = Random.Range(minRoomSize, node.Width);
                var roomHeight = Random.Range(minRoomSize, node.Height);
                var roomX = Random.Range(node.Rect.xMin, node.Rect.xMax - roomWidth);
                var roomY = Random.Range(node.Rect.yMin, node.Rect.yMax - roomHeight);

                node.Room = new RectInt(roomX, roomY, roomWidth, roomHeight);
                rooms.Add(node.Room);
            }
            else
            {
                CollectRooms(node.Left, rooms);
                CollectRooms(node.Right, rooms);
            }
        }

        #endregion

        #region Corridor Generation

        /// <summary>
        /// Creates corridors between rooms in the BSP tree.
        /// </summary>
        /// <param name="node">The current node in the BSP tree.</param>
        /// <param name="walkableTiles">The set of walkable tiles to update.</param>
        /// <param name="rooms">The list of rooms to ensure connectivity.</param>
        private void CreateCorridors(BspNode node, HashSet<Vector2Int> walkableTiles, List<RectInt> rooms)
        {
            if (node.Left == null || node.Right == null)
                return;

            EnsureAllRoomsConnected(node, walkableTiles);
        }

        /// <summary>
        /// Creates a corridor between two points.
        /// </summary>
        /// <param name="start">The starting point of the corridor.</param>
        /// <param name="end">The ending point of the corridor.</param>
        /// <param name="walkableTiles">The set of walkable tiles to update.</param>
        private void CreateCorridor(Vector2Int start, Vector2Int end, HashSet<Vector2Int> walkableTiles)
        {
            var current = start;

            while (current != end)
            {
                if (current.x != end.x)
                {
                    current.x += current.x < end.x ? 1 : -1;
                }
                else if (current.y != end.y)
                {
                    current.y += current.y < end.y ? 1 : -1;
                }

                // Expand the corridor width.
                for (var i = 0; i < corridorWidth; i++)
                {
                    walkableTiles.Add(new Vector2Int(current.x + i, current.y));
                    walkableTiles.Add(new Vector2Int(current.x, current.y + i));
                }
            }
        }

        /// <summary>
        /// Ensures all rooms are connected using the BSP corridor algorithm.
        /// </summary>
        /// <param name="node">The root node of the BSP tree.</param>
        /// <param name="walkableTiles">The set of walkable tiles to update.</param>
        private void EnsureAllRoomsConnected(BspNode node, HashSet<Vector2Int> walkableTiles)
        {
            if (node?.Left == null || node.Right == null)
                return;

            EnsureAllRoomsConnected(node.Left, walkableTiles);
            EnsureAllRoomsConnected(node.Right, walkableTiles);

            var leftRoom = GetRoomFromNode(node.Left);
            var rightRoom = GetRoomFromNode(node.Right);

            if (!leftRoom.HasValue || !rightRoom.HasValue)
                return;

            var leftCenter = Vector2Int.RoundToInt(leftRoom.Value.center);
            var rightCenter = Vector2Int.RoundToInt(rightRoom.Value.center);

            CreateCorridor(leftCenter, rightCenter, walkableTiles);
        }

        /// <summary>
        /// Retrieves a room from a BSP node.
        /// </summary>
        /// <param name="node">The node to retrieve the room from.</param>
        /// <returns>The room if available, otherwise null.</returns>
        private static RectInt? GetRoomFromNode(BspNode node)
        {
            if (node.Room != default)
                return node.Room;

            var leftRoom = node.Left != null ? GetRoomFromNode(node.Left) : null;
            var rightRoom = node.Right != null ? GetRoomFromNode(node.Right) : null;

            return leftRoom ?? rightRoom;
        }

        #endregion

        public void RUUUUN()
        {
            RunGeneration();
        }
    }

    /// <summary>
    /// Represents a node in the BSP tree.
    /// </summary>
    public class BspNode
    {
        /// <summary>
        /// The rectangle representing the node's area.
        /// </summary>
        public RectInt Rect { get; }

        /// <summary>
        /// The left child node.
        /// </summary>
        public BspNode Left { get; set; }

        /// <summary>
        /// The right child node.
        /// </summary>
        public BspNode Right { get; set; }

        /// <summary>
        /// The room created in this node.
        /// </summary>
        public RectInt Room { get; set; }

        /// <summary>
        /// The width of the node's rectangle.
        /// </summary>
        public int Width => Rect.width;

        /// <summary>
        /// The height of the node's rectangle.
        /// </summary>
        public int Height => Rect.height;

        /// <summary>
        /// Initializes a new instance of the BspNode class.
        /// </summary>
        /// <param name="rect">The rectangle representing the node's area.</param>
        public BspNode(RectInt rect)
        {
            Rect = rect;
        }
    }
}