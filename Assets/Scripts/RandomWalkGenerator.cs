using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomWalkGenerator : BaseGenerator
{
    [SerializeField] private RandomWalkSettings randomWalkSettings;

    protected override void RunGeneration()
    { 
        var floorPositions = GenerateWalkableArea();
        tilemapRenderer.RenderWalkableTiles(floorPositions);
    }

    private HashSet<Vector2Int> GenerateWalkableArea()
    {
        var currentPos = origin;
        var tiles = new HashSet<Vector2Int>();

        for (var i = 0; i < randomWalkSettings.walkIterations; i++)
        {
            var path = RandomWalkAlgorithm(currentPos, randomWalkSettings.stepsPerIteration);
            tiles.UnionWith(path);
            if (randomWalkSettings.randomizeStartPos && tiles.Count > 0)
            {
                currentPos = tiles.ElementAt(Random.Range(0, tiles.Count));
            }
        }
        return tiles;
    }
    
    private static HashSet<Vector2Int> RandomWalkAlgorithm(Vector2Int origin, int steps)
    {
        var path = new HashSet<Vector2Int> { origin };
        var currentPos = origin;

        for (var i = 0; i < steps; i++)
        {
            currentPos += Utils.GetRandomCardinalDirection();
            path.Add(currentPos);
        }

        return path;
    }
}