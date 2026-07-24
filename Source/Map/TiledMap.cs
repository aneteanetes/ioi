using Geranium.Reflection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;

namespace ioi.Tiled.Map
{
    public class TiledMap : TiledBase
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        private TiledMap() { }

        public static TiledMap Load(Stream stream, string assetName)
        {
            var xdoc = XDocument.Load(stream);
            var map = xdoc.Root;


            var tiledMap = new TiledMap
            {
                width = map.GetTagAttrInteger(nameof(width)),
                height = map.GetTagAttrInteger(nameof(height)),
                tilewidth = map.GetTagAttrInteger(nameof(tilewidth)),
                tileheight = map.GetTagAttrInteger(nameof(tileheight)),
                infinite = map.GetTagAttrInteger(nameof(infinite)) == 1 ? true : false
            };
            
            ReadProperties(map, tiledMap);

            foreach (var xmlTileSet in map.Elements("tileset"))
            {
                var tileSet = new TiledTileset()
                {
                    firstgid = xmlTileSet.GetTagAttrInteger(nameof(TiledTileset.firstgid)),
                    tilecount = xmlTileSet.GetTagAttrInteger(nameof(TiledTileset.tilecount)),
                    tilewidth = xmlTileSet.GetTagAttrInteger(nameof(TiledTileset.tilewidth)),
                    tileheight = xmlTileSet.GetTagAttrInteger(nameof(TiledTileset.tileheight)),
                    name = xmlTileSet.GetTagAttrString(nameof(TiledTileset.name)),
                    spacing = xmlTileSet.GetTagAttrInteger(nameof(TiledTileset.spacing)),
                };

                tileSet.Tiles = xmlTileSet
                    .Elements("tile")
                    .Select(x =>
                    {
                        var tile = new TiledTile()
                        {
                            Id = x.GetTagAttrInteger("id")
                        };

                        BindProperties(x, tile);

                        var shapes = x.Element("objectgroup");
                        if (shapes != null)
                        {
                            foreach (var obj in shapes.Elements("object"))
                            {
                                var polygonTag = obj.Element("polygon");
                                if (polygonTag == null)
                                {
                                    tile.Bounds.Add(new RectangleF(
                                        obj.GetTagAttrFloatRound("x"),
                                        obj.GetTagAttrFloatRound("y"),
                                        obj.GetTagAttrFloatRound("width"),
                                        obj.GetTagAttrFloatRound("height")
                                        ));
                                }
                                else
                                {
                                    var polygonsString = polygonTag.GetTagAttrString("points");
                                    var polygonList = polygonsString
                                        .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => x.AsVector2())
                                        .ToArray();
                                    
                                    var pos = new Vector2(obj.GetTagAttrFloatRound("x"), obj.GetTagAttrFloatRound("y"));

                                    var polygon = new TiledPolygonObject(polygonList,pos);
                                    BindProperties(obj,polygon);

                                    tile.Polygons.Add(polygon);
                                }
                            }
                        }

                        return tile;
                    })
                    .ToList();

                tileSet.Autotiled = true;

                var source = xmlTileSet.Element("image").GetTagAttrString("source");
                

                tiledMap.Tilesets.Add(tileSet);
            }

            ProcessLayers(map, tiledMap);
            ProcessObjects(map, tiledMap);

