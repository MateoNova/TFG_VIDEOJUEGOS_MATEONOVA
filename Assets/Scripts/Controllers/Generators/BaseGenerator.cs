using UnityEngine;
using TilemapPainter = Models.TilemapPainter;

namespace Controllers.Generators
{
    public abstract class BaseGenerator : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField, Tooltip("TilemapPainter used to render the dungeon")]
        protected TilemapPainter tilemapPainter;
        [SerializeField, Tooltip("Origin for dungeon generation")]
        protected Vector2Int origin = Vector2Int.zero;
        #endregion

        #region Properties
        public Vector2Int Origin => origin;
        public TilemapPainter TilemapPainter => tilemapPainter;
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
            if (!tilemapPainter)
            {
                Debug.LogError("TilemapPainter is not assigned.");
                return;
            }
            tilemapPainter.ResetAllTiles();
        }
        public void SaveDungeon(string path)
        {
            if (!tilemapPainter)
            {
                Debug.LogError("TilemapPainter is not assigned.");
                return;
            }
            tilemapPainter.SaveTilemap(path);
        }
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
}