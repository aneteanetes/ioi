using Geranium.Reflection;
using Godot;
using ioi.Source.Scenes;
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
				var slotScene = slot.GetChild<BaseMap>(0);
				
				Vector2 localMousePos = _subViewport.GetMousePosition();
				Vector2 clickWorldPosition = currentCamera.GetCanvasTransform().AffineInverse() * localMousePos;
				
				if(slotScene is IViewportScene viewportScene)
				{
					viewportScene.ProcessClick(clickWorldPosition,localMousePos);
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
