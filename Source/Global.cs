using Godot;
using ioi;
using ioi.Scripting;
using ioi.Systems.Roguelike;

public partial class Global : Node
{
	
	public static Color CommonColor { get; } = Color.Color8(139, 107, 86);
	public static Color CommonColorLight { get; } = Color.Color8(234, 186, 155);
	
	private static Color _baseColor = "#cfc6b8".AsColor();
	public static Color BaseColor => _baseColor;
	
	private static Color _baseColorLight = "#ebe6df".AsColor();
	public static Color BaseColorLight => _baseColorLight;
	
	public static GameStrings Strings {get;private set;}
	
	public static GameWorld GameWorld { get; set; }
    
	public static LuaScripts LuaScripts { get; private set; }
	
	public static RandomNumberGenerator Random { get; set; }

	public static StatsContainer StatsContainer {get; set;}
	
	public static GameLog GameLog { get; set; }
	
	public static CombatSystem CombatSystem {get;set;}
	
	public static ItemRandomSystem ItemRandomSystem {get; set;}

	public static float TimeSpeed = 0.05f;
	
	public static bool ResetCameraMove { get; set; } = false;
    
    public static bool IsGameOver { get; internal set; }

	public static SpawnSystem SpawnSystem { get; set; }
    public static Creature Player { get; internal set; }

    public override void _Ready()
	{
		Random = new RandomNumberGenerator();
		ItemRandomSystem=new ItemRandomSystem();
		Strings = new GameStrings();
		GameWorld = new GameWorld();
		CombatSystem = new CombatSystem();
		SpawnSystem = new SpawnSystem();
		LuaScripts = new LuaScripts();
		LuaScripts.Init();
	}
	
	public override void _Process(double delta)
	{
		LuaScripts.Update(delta);
	}
}