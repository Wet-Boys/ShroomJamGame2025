using Godot;
using Tomlet.Models;

namespace ShroomGameReal.Utilities.Settings.SettingsEntries;

[Tool]
[GlobalClass]
public partial class FloatSettingsEntry : ScalarSettingsEntry<float>
{
    [Export]
    protected override float DefaultValue { get; set; }

    [Export]
    public override float Min { get; set; }

    [Export]
    public override float Max { get; set; }

    public override void Serialize(TomlDocument document)
    {
        document.Put(Key, Value);
    }

    public override void Deserialize(TomlDocument document)
    {
        if (!document.ContainsKey(Key))
        {
            GD.PushWarning($"Key '{Key}' not found! skipping entry!");
            return;
        }
        
        var tomlValue = document.GetValue(Key);

        if (tomlValue is TomlDouble tomlDouble)
        {
            Value = (float)tomlDouble.Value;
            return;
        }
        
        Value = DefaultValue;
    }
}