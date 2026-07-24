using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed { get; set; } = 300.0f;
    
    // Точка, к которой стремится персонаж. По умолчанию — он сам.
    private Vector2 _targetPosition;

    public override void _Ready()
    {
        _targetPosition = GlobalPosition;
    }

    // Этот метод будет вызывать скрипт вьюпорта, передавая точные координаты мира
    public void SetTargetPosition(Vector2 worldPos)
    {
        _targetPosition = worldPos;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Считаем расстояние до цели
        float distanceToTarget = GlobalPosition.DistanceTo(_targetPosition);

        // Если мы уже почти пришли (ближе чем на 5 пикселей), останавливаемся
        if (distanceToTarget < 5.0f)
        {
            Velocity = Vector2.Zero;
            return;
        }

        // Направление движения
        Vector2 direction = (_targetPosition - GlobalPosition).Normalized();
        
        // Задаем скорость и двигаемся с учетом коллизий (стен)
        Velocity = direction * Speed;
        MoveAndSlide();
    }
}
