using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace ioi.Tiled.Map
{
    [DebuggerDisplay("{name}")]
    public class TiledTileset : Propertied
    {
        public string name { get; set; }
        
        public int tilewidth { get; set; }
        
        public int tileheight { get; set; }
        
        public int firstgid { get; set; }
        
        public int tilecount { get; set; }
        
        public string image { get; set; }
        
        public int spacing { get; set; }
        
        /// <summary>
        /// Тайлы получают свой id по рассчёту ширины, высоты и размера
        /// </summary>
        public bool Autotiled { get; set; }
        
        public Point Coords(int gid)
        {
            if (gid == 0)
                return Point.Empty;
            
            if (Autotiled)
            {
                if (gid % tileheight == 0)
                {
                    // x == 0
                    // y == gid/32
                    return new Point(0, gid / tileheight);
                }
                var y = gid / tileheight;

                var integerGid = y * tileheight;

                var x = gid - integerGid - 1;

                return new Point(x, y);
            }
            return Point.Empty;
        }
        
        public int TileIndexFrom => firstgid;

        public int TiledIndexTo => firstgid + tilecount-1;

        private uint[] _tileids;
        public uint[] TileGids
        {
            get
            {
                if (_tileids == null)
                {
                    if (Autotiled)
                    {
                        _tileids = Enumerable.Range(1, 2336)
                            .Select(x => Convert.ToUInt32(x))
                            .ToArray();
                    }
                    else
                    {
                        _tileids = Tiles
                            .Select(x => firstgid + x.Id)
                            .Select(x => Convert.ToUInt32(x))
                            .ToArray();
                    }
                }

                return _tileids;
            }
        }

        public bool IsContainsGid(int gid)
        {
            return gid >= TileIndexFrom && gid <= TiledIndexTo;
        }

        private List<int> _tileIndexes;
        
        public int GetAtlasId(int gid)
        {
            if (_tileIndexes == null)
                _tileIndexes = Enumerable.Range(firstgid, tilecount).ToList();
            return _tileIndexes.IndexOf(gid);
        }
        
        public Dictionary<string,string> GetTileProperties(int gid)
        {
            var tileInfo = Tiles.FirstOrDefault(x=>x.Id == gid);
            return tileInfo == null ? ([]) : tileInfo.Properties;
        }
        
        public List<TiledTile> Tiles { get; set; } = new();

        public override void Dispose()
        {
            foreach (var tile in Tiles)
            {
                tile.Dispose();
            }
            Tiles.Clear();
            Tiles = null;
        }
    }
}