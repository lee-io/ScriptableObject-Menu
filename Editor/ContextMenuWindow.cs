using UnityEditor;
using UnityEngine;

namespace ScriptableObjectMenu
{
	internal sealed class ContextMenuWindow : EditorWindow
	{
		/// <summary>
		/// The context menu to display.
		/// </summary>
		private GenericMenu m_Menu;

		public static void Display (GenericMenu menu)
		{
			var window = CreateInstance<ContextMenuWindow>();
			window.position = new Rect(0f, 0f, 2f, 2f);
			window.m_Menu = menu;
			window.ShowPopup();
		}

		private void OnGUI ()
		{
			m_Menu?.ShowAsContext();
			Close();
		}
	}
}