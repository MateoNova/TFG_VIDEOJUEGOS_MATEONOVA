/*using System;
using System.Collections.Generic;
using System.Linq;
using BSPGeneration;
using UnityEngine;
using Random = UnityEngine.Random;

public class GeneticGenerator : BaseGenerator
{
    [SerializeField] private int PopulationSize = 10;
    [SerializeField] private int MaxGenerations = 50;
    [SerializeField] private float MutationRate = 0.1f;

    public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
    {
        if (resetTilemap)
            tilemapPainter.ResetAllTiles();

        List<Dungeon> population = InitializePopulation();
        for (int generation = 0; generation < MaxGenerations; generation++)
        {
            population = EvolvePopulation(population);
        }

        if (population.Any())
        {
            Dungeon bestDungeon = population.OrderByDescending(EvaluateFitness).First();
            RenderDungeon(bestDungeon);
        }
        else
        {
            Debug.LogWarning("Population is empty after evolution.");
        }
    }

    private List<Dungeon> InitializePopulation()
    {
        List<Dungeon> population = new List<Dungeon>();
        for (int i = 0; i < PopulationSize; i++)
        {
            population.Add(Dungeon.GenerateRandom());
        }

        return population;
    }

    private List<Dungeon> EvolvePopulation(List<Dungeon> population)
    {
        if (population.Count < 2)
            return population;

        population = population.OrderByDescending(EvaluateFitness).ToList();
        List<Dungeon> newPopulation = new List<Dungeon>();

        while (newPopulation.Count < PopulationSize)
        {
            Dungeon parent1 = population[UnityEngine.Random.Range(0, population.Count)];
            Dungeon parent2 = population[UnityEngine.Random.Range(0, population.Count)];

            if (parent1 == parent2 && population.Count > 1)
            {
                parent2 = population[UnityEngine.Random.Range(0, population.Count)];
            }

            Dungeon child = Crossover(parent1, parent2);
            if (UnityEngine.Random.value < MutationRate)
                child.Mutate();

            newPopulation.Add(child);
        }

        return newPopulation;
    }

    private Dungeon Crossover(Dungeon parent1, Dungeon parent2)
    {
        return Dungeon.Combine(parent1, parent2);
    }

    private float EvaluateFitness(Dungeon dungeon)
    {
        return dungeon.Evaluate();
    }

    private void RenderDungeon(Dungeon dungeon)
    {
        tilemapPainter.PaintWalkableTiles(dungeon.GetWalkableTiles());
        tilemapPainter.PaintWallTiles(dungeon.GetWallTiles());
    }
}
public class Dungeon
{
    /// <summary>
    /// Minimum size of the rooms.
    /// </summary>
    [SerializeField, Tooltip("Minimum size of the rooms.")]
    private static int minRoomSize = 5;

    /// <summary>
    /// Maximum size of the rooms.
    /// </summary>
    [SerializeField, Tooltip("Maximum size of the rooms.")]
    private static int maxRoomSize = 20;

    /// <summary>
    /// Maximum number of iterations for splitting the space.
    /// </summary>
    [SerializeField, Tooltip("Maximum number of iterations for splitting the space.")]
    private static int maxIterations = 5;

    /// <summary>
    /// Aspect ratio threshold for deciding split direction.
    /// </summary>
    [SerializeField, Tooltip("Aspect ratio threshold for deciding split direction.")]
    private static float aspectProportion = 1.5f;

    /// <summary>
    /// Width of the corridors.
    /// </summary>
    [SerializeField, Range(0f, 4f), Tooltip("Width of the corridors.")]
    private static int corridorWidth = 1;

    public List<Vector2Int> WalkableTiles { get; private set; } = new List<Vector2Int>();
    public List<Vector2Int> WallTiles { get; private set; } = new List<Vector2Int>();

    public static Dungeon GenerateRandom()
    {
        Dungeon dungeon = new Dungeon();

        var rootNode = new BspNode(new RectInt(0, 0, maxRoomSize * 2, maxRoomSize * 2));
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

        dungeon.WalkableTiles = walkableTiles.ToList();
        // Wall generation logic should be added here if needed

        return dungeon;
    }

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

            for (var i = 0; i < corridorWidth; i++)
            {
                walkableTiles.Add(new Vector2Int(current.x + i, current.y));
                walkableTiles.Add(new Vector2Int(current.x, current.y + i));
            }
        }
    }

    private static void CreateCorridors(BspNode node, HashSet<Vector2Int> walkableTiles, List<RectInt> rooms)
    {
        if (node.Left == null || node.Right == null) return;

        EnsureAllRoomsConnected(node, walkableTiles);
    }

    private static void EnsureAllRoomsConnected(BspNode node, HashSet<Vector2Int> walkableTiles)
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

    private static void CollectRooms(BspNode node, List<RectInt> rooms)
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

    private static void SplitNode(BspNode node, int iterations)
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

    private static bool ShouldSplitHorizontally(BspNode node)
    {
        if (node.Width > node.Height && node.Width / node.Height >= aspectProportion) return false;
        if (node.Height > node.Width && node.Height / node.Width >= aspectProportion) return true;
        return Random.value > 0.5f;
    }

    public static Dungeon Combine(Dungeon parent1, Dungeon parent2)
    {
        Dungeon child = new Dungeon();
        child.WalkableTiles = parent1.WalkableTiles.Take(parent1.WalkableTiles.Count / 2).ToList();
        child.WalkableTiles.AddRange(parent2.WalkableTiles.Skip(parent2.WalkableTiles.Count / 2));
        child.WallTiles = parent1.WallTiles.Take(parent1.WallTiles.Count / 2).ToList();
        child.WallTiles.AddRange(parent2.WallTiles.Skip(parent2.WallTiles.Count / 2));
        return child;
    }

    public void Mutate()
    {
        if (WalkableTiles.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, WalkableTiles.Count);
            WalkableTiles[index] = new Vector2Int(UnityEngine.Random.Range(0, 20), UnityEngine.Random.Range(0, 20));
        }
    }

    public float Evaluate()
    {
        return WalkableTiles.Count - WallTiles.Count;
    }

    public IEnumerable<Vector2Int> GetWalkableTiles() => WalkableTiles;
    public IEnumerable<Vector2Int> GetWallTiles() => WallTiles;
}*/