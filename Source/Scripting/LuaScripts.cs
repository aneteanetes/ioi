using Godot;
using ioi.Game;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            
            // when part unknown - create table
            Table globalsMeta = new Table(ScriptHost);
            globalsMeta["__newindex"] = (Action<Table, string, DynValue>)((g, key, value) =>
            {
                g.Set(key, value);
            });
            ScriptHost.Globals.MetaTable = globalsMeta;
        }
        
        public void Init()
        {            
            if (OS.HasFeature("editor"))
            {
#if DEBUG
                var scriptsPath = ProjectSettings.GlobalizePath("res://Scripts");
                
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
                
                _watcher.Changed += (s, e) =>
                {
                    _needsReload = true;
                    _changedFile = e.FullPath;
                };
                                
                if (Directory.Exists(scriptsPath))
                {
                    var files = Directory.GetFiles(scriptsPath, "*.lua", SearchOption.AllDirectories);
                    var ordered = files.OrderBy(f=>f).ToArray();
                    foreach (var file in ordered)
                    {
                        LoadFile(file);
                    }
                }
                else
                {
                    GD.PrintErr($"Debug папка со скриптами не найдена по пути: {scriptsPath}");
                }
#endif
            }
            else
            {
                var scriptPaths = CollectScriptsFromResources("res://Scripts");
                
                foreach (var fullPath in scriptPaths)
                {
                    using var file = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Read);
                    if (file != null)
                    {
                        string scriptText = file.GetAsText();
                        ScriptHost.DoString(scriptText, null, fullPath);
                    }
                    else
                    {
                        GD.PrintErr($"Не удалось прочитать файл из ресурсов: {fullPath}");
                    }
                }
            }
        }
        
        private List<string> CollectScriptsFromResources(string folderPath, List<string> pathList=null)
        {
            if(pathList==default)
                pathList = new();
            
            using var dir = DirAccess.Open(folderPath);
            if (dir == null) 
                return null;
            
            dir.ListDirBegin();
            string fileName = dir.GetNext();

            while (fileName != "")
            {
                string fullPath = $"{folderPath}/{fileName}";

                if (dir.CurrentIsDir())
                {
                    if (fileName != "." && fileName != "..")
                    {
                        CollectScriptsFromResources(fullPath, pathList);
                    }
                }
                else if (fileName.EndsWith(".lua") || fileName.EndsWith(".lua.remap"))
                {
                    if (fullPath.EndsWith(".remap"))
                    {
                        fullPath = fullPath.Replace(".remap", "");
                    }
                    
                    // Просто добавляем путь в список для последующей сортировки
                    pathList.Add(fullPath);
                }
                
                fileName = dir.GetNext();
            }
            
            pathList.Sort();
            return pathList;
        }

        public DynValue Call(DynValue function, params object[] args)
        {
            try
            {
                return ScriptHost.Call(function, args);
            }
            catch (InterpreterException ex)
            {
                GD.PrintErr($"Lua: {ex.DecoratedMessage ?? ex.Message}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Lua unhandled: {ex}");
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
            
            Table sources = new(ScriptHost);
            foreach (var template in templates)
            {
                sources.Append(DynValue.NewString(template));
            }
            
            table["_components"] = sources;
            table.MetaTable = LuaMeta;
            
            table.Init(initProps);

            return table;
        }
        
        public void Update(double delta)
        {
#if DEBUG
            if (_needsReload)
            {
                GD.Print($"Updated Lua script: {_changedFile}");
                LoadFile(_changedFile);
                _needsReload = false;
            }
#endif
        }

#if DEBUG
        private void LoadFile(string file)
        {
            try
            {
                ScriptHost.DoFile(file);
            }
            catch (InterpreterException ex)
            {
                GD.PrintErr($"Lua: {ex.DecoratedMessage}");
            }
        }
#endif
    }
}