            return tiledMap;
        }

        private static void BindProperties(XElement x, TiledBase tiledBase)
        {
            var isPropertiesExists = x.Element("properties");
            if (isPropertiesExists != null)
            {
                foreach (var prop in x.Element("properties").Elements("property"))
                {
                    tiledBase.Properties[prop.GetTagAttrString("name")] = prop.GetTagAttrString("value");
                }
            }
        }

        private static string ResourceFile(string file)
        {
            return file.Replace("../", "");
        }

        private static void ProcessObjects(XElement map, TiledMap tiledMap)
        {
            var objProps = typeof(TiledObject)
                .GetProperties()
                .Where(p => char.IsLower(p.Name[0]))
                .ToArray();

            var tiledObjProps = typeof(TiledObjectProperty)
                .GetProperties()
                .Where(p => char.IsLower(p.Name[0]))
                .ToArray();
            
            foreach (var objectlayer in map.Elements("objectgroup"))
            {
                foreach (var objtag in objectlayer.Elements())
                {
                    var tobj = new TiledObject()
                    {
                        objectgroup = objectlayer.Attribute("name").Value
                    };
                    
                    foreach (var prop in objProps)
                    {
                        var attr = objtag.Attribute(prop.Name);
                        if (attr != null)
                        {
                            var value = attr.Value.Replace(".", ",");
                            var type = tobj.GetPropValue(prop.Name).GetType();
                            var typeCode = Type.GetTypeCode(type);

                            if (value.Contains(",") && typeCode.IsInteger())
                            {
                                tobj.SetPropValue(prop.Name, Math.Round(double.Parse(value)));
                            }
                            else
                            {
                                tobj.SetPropValue(prop.Name, value);
                            }
                        }
                    }

                    tobj.Coords = new Vector2((float)(tobj.x/tobj.width), (float)(tobj.y/tobj.height) - 1);

                    tobj.Position = new Vector2((int)tobj.x, (int)tobj.y - tobj.height);
                    ReadProperties(objtag, tobj);

                    if (tobj.gid != 0)
                    {
                        tobj.Tileset = tiledMap.GetTileset(tobj.gid);

                        var localid = tobj.Tileset.GetAtlasId(tobj.gid);

                        tobj.Tileset.GetTileProperties(localid).ForEach(prop =>
                        {
                            if (!tobj.Properties.ContainsKey(prop.Key))
                                tobj.Properties.Add(prop.Key, prop.Value);

                            // overriding property value
                            tobj.Properties[prop.Key] = prop.Value;
                        });
                    }

                    var xmlPolygon = objtag.Element("polygon");
                    if (xmlPolygon != null)
                    {
                        var xmlPoints = xmlPolygon.Attribute("points");
                        if (xmlPoints != null)
                        {
                            var pointsString = xmlPoints.Value;
                            tobj.Polygon = pointsString.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                .Select(pointstr =>
                                {
                                    var p = pointstr.Split(",", StringSplitOptions.RemoveEmptyEntries);
                                    return new Point(int.Parse(p[0]), int.Parse(p[1]));
                                }).ToArray();
                        }
                    }

                    if (tobj.objectgroup == "NPC")
                    {
                        tiledMap.NPCs.Add(tobj);
                    }
                    else if (tobj.objectgroup == "Creatures")
                    {
                        tiledMap.Creatures.Add(tobj);
                    }
                    else
                        tiledMap.Objects.Add(tobj);
                }
            }
        }

        private static void ReadProperties(XElement rootWithPropsTag, TiledBase tobj)
        {
            var props = rootWithPropsTag.Element("properties");
            if (props != default)
            {
                foreach (var xmlprop in props.Elements())
                {
                    tobj.Properties[xmlprop.GetTagAttrString("name")] = xmlprop.GetTagAttrString("value");
                }
            }
        }

        private static void ProcessLayers(XElement map, TiledMap tiledMap)
        {
            foreach (var xmlLayer in map.Elements("layer"))
            {
                var layer = new TiledLayer()
                {
                    name = xmlLayer.GetTagAttrString(nameof(TiledLayer.name)),
                    height = xmlLayer.GetTagAttrInteger(nameof(TiledLayer.height)),
                    width = xmlLayer.GetTagAttrInteger(nameof(TiledLayer.width))
                };

                var dataTag = xmlLayer.Element("data");

                var gids = new List<uint>();

                var chunks = dataTag.Elements("chunk");
                if (chunks.Count() > 0)
                {
                    chunks.ForEach(chunk =>
                    {
                        var gidsChunk = chunk.Value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => uint.Parse(x));
                        gids.AddRange(gidsChunk);
                    });
                }

                if (gids.Count == 0)
                {
                    gids = dataTag.Value
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => uint.Parse(x))
                        .ToList();
                }

                var iX = 0;
                var iY = 0;
                foreach (var gidHASHED in gids)
                {
                    // for saving position
                    //if (gidHASHED == 0)
                    //    continue;

                    // Read out the flags
                    bool flipped_horizontally = (gidHASHED & FLIPPED_HORIZONTALLY_FLAG) != 0;
                    bool flipped_vertically = (gidHASHED & FLIPPED_VERTICALLY_FLAG) != 0;
                    bool flipped_diagonally = (gidHASHED & FLIPPED_DIAGONALLY_FLAG) != 0;

                    var gid = gidHASHED & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);

                    var coords = tiledMap.ParseCoordinates((int)gid);

                    var polygon = new TiledPolygon((int)gid)
                    {
                        FileName = tiledMap.TileFileNameByGid(gid),
                        FlippedDiagonally = flipped_diagonally,
                        FlippedHorizontally = flipped_horizontally,
                        FlippedVertically = flipped_vertically,
                        Layer = layer,
                        TileOffsetX = coords.X,
                        TileOffsetY = coords.Y,
                        Position = new Vector2(iX*16, iY*16),
                        Tileset = gid == 0 ? null : tiledMap.GetTileset((int)gid),
                        Coords = new Vector2(iX,iY)
                    };

                    layer.Tiles.Add(polygon);

                    if (gid != 0)
                    {
                        polygon.Properties = polygon.Tileset.GetTileProperties(polygon.Tileset.GetAtlasId(polygon.Gid));
                        polygon.Position = new Vector2(iX * polygon.Tileset.tilewidth, iY * polygon.Tileset.tileheight);
                    }

                    iX++;
                    if (iX > layer.width - 1)
                    {
                        iX = 0;
                        iY++;
                    }

                }

                layer.TilesArray = layer.Tiles.GroupBy(x => x.Position.Y)
                    .Select(x => new List<TiledPolygon>(x.OrderBy(x => x.Position.X)))
                    .ToList();

                tiledMap.Layers.Add(layer);
            }
        }

        public bool infinite { get; set; }

        public int width { get; set; }

        public int height { get; set; }

        public int tilewidth { get; set; }

        public int tileheight { get; set; }

        public List<TiledObject> Objects { get; set; } = new List<TiledObject>();

        public List<TiledObject> NPCs { get; set; } = new List<TiledObject>();

        public List<TiledObject> Creatures { get; set; } = new List<TiledObject>();

        public List<TiledTileset> Tilesets { get; set; } = new List<TiledTileset>();

        private Point ParseCoordinates(int gid)
        {
            var tileset = Tilesets.FirstOrDefault(x => x.IsContainsGid(gid));
            if (tileset != null)
                return tileset.Coords(gid);

            return Point.Empty;
        }

        private TiledTileset GetTileset(int gid)
        {
            var tileset = Tilesets.FirstOrDefault(x => x.IsContainsGid(gid));
            if (tileset != null)
                return tileset;

            throw new FileNotFoundException("Gid from unknown tileset!");
        }

        private string TileFileNameByGid(uint gid)
        {
            if (gid == 0)
                return null;

            var tileset = Tilesets.FirstOrDefault(x => x.TileGids.Contains(gid));

            if (tileset.Autotiled)
                return tileset.name;


            return null;
        }

        public List<TiledLayer> Layers { get; set; } = new List<TiledLayer>();

        public override void Dispose()
        {
            foreach (var obj in Objects)
            {
                obj.Dispose();
            }
            foreach (var npc in NPCs)
            {
                npc.Dispose();
            }
            foreach (var creature in Creatures)
            {
                creature.Dispose();
            }
            foreach (var tileset in Tilesets)
            {
                tileset.Dispose();
            }
            foreach (var layer in Layers)
            {
                layer.Dispose();
            }

            Layers.Clear();
            Layers = null;

            Objects.Clear();
            Objects = null;

            NPCs.Clear();
            NPCs = null;

            Creatures.Clear();
            Creatures= null;

            Tilesets.Clear();
            Tilesets = null;

            base.Dispose();
        }
    }
}