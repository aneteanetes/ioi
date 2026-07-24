using System.Collections.Generic;
using System.Numerics;

namespace ioi.Tiled.Map
{
    public class TiledBase : Propertied
    {
        public virtual Vector2 Position { get; set; }
        
        public Vector2 Coords { get; set; }

        public virtual Vector2 TitlePosition { get; set; }

        public TiledBase CopyBase()
        {
            return new TiledBase()
            {
                Properties = new Dictionary<string, string>(base.Properties)
            };
        }
    }
}
