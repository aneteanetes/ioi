using Godot;
using ioi;
using System;

public partial class GameLog : RichTextLabel
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Global.GameLog = this;
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void Log(string text)
	{
		var txt = DrawText.Create("")
			.Color(Global.BaseColor)
			.Size(16)
			.Append($"{DateTime.Now:HH:mm:ss}: ")
			.ResetAll()
			.Append($"{text}\n");

		AppendText(txt.ToString());
		CallDeferred(MethodName.ScrollToLine,GetLineCount());
	}
	
	private void ScrollToBottom()
	{
		VScrollBar vScroll = GetVScrollBar();
		if (vScroll != null)
		{
			vScroll.Value = vScroll.MaxValue;
		}
	}
	
	public void Log(DrawText drawText)
		=> Log(drawText.ToString());
}
