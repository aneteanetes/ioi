using Godot;
using System;

public partial class Mraumir : BaseMap
{
    public override void _Ready()
    {
        base._Ready();
        Global.StatsContainer.BindEntity(() =>
        {
           return Global.GameWorld.Player; 
        });
    }
}
