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

            // Create the root node with a rectangle starting at startPoint and size maxRoomSize * 2
            var rootNode = new BspNode(new RectInt(startPoint.x, startPoint.y, maxRoomSize * 2, maxRoomSize * 2));
            // Split the root node recursively
            SplitNode(rootNode, maxIterations);

            // List to store the created rooms
            var rooms = new List<RectInt>();
            // Create rooms in the split nodes
            CreateRooms(rootNode, rooms);

            // Set to store walkable tile positions
            var walkableTilesPositions = new HashSet<Vector2Int>();
            // Add all room tiles to the walkable tiles set
            foreach (var room in rooms)
            {
                for (var x = room.xMin; x < room.xMax; x++)
                {
                    for (var y = room.yMin; y < room.yMax; y++)
                    {
                        walkableTilesPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            // Paint the walkable tiles on the tilemap
            tilemapPainter.PaintWalkableTiles(walkableTilesPositions);
            // Generate walls around the walkable tiles
            WallGenerator.GenerateWalls(walkableTilesPositions, tilemapPainter);
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
                // Stop splitting if iterations are zero or the node is too small
                if (iterations <= 0 || node.Width <= minRoomSize * 2 || node.Height <= minRoomSize * 2) return;

                // Decide whether to split horizontally or vertically
                var splitHorizontally = Random.value > 0.5f;
                //if the node is too wide, split vertically
                if (node.Width > node.Height && node.Width / node.Height >= 1.25f)
                    splitHorizontally = false;
                else if (node.Height > node.Width && node.Height / node.Width >= 1.25f) splitHorizontally = true;

                // Create left and right child nodes based on the split direction
                if (splitHorizontally)
                {
                    var splitY = Random.Range(minRoomSize, node.Height - minRoomSize);
                    node.Left = new BspNode(new RectInt(node.Rect.xMin, node.Rect.yMin, node.Width, splitY));
                    node.Right = new BspNode(new RectInt(node.Rect.xMin, node.Rect.yMin + splitY, node.Width, node.Height - splitY));
                }
                else
                {
                    var splitX = Random.Range(minRoomSize, node.Width - minRoomSize);
                    node.Left = new BspNode(new RectInt(node.Rect.xMin, node.Rect.yMin, splitX, node.Height));
                    node.Right = new BspNode(new RectInt(node.Rect.xMin + splitX, node.Rect.yMin, node.Width - splitX, node.Height));
                }

                // Recursively split the left child node
                SplitNode(node.Left, iterations - 1);
                // Continue with the right child node
                node = node.Right;
                iterations -= 1;
            }
        }

        /// <summary>
        /// Creates rooms in the leaf nodes (nodes without children).
        /// </summary>
        /// <param name="node">The node to create rooms in.</param>
        /// <param name="rooms">The list to store the created rooms.</param>
        private void CreateRooms(BspNode node, List<RectInt> rooms)
        {
            while (true)
            {
                // If the node is a leaf node, create a room
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
                    // Recursively create rooms in the left and right child nodes
                    if (node.Left != null) CreateRooms(node.Left, rooms);
                    if (node.Right != null)
                    {
                        node = node.Right;
                        continue;
                    }
                }

                break;
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
        public BspNode(RectInt rect)
        {
            Rect = rect;
        }
    }
}