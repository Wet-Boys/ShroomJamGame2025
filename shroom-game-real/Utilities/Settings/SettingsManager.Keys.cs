using ShroomGameReal.Utilities.Settings.SettingsEntries;

namespace ShroomGameReal.Utilities.Settings
{
	public partial class SettingsManager
	{
		public static class Keys
		{
			public const string GameplayMouseHorizontalSensitivity = "gameplay.mouse.horizontal_sensitivity";
			public const string GameplayMouseVerticalSensitivity = "gameplay.mouse.vertical_sensitivity";
			public const string CameraFov = "camera.fov";
			public const string VolumeMaster = "volume.master";
			public const string VolumeMusic = "volume.music";
			public const string VolumeSfx = "volume.sfx";
		}
		public static partial class Gameplay
		{
			public static partial class Mouse
			{
				public static FloatSettingsEntry HorizontalSensitivity => (FloatSettingsEntry)Instance.GetSettingsEntry("gameplay.mouse.horizontal_sensitivity");
				public static FloatSettingsEntry VerticalSensitivity => (FloatSettingsEntry)Instance.GetSettingsEntry("gameplay.mouse.vertical_sensitivity");
			}
		}
		public static partial class Camera
		{
			public static FloatSettingsEntry Fov => (FloatSettingsEntry)Instance.GetSettingsEntry("camera.fov");
		}
		public static partial class Volume
		{
			public static FloatSettingsEntry Master => (FloatSettingsEntry)Instance.GetSettingsEntry("volume.master");
			public static FloatSettingsEntry Music => (FloatSettingsEntry)Instance.GetSettingsEntry("volume.music");
			public static FloatSettingsEntry Sfx => (FloatSettingsEntry)Instance.GetSettingsEntry("volume.sfx");
		}
	}
}