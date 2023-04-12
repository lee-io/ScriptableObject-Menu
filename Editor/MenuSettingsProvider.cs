using UnityEditor;
using UnityEngine;

namespace ScriptableObjectMenu
{
	internal sealed class MenuSettingsProvider : AssetSettingsProvider
	{
		/// <summary>
		/// The Editor's Project Settings window path.
		/// </summary>
		private const string PROJECT_SETTINGS_PATH = "Project/Scriptable Object Menu";

		/// <summary>
		/// The menu settings instance.
		/// </summary>
		private readonly MenuSettings m_Settings;

		[SettingsProvider]
		private static SettingsProvider Create () => new MenuSettingsProvider(MenuSettings.instance);

		private MenuSettingsProvider (MenuSettings settings) :
			base(PROJECT_SETTINGS_PATH, () => Editor.CreateEditor(settings),
				GetSearchKeywordsFromSerializedObject(new SerializedObject(settings)))
		{
			m_Settings = settings;
		}

		public sealed override void OnTitleBarGUI ()
		{
			if (GUILayout.Button("Reset"))
			{
				m_Settings.Reset();
			}
		}
	}
}