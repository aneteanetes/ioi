using Godot;
using ioi.Game;
using MoonSharp.Interpreter;
using System;
using System.IO;
using System.Reflection;
using Script = MoonSharp.Interpreter.Script;

namespace ioi.Scripting
{
    public class LuaScripts
    {
        public Table LuaMeta { get; }

        public Script ScriptHost;

#if DEBUG
        private FileSystemWatcher _watcher;
        private bool _needsReload;
        private string _changedFile;
#endif

        public Table Globals => ScriptHost.Globals;

        internal LuaScripts()
        {            
            ScriptHost = new Script();
            Script.DefaultOptions.DebugPrint = s => GD.Print(s);

            UserData.RegisterAssembly(Assembly.GetExecutingAssembly());
            UserData.RegisterType<GameEntity>();
            UserData.RegisterType<Random>();
            
            Table mathTable = Globals.Get("math").Table;
            mathTable["clamp"] = (Func<double, double, double, double>)Math.Clamp;
            
            Func<string, string> localizationfunc = str => Global.Strings.Get(str);
            Globals["loco"] = localizationfunc;
            //Globals["toHexString"] = (Func<Table, string>)GameEntity.ColorFromTableToHex;
            
            LuaMeta = new Table(ScriptHost);
            LuaMeta["__index"] = (Func<Table, string, DynValue>)((t, k) => t.GetSmart(k));
            ScriptHost.Globals.Set("LuaMeta", DynValue.NewTable(LuaMeta));

        }

        public void Init()
        {
            UserData.RegisterType<GameEntity>();

            if (Global.IsDebug)
            {
#if DEBUG
                var scriptsPath = Path.Combine(Global.PathToProject, "Scripts");

                _watcher = new FileSystemWatcher(scriptsPath, "*.lua")
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.Attributes |
                       NotifyFilters.CreationTime |
                       NotifyFilters.FileName |
                       NotifyFilters.LastAccess |
                       NotifyFilters.LastWrite |
                       NotifyFilters.Size |
                       NotifyFilters.Security,
                    EnableRaisingEvents = true
                };

                // Это событие сработает в другом потоке!
                _watcher.Changed += (s, e) =>
                {
                    _needsReload = true;
                    _changedFile = e.FullPath;
                };

                var files = Directory.GetFiles(scriptsPath, "*.lua", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    LoadFile(file);
                }
#endif
            }
            else
            {
                #warning load all scripts
                // var scripts = Game.Content.LoadResourcePack("Data/Scprits");
                // foreach (var script in scripts)
                // {
                //     ScriptHost.DoStream(script.Stream);
                // }
            }
        }

        public DynValue Call(DynValue function, params object[] args)
        {
            try
            {
                return ScriptHost.Call(function, args);
            }
            catch (InterpreterException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Lua: {ex.DecoratedMessage ?? ex.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Lua unhandled: {ex}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            return DynValue.Nil;
        }

        public DynValue Execute(string scriptText)
        {
            return ScriptHost.DoString(scriptText);
        }
        public Table MergeTables(Table t1, Table t2)
        {
            var func = ScriptHost.Globals.Get("Core").Table.Get("mergeTables");
            return ScriptHost.Call(func, t1, t2).Table;
        }

        public Table CreateTable(Table initProps, params string[] templates)
        {
            var table = new Table(ScriptHost);

            Table sources = new Table(ScriptHost);
            foreach (var template in templates)
            {
                sources.Append(DynValue.NewString(template));
            }

            table["_components"] = sources;
            table.MetaTable = LuaMeta;

            table.Init(initProps);

            return table;
        }
        
        #warning replace to _Ready
//         public void Update(GameTime gameTime)
//         {
// #if DEBUG
//             if (_needsReload)
//             {
//                 Console.WriteLine($"Updated Lua script: {_changedFile}");
//                 LoadFile(_changedFile);
//                 _needsReload = false;
//             }
// #endif
//         }

#if DEBUG
        private void LoadFile(string file)
        {
            try
            {
                ScriptHost.DoFile(file);
            }
            catch (InterpreterException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine($"Lua: {ex.DecoratedMessage}");

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
#endif
    }
}