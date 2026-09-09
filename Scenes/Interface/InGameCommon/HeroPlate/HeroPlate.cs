using Godot;
using ioi.Game;
using System;

public partial class HeroPlate : PanelContainer
{
    private Texture2D iconTexture;
    private GameEntity entity;
	
	[Export] public TextureRect Icon { get; set; }
	[Export] public RichTextLabel NameEntity { get; set; }
	[Export] public ProgressBar Health { get; set; }
	[Export] public RichTextLabel Resource { get; set; }
    
    public override void _Process(double delta)
	{
		if(entity==null)
			return;
		
		Icon.Texture = iconTexture;
		NameEntity.Text = DrawText.Create(entity.GetName(),Colors.Cyan);
		Health.MaxValue = entity["mhp"].Number;
		Health.Value = entity["hp"].Number;
		Resource.Text = DrawText.Create(
			$"{Global.Strings[entity["res"].String]}: {entity.Func("resstring").String}",
			entity.Color("rescolor"));
	}
	
	public void BindEntity(GameEntity entity)
	{
		this.iconTexture = entity.Texture;
		this.entity = entity;
	}
}
