using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectMenu
{
	internal sealed class CloneAssetAction : MenuActionBase
	{
		/// <summary>
		/// The path of the asset to be cloned.
		/// </summary>
		private string m_AssetPath;

		[MenuItem(EDITOR_CREATE_MENU_PATH + "Clone", true, EDITOR_CREATE_MENU_PRIORITY + EDITOR_CREATE_MENU_SEPARATOR)]
		private static bool Validate ()
		{
			return m_Settings.IsValid && IsScriptableObject(Selection.activeObject);
		}

		[MenuItem(EDITOR_CREATE_MENU_PATH + "Clone", false, EDITOR_CREATE_MENU_PRIORITY + EDITOR_CREATE_MENU_SEPARATOR)]
		private static void Initiate ()
		{
			var path = AssetDatabase.GetAssetPath(Selection.activeObject);
			InvokeAction<CloneAssetAction>(path, EDITOR_ASSET_ICON_RESOURCE).m_AssetPath = path;
		}

		protected sealed override void Execute (string path)
		{
			if (!AssetDatabase.CopyAsset(m_AssetPath, path))
			{
				throw new Exception($"Failed to clone asset at path '{path}'.");
			}

			var asset = UpdateAssetDatabase<ScriptableObject>(path);

			if (asset == null)
			{
				throw new Exception($"Failed to clone asset at path '{path}'.");
			}

			PingProjectWindow(asset);

			if (m_Settings.LogOnSuccess)
			{
				Debug.Log($"Asset cloned at path '{path}'");
			}
		}
	}
}