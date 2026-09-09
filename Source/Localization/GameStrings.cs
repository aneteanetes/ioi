using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    
    public string Get(string key, Dictionary<string,object> data=null)
    {
        if (string.IsNullOrEmpty(key)) 
            return Empty;
        
        string template = TranslationServer.Translate(key);
        
        if (string.IsNullOrEmpty(template)) 
            return NotFound;
        
        return Replace(template,data);
    }

    public string Get(string key,string keyPlural, int count=1,Dictionary<string,object> data=null)
    {
        if (string.IsNullOrEmpty(key)) 
            return Empty;

        string template = TranslationServer.TranslatePlural(key,keyPlural,count);
        
        if (string.IsNullOrEmpty(template)) 
            return NotFound;
        
        return Replace(template,data);
    }

    public string Replace(string text, Dictionary<string,object> data)
    {
        if(data==null || data.Count==0)
            return text;

        var sb = new StringBuilder();
        sb.Append(text);
        
        string pattern = @"\{([^}]+)\}";
        
        foreach (Match match in Regex.Matches(text, pattern))
        {
            var key = match.Value;
            if (data.TryGetValue(key, out var value))
            {
                sb.Replace("{"+key+"}",value.ToString());
            }
            else
            {
                var translatedValue = TranslationServer.Translate(key);
                sb.Replace(key,translatedValue);
            }
        }
        
        return sb.ToString();
    }
}