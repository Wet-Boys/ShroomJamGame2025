using System.IO;
using System.Runtime.CompilerServices;
using Godot;

namespace ShroomGameReal.Utilities.Settings;

internal static class AddonResourceLoader
{
    public static T Load<T>(string path, [CallerFilePath] string callerFilePath = "")
        where T : class
    {
        var parentInfo = Directory.GetParent(callerFilePath);

        if (parentInfo == null)
            return null;

        var localPath = ProjectSettings.LocalizePath(Path.GetFullPath(Path.Join(parentInfo.FullName, path)));

        return GD.Load<T>(localPath);
    }

    public static string GetFullPath(string addonLocalPath, [CallerFilePath] string callerFilePath = "")
    {
        var parentInfo = Directory.GetParent(callerFilePath);

        if (parentInfo == null)
            return $"res://{addonLocalPath}";
        
        return ProjectSettings.LocalizePath(Path.GetFullPath(Path.Join(parentInfo.FullName, addonLocalPath)));
    }
}