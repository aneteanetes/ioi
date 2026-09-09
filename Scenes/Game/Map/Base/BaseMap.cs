using Geranium.Reflection;
using Godot;
using ioi.Source.Scenes;
using ioi.Tiled.Map;
using System;
using System.Collections.Generic;

public partial class BaseMap : Node2D, IViewportScene
{
	[Export] public float ZoomSpeed = 0.1f;
	[Export] public float MinZoom = 1.5f;
	[Export] public float MaxZoom = 3.0f;
	
	// lighting
	
	[Export] public TileMapLayer TileMap { get; set; }
    [Export] public Creature Player { get; set; }
    [Export] public PlayerCamera Camera { get; set; }
    [Export] public Node2D EnemiesContainer { get; set; }
    [Export] public ColorRect FogColorRect { get; set; }
    [Export] public CanvasModulate DayNightLight { get; set; }
    [Export] public int BaseViewRadius = 4;
    [Export] public int TileSize = 16;
    
    private int _currentRadius;
    public int MapWidth {get; private set;}
    public int MapHeight{get; private set;}
    private Vector2I _mapOrigin;
	
	/// <summary>
	/// Light map
	/// <para>0 - darkness</para>
	/// <para>1 - shadow</para>
	/// <para>2 - light</para>
	/// </summary>
    public byte[,] FogGrid { get; private set;}
    
    private Image _fogImage;
    private ImageTexture _fogTexture;
    private ShaderMaterial _fogMaterial;
    private float _time = 0.0f;
	
	public override void _Ready()
    {
        CalculateMap();
        InitShader();
        
        Player.FindChild("RemoteTransform2D").As<RemoteTransform2D>().RemotePath = Camera.GetPath();
        PossessCreature(Player);
        Global.Player = Player;
        Global.Player.BindGameEntity(Global.GameWorld.Player);
        
        _currentRadius = BaseViewRadius;
        
        AfterReady?.Invoke();
    }
    
    public Action AfterReady;

     public void PossessCreature(Creature targetCreature)
    {
        if (targetCreature == null) return;
        
        // switch to AI
        if (Player != null)
        {
            // Player.GetComponent<AI>().Enabled = true;
        }
        
        Player = targetCreature;        
        Camera.Target = targetCreature;

        // disable AI
        // Player.GetComponent<AI>().Enabled = false;
    }

    private void CalculateMap()
{
    Rect2I mapBounds = TileMap.GetUsedRect();
    _mapOrigin = mapBounds.Position;
    MapWidth = mapBounds.Size.X > 0 ? mapBounds.Size.X : 100;
    MapHeight = mapBounds.Size.Y > 0 ? mapBounds.Size.Y : 100;

    FogGrid = new byte[MapWidth, MapHeight];

    // Берем крайние угловые клетки нашего региона
    Vector2I topLeftCell = _mapOrigin;
    Vector2I bottomRightCell = _mapOrigin + mapBounds.Size;

    // Считаем их точные физические координаты на экране
    Vector2 topLeftWorld = TileMap.MapToLocal(topLeftCell);
    Vector2 bottomRightWorld = TileMap.MapToLocal(bottomRightCell);

    // Добавляем безопасный отступ в размере полутора тайлов вокруг всей карты
    Vector2 hexSize = TileMap.TileSet?.TileSize ?? new Vector2I(TileSize, TileSize);
    Vector2 targetPos = topLeftWorld - (hexSize * 1.5f);
    Vector2 targetSize = (bottomRightWorld - topLeftWorld) + (hexSize * 3.0f);

    _fogImage = Image.CreateEmpty(MapWidth, MapHeight, false, Image.Format.Rgb8);
    _fogTexture = ImageTexture.CreateFromImage(_fogImage);
    
    if (FogColorRect.Material is ShaderMaterial mat)
    {
        _fogMaterial = mat;
        _fogMaterial.SetShaderParameter("fog_texture", _fogTexture);
    }

    Callable.From(() =>
    {
        FogColorRect.Position = targetPos;
        FogColorRect.Size = targetSize;
    }).CallDeferred();
}
    

    private void InitShader()
    {
        FogGrid = new byte[MapWidth, MapHeight];
        _fogMaterial = (ShaderMaterial)FogColorRect.Material;

        _fogImage = Image.CreateEmpty(MapWidth, MapHeight, false, Image.Format.R8);
        _fogTexture = ImageTexture.CreateFromImage(_fogImage);
        _fogMaterial.SetShaderParameter("fog_texture", _fogTexture);
    }
    
    public override void _Process(double delta)
    {
        UpdateDayNight((float)delta);
        UpdateFogAndVisibility();
    }
	
