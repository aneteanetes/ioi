using Godot;
using ioi.Source.Scenes;
using System;

public partial class BaseMap : Node2D, IViewportScene
{
	private Camera2D _camera;
	

	[Export] public float ZoomSpeed = 0.1f;
	[Export] public float MinZoom = 1.5f;  // Максимальное отдаление
	[Export] public float MaxZoom = 4.0f;  // Максимальное приближение

	public override void _Ready()
	{
		_camera = GetNode<Camera2D>("Camera2D");
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
	}
    
    public void ProcessClick(Vector2 posWorld, Vector2 posLocal)
    {
		var player = GetNode<Player>("Player");
		if (player != null)
		{
			player.SetTargetPosition(posWorld);
			GetViewport().SetInputAsHandled();
		}
    }
}
