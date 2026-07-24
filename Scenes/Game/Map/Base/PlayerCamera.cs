using Godot;
using System;

public partial class PlayerCamera : Camera2D, IGameCamera
{
	public bool CanZoom => true;
	
	public bool CanDrag => true;
	
	public float MinZoom => 3f;
	
	public float MaxZoom => 4f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
