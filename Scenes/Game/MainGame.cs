using Godot;
using System;

public partial class MainGame : Node2D
{
	// NodePath или экспортируемые переменные для быстрого доступа к узлам
	[Export] private NodePath _sceneSlotPath = "GameLayout/GameViewportContainer/GameViewport/SceneSlot";
	private Node2D _sceneSlot;
	private Node _currentZone;
	
	private string _mapName;
	public void Init(string mapName)
	{
		_mapName=mapName;
	}
	
	public override void _Ready()
	{
		_sceneSlot = GetNode<Node2D>(_sceneSlotPath);
		
		// При старте загружаем первую карту
		SwitchZone(_mapName);
	}

	public void SwitchView(bool isFreeCamera = true)
	{
		
	}
	
	public void SwitchZone(string scenePath)
	{
		// 1. Безопасно удаляем старую сцену (карту или бой)
		if (_currentZone != null)
		{
			_currentZone.QueueFree();
			_currentZone = null;
		}
		
		// 2. Загружаем новую сцену (.tscn префаб) с диска
		var packedScene = GD.Load<PackedScene>(scenePath);
		if (packedScene != null)
		{
			_currentZone = packedScene.Instantiate();
			
			// 3. Засовываем её внутрь нашего изолированного Viewport
			_sceneSlot.AddChild(_currentZone);
		}
		else
		{
			GD.PrintErr($"Не удалось загрузить сцену по пути: {scenePath}");
		}
	}
}
