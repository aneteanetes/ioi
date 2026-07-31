using Godot;
using ioi.Game;
using ioi.Scripting;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class SpawnSystem
{    
    public LuaScripts Lua => Global.LuaScripts;
        
    public GameEntity SpawnCharacter(string name, string race, string @class)
    {
        var entity = SpawnEntity(
            "Templates.Base.Object",
            "Templates.Base.Player",
            $"Templates.Races.{race}", 
            $"Templates.Classes.{@class}");

        entity.Name = name;
        entity["namevalue"] = DynValue.NewString(entity.Name);
        
        return entity;
    }
    
    public GameEntity SpawnObject(string id, string type, Table props)
        => SpawnEntity(props, "Templates.Base.Object", $"Templates.{type}.{id}");
    
    public GameEntity SpawnEntity(params string[] prototypes)
        => SpawnEntity(null,prototypes);
    
    public GameEntity SpawnLootTable(string name)
        => SpawnEntity("Templates.loot.table", $"Templates.loot.table.{name}");

    public GameEntity SpawnEntity(Table props, params string[] prototypes)
    {
        var entity = new GameEntity(Global.LuaScripts, props, prototypes);
        entity["seed"] = DynValue.NewNumber(Global.ItemRandomSystem.GetSeed());
        
        #warning spawn entity autorefresh + applyheal - or not?
        entity.Func("refresh");
        entity.Func("applyheal",entity["mhp"]);
        
        return entity;
    }
    
    // public ObjectMap SpawnObjectMap(string type, string id, Table props, int x, int y, string tileset, int tileId)
    // {
    //     var entity = SpawnObject(id, type, props);
        
    //     var obj = new ObjectMap(Game, $"{type}.{id}.{Guid.NewGuid().ToString().Substring(0,5)}")
    //     {
    //         Sprite = Game.GameState.Map.Tilesets[tileset].CreateSprite(tileId),
    //         Color = entity.Color("color"),
    //         IsBounds = entity["isBounds"].Boolean,
    //         Coords = new Point(x,y),
    //         Size = Game.CellSize.ToVector2()
    //     };
        
    //     obj.Position = obj.GetPositionFromCoords();
    //     obj.BindEntity(entity);

    //     Game.GameState.Map.Add(obj);
        
    //     return obj;
    // }
}