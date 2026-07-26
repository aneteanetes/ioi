using System;
using System.Collections.Generic;
using System.Linq;
using Geranium.Reflection;
using Godot;
using ioi.Scripting;
using MoonSharp.Interpreter;

namespace ioi.Game
{
    public class GameEntity
    {
        private LuaScripts _script;
        public Table Data;

        public string Name { get; set; }

        public GameEntitySquad Squad { get; internal set; }
        
        public IEnumerable<GameEntity> Abilities
        {
            get
            {
                var abils = this.Func("getAbilities");
                if (abils.IsNil())
                    return [];

                return abils.Table.Values.Select(x => x.UserData.Object.As<GameEntity>());
            }
        }

        public IEnumerable<string> Components
        {
            get
            {
                var comps = Data.Get("_components");
                if (!comps.IsNil())
                {
                    return comps.Table.Values.Select(x => x.String);
                }

                return [];
            }
        }

        public GameEntity(LuaScripts script,Table initProps=null, params string[] templates)
        {
            _script = script;
            Data = script.CreateTable(initProps, templates);
            Data["Destroy"] = (Action)Destroy;
            Data["entity"] = this;
            Squad = new GameEntitySquad(this);
        }

        /// <summary>
        /// First argument self
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public DynValue Func(string name, params object[] args)
        {
            var func = this[name];

            if (func.IsNil() || func.Type != DataType.Function)
                return DynValue.Nil;

            return _script.Call(func, [Data, .. args]);
        }
        
        public Color Color(string key, Color? set=default)
        {
            if (set.HasValue)
            {
                var color = set.Value;

                var colorTable = new Table(_script.ScriptHost);
                colorTable.Set("r", DynValue.NewNumber(color.R));
                colorTable.Set("g", DynValue.NewNumber(color.G));
                colorTable.Set("b", DynValue.NewNumber(color.B));
                colorTable.Set("a", DynValue.NewNumber(255));

                this[key] = DynValue.NewTable(colorTable);

                return set.Value;
            }
            else
            {
                var value = this[key];
                if (value.IsNil() || value.Type != DataType.Table)
                    return new Color(1,1,1);
                
                var table = value.Table;
                return ColorFromTable(table);
            }
        }
        
        public static Color ColorFromTable(Table table)
        {
            var isrgb = table.Keys.Any(x => x.String == "r");
            if (isrgb)
            {
                var r = Convert.ToByte(table["r"]);
                var g = Convert.ToByte(table["g"]);
                var b = Convert.ToByte(table["b"]);
                var a = Convert.ToByte(table["a"]);

                return new Color(r, g, b, a);
            }
            else
            {
                var r = Convert.ToByte(table[1]);
                var g = Convert.ToByte(table[2]);
                var b = Convert.ToByte(table[3]);
                var a = Convert.ToByte(table[4]);

                if (a == 0)
                    a = 255;

                return new Color(r, g, b, a);
            }
        }

