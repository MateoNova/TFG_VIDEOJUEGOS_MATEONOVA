using RandomWalkGeneration;
using UnityEngine;

/// <summary>
/// Abstract base class for dungeon generators.
/// </summary>
public abstract class BaseGenerator : MonoBehaviour
{
    #region Inspector Fields

    /// <summary>
    /// The TilemapPainter used to visualize the dungeon.
    /// </summary>
    [SerializeField, Tooltip(BaseGeneratorTooltips.tilemapPainterTooltip)]
    protected TilemapPainter tilemapPainter;

    /// <summary>
    /// The origin point for dungeon generation.
    /// </summary>
    [SerializeField, Tooltip(BaseGeneratorTooltips.originTooltip)]
    protected Vector2Int origin = Vector2Int.zero;

    #endregion

    #region Properties

    /// <summary>
    /// Read-only property to get the generation origin.
    /// </summary>
    public Vector2Int Origin => origin;

    /// <summary>
    /// Read-only property to get the assigned TilemapPainter.
    /// </summary>
    public TilemapPainter TilemapPainter => tilemapPainter;

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Abstract method that executes the dungeon generation algorithm.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <param name="resetTilemap">Indicates whether to reset the tilemap.</param>
    /// <param name="startPoint">Starting point for generation (default value: (0,0)).</param>
    public abstract void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default);

    #endregion

    # region Virtual Methods

    public virtual void OpenGraphWindow()
    {
        Debug.LogWarning("OpenGraphWindow is not implemented.");
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Clears the dungeon by resetting all tiles.
    /// </summary>
    public void ClearDungeon()
    {
        if (!tilemapPainter)
        {
            Debug.LogError("TilemapPainter is not assigned.");
            return;
        }

        tilemapPainter.ResetAllTiles();
    }

    /// <summary>
    /// Saves the dungeon to the specified path.
    /// </summary>
    /// <param name="path">Path where the dungeon will be saved.</param>
    public void SaveDungeon(string path)
    {
        if (!tilemapPainter)
        {
            Debug.LogError("TilemapPainter is not assigned.");
            return;
        }

        tilemapPainter.SaveTilemap(path);
    }

    /// <summary>
    /// Loads the dungeon from the specified path.
    /// </summary>
    /// <param name="path">Path of the file containing the dungeon.</param>
    public void LoadDungeon(string path)
    {
        if (!tilemapPainter)
        {
            Debug.LogError("TilemapPainter is not assigned.");
            return;
        }

        tilemapPainter.LoadTilemap(path);
    }

    #endregion
}