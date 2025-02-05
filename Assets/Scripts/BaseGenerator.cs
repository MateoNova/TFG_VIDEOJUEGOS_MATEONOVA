using UnityEngine;

public abstract class BaseGenerator : MonoBehaviour
{
    [SerializeField]
    protected TilemapRenderer tilemapRenderer;

    [SerializeField]
    protected Vector2Int origin = Vector2Int.zero;

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

    protected abstract void RunGeneration();
}