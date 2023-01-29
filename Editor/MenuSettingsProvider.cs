using UnityEditor;

namespace ScriptableObjectMenu
{
	internal static class MenuSettingsProvider
	{
		/// <summary>
		/// The Editor's Project Settings window path.
		/// </summary>
		private const string PROJECT_SETTINGS_PATH = "Project/Scriptable Object Menu";

		[SettingsProvider]
		private static SettingsProvider Create ()
		{
			return new AssetSettingsProvider(PROJECT_SETTINGS_PATH, () => Editor.CreateEditor(MenuSettings.Instance), new[]
			{
				"Asset", "Script", "Scriptable Object", "ScriptableObject"
			});
		}
	}
}