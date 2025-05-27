using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Views.Attributes;

namespace Models
{
    [CreateAssetMenu(menuName = "Tileset/Tileset Preset", fileName = "NewTilesetPreset")]
    public class TilesetPreset : ScriptableObject
    {
        [Header("Floor tiles")] [SerializeField]
        public List<TileBase> walkableTileBases = new();

        [SerializeField] public List<int> walkableTilesPriorities = new();
        [SerializeField] public bool randomWalkableTilesPlacement;

        [SerializeField, WallTileGroup("CardinalDirections")]
        public TileBase upWall;

        [SerializeField, WallTileGroup("CardinalDirections")]
        public TileBase downWall;

        [SerializeField, WallTileGroup("CardinalDirections")]
        public TileBase leftWall;

        [SerializeField, WallTileGroup("CardinalDirections")]
        public TileBase rightWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        public TileBase topLeftWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        public TileBase topRightWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        public TileBase bottomLeftWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        public TileBase bottomRightWall;

        [SerializeField, WallTileGroup("ExternalCorners")]
        public TileBase allCornersWall;

        [SerializeField, WallTileGroup("InnerCorners")]
        public TileBase topLeftInnerWall;

        [SerializeField, WallTileGroup("InnerCorners")]
        public TileBase topRightInnerWall;

        [SerializeField, WallTileGroup("InnerCorners")]
        public TileBase bottomLeftInnerWall;

        [SerializeField, WallTileGroup("InnerCorners")]
        public TileBase bottomRightInnerWall;

        [SerializeField, WallTileGroup("TripleWalls")]
        public TileBase tripleExceptUpWall;

        [SerializeField, WallTileGroup("TripleWalls")]
        public TileBase tripleExcetDownWall;

        [SerializeField, WallTileGroup("TripleWalls")]
        public TileBase tripleExceptLeftWall;

        [SerializeField, WallTileGroup("TripleWalls")]
        public TileBase tripleExceptRightWall;

        [SerializeField, WallTileGroup("TripleInnerWalls")]
        public TileBase tripleExceptLeftInnerWall;

        [SerializeField, WallTileGroup("TripleInnerWalls")]
        public TileBase tripleExceptRightInnerWall;

        [SerializeField, WallTileGroup("AloneWalls")]
        public TileBase aloneWall;

        [SerializeField] public TileBase doorClosed;
        [SerializeField] public TileBase doorOpen;

        public TileBase[] maskTiles = new TileBase[256];

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureMaskSize();

            for (var mask = 0; mask < maskTiles.Length; mask++)
            {
                var n = Utils.Utils.GetNeighborBits(mask);
                var wp = Utils.Utils.DetermineWallPosition(n);
                maskTiles[mask] = GetTileForPosition(wp);
            }

            EditorUtility.SetDirty(this);
        }

        private void EnsureMaskSize()
        {
            if (maskTiles == null || maskTiles.Length != 256)
                maskTiles = new TileBase[256];
        }
        

        // Map WallPosition to the corresponding TileBase
        private TileBase GetTileForPosition(Utils.Utils.WallPosition wp) => wp switch
        {
            Utils.Utils.WallPosition.Up => upWall,
            Utils.Utils.WallPosition.Down => downWall,
            Utils.Utils.WallPosition.Left => leftWall,
            Utils.Utils.WallPosition.Right => rightWall,
            Utils.Utils.WallPosition.TopLeft => topLeftWall,
            Utils.Utils.WallPosition.TopRight => topRightWall,
            Utils.Utils.WallPosition.BottomLeft => bottomLeftWall,
            Utils.Utils.WallPosition.BottomRight => bottomRightWall,
            Utils.Utils.WallPosition.TopLeftInner => topLeftInnerWall,
            Utils.Utils.WallPosition.TopRightInner => topRightInnerWall,
            Utils.Utils.WallPosition.BottomLeftInner => bottomLeftInnerWall,
            Utils.Utils.WallPosition.BottomRightInner => bottomRightInnerWall,
            Utils.Utils.WallPosition.TripleExceptUp => tripleExceptUpWall,
            Utils.Utils.WallPosition.TripleExceptDown => tripleExcetDownWall,
            Utils.Utils.WallPosition.TripleExceptLeft => tripleExceptLeftWall,
            Utils.Utils.WallPosition.TripleExceptRight => tripleExceptRightWall,
            Utils.Utils.WallPosition.TripleExceptLeftInner => tripleExceptLeftInnerWall,
            Utils.Utils.WallPosition.TripleExceptRightInner => tripleExceptRightInnerWall,
            Utils.Utils.WallPosition.AllWallCorner => allCornersWall,
            Utils.Utils.WallPosition.Alone => aloneWall,
            _ => aloneWall
        };
#endif
    }
}