	private void UpdateDayNight(float delta)
    {
        _time += delta * Global.TimeSpeed;

        float dayIntensity = (Mathf.Sin(_time) + 2.0f) / 4.0f;
        DayNightLight.Color = new Color(dayIntensity, dayIntensity, dayIntensity + 0.1f); //light radius here

        //tile radius
        _currentRadius = (int)Mathf.Lerp(BaseViewRadius - 1, BaseViewRadius + 5, dayIntensity);
        
        ScalePlayerLight(dayIntensity);
    }
    
    private void ScalePlayerLight(float dayIntensity)
    {
        var playerLight = Player.GetNodeOrNull<PointLight2D>("PointLight2D");
        if (playerLight != null)
        {
            float targetScale = Mathf.Lerp(1f, 2f, dayIntensity);
            playerLight.Scale = new Vector2(targetScale, targetScale);
        }
    }
    
private void UpdateFogAndVisibility()
{
    if (TileMap == null || Player == null || FogGrid == null) return;

    // 1. Переводим позицию игрока в координаты сетки относительно _mapOrigin
    Vector2I playerTile = TileMap.LocalToMap(TileMap.ToLocal(Player.GlobalPosition)) - _mapOrigin;
    
    // 2. Сбрасываем старый свет (2 -> 1, превращаем в разведанный туман)
    for (int x = 0; x < MapWidth; x++) 
    {
        for (int y = 0; y < MapHeight; y++) 
        {
            if (FogGrid[x, y] == 2) FogGrid[x, y] = 1;
        }
    }
    
    // Проверяем, что игрок внутри границ карты
    if (playerTile.X < 0 || playerTile.X >= MapWidth || playerTile.Y < 0 || playerTile.Y >= MapHeight)
    {
        UpdateShaderTexture();
        ObjectsVisibility();
        return;
    }
    
    // 3. Массив направлений для гексагональной сетки (Odd-Rows Layout)
    Vector2I[][] hexDirections = new Vector2I[][] {
        // Для четных строк (Y % 2 == 0)
        new Vector2I[] { new Vector2I(1, 0), new Vector2I(0, 1), new Vector2I(-1, 1), new Vector2I(-1, 0), new Vector2I(-1, -1), new Vector2I(0, -1) },
        // Для нечетных строк (Y % 2 != 0)
        new Vector2I[] { new Vector2I(1, 0), new Vector2I(1, 1), new Vector2I(0, 1), new Vector2I(-1, 0), new Vector2I(0, -1), new Vector2I(1, -1) }
    };
    
    // Очередь для обхода: хранит клетку и сколько шагов осталось пройти
    Queue<(Vector2I tile, int remainingRange)> queue = new Queue<(Vector2I, int)>();
    // Множество, чтобы не обрабатывать одну клетку дважды
    HashSet<Vector2I> visited = new HashSet<Vector2I>();

    // Стартуем с позиции игрока
    queue.Enqueue((playerTile, _currentRadius));
    visited.Add(playerTile);
    FogGrid[playerTile.X, playerTile.Y] = 2;

    // 4. Запускаем наводнение
    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        Vector2I currentTile = current.tile;
        int remainingRange = current.remainingRange;

        // Если шаги кончились, дальше не идем
        if (remainingRange <= 0) continue;

        // Текущая глобальная Y координата на слое для правильного выбора четности ряда
        int globalY = currentTile.Y + _mapOrigin.Y;
        int parity = (globalY & 1) == 0 ? 0 : 1;

        // Проверяем всех 6 соседей текущего гексагона
        for (int i = 0; i < 6; i++)
        {
            Vector2I neighbor = currentTile + hexDirections[parity][i];

            // Проверяем границы карты и что мы тут еще не были
            if (neighbor.X >= 0 && neighbor.X < MapWidth && neighbor.Y >= 0 && neighbor.Y < MapHeight && !visited.Contains(neighbor))
            {
                visited.Add(neighbor);
                FogGrid[neighbor.X, neighbor.Y] = 2;

                // Если сосед — стена, мы его подсветили (игрок видит стену), но луч дальше не идет (в очередь не добавляем)
                if (IsWall(neighbor.X, neighbor.Y))
                {
                    continue; 
                }

                // Если это пустая клетка, добавляем в очередь и уменьшаем радиус на 1 шаг
                queue.Enqueue((neighbor, remainingRange - 1));
            }
        }
    }
    
