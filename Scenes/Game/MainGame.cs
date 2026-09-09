using Geranium.Reflection;
using Godot;
using ioi.Source.Scenes;
using Ioi.Widgets.Menu;
using System;
using System.Collections.Generic;

public partial class MainGame : Node2D
{
	// NodePath или экспортируемые переменные для быстрого доступа к узлам
	[Export] 
	private NodePath _sceneSlotPath = "GameLayout/SceneSlot";
	

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

	[Export] public float ZoomSpeed { get; set; } = 0.3f;
	
	private bool _isDragging = false;
	
	private void ProcessMouse(InputEventMouseButton mouseButton, Camera2D currentCamera)
	{
		if (mouseButton.ButtonIndex== MouseButton.Right && !mouseButton.Pressed)
			_isDragging = false;
		
		if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
			{
				if (_isDragging)
					return;
				
				var slotScene = _sceneSlot.GetChild<BaseMap>(0);
				
				if(slotScene is IViewportScene viewportScene)
				{
					viewportScene.ProcessClick(GetGlobalMousePosition(),GetLocalMousePosition());
				}
			}
		
		ProcessCamera(@mouseButton,currentCamera);
	}
	
	private void ProcessCamera(InputEventMouseButton mouseButton, Camera2D currentCamera)
	{
		if (currentCamera is not IGameCamera gameCamera) 
			return;		
			
		if (mouseButton.Pressed)
		{
			if (gameCamera.CanZoom)
			{
				if (mouseButton.ButtonIndex == MouseButton.WheelUp)
				{
					AdjustZoom(currentCamera, ZoomSpeed, gameCamera.MinZoom, gameCamera.MaxZoom);
				}
				else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
				{
					AdjustZoom(currentCamera, -ZoomSpeed, gameCamera.MinZoom, gameCamera.MaxZoom);
				}
			}
			
			if (mouseButton.ButtonIndex == MouseButton.Right && gameCamera.CanDrag)
			{
				Global.ResetCameraMove=true;
				_isDragging = !_isDragging;
			}
		}
	}

	private void AdjustZoom(Camera2D camera, float delta, float min, float max)
	{
		float newZoomX = Mathf.Clamp(camera.Zoom.X + delta, min, max);
		camera.Zoom = new Vector2(newZoomX, newZoomX);
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

		Camera2D currentCamera = _sceneSlot.GetChild<BaseMap>(0).GetNode<Camera2D>("Camera2D");
		
		if(@event is InputEventMouseButton mouseButton)
			ProcessMouse(mouseButton, currentCamera);
				
		if (@event is InputEventMouseMotion mouseMotion && _isDragging)
			currentCamera.Offset -= mouseMotion.Relative / currentCamera.Zoom;
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
