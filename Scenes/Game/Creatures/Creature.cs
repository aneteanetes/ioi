using Geranium.Reflection;
using Godot;
using ioi.Game;
using System.Collections.Generic;

public partial class Creature : CharacterBody2D
{
    // Оставляем вашу скорость (количество клеток в секунду)
    [Export] public float TileSize { get; set; } = 16.0f;
    [Export] public float MoveSpeed { get; set; } = 10.0f;
    [Export] public string[] Prototypes { get; set; } = [];
    [Export] public Area2D MouseArea { get; set; }
    
    public GameEntity GameEntity { get; private set; }
            
    private AStar2D _astar = new AStar2D();
    private Queue<Vector2> _currentPath = new Queue<Vector2>();
    private bool _isMoving = false;
    private Vector2 _targetWorldPos;
    private Sprite2D _sprite;
    private Tween _idleTween;
    private Tween _stepTween;
    

    private TileMapLayer _wallLayer;
    private TileMapLayer _groundLayer;
    private TileMapLayer _mainTileMap; // Используем вместо пустого groundLayer
    
    
    private bool _moveOffset;
    
    public override async void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        
        GlobalPosition = SnapToGrid(GlobalPosition);
        _targetWorldPos = GlobalPosition;
        
        InitGameEntity();
        InitTileMapLayers();
        InitAStar();
        StartIdleAnimation();
        
        MouseArea.MouseEntered += OnMouseEntered;
        MouseArea.MouseExited += OnMouseExited;
        MouseArea.InputEvent += OnMouseInputEvent;
    }

    private void OnMouseEntered()
    {
        Global.StatsContainer.BindEntity(this.GameEntity);
    }
    
    private void OnMouseExited()
    {
        Global.StatsContainer.BindEntity(Global.GameWorld.Player);
    }

    private void OnMouseInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        // Движок сам поймал клик в зоне Area2D
        if (@event is InputEventMouseButton mouseBtn && mouseBtn.Pressed && mouseBtn.ButtonIndex == MouseButton.Left)
        {
            GD.Print($"Иммерсивный клик по персонажу {Name}! Открываем диалог/осматриваем.");
        }
    }
    
    private void InitGameEntity()
    {
        if(this.Prototypes.IsEmpty())
            return;
        
        var entity = Global.SpawnSystem.SpawnEntity(this.Prototypes);
        BindGameEntity(entity);
    }
    
    public void BindGameEntity(GameEntity gameEntity)
    {
        this.GameEntity = gameEntity;
        LoadTileFromGameEntity(GameEntity);
        gameEntity.Texture = this._sprite.Texture;
    }
    
    public void LoadTileFromGameEntity(GameEntity gameEntity)
    {
        var tileset = gameEntity["tileset"].String;
        var region = gameEntity.Region("tileset_region");
        var color = gameEntity.Color("color");
        
        if(tileset.IsNotEmpty() && region!=default)
            this.LoadTile(tileset, region, color);
    }
    
    public void LoadTile(string tilset, Rect2 region, Color color)
    {
        var atlas = new AtlasTexture();
        atlas.Atlas = GD.Load<Texture2D>(tilset);
        atlas.Region = region;
        
        _sprite.Texture = atlas;
        _sprite.Modulate = color;
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
                    StopStepAnimation();
                    StartIdleAnimation();
                }
            }
        }
        else if (_currentPath.Count > 0)
        {
            StopIdleAnimation();
            
            // Достаем следующую МИРОВУЮ позицию (центр следующего гексагона)
            _targetWorldPos = _currentPath.Dequeue();
            
            // Поворот спрайта влево/вправо в зависимости от направления движения
            if (_targetWorldPos.X > GlobalPosition.X) _sprite.FlipH = false;
            else if (_targetWorldPos.X < GlobalPosition.X) _sprite.FlipH = true;
            
            _isMoving = true;
            
            StartStepAnimation();
        }
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

    
    private void InitTileMapLayers()
    {
        _wallLayer = GetParent().GetParent().GetNodeOrNull<TileMapLayer>("WallLayer");
        _groundLayer = GetParent().GetParent().GetNodeOrNull<TileMapLayer>("GroundLayer");
    }
    
    private void InitAStar()
    {
        _astar.Clear();
        
        if (_wallLayer == null)
        {
            GD.PrintErr("[AStar Ошибка] WallLayer не найден! Пути построить нельзя.");
            return;
        }

        // Твои оригинальные границы генерации сетки
        int startX = -100;
        int startY = -100;
        int endX = 100;
        int endY = 100;

        // Шаг 1: Добавляем в граф все свободные гексагоны в этом радиусе
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Vector2I cell = new Vector2I(x, y);

                // ТВОЯ НАТИВНАЯ ЛОГИКА: если в WallLayer есть тайл — это стена. Пропускаем её.
                if (_wallLayer.GetCellSourceId(cell) != -1)
                {
                    continue; 
                }

                long pointId = GetPointId(cell);
                
                // Считаем центр гексагона через WallLayer, чтобы гарантировать совпадение координат
                Vector2 worldPos = _wallLayer.MapToLocal(cell); 
                
                _astar.AddPoint(pointId, worldPos);
            }
        }

        // Шаг 2: Связываем свободные гексагоны между собой
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Vector2I cell = new Vector2I(x, y);
                long pointId = GetPointId(cell);
                
                if (!_astar.HasPoint(pointId)) continue;

                // Заставляем движок вернуть 6 гексагональных соседей для этой клетки
                Godot.Collections.Array<Vector2I> neighbors = _wallLayer.GetSurroundingCells(cell);

                foreach (Vector2I neighborCell in neighbors)
                {
                    long neighborId = GetPointId(neighborCell);
                    
                    if (_astar.HasPoint(neighborId))
                    {
                        _astar.ConnectPoints(pointId, neighborId, bidirectional: true);
                    }
                }
            }
        }
    }

    public void SetTargetPosition(Vector2 worldPos)
    {
        if (_wallLayer == null) return;

        // Клик переводим в координаты через единственный рабочий WallLayer
        Vector2I startCell = _wallLayer.LocalToMap(_wallLayer.ToLocal(GlobalPosition));
        Vector2I endCell = _wallLayer.LocalToMap(_wallLayer.ToLocal(worldPos));
        
        long startId = GetPointId(startCell);
        long endId = GetPointId(endCell);

        if (!_astar.HasPoint(endId)) return;
        
        Vector2[] pathPoints = _astar.GetPointPath(startId, endId);
        
        if (pathPoints.Length > 0)
        {
            _currentPath.Clear();
            foreach (Vector2 point in pathPoints)
            {
                _currentPath.Enqueue(point);
            }
            if (_currentPath.Count > 0) _currentPath.Dequeue();
        }
    }

    private Vector2 SnapToGrid(Vector2 pos)
    {
        if (_wallLayer == null) return pos;
        Vector2I cell = _wallLayer.LocalToMap(_wallLayer.ToLocal(pos));
        return _wallLayer.MapToLocal(cell);
    }

    private long GetPointId(Vector2I cell)
    {
        // Сдвигаем координаты на большой шаг, чтобы даже -100 превратилось в положительное число
        // 50000 выбрано с запасом, чтобы код работал на картах размером до 50000x50000 клеток
        long x = (long)cell.X + 50000;
        long y = (long)cell.Y + 50000;
        
        // Безопасно склеиваем два гарантированно положительных числа в один положительный long
        return (x << 32) | (uint)y;
    }
    
    public override void _ExitTree()
    {
        this.GameEntity.Texture = null;
        this.GameEntity = null;
        base._ExitTree();
    }
}
