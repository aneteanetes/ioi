using Godot;
using System;
using System.Collections.Generic;

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
    private Vector2 _tileSizeInPixels;
    private Node _sceneSlot;
    private BaseMap _pendingMap;

    public override void _Ready()
    {
        // Отключаем дефолтную текстуру, так как теперь рисуем сами на GPU
        Texture = null;

        _sceneSlot = GetTree().Root.GetNodeOrNull("MainGame/GameLayout/SceneSlot");
        if (_sceneSlot != null)
        {
            if (_sceneSlot.GetChildCount() > 0)
            {
                SetupMinimap(_sceneSlot.GetChild(0) as BaseMap);
            }
            else
            {
                _sceneSlot.ChildEnteredTree += OnMapEnteredSlot;
            }
        }
    }

    private void OnMapEnteredSlot(Node node)
    {        
        if (node is BaseMap map)
        {
            map.AfterReady += OnPendingMapReady;
            _pendingMap = map;
        }
    }
    
    private void OnPendingMapReady()
    {
        SetupMinimap(_pendingMap);
        
        if (_sceneSlot != null) 
            _sceneSlot.ChildEnteredTree -= OnMapEnteredSlot;
        
        if (_pendingMap != null)
        {
            _pendingMap.AfterReady -= OnPendingMapReady;
            _pendingMap = null;
        }
    }

    private void SetupMinimap(BaseMap map)
    {
        _baseMap = map;
    }

    public override void _Process(double delta)
    {
        if (_baseMap == null) return;

        // Вычисляем размер одного тайла на миникарте
        float scale = 1.5f;
        _tileSizeInPixels = new Vector2(Size.X / (VisibleTilesRadius * scale), Size.Y / (VisibleTilesRadius * scale));
        
        // Заставляем Godot вызвать метод _Draw на этом кадре
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_baseMap == null) return;

        Vector2 centerOfScreen = Size / 2f;
        byte[,] grid = _baseMap.FogGrid;
        int mapWidth = _baseMap.MapWidth;
        int mapHeight = _baseMap.MapHeight;
        Vector2I playerTile = _baseMap.GetPlayerTile();

        // 1. Сначала заливаем фон миникарты цветом темноты
        DrawRect(new Rect2(Vector2.Zero, Size), DarknessColor);

        // Вычисляем диапазон тайлов вокруг игрока, которые физически влезают в окно миникарты
        int halfTilesX = Mathf.CeilToInt(centerOfScreen.X / _tileSizeInPixels.X);
        int halfTilesY = Mathf.CeilToInt(centerOfScreen.Y / _tileSizeInPixels.Y);

        int minX = Mathf.Max(0, playerTile.X - halfTilesX);
        int maxX = Mathf.Min(mapWidth - 1, playerTile.X + halfTilesX);
        int minY = Mathf.Max(0, playerTile.Y - halfTilesY);
        int maxY = Mathf.Min(mapHeight - 1, playerTile.Y + halfTilesY);

        // 2. Отрисовка тайлов (GPU Draw Rects гораздо быстрее попиксельного CPU цикла)
        for (int mapX = minX; mapX <= maxX; mapX++)
        {
            for (int mapY = minY; mapY <= maxY; mapY++)
            {
                byte fogStatus = grid[mapX, mapY];
                if (fogStatus == 0) continue; // Не исследован (уже залит черным)

                Color tileColor = DarknessColor;
                bool isWall = _baseMap.IsWallInGrid(mapX, mapY);

                if (fogStatus == 1)
                    tileColor = isWall ? ShadowWallColor : ShadowFloorColor;
                else if (fogStatus == 2)
                    tileColor = isWall ? VisibleWallColor : VisibleFloorColor;

                // Вычисляем позицию прямоугольника на экране миникарты относительно центра
                Vector2 tileOffset = new Vector2(mapX - playerTile.X, mapY - playerTile.Y);
                Vector2 screenPos = centerOfScreen + (tileOffset * _tileSizeInPixels);

                // Отрисовываем тайл
                DrawRect(new Rect2(screenPos, _tileSizeInPixels), tileColor);
            }
        }

        // 3. Отрисовка противников
        foreach (Vector2I enemyTile in _baseMap.GetVisibleEnemiesTiles())
        {
            Vector2 tileOffset = new Vector2(enemyTile.X - playerTile.X, enemyTile.Y - playerTile.Y);
            Vector2 enemyScreenPos = centerOfScreen + (tileOffset * _tileSizeInPixels) + (_tileSizeInPixels / 2f);

            // Проверяем, находится ли противник в границах текстуры миникарты
            if (enemyScreenPos.X >= 0 && enemyScreenPos.X <= Size.X && enemyScreenPos.Y >= 0 && enemyScreenPos.Y <= Size.Y)
            {
                DrawCircle(enemyScreenPos, MarkerSize, EnemyColor);
            }
        }

        // 4. Отрисовка игрока строго по центру
        DrawCircle(centerOfScreen, MarkerSize, PlayerColor);
    }
    
    public override void _ExitTree()
    {
        if (_pendingMap != null)
        {
            _pendingMap.AfterReady -= OnPendingMapReady;
        }
        base._ExitTree();
    }
}
