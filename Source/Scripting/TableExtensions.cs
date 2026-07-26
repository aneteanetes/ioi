using ioi.Tiled.Map;
using MoonSharp.Interpreter;

namespace ioi.Scripting
{
    internal static class TableExtensions
    {
        public static Table ToTable(this Propertied propertied, LuaScripts lua)
        {
            var table = new Table(lua.ScriptHost);

            foreach (var propKey in propertied.Properties.Keys)
            {
                table[propKey] = propertied.Properties[propKey];
            }

            return table;
        }
    }
}
