using Models;
using UnityEngine;

namespace Generators
{
    /// <summary>
    /// Abstract base class for dungeon generators. Provides common functionality for managing
    /// dungeon generation, tilemap rendering, and persistence.
    /// </summary>
    public abstract class BaseGenerator : MonoBehaviour
    {
        #region Inspector Fields

        /// <summary>
        /// The TilemapPainter used to render the dungeon. This is assigned in the Unity Inspector.
        /// </summary>
        [SerializeField, Tooltip("TilemapPainter used to render the dungeon")]
        protected TilemapPainter tilemapPainter;

        /// <summary>
        /// The origin point for dungeon generation. This is assigned in the Unity Inspector.
        /// </summary>
        [SerializeField, Tooltip("Origin for dungeon generation")]
        protected Vector2Int origin = Vector2Int.zero;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the origin point for dungeon generation.
        /// </summary>
        public Vector2Int Origin => origin;

        /// <summary>
        /// Gets the TilemapPainter used to render the dungeon.
        /// </summary>
        public TilemapPainter TilemapPainter => tilemapPainter;

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Abstract method to run the dungeon generation process. Must be implemented by derived classes.
        /// </summary>
        /// <param name="resetTilemap">Whether to reset the tilemap before generation.</param>
        /// <param name="startPoint">The starting point for generation (optional).</param>
        public abstract void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Opens the graph window for the generator. Can be overridden by derived classes.
        /// </summary>
        public virtual void OpenGraphWindow()
        {
            Debug.LogWarning("OpenGraphWindow is not implemented.");
        }

        /// <summary>
        /// Clears all tiles from the dungeon by resetting the TilemapPainter.
        /// </summary>
        public void ClearDungeon()
        {
            if (tilemapPainter == null)
            {
                Debug.LogError("TilemapPainter is not assigned.");
                return;
            }

            tilemapPainter.ResetAllTiles();
        }

        /// <summary>
        /// Saves the current dungeon state to a file using the TilemapPersistenceManager.
        /// </summary>
        /// <param name="path">The file path to save the dungeon.</param>
        public void SaveDungeon(string path)
        {
            if (tilemapPainter == null)
            {
                Debug.LogError("TilemapPainter is not assigned.");
                return;
            }

            // Assumes that the three tilemaps (walkable, wall, and door) are correctly assigned in the TilemapPainter.
            var persistenceManager = new TilemapPersistenceManager(
                tilemapPainter.walkableTilemap, tilemapPainter.wallTilemap, tilemapPainter.doorTilemap
            );
            persistenceManager.SaveTilemap(path);
        }

        /// <summary>
        /// Loads a dungeon state from a file using the TilemapPersistenceManager.
        /// </summary>
        /// <param name="path">The file path to load the dungeon from.</param>
        public void LoadDungeon(string path)
        {
            if (tilemapPainter == null)
            {
                Debug.LogError("TilemapPainter is not assigned.");
                return;
            }

            var persistenceManager = new TilemapPersistenceManager(
                tilemapPainter.walkableTilemap, tilemapPainter.wallTilemap, tilemapPainter.doorTilemap
            );
            persistenceManager.LoadTilemap(path, tilemapPainter);
        }

        #endregion
    }
}