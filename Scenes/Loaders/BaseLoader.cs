using Godot;
using System;
using System.Threading.Tasks;
using static Godot.Control;

public partial class BaseLoader : CanvasLayer
{
	private ColorRect _back;
	private TextureRect _icon;
	private Label _label;
	private double _timePassed;
	
	public override void _Ready()
	{
		_back ??= GetNode<ColorRect>("BaseLoader");
		_icon ??= GetNode<TextureRect>("BaseLoader/LoadingContainer/Icon");
		_label ??= GetNode<Label>("BaseLoader/LoadingContainer/LoadText");
		_label.Text = Global.Strings.Get("LOADING");
		
		 if (_icon != null && _icon.Texture != null)
		{
			Vector2 originalSize = _icon.Texture.GetSize();
			_icon.PivotOffset = originalSize / 2; // Ровно половина ширины и высоты
		}
		
		_back.Color = Colors.Black;
		_back.SetAnchorsPreset(LayoutPreset.FullRect);
		_back.Modulate = new Color(1, 1, 1, 0); 
	}
	
	public override void _Process(double delta)
	{
		if (_icon != null)
		{
			_icon.Rotation += 1.5f * (float)delta;
		}
		
		// Анимация точек «...» каждые 250 миллисекунд
		_timePassed += delta;
		if (_timePassed >= 0.25)
		{
			_timePassed = 0;
			if (_label.Text.EndsWith("...."))
				_label.Text = Global.Strings.Get("LOADING");
			else
				_label.Text += ".";
		}
	}
	
	public async Task FadeIn()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(_back, "modulate:a", 1.0f, 0.5f); 
		await ToSignal(tween, Tween.SignalName.Finished);
	}
	
	public async Task FadeOut()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(_back, "modulate:a", 0.0f, 0.5f);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
}
