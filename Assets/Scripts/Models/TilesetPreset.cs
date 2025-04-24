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

        // Autotile mask lookup
        public TileBase[] maskTiles = new TileBase[256];

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Asegurar tamaño
            if (maskTiles == null || maskTiles.Length != 256)
                maskTiles = new TileBase[256];

            for (int mask = 0; mask < 256; mask++)
            {
                // Bits de vecinos
                bool n = (mask & (1 << 0)) != 0;
                bool ne = (mask & (1 << 1)) != 0;
                bool e = (mask & (1 << 2)) != 0;
                bool se = (mask & (1 << 3)) != 0;
                bool s = (mask & (1 << 4)) != 0;
                bool sw = (mask & (1 << 5)) != 0;
                bool w = (mask & (1 << 6)) != 0;
                bool nw = (mask & (1 << 7)) != 0;

                Utils.Utils.WallPosition wp;

                //REGLAS DE ASIGNACION DE PAREDES -> FALSE ES PARED TRUE ES WALL

                if (n && ne && e && !w) wp = Utils.Utils.WallPosition.TopRightInner;
                else if (n && !e && (sw || nw) && w) wp = Utils.Utils.WallPosition.TopLeftInner;
                else if (!n && e && se && s && !w) wp = Utils.Utils.WallPosition.BottomRightInner;
                else if (!n && !e && s && (sw || nw) && w) wp = Utils.Utils.WallPosition.BottomLeftInner;
                else if (!n && e && !s && w) wp = Utils.Utils.WallPosition.Alone;
                else if (!n && !e && !s && w && (sw || nw) && (ne || se))
                    wp = Utils.Utils.WallPosition.TripleExceptLeftInner;
                else if (!n && e && !s && ne && (nw || sw)) wp = Utils.Utils.WallPosition.TripleExceptRightInner;
                else if (!n && !e && !w && ne && nw) wp = Utils.Utils.WallPosition.TripleExceptDown;
                
                else if (!e && s && !w) wp = Utils.Utils.WallPosition.Up;
                else if (n && (ne || nw) && !e && !s && (sw || se)) wp = Utils.Utils.WallPosition.TripleExceptUp;
                else if (n && !e && !w) wp = Utils.Utils.WallPosition.Down;
                else if (!n && !e && !s && !w &&
                         ((nw && ne) || (nw && se) || (nw && sw) || (ne && sw) || (ne && se) || (sw && se)))
                    wp = Utils.Utils.WallPosition.AllWallCorner;
                else if (!s && sw && !w) wp = Utils.Utils.WallPosition.TopRight;
                else if (!n && !s && w) wp = Utils.Utils.WallPosition.Right;
                
                else if (!n && !w && nw) wp = Utils.Utils.WallPosition.BottomRight;
                else if (!n && !e && ne) wp = Utils.Utils.WallPosition.BottomLeft;
                else if (!n && !s && e) wp = Utils.Utils.WallPosition.Left;
                else if (!e && se && !s) wp = Utils.Utils.WallPosition.TopLeft;
                else wp = Utils.Utils.WallPosition.Alone;

                // Asignación final de TileBase
                maskTiles[mask] = wp switch
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
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}