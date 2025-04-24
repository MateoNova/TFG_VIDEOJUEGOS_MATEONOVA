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
        //[Header("Renamed Tiles")] public Tile[] tiles;

        [Header("Floor tiles")] [SerializeField]
        public List<TileBase> walkableTileBases = new();

        [SerializeField] public List<int> walkableTilesPriorities = new();
        [SerializeField] public bool randomWalkableTilesPlacement;


        // Tilemap for wall tiles.

        // Various wall tile types categorized by their positions.
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
                var n = GetNeighborBits(mask);
                var wp = DetermineWallPosition(n);
                maskTiles[mask] = GetTileForPosition(wp);
            }

            EditorUtility.SetDirty(this);
        }

        private void EnsureMaskSize()
        {
            if (maskTiles == null || maskTiles.Length != 256)
                maskTiles = new TileBase[256];
        }

        // Extract neighbor bits from mask integer
        private static bool[] GetNeighborBits(int mask)
        {
            // order: N, NE, E, SE, S, SW, W, NW
            var bits = new bool[8];
            for (var i = 0; i < 8; i++)
                bits[i] = (mask & (1 << i)) != 0;
            return bits;
        }

        private Utils.Utils.WallPosition DetermineWallPosition(bool[] n)
        {
            // shorthand for readability
            bool N = n[0], NE = n[1], E = n[2], SE = n[3];
            bool S = n[4], SW = n[5], W = n[6], NW = n[7];

            // 1) Inner corners
            if (N && NE && E && !W) return Utils.Utils.WallPosition.TopRightInner;
            if (N && !E && (SW || NW) && W) return Utils.Utils.WallPosition.TopLeftInner;
            if (!N && E && SE && S && !W) return Utils.Utils.WallPosition.BottomRightInner;
            if (!N && !E && S && (SW || NW) && W) return Utils.Utils.WallPosition.BottomLeftInner;

            // 2) Alone
            if (!N && E && !S && W) return Utils.Utils.WallPosition.Alone;

            // 3) Triple inner corners
            if (!N && !E && !S && W && (SW || NW) && (NE || SE))
                return Utils.Utils.WallPosition.TripleExceptLeftInner;
            if (!N && E && !S && (NE || SE) && (NW || SW))
                return Utils.Utils.WallPosition.TripleExceptRightInner;

            // 4) Triple walls
            if (!N && !E && !W && NE && NW) return Utils.Utils.WallPosition.TripleExceptDown;
            if (N && (NE || NW) && !E && !S && (SW || SE))
                return Utils.Utils.WallPosition.TripleExceptUp;

            // 5) Straight walls
            if (!E && S && !W) return Utils.Utils.WallPosition.Up;
            if (N && !E && !W) return Utils.Utils.WallPosition.Down;
            if (!N && !S && W) return Utils.Utils.WallPosition.Right;
            if (!N && !S && E) return Utils.Utils.WallPosition.Left;

            // 6) All-corners
            if (!N && !E && !S && !W &&
                ((NW && NE) || (NW && SE) || (NW && SW && (NE || SE)) ||
                 (NE && SW) || (NE && SE && (NW || SW)) || (SW && SE)))
                return Utils.Utils.WallPosition.AllWallCorner;

            // 7) Triple walls (sides)
            if (!N && !E && !S && NE && SE) return Utils.Utils.WallPosition.TripleExceptLeft;
            if (!N && !W && !S && NW && SW) return Utils.Utils.WallPosition.TripleExceptRight;

            // 8) External corners
            if (!S && SW && !W) return Utils.Utils.WallPosition.TopRight;
            if (!E && SE && !S) return Utils.Utils.WallPosition.TopLeft;
            if (!N && !W && NW) return Utils.Utils.WallPosition.BottomRight;
            if (!N && !E && NE) return Utils.Utils.WallPosition.BottomLeft;

            // default
            return Utils.Utils.WallPosition.Alone;
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