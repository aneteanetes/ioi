using System.Threading.Tasks;
using Godot;

public partial class SceneManager : Node
{
	private static SceneManager _instance;
    public static SceneManager Instance => _instance;
    
    protected PackedScene _loadingScreenScene = GD.Load<PackedScene>("res://Scenes/Loaders/base_loader.tscn");
    
    public override void _EnterTree()
    {
        _instance = this;
    }
        
    public static async Task SwitchScreen(string targetScenePath, System.Func<Node,Task> loadingMethod = null)
    {
        // 1. Создаем и отображаем экран загрузки
        var loadingScreen = _instance._loadingScreenScene.Instantiate<BaseLoader>();
        _instance.GetTree().Root.AddChild(loadingScreen);
        
        // 2. Запускаем анимацию появления (Fade In)
        await loadingScreen.FadeIn();
        
        // 4. Загружаем саму сцену Godot (ресурсы)
        var nextScene = GD.Load<PackedScene>(targetScenePath);
        var inst = nextScene.Instantiate();

        // 3. Выполняем фоновую логику загрузки, если она есть
        if (loadingMethod != null)
        {
            await loadingMethod(inst);
        }        
        
        // 5. Меняем старый экран на новый
        _instance.GetTree().ChangeSceneToNode(inst);
        
        await _instance.ToSignal(_instance.GetTree(), SceneTree.SignalName.ProcessFrame);
                
        await loadingScreen.FadeOut();
        loadingScreen.QueueFree();
    }
}
