using System.Collections.Generic;
using System.Drawing;

namespace ioi.Tiled.Map
{
    public class TiledTile : TiledBase
    {
        public int Id { get; set; }

        public List<RectangleF> Bounds { get; set; } = new();

        public List<TiledPolygonObject> Polygons { get; set; } = new();

        public override void Dispose()
        {
            Bounds.Clear();
            Bounds = null;

            foreach (var polo in Polygons)
            {
                polo.Dispose();
            }
            Polygons.Clear();
            Polygons = null;

            base.Dispose();
        }
    }
}
