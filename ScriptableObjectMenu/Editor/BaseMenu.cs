using System;
using UnityEditor;

namespace ScriptableObjectMenu
{
	internal abstract class BaseMenu : EditorWindow
	{
		// The Editor's create asset menu path
		protected const string EDITOR_ASSET_MENU_PATH = "Assets/Create/Scriptable Object/";

		// The Editor's create asset menu position
		protected const int EDITOR_ASSET_MENU_PRIORITY = 82;

		protected static void UpdateAssetDatabase (Type type, string path)
		{
			// Update database
			var options = ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport;
			AssetDatabase.ImportAsset(path, options);

			// Focus asset
			var asset = AssetDatabase.LoadAssetAtPath(path, type);
			ProjectWindowUtil.ShowCreatedAsset(asset);
			EditorGUIUtility.PingObject(asset);
		}
	}
}