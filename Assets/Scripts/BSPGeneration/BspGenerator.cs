using UnityEngine;
using System.Collections.Generic;

namespace BSPGeneration
{
    /// <summary>
    /// Generates a dungeon using the Binary Space Partitioning (BSP) algorithm.
    /// </summary>
    public class BspGenerator : BaseGenerator
    {
        /// <summary>
        /// Minimum size of the rooms.
        /// </summary>
        [SerializeField] private int minRoomSize = 5;

        /// <summary>
        /// Maximum size of the rooms.
        /// </summary>
        [SerializeField] private int maxRoomSize = 20;

        /// <summary>
        /// Maximum number of iterations for splitting the space.
        /// </summary>
        [SerializeField] private int maxIterations = 5;

        /// <summary>
        /// Runs the generation process.
        /// </summary>
        /// <param name="resetTilemap">If true, resets the tilemap before generation.</param>
        /// <param name="startPoint">The starting point for the generation.</param>
        public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap) tilemapPainter.ResetAllTiles();

            var rootNode = new BspNode(new RectInt(startPoint.x, startPoint.y, maxRoomSize * 2, maxRoomSize * 2));
            SplitNode(rootNode, maxIterations);

            var rooms = new List<RectInt>();
            CollectRooms(rootNode, rooms);

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

            tilemapPainter.PaintWalkableTiles(walkableTiles);
            WallGenerator.GenerateWalls(walkableTiles, tilemapPainter);
        }

        /// <summary>
        /// Recursively splits a node into smaller nodes.
        /// </summary>
        /// <param name="node">The node to split.</param>
        /// <param name="iterations">The number of iterations left for splitting.</param>
        private void SplitNode(BspNode node, int iterations)
        {
            while (true)
            {
                if (iterations <= 0 || node.Width <= minRoomSize * 2 || node.Height <= minRoomSize * 2) return;

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
                    node.Right = new BspNode(new RectInt(node.Rect.xMin + splitPos, node.Rect.yMin,
                        node.Width - splitPos, node.Height));
                }

                SplitNode(node.Left, iterations - 1);
                node = node.Right;
                iterations -= 1;
            }
        }

        /// <summary>
        /// Determines whether to split the node horizontally.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <returns>True if the node should be split horizontally, otherwise false.</returns>
        private static bool ShouldSplitHorizontally(BspNode node)
        {
            if (node.Width > node.Height && node.Width / node.Height >= 1.25f) return false;
            if (node.Height > node.Width && node.Height / node.Width >= 1.25f) return true;
            return Random.value > 0.5f;
        }

        /// <summary>
        /// Collects rooms from the BSP tree.
        /// </summary>
        /// <param name="node">The node to collect rooms from.</param>
        /// <param name="rooms">The list to store the collected rooms.</param>
        private void CollectRooms(BspNode node, List<RectInt> rooms)
        {
            if (node == null) return;

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
        public BspNode(RectInt rect) => Rect = rect;
    }
}