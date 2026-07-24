using System.Threading.Tasks;
using Geranium.Reflection;
using Godot;

namespace Ioi.Widgets.Menu;

public partial class MainMenu : Control
{
	// [Export] позволяет переключать этот флаг прямо из редактора Godot для тестов
	[Export] public bool IsInGame { get; set; } = false;
	
	// Ссылки на кнопки в сцене (в Godot 4 NodePath генерируется автоматически или ищется через %)
	private Button _newGameButton;
	private Button _loadButton;
	private Button _backButton;
	private Button _exitButton;
	
	public override void _Ready()
	{
		// Находим кнопки на сцене по их именам
		_newGameButton = GetNode<Button>("CenteredButtons/MenuButtons/NewGameButton");
		_loadButton = GetNode<Button>("CenteredButtons/MenuButtons/LoadGameButton");
		_backButton = GetNode<Button>("CenteredButtons/MenuButtons/BackButton");
		_exitButton = GetNode<Button>("CenteredButtons/MenuButtons/ExitButton");

		// Подключаем события кликов (Замена ваших Click += ...)
		_newGameButton.Pressed += OnNewGamePressed;
		_loadButton.Pressed += OnLoadPressed;
		_backButton.Pressed += OnBackPressed;
		_exitButton.Pressed += OnExitPressed;

		// Логика отображения кнопок в зависимости от контекста
		if (IsInGame)
		{
			_backButton.Visible = true;
			// В Godot аналог модальности для мыши — режим перехвата фокуса
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
		GD.Print("Старт новой игры");
		// Ваш эквивалент: GameController.StartNewGame();
	}
	
    private static async Task NewRoguelikeAsync(Node scene)
    {
		scene.As<MainGame>().Init("res://Scenes/Game/Map/Locations/Mraumir/mraumir.tscn");
		
        //await Task.Delay(2000); 
    }
	
	static int count=1;
	private void OnLoadPressed()
	{
		GD.Print("Загрузка игры");
		count++;
		
		var result = Global.Strings.Get("UI_MONSTERS", "UI_MONSTERS_PLURAL", count,("count",count));
		
		_loadButton.Text = result;
		
		GD.Print(result);
		// Выведет строго: "Есть 5 монстров"
	}

	private void OnBackPressed()
	{
		// Если это пауза внутри игры — просто закрываем сцену меню
		QueueFree(); // Удаляет этот узел из памяти (Аналог Dispose/RemoveDesktopWidgets)
	}
	
	private void OnExitPressed()
	{
		// Замена GameController.Exit()
		GetTree().Quit();
	}
}
