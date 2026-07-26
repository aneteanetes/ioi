using Geranium.Reflection;
using Godot;
using Ioi.Widgets.Menu;
using System;
using System.Collections.Generic;

public partial class MainGame : Node2D
{
	// NodePath или экспортируемые переменные для быстрого доступа к узлам
	[Export] 
	private NodePath _sceneSlotPath = "GameLayout/GameViewportContainer/GameViewport/SceneSlot";
	

	private Node2D _sceneSlot;
	private Node _currentZone;
	
	[Export] 
	private MainGameUI _uiLayer;
	[Export]
	private PackedScene MainMenuScene;
	public Control MainMenu;
	
	private readonly Stack<Control> _windowStack = new();
	
	private string _mapName;

	public void Init(string mapName)
	{
		_mapName=mapName;
	}
	
	public override void _Ready()
	{	
		if (MainMenuScene != null)
		{
			MainMenu = MainMenuScene.Instantiate<Control>();
			if(MainMenu is MainMenu menuObj)
			{
				menuObj.IsInGame= true;
				menuObj.Back = () =>
				{
					this.ToggleMainMenu();
				};
			}
			_uiLayer.AddChild(MainMenu);
			MainMenu.Visible = false;
		}
		
		_uiLayer.As<MainGameUI>().MainGame = this;
		
		_sceneSlot = GetNode<Node2D>(_sceneSlotPath);
		SwitchZone(_mapName);
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			GetViewport().SetInputAsHandled();
			
			if (_windowStack.Count > 0)
			{
				Control topWindow = _windowStack.Pop();
				topWindow.Visible = false;
				return;
			}
			
			ToggleMainMenu();
		}
	}
	
	public void ToggleMainMenu()
	{
		if (MainMenu == null) 
			return;
		
		bool showMenu = !MainMenu.Visible;
		MainMenu.Visible = showMenu;
		
		GetTree().Paused = showMenu;
		
		if (showMenu)
			_uiLayer.BlurIn();
		else
			_uiLayer.BlurOut();
	}
	
	public void OpenWindow(Control window)
	{
		if (window == null) 
			return;
		
		window.Visible = true;
		
		if (!_windowStack.Contains(window))
		{
			_windowStack.Push(window);
		}
	}

	public void SwitchView(bool isFreeCamera = true)
	{
		
	}
	
	public void SwitchZone(string scenePath)
	{
		if (_currentZone != null)
		{
			_currentZone.QueueFree();
			_currentZone = null;
		}
		
		var packedScene = GD.Load<PackedScene>(scenePath);
		if (packedScene != null)
		{
			_currentZone = packedScene.Instantiate();
			
			_sceneSlot.AddChild(_currentZone);
		}
		else
		{
			GD.PrintErr($"Не удалось загрузить сцену по пути: {scenePath}");
		}
	}
}
