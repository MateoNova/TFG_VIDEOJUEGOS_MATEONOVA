using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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
        /// <remarks>
        /// A smaller value will create smaller rooms, which can result in a more complex and dense dungeon layout.
        /// A larger value will create larger rooms, which can result in a more open and spacious dungeon layout.
        /// </remarks>
        [SerializeField,
         Tooltip(
             "Minimum size of the rooms. Smaller values create smaller rooms, resulting in a more complex and dense dungeon layout. Larger values create larger rooms, resulting in a more open and spacious dungeon layout.")]
        private int minRoomSize = 5;

        /// <summary>
        /// Maximum size of the rooms.
        /// </summary>
        /// <remarks>
        /// A smaller value will limit the maximum size of the rooms, leading to a more uniform room size.
        /// A larger value will allow for larger rooms, creating more variation in room sizes.
        /// </remarks>
        [SerializeField,
         Tooltip(
             "Maximum size of the rooms. Smaller values limit the maximum size of the rooms, leading to a more uniform room size. Larger values allow for larger rooms, creating more variation in room sizes.")]
        private int maxRoomSize = 20;

        /// <summary>
        /// Maximum number of iterations for splitting the space.
        /// </summary>
        /// <remarks>
        /// A smaller value will result in fewer splits, creating fewer but larger rooms.
        /// A larger value will result in more splits, creating more but smaller rooms.
        /// </remarks>
        [SerializeField,
         Tooltip(
             "Maximum number of iterations for splitting the space. Smaller values result in fewer splits, creating fewer but larger rooms. Larger values result in more splits, creating more but smaller rooms.")]
        private int maxIterations = 5;

        /// <summary>
        /// Aspect ratio threshold for deciding split direction.
        /// </summary>
        /// <remarks>
        /// A smaller value will make the algorithm more likely to split nodes in both directions.
        /// A larger value will make the algorithm more likely to split nodes in one direction, creating more elongated rooms.
        /// </remarks>
        [SerializeField,
         Tooltip(
             "Aspect ratio threshold for deciding split direction. Smaller values make the algorithm more likely to split nodes in both directions. Larger values make the algorithm more likely to split nodes in one direction, creating more elongated rooms.")]
        private float aspectProportion = 1.5f;

        public void Start()
        {
            RunGeneration(true, origin);
        }

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

            CreateCorridors(rootNode, walkableTiles, rooms);

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
            if (node.Width > node.Height && node.Width / node.Height >= aspectProportion) return false;
            if (node.Height > node.Width && node.Height / node.Width >= aspectProportion) return true;
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

        /// <summary>
        /// Creates corridors between rooms in the BSP tree.
        /// </summary>
        /// <param name="node">The current node in the BSP tree.</param>
        /// <param name="walkableTiles">The set of walkable tiles to update.</param>
        /// <param name="rooms">The list of rooms to ensure connectivity.</param>
        private void CreateCorridors(BspNode node, HashSet<Vector2Int> walkableTiles, List<RectInt> rooms)
        {
            if (node.Left == null || node.Right == null) return;

            EnsureAllRoomsConnected(node, walkableTiles);
        }

        /// <summary>
        /// Creates a corridor between two points.
        /// </summary>
        /// <param name="start">The starting point of the corridor.</param>
        /// <param name="end">The ending point of the corridor.</param>
        /// <param name="walkableTiles">The set of walkable tiles to update.</param>
        private static void CreateCorridor(Vector2Int start, Vector2Int end, HashSet<Vector2Int> walkableTiles)
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

                walkableTiles.Add(current);
            }
        }

        /// <summary>
        /// Ensures all rooms are connected using BSP corridor algorithm.
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

            if (!leftRoom.HasValue || !rightRoom.HasValue) return;
            
            var leftCenter = Vector2Int.RoundToInt(leftRoom.Value.center);
            var rightCenter = Vector2Int.RoundToInt(rightRoom.Value.center);

            CreateCorridor(leftCenter, rightCenter, walkableTiles);
        }

        /// <summary>
        /// Retrieves a room from a BSP node.
        /// </summary>
        /// <param name="node">The node to retrieve the room from.</param>
        /// <returns>The room if available, otherwise null.</returns>
        private RectInt? GetRoomFromNode(BspNode node)
        {
            if (node.Room != default)
                return node.Room;

            var leftRoom = node.Left != null ? GetRoomFromNode(node.Left) : null;
            var rightRoom = node.Right != null ? GetRoomFromNode(node.Right) : null;

            return leftRoom ?? rightRoom;
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