    // 5. Обновляем шейдер и видимость объектов
    UpdateShaderTexture();
    ObjectsVisibility();
}
    
    private void CastRay(Vector2I start, Vector2I end)
{
    // Чтобы пустить луч по физической прямой, переводим координаты сетки в мировые пиксели
    Vector2 startWorld = TileMap.MapToLocal(start + _mapOrigin);
    Vector2 endWorld = TileMap.MapToLocal(end + _mapOrigin);
    
    // Определяем количество шагов на основе расстояния на экране
    float distance = startWorld.DistanceTo(endWorld);
    // 8.0f — это половина стандартного размера тайла (16). Чем меньше число, тем точнее луч.
    int steps = Mathf.Max(1, Mathf.RoundToInt(distance / 8.0f)); 

    for (int i = 0; i <= steps; i++)
    {
        float t = (float)i / steps;
        // Линейно интерполируем точку на физическом экране
        Vector2 currentWorldPos = startWorld.Lerp(endWorld, t);
        
        // Переводим текущую точку луча обратно в индекс гексагональной клетки
        Vector2I currentTile = TileMap.LocalToMap(TileMap.ToLocal(currentWorldPos)) - _mapOrigin;

        int cx = currentTile.X;
        int cy = currentTile.Y;

        if (cx >= 0 && cx < MapWidth && cy >= 0 && cy < MapHeight)
        {
            FogGrid[cx, cy] = 2;

            // Если луч наткнулся на препятствие в WallLayer — прерываем его, дальше темнота
            if (IsWall(cx, cy))
            {
                break; 
            }
        }
        else
        {
            break; // Вышли за границы карты
        }
    }
}
    
    
    private void ComputeHexFOV(Vector2I center, int radius)
    {
        // Перебираем «кольцо» на максимальном расстоянии радиуса видимости
        // и пускаем лучи из центра к каждой граничной гексагональной клетке
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            
            for (int r = r1; r <= r2; r++)
            {
                // Нам нужны только клетки на самом краю нашей окружности видимости
                if (Mathf.Abs(q) == radius || Mathf.Abs(r) == radius || Mathf.Abs(-q - r) == radius)
                {
                    Vector2I targetMapCell = CubeToMap(MapToCube(center) + new Vector3I(q, r, -q - r));
                    CastHexRay(center, targetMapCell);
                }
            }
        }
    }

    private void CastHexRay(Vector2I start, Vector2I end)
    {
        Vector3I cubeStart = MapToCube(start);
        Vector3I cubeEnd = MapToCube(end);
        
        int distance = HexDistance(cubeStart, cubeEnd);
        if (distance == 0) return;

        for (int i = 0; i <= distance; i++)
        {
            float t = (float)i / distance;
            
            // Линейная интерполяция между 3D-координатами гексагонов
            float rx = cubeStart.X + (cubeEnd.X - cubeStart.X) * t;
            float ry = cubeStart.Y + (cubeEnd.Y - cubeStart.Y) * t;
            float rz = cubeStart.Z + (cubeEnd.Z - cubeStart.Z) * t;
            
            // Округление к ближайшему гексагону
            Vector3I cubeCurrent = CubeRound(rx, ry, rz);
            Vector2I mapCurrent = CubeToMap(cubeCurrent);
            
            int cx = mapCurrent.X - _mapOrigin.X;
            int cy = mapCurrent.Y - _mapOrigin.Y;

            if (cx >= 0 && cx < MapWidth && cy >= 0 && cy < MapHeight)
            {
                FogGrid[cx, cy] = 2;

                // Если встретили стену — луч прерывается, за ней ничего не видно
                if (IsWall(mapCurrent.X, mapCurrent.Y))
                {
                    break;
                }
            }
            else
            {
                break; // Вышли за границы карты
            }
        }
    }
    // ИЗМЕНЕНО: Точный перевод координат сетки Godot (Odd-Rows) в 3D кубические координаты
    private Vector3I MapToCube(Vector2I mapPos)
    {
        // Для нечетных строк (Odd-Rows) гексагональной сетки
        int x = mapPos.X - (mapPos.Y - (mapPos.Y & 1)) / 2;
        int z = mapPos.Y;
        int y = -x - z;
        return new Vector3I(x, y, z);
    }

    // ИЗМЕНЕНО: Обратный перевод из кубических координат в плоскую сетку Godot
    private Vector2I CubeToMap(Vector3I cubePos)
    {
        int col = cubePos.X + (cubePos.Z - (cubePos.Z & 1)) / 2;
        int row = cubePos.Z;
        return new Vector2I(col, row);
    }

    private int HexDistance(Vector3I a, Vector3I b)
    {
        return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
    }

    private Vector3I CubeRound(float x, float y, float z)
    {
        int rx = Mathf.RoundToInt(x);
        int ry = Mathf.RoundToInt(y);
        int rz = Mathf.RoundToInt(z);

        float xDiff = Mathf.Abs(rx - x);
        float yDiff = Mathf.Abs(ry - y);
        float zDiff = Mathf.Abs(rz - z);

        if (xDiff > yDiff && xDiff > zDiff)
            rx = -ry - rz;
        else if (yDiff > zDiff)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        return new Vector3I(rx, ry, rz);
    }

    private Vector3I HexGridMapToCube(Vector2I cell)
    {
        // Формула для Odd-Rows (сдвиг нечетных строк), которая используется по умолчанию в Godot 4 для гексагонов
        // Если у вас в TileSet выбран другой режим (Even-Rows, Odd-Columns), формулу нужно будет зеркально поменять.
        int x = cell.X - (cell.Y - (cell.Y & 1)) / 2;
        int z = cell.Y;
        int y = -x - z;
        return new Vector3I(x, y, z);
    }

    private Vector2I HexGridCubeToMap(Vector3I cube)
    {
        int col = cube.X + (cube.Z - (cube.Z & 1)) / 2;
        int row = cube.Z;
        return new Vector2I(col, row);
    }

    private void UpdateShaderTexture()
    {
        if (_fogImage == null) return;

        for (int x = 0; x < MapWidth; x++) 
        {
            for (int y = 0; y < MapHeight; y++) 
            {
                float colorValue = FogGrid[x, y] == 2 ? 1.0f : (FogGrid[x, y] == 1 ? 0.5f : 0.0f);
                _fogImage.SetPixel(x, y, new Color(colorValue, 0, 0));
            }
        }
        _fogTexture.Update(_fogImage);
    }
    
    private void ObjectsVisibility(){
        if (EnemiesContainer == null) return;
        foreach (Node enemy in EnemiesContainer.GetChildren()){
            if (enemy is Node2D enemy2D){
                Vector2I enemyTile = TileMap.LocalToMap(TileMap.ToLocal(enemy2D.GlobalPosition)) - _mapOrigin;
                if (enemyTile.X >= 0 && enemyTile.X < MapWidth && enemyTile.Y >= 0 && enemyTile.Y < MapHeight){
                    enemy2D.Visible = (FogGrid[enemyTile.X, enemyTile.Y] == 2);
                    }else{
                        enemy2D.Visible = false;}}}
    }
    
    public byte[] GetFogSaveData(){
        byte[] flatArray = new byte[MapWidth * MapHeight];
        Buffer.BlockCopy(FogGrid, 0, flatArray, 0, flatArray.Length);
        return flatArray;
    }

    public void LoadFogSaveData(byte[] loadedData){
        if (loadedData == null || loadedData.Length != MapWidth * MapHeight) return;
        Buffer.BlockCopy(loadedData, 0, FogGrid, 0, loadedData.Length);
    }
    
	private bool IsWall(int tileX, int tileY)
	{
		Vector2I globalTilePos = new Vector2I(tileX, tileY) + _mapOrigin;
		
		TileData tileData = TileMap.GetCellTileData(globalTilePos);
		
		return tileData != null;
	}
	
	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
	}
	
	public void ProcessClick(Vector2 posWorld, Vector2 posLocal)
	{
		//var player = GetNode<Creature>("Player");
		if (Player != null)
		{
			Player.SetTargetPosition(posWorld);
		}
	}
    
    //minimap part
    
    public bool IsWallInGrid(int tileX, int tileY)
    {
        Vector2I globalTilePos = new Vector2I(tileX, tileY) + _mapOrigin;
        TileData tileData = TileMap.GetCellTileData(globalTilePos);
        return tileData != null;
    }
    
    public Vector2I GetPlayerTile()
    {
        if (Player == null) return new Vector2I(-1, -1);
        return TileMap.LocalToMap(TileMap.ToLocal(Player.GlobalPosition)) - _mapOrigin;
    }
    
    public System.Collections.Generic.List<Vector2I> GetVisibleEnemiesTiles()
    {
        var list = new System.Collections.Generic.List<Vector2I>();
        if (EnemiesContainer == null) return list;

        foreach (Node enemy in EnemiesContainer.GetChildren())
        {
            if (enemy is Node2D enemy2D)
            {
                Vector2I enemyTile = TileMap.LocalToMap(TileMap.ToLocal(enemy2D.GlobalPosition)) - _mapOrigin;
                if (enemyTile.X >= 0 && enemyTile.X < MapWidth && enemyTile.Y >= 0 && enemyTile.Y < MapHeight)
                {
                    if (FogGrid[enemyTile.X, enemyTile.Y] == 2)
                    {
                        list.Add(enemyTile);
                    }
                }
            }
        }
        return list;
    }
    
    public override void _ExitTree()
    {
        Global.Player=null;
        base._ExitTree();
    }
}
