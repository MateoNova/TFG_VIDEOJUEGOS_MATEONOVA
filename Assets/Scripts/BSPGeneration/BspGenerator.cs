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
        [SerializeField, Tooltip("Minimum size of the rooms.")]
        private int minRoomSize = 5;

        /// <summary>
        /// Maximum size of the rooms.
        /// </summary>
        [SerializeField, Tooltip("Maximum size of the rooms.")]
        private int maxRoomSize = 20;

        /// <summary>
        /// Maximum number of iterations for splitting the space.
        /// </summary>
        [SerializeField, Tooltip("Maximum number of iterations for splitting the space.")]
        private int maxIterations = 5;

        /// <summary>
        /// Aspect ratio threshold for deciding split direction.
        /// </summary>
        [SerializeField, Tooltip("Aspect ratio threshold for deciding split direction.")]
        private float aspectProportion = 1.5f;

        /// <summary>
        /// Width of the corridors.
        /// </summary>
        [SerializeField, Range(0f,4f), Tooltip("Width of the corridors.")]
        private int corridorWidth = 1;

        public void Start()
        {
            RunGeneration(true, origin);
        }

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

        private bool ShouldSplitHorizontally(BspNode node)
        {
            if (node.Width > node.Height && node.Width / node.Height >= aspectProportion) return false;
            if (node.Height > node.Width && node.Height / node.Width >= aspectProportion) return true;
            return Random.value > 0.5f;
        }

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

        private void CreateCorridors(BspNode node, HashSet<Vector2Int> walkableTiles, List<RectInt> rooms)
        {
            if (node.Left == null || node.Right == null) return;

            EnsureAllRoomsConnected(node, walkableTiles);
        }

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

                for (var i = 0; i < corridorWidth; i++)
                {
                    walkableTiles.Add(new Vector2Int(current.x + i, current.y));
                    walkableTiles.Add(new Vector2Int(current.x, current.y + i));
                }
            }
        }

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

        private static RectInt? GetRoomFromNode(BspNode node)
        {
            if (node.Room != default)
                return node.Room;

            var leftRoom = node.Left != null ? GetRoomFromNode(node.Left) : null;
            var rightRoom = node.Right != null ? GetRoomFromNode(node.Right) : null;

            return leftRoom ?? rightRoom;
        }
    }

    public class BspNode
    {
        public RectInt Rect { get; }
        public BspNode Left { get; set; }
        public BspNode Right { get; set; }
        public RectInt Room { get; set; }
        public int Width => Rect.width;
        public int Height => Rect.height;

        public BspNode(RectInt rect) => Rect = rect;
    }
}