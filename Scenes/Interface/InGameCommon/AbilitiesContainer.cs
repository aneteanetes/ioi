using Godot;
using ioi.Game;
using System;

public partial class AbilitiesContainer : HBoxContainer
{
    private GameEntity gameEntity;

    [Export] public TextureButton Ability1 { get; set; }
	[Export] public TextureButton Ability2 { get; set; }
	[Export] public TextureButton Ability3 { get; set; }
	[Export] public TextureButton Ability4 { get; set; }
	
	public override void _Ready()
	{
		Global.AbilitiesContainer = this;
	}
	
	public override void _Process(double delta)
	{
	}
	
	public void BindEntity(GameEntity gameEntity)
	{
		this.gameEntity = gameEntity;
		
		TextureButton[] arr = [this.Ability1,this.Ability2,this.Ability3,this.Ability4];
		var i = 1;
		foreach (var abilBtn in arr)
		{
			var abil = this.gameEntity.GetAbility(i);
            var atlas = new AtlasTexture
            {
                Atlas = GD.Load<Texture2D>(abil["tileset"].String),
                Region = abil.Region("tileset_region")
            };
			abilBtn.Modulate = abil.Color("color");
            Bind(abilBtn,atlas);
			i++;
		}
	}
	
	private void Bind(TextureButton btn, AtlasTexture texture)
	{
		btn.TextureNormal = texture;
	}

    public override void _ExitTree()
    {
		Global.AbilitiesContainer=null;
        base._ExitTree();
    }
}
