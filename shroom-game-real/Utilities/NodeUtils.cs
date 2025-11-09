using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ShroomGameReal.Utilities;

public static class NodeUtils
{
    public static IEnumerable<T> GetChildren<T>(this Node node) => node.GetChildren().OfType<T>();
    
    public static IEnumerable<T> GetChildrenRecursively<T>(this Node node) where T : Node
    {
        foreach (var child in node.GetChildren())
        {
            if (child is T t)
                yield return t;

            foreach (var subChild in child.GetChildrenRecursively<T>())
                yield return subChild;
        }
    }
}