using System;
using System.Threading.Tasks;
using Geranium.Reflection;
using Godot;

namespace Ioi.Widgets.Menu;

public partial class MainMenu : Control
{
	[Export]
	public bool IsInGame { get; set; } = false;
	
	public Action Back;
	
	private Button _newGameButton;
	private Button _loadButton;
	private Button _backButton;
	private Button _exitButton;
	
	public override void _Ready()
	{
		_newGameButton = GetNode<Button>("CenteredButtons/MenuButtons/NewGameButton");
		_loadButton = GetNode<Button>("CenteredButtons/MenuButtons/LoadGameButton");
		_backButton = GetNode<Button>("CenteredButtons/MenuButtons/BackButton");
		_exitButton = GetNode<Button>("CenteredButtons/MenuButtons/ExitButton");
		
		_newGameButton.Pressed += OnNewGamePressed;
		_loadButton.Pressed += OnLoadPressed;
		_backButton.Pressed += OnBackPressed;
		_exitButton.Pressed += OnExitPressed;

		if (IsInGame)
		{
			_backButton.Visible = true;
			MouseFilter = MouseFilterEnum.Stop;
		}
		else
		{
			_backButton.Visible = false;
			MouseFilter = MouseFilterEnum.Pass;
		}
	}
	
	private async void OnNewGamePressed()
	{
		await SceneManager.SwitchScreen("res://Scenes/Game/main_game.tscn",NewRoguelikeAsync);
	}
	
    private static async Task NewRoguelikeAsync(Node scene)
    {
		Global.GameWorld.Player = Global.SpawnSystem.SpawnCharacter("Comebached","Human","Warrior");
		scene.As<MainGame>().Init("res://Scenes/Game/Map/Locations/Mraumir/mraumir.tscn");
    }
	
	static int count=1;
	private void OnLoadPressed()
	{
		GD.Print("Загрузка игры");
		count++;
		
		var result = Global.Strings.Get("UI_MONSTERS", "UI_MONSTERS_PLURAL", count,("count",count));
		
		_loadButton.Text = result;
		
		GD.Print(result);
	}
	
	private void OnBackPressed()
	{
		Back?.Invoke();
	}
	
	private void OnExitPressed()
	{
		GetTree().Quit();
	}
}
