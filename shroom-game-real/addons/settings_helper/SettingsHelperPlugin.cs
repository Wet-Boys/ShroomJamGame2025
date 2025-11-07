#if TOOLS
using System.Collections.Generic;
using Godot;

namespace SettingsHelper;

[Tool]
public partial class SettingsHelperPlugin : EditorPlugin
{
	private static readonly HashSet<string> RegisteredCustomTypes = [];

	public static string GetPathRelativeToAddon(string path) => AddonResourceLoader.GetFullPath(path);

	public override void _EnterTree()
	{
		RegisteredCustomTypes.Clear();
		
		RegisterCustomType("SettingsManager", "Node", "SettingsManager.cs", "Tools.svg");
		RegisterCustomType("BoolSettingsEntry", "Resource", "SettingEntries/BoolSettingsEntry.cs", "bool.svg");
		RegisterCustomType("FloatSettingsEntry", "Resource", "SettingEntries/FloatSettingsEntry.cs", "float.svg");
		RegisterCustomType("IntSettingsEntry", "Resource", "SettingEntries/IntSettingsEntry.cs", "int.svg");
		
		AddAutoloadSingleton("SettingsManagerSingleton", AddonResourceLoader.GetFullPath("SettingsManager Singleton.tscn"));
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton("SettingsManagerSingleton");
		
		foreach (var registeredCustomType in RegisteredCustomTypes)
			RemoveCustomType(registeredCustomType);
		RegisteredCustomTypes.Clear();
	}

	private void RegisterCustomType(string type, string @base, string scriptPath, string iconPath)
	{
		var script = AddonResourceLoader.Load<Script>($"{scriptPath}");
		var icon = AddonResourceLoader.Load<Texture2D>($"Icons/{iconPath}");
		
		AddCustomType(type, @base, script, icon);
		RegisteredCustomTypes.Add(type);
	}
}
#endif
