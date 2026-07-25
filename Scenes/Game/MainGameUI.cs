using Godot;
using System;

public partial class MainGameUI : CanvasLayer
{
	public MainGame MainGame { get; set; }

    [Export] 
	public ColorRect Blur { get; set; }
    
    public override void _Ready()
    {
        SetBlurScale(0.0f);
    }
	
	private void SetBlurScale(float value)
    {
        if (Blur?.Material is ShaderMaterial matH)
        {
            matH.SetShaderParameter("blur_scale", value);
        }
    }
    
    public void BlurIn(float duration = 0.4f)
    {
        Tween tween = CreateTween();
        
        tween.TweenMethod(Callable.From<float>(SetBlurScale), 0.0f, 1.5f, duration)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.Out);
    }
    
    public void BlurOut(float duration = 0.4f)
    {
        float currentScale = 0.0f;
        if (Blur?.Material is ShaderMaterial matH)
        {
            currentScale = (float)matH.GetShaderParameter("blur_scale");
        }
        
        Tween tween = CreateTween();
        tween.TweenMethod(Callable.From<float>(SetBlurScale), currentScale, 0.0f, duration)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.In);
    }
    
    public override void _Input(InputEvent @event)
    {
		if (@event.IsActionPressed("ui_cancel"))
		{
			if (MainGame.MainMenu.Visible)
			{
				MainGame.ToggleMainMenu();
				GetViewport().SetInputAsHandled();
			}
		}
    }
}
