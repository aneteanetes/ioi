using Godot;
using System;

public partial class GameViewportContainer : SubViewportContainer
{ 
	[Export] public float ZoomSpeed { get; set; } = 0.3f;

	private bool _isHovered = false;
	private bool _isDragging = false;
	private SubViewport _subViewport;
	
	public override void _Ready()
	{
		_subViewport = GetNode<SubViewport>("GameViewport");

		MouseEntered += () => _isHovered = true;
		MouseExited += () => _isHovered = false;
	}
	
	public override void _Input(InputEvent @event)
	{
		Camera2D currentCamera = _subViewport.GetCamera2D();

		if(@event is InputEventMouseButton mouseButton)
			ProcessMouse(mouseButton, currentCamera);
				
		if (@event is InputEventMouseMotion mouseMotion && _isDragging)
			currentCamera.Offset -= mouseMotion.Relative / currentCamera.Zoom;
	}

	private void ProcessMouse(InputEventMouseButton mouseButton, Camera2D currentCamera)
	{
		if (mouseButton.ButtonIndex== MouseButton.Right && !mouseButton.Pressed)
			_isDragging = false;
		
		if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed && _isHovered)
			{
				if (_isDragging) 
					return;
				
				var slot = GetNode<Node2D>("GameViewport/SceneSlot");

				// ВАЖНО: Получаем координаты мыши внутри игрового мира SubViewport,
				// учитывая текущую позицию камеры, её Offset и Zoom.
				Vector2 clickWorldPosition = _subViewport.GetMousePosition();
				
				// Ищем персонажа на сцене. 
				// Предполагается, что узел Player находится прямо внутри SubViewport (как на вашем скриншоте)
				var player = _subViewport.GetNodeOrNull<Player>("Player"); 
				
				// Если структура сложнее (например, SubViewport/BaseMap/Player), используйте:
				// var player = _subViewport.GetNodeOrNull<Player>("BaseMap/Player");
				
				if (player != null)
				{
					player.SetTargetPosition(clickWorldPosition);
					GetViewport().SetInputAsHandled(); // Поглощаем клик
				}
			}

		if(_isHovered)
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
				_isDragging = !_isDragging;
			}
		}
	}

	private void AdjustZoom(Camera2D camera, float delta, float min, float max)
	{
		float newZoomX = Mathf.Clamp(camera.Zoom.X + delta, min, max);
		camera.Zoom = new Vector2(newZoomX, newZoomX);
	}
}
