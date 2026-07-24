namespace ioi.Tiled.Map
{
    public class TiledPolygon : TiledBase
    {
        public TiledPolygon(int gid) => Gid = gid;

        public int Gid { get; set; }
        
        public string FileName { get; set; }

        public bool FlippedHorizontally { get; set; }

        public bool FlippedVertically { get; set; }

        public bool FlippedDiagonally { get; set; }

        public int TileOffsetX { get; set; }

        public int TileOffsetY { get; set; }

        public TiledLayer Layer { get; set; }

        public TiledTileset Tileset { get; set; }
    }
}
