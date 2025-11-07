using Godot;
using Tomlet.Models;

namespace SettingsHelper.SettingsEntries;

[Tool]
[GlobalClass]
public partial class BoolSettingsEntry : SettingsEntryTyped<bool>
{
    [Export]
    protected override bool DefaultValue { get; set; }
    
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

        if (tomlValue is TomlBoolean tomlBoolean)
        {
            Value = tomlBoolean.Value;
            return;
        }

        Value = DefaultValue;
    }
}