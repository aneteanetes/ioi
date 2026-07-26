using Godot;
using ioi;

public partial class Global : Node
{
	public static GameStrings Strings {get;private set;}

	public static GameWorld GameWorld { get; set; }

	public static GameLog GameLog { get; set; }
	
	public static float TimeSpeed = 0.05f;
	
	public static bool ResetCameraMove { get; set; } = false;

    public static bool IsDebug { get; internal set; } = true;

    public static string PathToProject { get; internal set; }
    
    public override void _Ready()
	{
		Strings = new GameStrings();
		GameLog=new GameLog();
		GameWorld = new GameWorld();
	}
	
	public override void _Process(double delta)
	{
	}
}