/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TemplateGenerator
{
    [System.Serializable]
    public class RoomTemplate
    {
        public TileBase[] tiles;
        public Vector2Int size;
        public Vector2Int entryPoint;
    }

    public class TemplateGenerator : BaseGenerator
    {
        [SerializeField] private List<RoomTemplate> roomTemplates;
        [SerializeField] private int roomSpacing = 5;

        public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap)
            {
                ClearDungeon();
            }

            var placedRooms = new List<RectInt>();
            var walkableTiles = new HashSet<Vector2Int>();

            foreach (var template in roomTemplates)
            {
                var roomPosition = FindRoomPosition(placedRooms, template.size);
                PlaceRoom(template, roomPosition);
                placedRooms.Add(new RectInt(roomPosition, template.size));
            }

            ConnectRooms(placedRooms, walkableTiles);
            tilemapPainter.PaintWalkableTiles(walkableTiles);
        }

        private Vector2Int FindRoomPosition(List<RectInt> placedRooms, Vector2Int roomSize)
        {
            Vector2Int position;
            bool isValidPosition;

            do
            {
                position = new Vector2Int(
                    Random.Range(0, 100),
                    Random.Range(0, 100)
                );

                isValidPosition = true;
                foreach (var placedRoom in placedRooms)
                {
                    if (placedRoom.Overlaps(new RectInt(position, roomSize)))
                    {
                        isValidPosition = false;
                        break;
                    }
                }
            } while (!isValidPosition);

            return position;
        }

        private void PlaceRoom(RoomTemplate template, Vector2Int position)
        {
            for (int x = 0; x < template.size.x; x++)
            {
                for (int y = 0; y < template.size.y; y++)
                {
                    var tilePosition = new Vector2Int(position.x + x, position.y + y);
                    if 
                    tilemapPainter.PaintTile(tilePosition, template.tiles[y * template.size.x + x]);
                }
            }
        }

        private void ConnectRooms(List<RectInt> rooms, HashSet<Vector2Int> walkableTiles)
        {
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                var startRoom = rooms[i];
                var endRoom = rooms[i + 1];

                var start = new Vector2Int(
                    startRoom.x + startRoom.width / 2,
                    startRoom.y + startRoom.height / 2
                );

                var end = new Vector2Int(
                    endRoom.x + endRoom.width / 2,
                    endRoom.y + endRoom.height / 2
                );

                CreateCorridor(start, end, walkableTiles);
            }
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

                walkableTiles.Add(current);
            }
        }
    }
}*/