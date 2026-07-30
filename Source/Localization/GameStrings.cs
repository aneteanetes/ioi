using System.Collections.Generic;
using System.Text;
using Godot;

namespace ioi;

public class GameStrings
{
    public const string Empty=nameof(Empty);
    public const string NotFound=nameof(NotFound);

    public string this[string key]
    {
        get => Get(key);
    }
    
    public string Get(string key, params (string Key, object Value)[] data)
    {
        if (string.IsNullOrEmpty(key)) 
            return Empty;
        
        string template = TranslationServer.Translate(key);
        
        if (string.IsNullOrEmpty(template)) 
            return NotFound;
        
        return Replace(template,data);
    }

    public string Get(string key,string keyPlural, int count=1,params (string Key, object Value)[] data)
    {
        if (string.IsNullOrEmpty(key)) 
            return Empty;

        string template = TranslationServer.TranslatePlural(key,keyPlural,count);
        
        if (string.IsNullOrEmpty(template)) 
            return NotFound;
        
        return Replace(template,data);
    }

    public string Replace(string text, params (string Key, object Value)[] data)
    {
        if(data==null || data.Length==0)
            return text;

        var sb = new StringBuilder(text.Length + (data.Length * 10));
        sb.Append(text);
        
        for (int i = 0; i < data.Length; i++)
        {
            sb.Replace($"{{{data[i].Key}}}", data[i].Value.ToString());
        }
        
        return sb.ToString();
    }
}