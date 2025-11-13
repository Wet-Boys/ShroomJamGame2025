using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using ShroomGameReal.Utilities.Settings.SettingsEntries;

namespace ShroomGameReal.Utilities.Settings.CodeGen;

internal static class SettingsEntryKeyCodeGen
{
    public static void GenerateKeys()
    {
        if (!Engine.IsEditorHint())
            return;
        
        var fullFilePath = ProjectSettings.GlobalizePath("res://Utilities/Settings/SettingsManager.Keys.cs");

        if (fullFilePath is null)
        {
            GD.PushError("Failed to get file path of SettingsManager.Keys.cs");
            return;
        }
        
        // Organize entries by class path ex: Key: gameplay.camera.fov, Class Path: Gameplay.Camera
        var classTree = new Dictionary<string, HashSet<SettingsEntry>>();
        foreach (var entry in SettingsManager.Instance.Entries)
        {
            var keyParts = entry.Key.Split('.');

            var classPath = entry.Key;
            if (keyParts.Length > 1)
            { 
                classPath = keyParts[..^1].Join(".");
            }

            if (!classTree.TryGetValue(classPath, out var entryList))
            {
                entryList = [];
                classTree.Add(classPath, entryList);
            }
            
            entryList.Add(entry);
        }

        var code = new CodeBuilder()
            .WithNamespace("ShroomGameReal.Utilities.Settings")
            .WithImports("ShroomGameReal.Utilities.Settings.SettingsEntries");

        code.AppendLine("public partial class SettingsManager\n{");

        code.AppendLine("public static class Keys\n{");
        foreach (var key in SettingsManager.Instance.EntryKeys)
        {
            code.AppendLine($"public const string {key.ToPascalCase()} = \"{key}\";");
        }
        code.AppendLine("}");

        foreach (var (classPath, entries) in classTree)
        {
            var firstEntry = entries.First();
            var fullClassPath = firstEntry.Key.Split('.');
            var classes = classPath.Split('.');
            
            if (fullClassPath.Length >= 1 && fullClassPath[0] == firstEntry.Key)
            {
                AddEntries(code, entries);
            }
            else
            {
                foreach (var className in classes)
                {
                    code.AppendLine($"public static partial class {className.ToPascalCase()}\n{{");
                }
                
                AddEntries(code, entries);

                for (int i = 0; i < classes.Length; i++)
                {
                    code.AppendLine("}");
                }
            }
        }

        code.AppendLine("}");

        if (File.Exists(fullFilePath))
            File.Delete(fullFilePath);
        
        File.WriteAllText(fullFilePath, code.ToString());
    }

    private static void AddEntries(CodeBuilder code, IEnumerable<SettingsEntry> entries)
    {
        foreach (var entry in entries)
        {
            var keyType = entry.GetType().Name;

            var keyName = entry.Key.Split('.').Last();
            
            code.AppendLine($"public static {keyType} {keyName.ToPascalCase()} => ({keyType})Instance.GetSettingsEntry(\"{entry.Key}\");");
        }
    }
}