using Godot;
using ioi.Game;
using System;
using System.Collections.Generic;

public partial class PartyContainer : PanelContainer
{
	[Export] public PackedScene HeroSlotScene { get; set; }

    public override void _Ready()
    {
		Global.PartyContainer = this;
        base._Ready();
    }
	
	public void UpdateParty(List<GameEntity> partyMembers)
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
        
        foreach (var member in partyMembers)
        {
            var slot = HeroSlotScene.Instantiate<HeroPlate>();
            slot.BindEntity(member);
			AddChild(slot);
        }
    }
}