        public static string ColorFromTableToHex(Table table)
        {
            #warning color to hex
            return ColorFromTable(table).ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx">From 1</param>
        /// <returns></returns>
        public GameEntity GetAbility(int idx)
        {
            var abilValue = this.Func("getAbility", idx);

            if (abilValue.IsNotNil())
            {
                return abilValue.UserData.Object.As<GameEntity>();
            }

            return null;
        }

        internal string GetAbilityName(int idx)
        {
            var abilValue = this.Func("getAbility", idx);

            if (abilValue.IsNotNil())
            {
                var entity = abilValue.UserData.Object.As<GameEntity>();
                var name = entity.GetName();// abtable.Get("name").String;
                #warning color to hex string
                var rescolor = this.Color("rescolor").ToString();
                var cost = entity["cost"].Number;

                var costtext = $" /c[{rescolor}][{cost}]";

                if (entity["mode"].String == "passive")
                    costtext = string.Empty;

                return $"{name}{costtext}";
            }

            return "...";
        }
        
        public bool CastAbility(int slot)
        {
            var abilityVal = this.Func("getAbility", slot);
            if (abilityVal.IsNil())
                return false;
            
            var entity = abilityVal.UserData.Object.As<GameEntity>();
            
            #warning .ToHexString()
            var abilcolor = $"/c[{entity.Color("color")}]";
            var abname = entity.GetName();
            
            if (entity["mode"].String == "passive")
            {
                Global.GameLog.Log($"{Global.Strings.Get("passiveab")} {Global.Strings.Get("ability").ToLower()} '{abilcolor}{abname}' /cd{Global.Strings.Get("cantuse")}!");
                return false;
            }
            
            // var enemy = _script.Game.GameState.Enemy;

            // var location = entity["location"].String;
            // if (enemy != null && location != "combat")
            // {
            //     _script.Game.World.LogSystem.Log($"{_script.Game.Strings["Roguelike"]["ability"]} '{abilcolor}{abname}/cd' {_script.Game.Strings["Roguelike"]["cantuseincombat"]}!");
            //     return false;

            // }
            
            // if (enemy == null && location != "world")
            // {
            //     _script.Game.World.LogSystem.Log($"{_script.Game.Strings["Roguelike"]["ability"]} '{abilcolor}{abname}/cd' {_script.Game.Strings["Roguelike"]["cantuseinworld"]}!");
            //     return false;
            // }

            // var canCast = entity.Func("canCast", this, enemy).Boolean;
            // if (canCast)
            // {
            //     entity.Func("cast", this, enemy);
            //     return true;
            // }
            // else
            // {
            //     _script.Game.World.LogSystem.Log($"{this.GetNameColored()} /cd{_script.Game.Strings["Roguelike"]["cantuseabil"]} {abilcolor}{abname}/cd!");
            // }
            
            return false;
        }
        
        public DynValue this[string key]
        {
            get
            {
                return Data.GetSmart(key);
            }
            set => Data.Set(key, value);
        }

        public string GetName()
        {
            if(this.Name.IsNotEmpty())
                return this.Name;

            var nameValue = this["name"];
            if (nameValue.IsNil())
                return "#";

            var nameToken = nameValue.String;

            return Global.Strings.Get(nameToken);
        }

        public string GetNameColored()
        {
            return Func("coloredName").String;
        }

        public void Heal(int heal, GameEntity healer = null)
        {
            this.Func("applyheal", heal, healer, null);
        }
        
        public void Destroy()
        {
            Func("destroy");
            // Node.Destroy
        }

        /// <summary>
        /// Based on negative hp entity gets some condition: unconscious, dead, destroying
        /// </summary>
        internal void Unconscious()
        {
            IsUnconscious = true;
        }

        internal void TakeItems(params GameEntity[] entities)
        {
            foreach (var item in entities)
            {
                TakeItem(item);
            }
        }

        private void TakeItem(GameEntity item)
        {
            var data = UserData.Create(item);
            this["inventory"].Table.Append(data);
            
            // if (this == game.GameState.Player.Entity)
            // {
            //     var str = game.Strings["Roguelike"];
                
            //     var text = DrawText.Create(this.GetNameColored())
            //         .AppendSpace().ResetColor()
            //         .Append(str["getting"])
            //         .AppendSpace()
            //         .Append(item.GetNameColored())
            //         .ResetColor().Append("!");
                
            //     world.LogSystem.Log(text);
            // }
        }

        public GameEntity[] GetEquiped()
        {
            return this["equiped"].Table.Values.Select(v => v.UserData.Object.As<GameEntity>()).ToArray();
        }

        public GameEntity[] GetInventory()
        {
            return this["inventory"].Table.Values.Select(v => v.UserData.Object.As<GameEntity>()).ToArray();
        }

        /// <summary>
        /// Is entity is unconscious, than player can't control it
        /// </summary>
        public bool IsUnconscious { get; set; }
    }
}
