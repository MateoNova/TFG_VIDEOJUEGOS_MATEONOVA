using System;
using System.Collections.Generic;
using System.Linq;
using RandomWalkGeneration;
using UnityEngine;

public class CorridorFirstGenerator : RandomWalkGenerator
{
    [SerializeField] private int corridorLength = 10;
    [SerializeField] private int corridorCount = 5;
    [SerializeField, Range(0.1f,1)] private float roomPercentage = 0.8f;
    
    public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
    {
        if (resetTilemap)
        {
            tilemapPainter.ResetAllTiles();
        }
        
        HashSet<Vector2Int> walkableTilesPositions = new();
        HashSet<Vector2Int> potentialRoomPositions = new();

        CreateCorridors(walkableTilesPositions, potentialRoomPositions);

        var roomsPositions = CreateRooms(potentialRoomPositions);
        
        var deadEnds = FindAllDeadEnds(walkableTilesPositions);
        
        CreateRoomsAtDeadEnds(deadEnds, roomsPositions);
        
        walkableTilesPositions.UnionWith(roomsPositions);
        
        tilemapPainter.PaintWalkableTiles(walkableTilesPositions);
        WallGenerator.GenerateWalls(walkableTilesPositions, tilemapPainter);
    }

    private void CreateRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        foreach (var deadEnd in deadEnds)
        {
            if (roomFloors.Contains(deadEnd)==false)
            {
                var room = GenerateWalkableArea(deadEnd);
                roomFloors.UnionWith(room);
            }
        }
    }

    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new();
        foreach (var pos in floorPositions)
        {
            var neighboursCount = 0;
            foreach (var direction in Utils.Directions)
            {
                var neighbourPos = pos + direction;
                if (floorPositions.Contains(neighbourPos))
                {
                    neighboursCount++;
                }
            }

            if (neighboursCount == 1)
            {
                deadEnds.Add(pos);
            }
        }
        return deadEnds;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomsPositions = new();
        var roomToCreateCount = (int)(potentialRoomPositions.Count * roomPercentage);
        
        var roomToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();
        foreach (var roomPos in roomToCreate)
        {
            var room = GenerateWalkableArea(roomPos);
            roomsPositions.UnionWith(room);
        }
        return roomsPositions;
    }

    private void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> roomsPotentialPositions)
    {
        var currentPos = origin;
        roomsPotentialPositions.Add(currentPos); 
        
        for (var i = 0; i < corridorCount; i++)
        {
            var corridor = RandomWalkCorridor(currentPos, corridorLength);
            currentPos = corridor[^1];
            roomsPotentialPositions.Add(currentPos); // Add pos at the end of the corridor path
            floorPositions.UnionWith(corridor);
        }
    }

    private static List<Vector2Int> RandomWalkCorridor(Vector2Int startPos, int length)
    {
        List<Vector2Int> path = new();
        var direction = Utils.GetRandomCardinalDirection();
        var currentPos = startPos;
        path.Add(startPos);
        
        for (var i = 0; i < length; i++)
        {
            currentPos += direction;
            path.Add(currentPos);
        }
        
        return path;
    }
}
