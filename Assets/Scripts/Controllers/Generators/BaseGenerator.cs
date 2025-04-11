using UnityEngine;

namespace Controllers.Generators
{
    public abstract class BaseGenerator : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField, Tooltip("TilemapPainter used to render the dungeon")]
        protected Models.TilemapPainter tilemapPainter;

        [SerializeField, Tooltip("Origin for dungeon generation")]
        protected Vector2Int origin = Vector2Int.zero;

        #endregion

        #region Properties

        public Vector2Int Origin => origin;
        public Models.TilemapPainter TilemapPainter => tilemapPainter;

        #endregion

        #region Abstract Methods

        public abstract void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default);

        #endregion

        #region Utility Methods

        public virtual void OpenGraphWindow()
        {
            Debug.LogWarning("OpenGraphWindow is not implemented.");
        }

        public void ClearDungeon()
        {
            if (tilemapPainter == null)
            {
                Debug.LogError("TilemapPainter is not assigned.");
                return;
            }

            tilemapPainter.ResetAllTiles();
        }

        // Ahora delega la persistencia a TilemapPersistenceManager
        public void SaveDungeon(string path)
        {
            if (tilemapPainter == null)
            {
                Debug.LogError("TilemapPainter is not assigned.");
                return;
            }

            // Suponiendo que en el TilemapPainter se han asignado los tres tilemaps correctamente.
            var persistenceManager = new Models.TilemapPersistenceManager(
                tilemapPainter.walkableTilemap, tilemapPainter.wallTilemap, tilemapPainter.doorTilemap
            );
            persistenceManager.SaveTilemap(path);
        }

        public void LoadDungeon(string path)
        {
            if (tilemapPainter == null)
            {
                Debug.LogError("TilemapPainter is not assigned.");
                return;
            }

            var persistenceManager = new Models.TilemapPersistenceManager(
                tilemapPainter.walkableTilemap, tilemapPainter.wallTilemap, tilemapPainter.doorTilemap
            );
            persistenceManager.LoadTilemap(path, tilemapPainter);
        }

        #endregion
    }
}