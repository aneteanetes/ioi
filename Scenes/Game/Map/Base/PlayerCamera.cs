using Godot;
using System;

public partial class PlayerCamera : Camera2D, IGameCamera
{
	public bool CanZoom => true;
	
	public bool CanDrag => true;
	
	public float MinZoom => 3f;
	
	public float MaxZoom => 4f;

    [Export] public float TransitionSpeed { get; set; } = 8.0f; // Скорость перелета камеры
    
    private Node2D _target;
    public Node2D Target
    {
        get => _target;
        set
        {
            _target = value;
            // Если камера уже была на каком-то объекте, включаем плавный перелет
            if (_target != null)
            {
                _isTransitioning = true;
            }
        }
	}    
    
    private bool _isTransitioning = false;

    private bool _moveOffset = false;
	
	
	public override void _Ready()
	{
        MakeCurrent();
	}

	public override void _Process(double delta)
	{
        if (Global.ResetCameraMove)
        {
            _moveOffset = false;
            Global.ResetCameraMove = false;
        }
        
        if (Target != null)
        {
			if (_isTransitioning)
            {
                GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition, TransitionSpeed * (float)delta);
                if (GlobalPosition.DistanceSquaredTo(Target.GlobalPosition) < 0.1f)
                {
                    GlobalPosition = Target.GlobalPosition;
                    _isTransitioning = false;
                }
            }
			else
            	GlobalPosition = Target.GlobalPosition;
        }
        
        if (_moveOffset)
        {            
            Offset = Offset.Lerp(Vector2.Zero, 5 * (float)delta);
            
            if (Offset.DistanceSquaredTo(Vector2.Zero) < 0.01f)
            {
                Offset = Vector2.Zero;
                _moveOffset = false;
            }
        }
	}

	public void ApplyOffset(Vector2 newOffset)
    {
        Offset = newOffset;
        _moveOffset = true;
    }
}
