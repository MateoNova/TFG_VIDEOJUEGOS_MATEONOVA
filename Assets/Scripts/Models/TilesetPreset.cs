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

                // 1) Alone
                if (!(n || ne || e || se || s || sw || w || nw))
                {
                    wp = Utils.Utils.WallPosition.Alone;
                }
                // 2) End-caps
                else if (e && !(n || ne || se || s || sw || w || nw))
                    wp = Utils.Utils.WallPosition.Left;
                else if (n && !(nw || ne || e || se || s || sw || w))
                    wp = Utils.Utils.WallPosition.Down;
                else if (s && !(se || sw || w || nw || n || ne || e))
                    wp = Utils.Utils.WallPosition.Up;
                else if (w && !(nw || sw || s || se || e || ne || n))
                    wp = Utils.Utils.WallPosition.Right;

                // 3) External corners prioritarios
                else if (!e && !s && se)
                    wp = Utils.Utils.WallPosition.TopLeft;
                else if (!w && !s && sw)
                    wp = Utils.Utils.WallPosition.TopRight;

                // 4) Straights
                else if (n && s && !(e || w || ne || se || sw || nw))
                    wp = Utils.Utils.WallPosition.Up;
                else if (e && w && !(n || s || ne || se || sw || nw))
                    wp = Utils.Utils.WallPosition.Left;

                // 5) Esquinas externas estándar
                else if (s && w && !(n || e || nw || sw || ne || se))
                    wp = Utils.Utils.WallPosition.TopRight;
                else if (s && e && !(n || w || ne || se || sw || nw))
                    wp = Utils.Utils.WallPosition.TopLeft;
                else if (n && w && !(s || e || nw || sw || ne || se))
                    wp = Utils.Utils.WallPosition.BottomLeft;
                else if (n && e && !(s || w || ne || se || sw || nw))
                    wp = Utils.Utils.WallPosition.BottomRight;

                // 6) Casos especiales añadidos:
                // 6.1 bottomLeftWall: vecinos 8 y 6 deben ser walls (no floor), diagonal 9 floor
                else if (!n && !e && ne)
                    wp = Utils.Utils.WallPosition.BottomLeft;
                // 6.2 bottomRightWall: vecinos 8 y 4 deben ser walls (no floor), diagonal 7 floor
                else if (!n && !w && nw)
                    wp = Utils.Utils.WallPosition.BottomRight;

                // 7) Inner corners
                else if (n && e && w && !s)
                    wp = Utils.Utils.WallPosition.BottomLeftInner;
                else if (n && e && s && !w)
                    wp = Utils.Utils.WallPosition.TopLeftInner;
                else if (s && e && w && !n)
                    wp = Utils.Utils.WallPosition.TopRightInner;
                else if (n && s && w && !e)
                    wp = Utils.Utils.WallPosition.BottomRightInner;

                // 8) T-junctions
                else if (e && w && s && !n)
                    wp = Utils.Utils.WallPosition.TripleExceptUp;
                else if (e && w && n && !s)
                    wp = Utils.Utils.WallPosition.TripleExceptDown;
                else if (n && s && w && !e)
                    wp = Utils.Utils.WallPosition.TripleExceptRight;
                else if (n && s && e && !w)
                    wp = Utils.Utils.WallPosition.TripleExceptLeft;

                // 9) Triple-inner específicos
                else if (w && s && !n && !nw && !e && !ne && !se)
                    wp = Utils.Utils.WallPosition.TopRightInner;
                else if (e && s && !n && !ne && !w && !nw && !sw)
                    wp = Utils.Utils.WallPosition.TopLeftInner;

                // 10) Cross completo
                else if (n && ne && e && se && s && sw && w && nw)
                    wp = Utils.Utils.WallPosition.AllWallCorner;

                // 11) Fallback a cardinales
                else if (n) wp = Utils.Utils.WallPosition.Down;
                else if (s) wp = Utils.Utils.WallPosition.Up;
                else if (e) wp = Utils.Utils.WallPosition.Left;
                else if (w) wp = Utils.Utils.WallPosition.Right;
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