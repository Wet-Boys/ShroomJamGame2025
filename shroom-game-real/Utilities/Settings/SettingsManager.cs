using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ShroomGameReal.Utilities.Settings.CodeGen;
using ShroomGameReal.Utilities.Settings.SettingsEntries;
using Tomlet;
using Tomlet.Models;

namespace ShroomGameReal.Utilities.Settings;

[Tool]
[GlobalClass]
public partial class SettingsManager : Node
{
    public static SettingsManager Instance { get; private set; }

    [Export]
    private string _fileName = string.Empty;
    
    [Export]
    public SettingsEntry[] Entries { get; private set; } = [];

    private string FilePath => $"user://{_fileName}.toml";
    
    public IEnumerable<string> EntryKeys => Entries.Select(entry => entry.Key);

    public override void _Ready()
    {
        Instance = this;

        if (Engine.IsEditorHint())
            SettingsEntryKeyCodeGen.GenerateKeys();
    }

    public override void _EnterTree()
    {
        Load();
        
        Instance = this;
        
        if (Engine.IsEditorHint())
            SettingsEntryKeyCodeGen.GenerateKeys();
    }

    public void Save()
    {
        var document = TomlDocument.CreateEmpty();
        foreach (var entry in Entries)
            entry.Serialize(document);
        
        using var file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Write);
        file.StoreString(document.SerializedValue);
    }

    public void Load()
    {
        if (!FileAccess.FileExists(FilePath))
            return;

        using var file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Read);

        if (file is null)
        {
            GD.PushWarning($"Failed to open file '{FilePath}'!");
            return;
        }

        var parser = new TomlParser();
        var document = parser.Parse(file.GetAsText());
        
        foreach (var entry in Entries)
            entry.Deserialize(document);
    }

    public T GetSettingValue<[MustBeVariant] T>(string key)
        where T : IEquatable<T>
    {
        foreach (var entry in Entries)
        {
            if (entry.Key != key)
                continue;

            if (entry is SettingsEntryTyped<T> typed)
                return typed.Value;
            
            GD.PushError($"Unknown settings entry type '{key}'!");
            return default;
        }
        
        return default;
    }

    public SettingsEntry GetSettingsEntry(string key)
    {
        foreach (var entry in Entries)
        {
            if (entry.Key != key)
                continue;
            
            return entry;
        }
        
        GD.PushError($"Couldn't find settings entry '{key}'!");
        return null;
    }
}