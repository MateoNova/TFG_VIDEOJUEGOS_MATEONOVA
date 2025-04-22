using System.Collections.Generic;
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
    }
}