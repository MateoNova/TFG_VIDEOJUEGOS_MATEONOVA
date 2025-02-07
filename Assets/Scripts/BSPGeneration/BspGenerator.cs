using UnityEngine;
using System.Collections.Generic;

namespace BSPGeneration
{
    public class BspGenerator : BaseGenerator
    {
        [SerializeField] private int minRoomSize = 5;
        [SerializeField] private int maxRoomSize = 20;
        [SerializeField] private int maxIterations = 5;

        public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap) tilemapPainter.ResetAllTiles();

            var rootNode = new BspNode(new RectInt(startPoint.x, startPoint.y, maxRoomSize * 2, maxRoomSize * 2));
            SplitNode(rootNode, maxIterations);

            var rooms = new List<RectInt>();
            CreateRooms(rootNode, rooms);

            var walkableTilesPositions = new HashSet<Vector2Int>();
            foreach (var room in rooms)
            {
                for (int x = room.xMin; x < room.xMax; x++)
                {
                    for (int y = room.yMin; y < room.yMax; y++)
                    {
                        walkableTilesPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            tilemapPainter.PaintWalkableTiles(walkableTilesPositions);
            WallGenerator.GenerateWalls(walkableTilesPositions, tilemapPainter);
        }

        private void SplitNode(BspNode node, int iterations)
        {
            if (iterations <= 0 || node.Width <= minRoomSize * 2 || node.Height <= minRoomSize * 2)
                return;

            bool splitHorizontally = Random.value > 0.5f;
            if (node.Width > node.Height && node.Width / node.Height >= 1.25f)
                splitHorizontally = false;
            else if (node.Height > node.Width && node.Height / node.Width >= 1.25f)
                splitHorizontally = true;

            if (splitHorizontally)
            {
                int splitY = Random.Range(minRoomSize, node.Height - minRoomSize);
                node.Left = new BspNode(new RectInt(node.Rect.xMin, node.Rect.yMin, node.Width, splitY));
                node.Right = new BspNode(new RectInt(node.Rect.xMin, node.Rect.yMin + splitY, node.Width,
                    node.Height - splitY));
            }
            else
            {
                int splitX = Random.Range(minRoomSize, node.Width - minRoomSize);
                node.Left = new BspNode(new RectInt(node.Rect.xMin, node.Rect.yMin, splitX, node.Height));
                node.Right = new BspNode(new RectInt(node.Rect.xMin + splitX, node.Rect.yMin, node.Width - splitX,
                    node.Height));
            }

            SplitNode(node.Left, iterations - 1);
            SplitNode(node.Right, iterations - 1);
        }

        private void CreateRooms(BspNode node, List<RectInt> rooms)
        {
            if (node.Left == null && node.Right == null)
            {
                int roomWidth = Random.Range(minRoomSize, node.Width);
                int roomHeight = Random.Range(minRoomSize, node.Height);
                int roomX = Random.Range(node.Rect.xMin, node.Rect.xMax - roomWidth);
                int roomY = Random.Range(node.Rect.yMin, node.Rect.yMax - roomHeight);
                node.Room = new RectInt(roomX, roomY, roomWidth, roomHeight);
                rooms.Add(node.Room);
            }
            else
            {
                if (node.Left != null) CreateRooms(node.Left, rooms);
                if (node.Right != null) CreateRooms(node.Right, rooms);
            }
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

        public BspNode(RectInt rect)
        {
            Rect = rect;
        }
    }
}