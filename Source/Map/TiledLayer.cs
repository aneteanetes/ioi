using System.Collections.Generic;
using System.Diagnostics;

namespace ioi.Tiled.Map
{
    [DebuggerDisplay("{name}")]
    public class TiledLayer : Propertied
    {
        public string name { get; set; }

        public int width { get; set; }

        public int height { get; set; }

        public List<TiledPolygon> Tiles { get; set; } = new List<TiledPolygon>();

        public List<List<TiledPolygon>> TilesArray { get; set; } = new();

        public override void Dispose()
        {
            foreach (var tile in Tiles)
            {
                tile.Dispose();
            }
            Tiles.Clear();
            Tiles = null;

            foreach (var tileArray in TilesArray)
            {
                foreach (var poly in tileArray)
                {
                    poly.Dispose();
                }
                tileArray.Clear();
            }
            TilesArray.Clear();
            TilesArray = null;

            base.Dispose();
        }
    }
}