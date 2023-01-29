using UnityEditor;
using UnityEngine;

namespace ScriptableObjectMenu
{
	internal sealed class ContextMenuItem
	{
		/// <summary>
		/// The menu item's path.
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// The menu item's text content.
		/// </summary>
		public GUIContent Content { get; private set; }

		/// <summary>
		/// The menu item's function to execute.
		/// </summary>
		public GenericMenu.MenuFunction Function { get; private set; }

		public ContextMenuItem (string path, GUIContent content, GenericMenu.MenuFunction function)
		{
			Path = path;
			Content = content;
			Function = function;
		}
	}
}