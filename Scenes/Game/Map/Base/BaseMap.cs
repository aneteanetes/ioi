using Godot;
using ioi.Source.Scenes;
using ioi.Tiled.Map;
using System;

public partial class BaseMap : Node2D, IViewportScene
{
	[Export] public float ZoomSpeed = 0.1f;
	[Export] public float MinZoom = 1.5f;
	[Export] public float MaxZoom = 4.0f;
	
	// lighting
	
	[Export] public TileMapLayer TileMap { get; set; }
    [Export] public CharacterBody2D Player { get; set; }
    [Export] public Node2D EnemiesContainer { get; set; }
    [Export] public ColorRect FogColorRect { get; set; }
    [Export] public CanvasModulate DayNightLight { get; set; }
    [Export] public int BaseViewRadius = 4;
    [Export] public int TileSize = 16;
    
    private int _currentRadius;
    private int _mapWidth;
    private int _mapHeight;
    private Vector2I _mapOrigin;
	
	/// <summary>
	/// Light map
	/// <para>0 - darkness</para>
	/// <para>1 - shadow</para>
	/// <para>2 - light</para>
	/// </summary>
    private byte[,] _fogGrid; 
    
    private Image _fogImage;
    private ImageTexture _fogTexture;
    private ShaderMaterial _fogMaterial;
    private float _time = 0.0f;
	
	public override void _Ready()
    {
        CalculateMap();
        InitShader();

        _currentRadius = BaseViewRadius;
    }
    
    private void CalculateMap()
    {
        Rect2I mapBounds = TileMap.GetUsedRect();
        _mapOrigin = mapBounds.Position;
        _mapWidth = mapBounds.Size.X > 0 ? mapBounds.Size.X : 100;
        _mapHeight = mapBounds.Size.Y > 0 ? mapBounds.Size.Y : 100;

        Vector2 targetPos = new Vector2(_mapOrigin.X * TileSize, _mapOrigin.Y * TileSize);
        Vector2 targetSize = new Vector2(_mapWidth * TileSize, _mapHeight * TileSize);

        Callable.From(() =>
        {
            FogColorRect.Position = targetPos;
            FogColorRect.Size = targetSize;
        }).CallDeferred();
    }
    
    private void InitShader()
    {
        _fogGrid = new byte[_mapWidth, _mapHeight];
        _fogMaterial = (ShaderMaterial)FogColorRect.Material;

        _fogImage = Image.CreateEmpty(_mapWidth, _mapHeight, false, Image.Format.R8);
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
		Vector2I playerTile = TileMap.LocalToMap(TileMap.ToLocal(Player.GlobalPosition)) - _mapOrigin;
		
		for (int x = 0; x < _mapWidth; x++) 
		{
			for (int y = 0; y < _mapHeight; y++) 
			{
				if (_fogGrid[x, y] == 2) _fogGrid[x, y] = 1;
			}
		}
		
		if (playerTile.X >= 0 && playerTile.X < _mapWidth && playerTile.Y >= 0 && playerTile.Y < _mapHeight)
		{
			_fogGrid[playerTile.X, playerTile.Y] = 2;
		}
		
		//raycast
		
		int r = _currentRadius;
		
		for (int x = -r; x <= r; x++)
		{
			CastRay(playerTile, playerTile + new Vector2I(x, -r));
			CastRay(playerTile, playerTile + new Vector2I(x, r));
		}
		for (int y = -r; y <= r; y++)
		{
			CastRay(playerTile, playerTile + new Vector2I(-r, y));
			CastRay(playerTile, playerTile + new Vector2I(r, y));
		}
		
		UpdateShaderTexture();
		ObjectsVisibility();
	}

	private void CastRay(Vector2I start, Vector2I end)
	{
		int dx = Math.Abs(end.X - start.X);
		int dy = Math.Abs(end.Y - start.Y);
		int sx = start.X < end.X ? 1 : -1;
		int sy = start.Y < end.Y ? 1 : -1;
		int err = dx - dy;

		int cx = start.X;
		int cy = start.Y;

		while (true)
		{
			if (cx >= 0 && cx < _mapWidth && cy >= 0 && cy < _mapHeight)
			{
				_fogGrid[cx, cy] = 2;

				if (IsWall(cx, cy))
				{
					break; 
				}
			}
			else
			{
				break; // edge of map
			}

			if (cx == end.X && cy == end.Y) break;

			int e2 = 2 * err;
			if (e2 > -dy)
			{
				err -= dy;
				cx += sx;
			}
			if (e2 < dx)
			{
				err += dx;
				cy += sy;
			}
		}
	}
    
	private bool IsWall(int tileX, int tileY)
	{
		Vector2I globalTilePos = new Vector2I(tileX, tileY) + _mapOrigin;
		
		TileData tileData = TileMap.GetCellTileData(globalTilePos);
		
		return tileData != null;
	}

	private void UpdateShaderTexture()
    {
        for (int x = 0; x < _mapWidth; x++) 
        {
            for (int y = 0; y < _mapHeight; y++) 
            {
                float colorValue = _fogGrid[x, y] == 2 ? 1.0f : (_fogGrid[x, y] == 1 ? 0.5f : 0.0f);
                _fogImage.SetPixel(x, y, new Color(colorValue, 0, 0));
            }
        }
        _fogTexture.Update(_fogImage);
    }

    private void ObjectsVisibility()
    {
        if (EnemiesContainer == null) return;
        
        foreach (Node enemy in EnemiesContainer.GetChildren()) 
        {
            if (enemy is Node2D enemy2D) 
            {
                // Переводим позицию врага в координаты сетки с учетом смещения карты
                Vector2I enemyTile = TileMap.LocalToMap(TileMap.ToLocal(enemy2D.GlobalPosition)) - _mapOrigin;
                
                if (enemyTile.X >= 0 && enemyTile.X < _mapWidth && enemyTile.Y >= 0 && enemyTile.Y < _mapHeight) 
                {
                    // Враг виден, только если тайл под ним имеет статус 2 (свет сейчас)
                    enemy2D.Visible = (_fogGrid[enemyTile.X, enemyTile.Y] == 2);
                } 
                else 
                {
                    enemy2D.Visible = false;
                }
            }
        }
    }
    
    public byte[] GetFogSaveData() 
    {
        byte[] flatArray = new byte[_mapWidth * _mapHeight];
        Buffer.BlockCopy(_fogGrid, 0, flatArray, 0, flatArray.Length);
        return flatArray;
    }
    
    public void LoadFogSaveData(byte[] loadedData) 
    {
        if (loadedData == null || loadedData.Length != _mapWidth * _mapHeight) return;
        Buffer.BlockCopy(loadedData, 0, _fogGrid, 0, loadedData.Length);
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
