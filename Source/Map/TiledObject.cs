#pragma warning disable IDE1006 // Naming Styles


using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ioi.Tiled.Map
{
    public class TiledObject : TiledBase
    {
        public int id { get; set; }

        public int gid { get; set; }
        
        public string file { get; set; }

        public double x { get; set; }

        public double y { get; set; }

        public int width { get; set; }

        public int height { get; set; }

        public bool collide => Properties["collide"] == "true";

        public string objectgroup { get; set; }

        public bool IsHaveBounds()
        {
            var tile = GetTile();

            if(tile == null) 
                return false;

            return tile.Bounds.Count != 0;
        }

        public bool IsHavePolygons()
        {
            var tile = GetTile();

            if (tile == null)
                return false;

            return tile.Polygons.Count != 0;
        }

        public List<RectangleF> GetBounds()
        {
            if (!IsHaveBounds())
                return [];

            var tile = GetTile();
            return tile.Bounds;
        }
        
        public List<TiledPolygonObject> GetPolygons()
        {
            if (!IsHavePolygons())
                return [];
            
            var tile = GetTile();
            return tile.Polygons;
        }

        public TiledTile GetTile() => Tileset?.Tiles?.FirstOrDefault(x => x.Id == this.gid-1);

        public Point[] Polygon { get; set; } = new Point[0];

        public TiledTileset Tileset { get; set; }

        public override void Dispose()
        {
            Tileset = null;
            base.Dispose();
        }
    }
    
    public class TiledObjectProperty
    {
        public string name { get; set; }

        public string value { get; set; }
    }
}
#pragma warning restore IDE1006 // Naming Styles