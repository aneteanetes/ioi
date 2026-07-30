using System;
using MoonSharp.Interpreter;

namespace ioi.Systems.Roguelike
{
    [MoonSharpUserData]
    public class ItemRandomSystem
    {
        public uint GetSeed()
        {
           return Global.Random.Randi();
        }
        
        public Random GetGenerator(double seedFromLua)
        {
            int seed = (int)Math.Round(seedFromLua);
            return new Random(seed);
        }
    }
}