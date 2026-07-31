using Godot;
using ioi.Game;
using System.Collections.Generic;

public partial class Creature : CharacterBody2D
{
    // Оставляем вашу скорость (количество клеток в секунду)
    [Export] public float TileSize { get; set; } = 16.0f;
    [Export] public float MoveSpeed { get; set; } = 10.0f;
    [Export] public string[] Prototypes { get; set; } = [];

    public GameEntity GameEntity { get; private set; }
            
    private AStarGrid2D _astar = new AStarGrid2D();
    private Queue<Vector2I> _currentPath = new Queue<Vector2I>();
    private bool _isMoving = false;
    private Vector2 _targetWorldPos;
    private Sprite2D _sprite;
    private Tween _idleTween;
    private Tween _stepTween;

    private bool _moveOffset;
    
    public override async void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        GlobalPosition = SnapToGrid(GlobalPosition);
        _targetWorldPos = GlobalPosition;
        
        InitGameEntity();

        // if (_camera != null)
        // {
        //     _camera.GlobalPosition = GlobalPosition;
        //     _camera.Offset = Vector2.Zero;
        //     _camera.MakeCurrent(); // Принудительно делаем её активной для этого SubViewport
        // }
        
        InitAStar();
        StartIdleAnimation();
    }

    private void InitGameEntity()
    {
        if(this.Prototypes.IsEmpty())
            return;
        
        this.GameEntity = Global.SpawnSystem.SpawnEntity(this.Prototypes);
        LoadTileFromGameEntity(GameEntity);
    }

    public void BindGameEntity(GameEntity gameEntity)
    {
        this.GameEntity = gameEntity;
        LoadTileFromGameEntity(GameEntity);
    }
    
    public void LoadTileFromGameEntity(GameEntity gameEntity)
    {
        this.LoadTile(gameEntity["tileset"].String,
            gameEntity.Region("tileset_region"),
            gameEntity.Color("color"));    
    }
    
    public void LoadTile(string tilset, Rect2 region, Color color)
    {
        var atlas = new AtlasTexture();
        atlas.Atlas = GD.Load<Texture2D>(tilset);
        atlas.Region = region;
        
        _sprite.Texture = atlas;
        _sprite.Modulate = color;
    }
    
    private void InitAStar()
    {
        _astar.Region = new Rect2I(-100, -100, 200, 200);
        _astar.CellSize = new Vector2(TileSize, TileSize);
        _astar.DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan;
        _astar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan;
        _astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never; 
        _astar.Update();
        
        var wallLayer = GetTree().CurrentScene.GetNodeOrNull<TileMapLayer>("WallLayer") 
                    ?? GetParent().GetNodeOrNull<TileMapLayer>("WallLayer");
        if (wallLayer != null)
        {
            foreach (Vector2I cell in wallLayer.GetUsedCells())
            {
                _astar.SetPointSolid(cell, true);
            }
        }
    }

    public void SetTargetPosition(Vector2 worldPos)
    {
        Vector2I startCell = (Vector2I)(GlobalPosition / TileSize);
        Vector2I endCell = (Vector2I)(worldPos / TileSize);
        
        if (_astar.IsPointSolid(endCell)) return;
        
            
        // if (_camera.Offset != Vector2.Zero)
        //     _moveOffset=true;
                
        Godot.Collections.Array<Vector2I> pathPoints = _astar.GetIdPath(startCell, endCell);
        
        if (pathPoints.Count > 0)
        {
            _currentPath.Clear();
            foreach (Vector2I point in pathPoints)
            {
                _currentPath.Enqueue(point);
            }
            
            if (_currentPath.Count > 0) _currentPath.Dequeue();
        }
    }
    
    public override void _Process(double delta)
    {
        if (_isMoving)
        {
            GlobalPosition = GlobalPosition.MoveToward(_targetWorldPos, MoveSpeed * TileSize * (float)delta);
            
            if (GlobalPosition.DistanceTo(_targetWorldPos) < 0.05f)
            {
                GlobalPosition = _targetWorldPos;
                _isMoving = false;
                
                if (_currentPath.Count == 0)
                {
                    // Останавливаем прыжки только когда персонаж ПОЛНОСТЬЮ пришел на конечную клетку
                    StopStepAnimation();
                    StartIdleAnimation();
                }
            }
        }
        else if (_currentPath.Count > 0)
        {
            StopIdleAnimation();
            
            Vector2I nextCell = _currentPath.Dequeue();
            
            Vector2I currentCell = (Vector2I)(GlobalPosition / TileSize);
            if (nextCell.X > currentCell.X) _sprite.FlipH = false;
            else if (nextCell.X < currentCell.X) _sprite.FlipH = true;
            
            Vector2 nextCellFloat = new Vector2(nextCell.X, nextCell.Y);
            _targetWorldPos = (nextCellFloat * TileSize) + new Vector2(TileSize / 2.0f, TileSize / 2.0f);
            
            _isMoving = true;
            
            // Запускаем непрерывную анимацию прыжков
            StartStepAnimation();
        }
        
        // if(_isMoving || _currentPath.Count > 0)
        // {
        //     if (_moveOffset)
        //     {            
        //         _camera.Offset = _camera.Offset.Lerp(Vector2.Zero, 5 * (float)delta);
                
        //         // Отключаем процесс, когда значение достаточно близко к нулю
        //         if (_camera.Offset.DistanceSquaredTo(Vector2.Zero) < 0.01f)
        //         {
        //             _camera.Offset = Vector2.Zero;
        //             _moveOffset=false;
        //         }
        //     }
        // }
    }

    private void StartStepAnimation()
    {
        // Если анимация шагов уже играет, не перезапускаем её!
        if (_stepTween != null && _stepTween.IsValid()) return;
        
        _stepTween = CreateTween();
        _stepTween.SetLoops(); // Бесконечный цикл, пока персонаж бежит

        // Рассчитываем базовое время клетки и замедляем его на треть (умножаем на ~1.33)
        float baseCellDuration = 1.0f / MoveSpeed; 
        float duration = baseCellDuration * 4f; 
        
        // ---- ФАЗА 1: Прыжок вверх (плавный взлет) ----
        _stepTween.SetParallel(true);
        _stepTween.TweenProperty(_sprite, "position:y", -6.0f, duration * 0.5f) // Чуть увеличил высоту до -6, так как прыжок стал длиннее
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _stepTween.TweenProperty(_sprite, "scale", new Vector2(0.88f, 1.12f), duration * 0.5f) // Чуть мягче сжатие
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        
        // ---- ФАЗА 2: Приземление ----
        _stepTween.Chain().SetParallel(true);
        _stepTween.TweenProperty(_sprite, "position:y", 0.0f, duration * 0.5f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _stepTween.TweenProperty(_sprite, "scale", new Vector2(1.08f, 0.92f), duration * 0.3f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);

        // ---- ФАЗА 3: Выравнивание в исходную форму ----
        _stepTween.Chain().TweenProperty(_sprite, "scale", Vector2.One, duration * 0.2f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
    }
        
        

    private void StopStepAnimation()
    {
        if (_stepTween != null && _stepTween.IsValid())
        {
            _stepTween.Kill();
        }
        
        // Плавно возвращаем спрайт в исходную форму при остановке, чтобы не было резкого обрыва
        Tween finalReset = CreateTween();
        finalReset.SetParallel(true);
        finalReset.TweenProperty(_sprite, "scale", Vector2.One, 0.1f);
        finalReset.TweenProperty(_sprite, "position:y", 0.0f, 0.1f);
    }
    
    private void StartIdleAnimation()
    {
        if (_idleTween != null && _idleTween.IsValid()) return;

        _idleTween = CreateTween();
        _idleTween.SetLoops();
        _idleTween.SetTrans(Tween.TransitionType.Sine);
        _idleTween.SetEase(Tween.EaseType.InOut);
        
        _idleTween.TweenProperty(_sprite, "scale", new Vector2(1.05f, 0.95f), 0.6f);
        _idleTween.TweenProperty(_sprite, "scale", Vector2.One, 0.6f);
    }

    private void StopIdleAnimation()
    {
        if (_idleTween != null && _idleTween.IsValid())
        {
            _idleTween.Kill();
        }
        _sprite.Scale = Vector2.One;
        _sprite.Position = Vector2.Zero;
    }

    private Vector2 SnapToGrid(Vector2 pos)
    {
        float x = Mathf.Floor(pos.X / TileSize) * TileSize + (TileSize / 2.0f);
        float y = Mathf.Floor(pos.Y / TileSize) * TileSize + (TileSize / 2.0f);
        return new Vector2(x, y);
    }
}
