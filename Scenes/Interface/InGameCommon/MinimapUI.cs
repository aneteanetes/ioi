using Godot;
using System;

public partial class MinimapUI : TextureRect
{
    [Export] public Color DarknessColor = new Color(0, 0, 0, 1);
    [Export] public Color ShadowWallColor = new Color(0.2f, 0.2f, 0.2f, 1);
    [Export] public Color ShadowFloorColor = new Color(0.1f, 0.1f, 0.1f, 1);
    [Export] public Color VisibleWallColor = new Color(0.5f, 0.5f, 0.5f, 1);
    [Export] public Color VisibleFloorColor = new Color(0.3f, 0.3f, 0.3f, 1);
    
    [Export] public Color PlayerColor = new Color(0, 1, 0, 1);
    [Export] public Color EnemyColor = new Color(1, 0, 0, 1);
    [Export] public float MarkerSize = 4f; 
    
    [Export] public int VisibleTilesRadius = 40; 
    
    private BaseMap _baseMap;
    private Image _minimapImage;
    private ImageTexture _minimapTexture;
    private Vector2 _tileSizeInPixels;

    public override void _Ready()
    {
        sceneSlot = GetTree().Root.GetNodeOrNull("MainGame/GameLayout/GameViewportContainer/GameViewport/SceneSlot");
        if (sceneSlot != null)
        {
            if (sceneSlot.GetChildCount() > 0)
            {
                SetupMinimap(sceneSlot.GetChild(0) as BaseMap);
            }
            else
            {
                sceneSlot.ChildEnteredTree += OnMapEnteredSlot;
            }
        }
    }

    private void OnMapEnteredSlot(Node node)
    {        
        if (node is BaseMap map)
        {
            map.AfterReady+= OnPendingMapReady;
            _pendingMap = map;
        }
    }
    
    private void OnPendingMapReady()
    {
        SetupMinimap(_pendingMap);
        
        // Отписываемся сразу после выполнения
        if (sceneSlot != null) 
            sceneSlot.ChildEnteredTree -= OnMapEnteredSlot;
        
        if (_pendingMap != null)
        {
            _pendingMap.AfterReady -= OnPendingMapReady;
            _pendingMap = null;
        }
    }

    private void SetupMinimap(BaseMap map)
    {
        _baseMap = map;
        if (_baseMap == null) return;

        // ВАЖНО: Теперь размер текстуры равен РАЗМЕРУ ОКНА миникарты в UI (например, 200x200 пикселей)
        int viewWidth = (int)Size.X;
        int viewHeight = (int)Size.Y;

        if (viewWidth <= 0 || viewHeight <= 0)
        {
            viewWidth = 200;
            viewHeight = 200;
        }

        _minimapImage = Image.CreateEmpty(viewWidth, viewHeight, false, Image.Format.Rgba8);
        _minimapTexture = ImageTexture.CreateFromImage(_minimapImage);
        
        TextureFilter = TextureFilterEnum.Nearest;
        Texture = _minimapTexture;
    }

    public override void _Process(double delta)
    {
        if (_baseMap == null) return;

        // Вычисляем, сколько экранных пикселей занимает один тайл при текущем зуме
        _tileSizeInPixels = new Vector2(Size.X / (VisibleTilesRadius * 2), Size.Y / (VisibleTilesRadius * 2));

        UpdateMinimapTexture();
        QueueRedraw();
    }

    private void UpdateMinimapTexture()
    {
        byte[,] grid = _baseMap.FogGrid;
        int mapWidth = _baseMap.MapWidth;
        int mapHeight = _baseMap.MapHeight;
        Vector2I playerTile = _baseMap.GetPlayerTile();

        int viewWidth = _minimapImage.GetWidth();
        int viewHeight = _minimapImage.GetHeight();

        // Проходим по каждому физическому ПИКСЕЛЮ окошка миникарты
        for (int screenX = 0; screenX < viewWidth; screenX++)
        {
            for (int screenY = 0; screenY < viewHeight; screenY++)
            {
                // Переводим координату экранного пикселя в координату тайла на большой карте относительно игрока
                float relativeTileX = (screenX - (viewWidth / 2f)) / _tileSizeInPixels.X + playerTile.X;
                float relativeTileY = (screenY - (viewHeight / 2f)) / _tileSizeInPixels.Y + playerTile.Y;

                int mapX = Mathf.FloorToInt(relativeTileX);
                int mapY = Mathf.FloorToInt(relativeTileY);

                Color pixelColor = DarknessColor;

                // Проверяем, попадает ли этот тайл в границы существующей карты
                if (mapX >= 0 && mapX < mapWidth && mapY >= 0 && mapY < mapHeight)
                {
                    byte fogStatus = grid[mapX, mapY];

                    if (fogStatus == 1)
                        pixelColor = _baseMap.IsWallInGrid(mapX, mapY) ? ShadowWallColor : ShadowFloorColor;
                    else if (fogStatus == 2)
                        pixelColor = _baseMap.IsWallInGrid(mapX, mapY) ? VisibleWallColor : VisibleFloorColor;
                }

                _minimapImage.SetPixel(screenX, screenY, pixelColor);
            }
        }

        _minimapTexture.Update(_minimapImage);
    }

    public override void _Draw()
    {
        if (_baseMap == null) return;

        Vector2 centerOfScreen = Size / 2f;

        // 1. Игрок ВСЕГДА находится ровно по центру миникарты, так как карта скроллится под ним
        DrawCircle(centerOfScreen, MarkerSize, PlayerColor);

        // 2. Отрисовка видимых врагов со смещением относительно игрока
        Vector2I playerTile = _baseMap.GetPlayerTile();
        foreach (Vector2I enemyTile in _baseMap.GetVisibleEnemiesTiles())
        {
            // Вычисляем расстояние от игрока до врага в тайлах
            Vector2 tileOffset = new Vector2(enemyTile.X - playerTile.X, enemyTile.Y - playerTile.Y);
            
            // Переводим это расстояние в пиксели экрана
            Vector2 enemyScreenPos = centerOfScreen + (tileOffset * _tileSizeInPixels) + (_tileSizeInPixels / 2f);

            // Рисуем врага, только если он физически попадает в границы окошка миникарты
            if (enemyScreenPos.X >= 0 && enemyScreenPos.X <= Size.X && enemyScreenPos.Y >= 0 && enemyScreenPos.Y <= Size.Y)
            {
                DrawCircle(enemyScreenPos, MarkerSize, EnemyColor);
            }
        }
    }
    
    private Node sceneSlot;
    private BaseMap _pendingMap;
    public override void _ExitTree()
    {
        Texture = null;
        
        _minimapImage?.Dispose();
        _minimapTexture?.Dispose();
        
        if (_pendingMap != null)
        {
            _pendingMap.AfterReady -= OnPendingMapReady;
        }
        
        base._ExitTree();
    }
}
