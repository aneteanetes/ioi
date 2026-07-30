using Godot;
using System.Collections.Generic;
using System.Text;

public class DrawText
{
    private readonly struct TagInfo
    {
        public string Name { get; }
        public string FullOpenTag { get; }

        public TagInfo(string name, string fullOpenTag)
        {
            Name = name;
            FullOpenTag = fullOpenTag;
        }
    }

    private readonly StringBuilder _text = new();
    private readonly StringBuilder _unformatText = new();
    
    private readonly LinkedList<TagInfo> _activeTags = new();

    private DrawText() { }

    public static DrawText Create(string text, Color color = default)
    {
        var dt = new DrawText();
        if (color != default && color != Colors.Transparent)
            dt.Color(color);

        dt.Append(text);
        return dt;
    }
    
    public DrawText Color(Color color)
    {
        OpenTag("color", $"color={color.ToHexString()}");
        return this;
    }

    public DrawText Font(string fontPath)
    {
        OpenTag("font", $"font={fontPath}");
        return this;
    }

    public DrawText Size(int size)
    {
        OpenTag("font_size", $"font_size={size}");
        return this;
    }
    
    public DrawText Stroke(int thickness, Color color = default)
    {
        string colorHex = color == default ? "#000000" : $"{color.ToHexString()}";
        OpenTag("outline", $"outline_size={thickness} outline_color={colorHex}");
        return this;
    }
    
    public DrawText Offset(int offsetInPixels)
    {
        OpenTag("v_offset", $"v_offset={offsetInPixels}");
        return this;
    }

    public DrawText Append(string text)
    {
        _text.Append(text);
        _unformatText.Append(text);
        return this;
    }

    public DrawText Append(IEnumerable<string> texts)
    {
        foreach (var text in texts) Append(text);
        return this;
    }

    public DrawText AppendSpace()
    {
        _text.Append(' ');
        _unformatText.Append(' ');
        return this;
    }

    public DrawText Append(DrawText text)
    {
        _text.Append(text._text);
        _unformatText.Append(text._unformatText);
        return this;
    }

    public DrawText AppendLine(DrawText text = default)
    {
        _text.Append('\n');
        _unformatText.Append('\n');
        if (text != default) Append(text);
        return this;
    }

    public DrawText AppendLine(string text = default)
    {
        _text.Append('\n');
        _unformatText.Append('\n');
        if (text != default) Append(text);
        return this;
    }

    public DrawText AppendSpace(int sizeInPixels)
    {
        _text.Append($"[font_size={sizeInPixels}] [/font_size]");
        return this;
    }

    public DrawText Underline()
    {
        OpenTag("u", "u");
        return this;
    }

    public DrawText StrikeThrough()
    {
        OpenTag("s", "s");
        return this;
    }

    public DrawText ResetStyle() => CloseTag("u").CloseTag("s");
    public DrawText ResetOffset() => CloseTag("v_offset");
    public DrawText ResetFont() => CloseTag("font").CloseTag("font_size");
    public DrawText ResetColor() => CloseTag("color");
    public DrawText ResetEffects() => CloseTag("fade").CloseTag("outline");

    public DrawText ResetAll()
    {
        while (_activeTags.Count > 0)
        {
            _text.Append($"[/{_activeTags.Last.Value.Name}]");
            _activeTags.RemoveLast();
        }
        return this;
    }

    public DrawText AppendImage(string imagePath, int width = 0, int height = 0)
    {
        if (width > 0 && height > 0)
            _text.Append($"[img={width}x{height}]{imagePath}[/img]");
        else
            _text.Append($"[img]{imagePath}[/img]");
        return this;
    }

    private void OpenTag(string name, string fullOpenTag)
    {
        _text.Append($"[{fullOpenTag}]");
        _activeTags.AddLast(new TagInfo(name, fullOpenTag));
    }

    private DrawText CloseTag(string tagName)
    {
        var node = _activeTags.Last;
        
        while (node != null)
        {
            if (node.Value.Name == tagName)
            {
                var current = _activeTags.Last;
                var tagsToReopen = new Stack<TagInfo>();

                while (current != node)
                {
                    _text.Append($"[/{current.Value.Name}]");
                    tagsToReopen.Push(current.Value);
                    current = current.Previous;
                }

                _text.Append($"[/{tagName}]");

                var nextNode = node.Next;
                _activeTags.Remove(node);

                while (tagsToReopen.Count > 0)
                {
                    var tag = tagsToReopen.Pop();
                    _text.Append($"[{tag.FullOpenTag}]");
                }
                
                break;
            }
            node = node.Previous;
        }
        return this;
    }

    public override string ToString()
    {
        var result = new StringBuilder(_text.ToString());
        
        var node = _activeTags.Last;
        while (node != null)
        {
            result.Append($"[/{node.Value.Name}]");
            node = node.Previous;
        }
        return result.ToString();
    }
    
    public string ToUnformatString() => _unformatText.ToString();

    public override int GetHashCode() => _text.ToString().GetHashCode();

    public override bool Equals(object obj) => obj is DrawText dt && _text.ToString().Equals(dt._text.ToString());

    public static implicit operator string(DrawText text)=>text.ToString();
}