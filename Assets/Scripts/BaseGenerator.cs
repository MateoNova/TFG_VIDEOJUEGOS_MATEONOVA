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
    protected TilemapRenderer tilemapRenderer;
    
    /// <summary>
    /// The origin point for the dungeon generation.
    /// </summary>
    [SerializeField]
    protected Vector2Int origin = Vector2Int.zero;
    
    /// <summary>
    /// Generates the dungeon by resetting the tilemap and running the generation algorithm.
    /// </summary>
    public void GenerateDungeon()
    {
        if (!tilemapRenderer)
        {
            Debug.LogError("TilemapVisualizer is not assigned.");
            return;
        }
        tilemapRenderer.ResetAllTiles();
        RunGeneration();
    }
    /// <summary>
    /// Abstract method to run the dungeon generation algorithm.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract void RunGeneration();
}