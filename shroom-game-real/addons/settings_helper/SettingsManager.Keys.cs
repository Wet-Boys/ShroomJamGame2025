using SettingsHelper.SettingsEntries;

namespace SettingsHelper
{
	public partial class SettingsManager
	{
		public static class Keys
		{
			public const string GameplayMouseHorizontalSensitivity = "gameplay.mouse.horizontal_sensitivity";
			public const string GameplayMouseVerticalSensitivity = "gameplay.mouse.vertical_sensitivity";
			public const string CameraFov = "camera.fov";
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
	}
}