using Godot;
using ioi.Game;
using System;

public partial class StatsContainer : PanelContainer
{
    [Export] RichTextLabel EntityName {get;set;}
    [Export] RichTextLabel Level {get;set;}
    [Export] RichTextLabel Exp {get;set;}
    [Export] RichTextLabel RaceClass {get;set;}
    [Export] RichTextLabel Resource {get;set;}
    [Export] RichTextLabel Damage {get;set;}
    [Export] RichTextLabel AD {get;set;}
    [Export] RichTextLabel ARM {get;set;}
    [Export] RichTextLabel AP {get;set;}
    [Export] RichTextLabel BAR {get;set;}
    [Export] ProgressBar Health {get;set;}
    [Export] RichTextLabel HealthText {get;set;}
	    
	GameEntity entity;
    
    public override void _Ready()
	{
		Global.StatsContainer = this;
	}
	
	public override void _Process(double delta)
	{
		if(entity==null)
			return;
		
		var str = Global.Strings;
		
		EntityName.Text = DrawText.Create(entity.GetName(),Color.FromHtml("#00ffff"));
		Level.Text = DrawText.Create($"{str["LVL_CUT"]}: {entity["level"].Number}",Color.FromHtml("#767676"));
		RaceClass.Text = DrawText.Create($"{str[entity["race"].String]} - {str[entity["class"].String]}",Color.FromHtml("#cccccc"));
		Exp.Text = DrawText.Create($"{str["EXP_CUT"]}: {entity["exp"]}/{entity.Func("mexp").Number}",Color.FromHtml("#767676"));
		
		Health.MaxValue = entity["mhp"].Number;
		Health.Value = entity["hp"].Number;
		HealthText.Text = $"{Health.Value}/{Health.MaxValue}";
		
		Resource.Text = DrawText.Create($"{str[entity["res"].String]}: {entity.Func("resstring").String}",entity.Color("rescolor"));
		Damage.Text = DrawText.Create($"{str["damage"]}: {entity["mindmg"]}-{entity["maxdmg"]}",Color.FromHtml("#c19c00"));
		AD.Text = DrawText.Create($"{str["ad"]}: {entity["ad"]}",Color.FromHtml("#c5101c"));
		AP.Text = DrawText.Create($"{str["ap"]}: {entity["ap"]}",Color.FromHtml("#2c96dd"));
		ARM.Text = DrawText.Create($"{str["def"]}: {entity["def"]}",Color.FromHtml("#13a10e"));
		BAR.Text = DrawText.Create($"{str["mdef"]}: {entity["mdef"]}",Color.FromHtml("#881798"));
	}
	
	public void BindEntity(GameEntity entityFetcher)
	{
		// this.fetcher = entityFetcher;
		entity = entityFetcher;
	}

    public override void _ExitTree()
    {
		entity=null;
		Global.StatsContainer=null;
        base._ExitTree();
    }
    
    protected override void Dispose(bool disposing)
    {
		entity=null;
        base.Dispose(disposing);
    }
}
