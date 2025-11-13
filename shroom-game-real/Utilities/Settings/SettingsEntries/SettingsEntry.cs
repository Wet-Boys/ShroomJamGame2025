using Godot;
using Tomlet.Models;

namespace ShroomGameReal.Utilities.Settings.SettingsEntries;

[Tool]
[GlobalClass]
public abstract partial class SettingsEntry : Resource
{
    [Export]
    public string Key { get; set; } = string.Empty;
    
    public object BoxedValue { get; set; }
    
    public abstract void Serialize(TomlDocument document);
    
    public abstract void Deserialize(TomlDocument document);
}