using Geranium.Reflection;
using Godot;

public static class NodeExtensions
{
    public static T GetChildByType<T>(this Node node, bool recursive = true) where T : Node
    {
        var t = typeof(T);
        foreach (Node child in node.GetChildren())
        {
            if(child.GetType().IsAssignableTo(t))
                return child.As<T>();
            
            
            if (child is T typedChild)
                return typedChild;
            
            if (recursive && child.GetChildCount() > 0)
            {
                var result = child.GetChildByType<T>(true);
                if (result != null) return result;
            }
        }
        return null;
    }
}