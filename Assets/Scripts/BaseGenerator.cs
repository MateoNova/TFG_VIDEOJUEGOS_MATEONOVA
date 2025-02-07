using UnityEngine;

/// <summary>
/// Abstract base class for dungeon generators.
/// </summary>
public abstract class BaseGenerator : MonoBehaviour
{
    /// <summary>
    /// The TilemapRenderer used to visualize the dungeon.
    /// </summary>
    [SerializeField]
    protected TilemapPainter tilemapPainter;
    
    /// <summary>
    /// The origin point for the dungeon generation.
    /// </summary>
    [SerializeField]
    protected Vector2Int origin = Vector2Int.zero;
    
    /// <summary>
    /// Abstract method to run the dungeon generation algorithm.
    /// Must be implemented by derived classes.
    /// </summary>
    public abstract void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default);

    public void ClearDungeon()
    {
        if (!tilemapPainter)
        {
            Debug.LogError("TilemapVisualizer is not assigned.");
            return;
        }
        tilemapPainter.ResetAllTiles();
    }

    public void SaveDungeon(string path)
    {
        if (!tilemapPainter)
        {
            Debug.LogError("TilemapVisualizer is not assigned.");
            return;
        }
        tilemapPainter.SaveTilemap(path);
    }

    public void LoadDungeon(string path)
    {
        if (!tilemapPainter)
        {
            Debug.LogError("TilemapVisualizer is not assigned.");
            return;
        }
        tilemapPainter.LoadTilemap(path);
    }
}