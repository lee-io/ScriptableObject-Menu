using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScriptableObjectMenu
{
	internal abstract class MenuActionBase : EndNameEditAction
	{
		/// <summary>
		/// The Editor's Create Asset menu separator offset.
		/// </summary>
		protected const int EDITOR_CREATE_MENU_SEPARATOR = 12;

		/// <summary>
		/// The Editor's Create Asset menu position.
		/// </summary>
		protected const int EDITOR_CREATE_MENU_PRIORITY = 81 - EDITOR_CREATE_MENU_SEPARATOR;

		/// <summary>
		/// The Editor's Create Asset menu path.
		/// </summary>
		protected const string EDITOR_CREATE_MENU_PATH = "Assets/Create/Scriptable Object/";

		/// <summary>
		/// The Editor's asset icon resource name.
		/// </summary>
		protected const string EDITOR_ASSET_ICON_RESOURCE = "ScriptableObject Icon";

		/// <summary>
		/// The Editor's script icon resource name.
		/// </summary>
		protected const string EDITOR_SCRIPT_ICON_RESOURCE = "cs Script Icon";

		/// <summary>
		/// The Editor's extension for asset files.
		/// </summary>
		protected const string EDITOR_ASSET_FILE_EXTENSION = "asset";

		/// <summary>
		/// The Editor's extension for script files.
		/// </summary>
		protected const string EDITOR_SCRIPT_FILE_EXTENSION = "cs";

		/// <summary>
		/// The settings instance cache.
		/// </summary>
		protected static readonly MenuSettings m_Settings = MenuSettings.instance;

		protected static T InvokeAction<T> (string path, string resource) where T : MenuActionBase
		{
			var action = CreateInstance<T>();
			var icon = EditorGUIUtility.IconContent(resource).image as Texture2D;
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, path, icon, null);
			return action;
		}

		protected static T UpdateAssetDatabase<T> (string path) where T : Object
		{
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate |
				ImportAssetOptions.ForceSynchronousImport);
			return AssetDatabase.LoadAssetAtPath<T>(path);
		}

		protected static void PingProjectWindow (Object asset)
		{
			ProjectWindowUtil.ShowCreatedAsset(asset);
			EditorGUIUtility.PingObject(asset);
		}

		protected static bool IsScriptableObject (Object asset)
		{
			return
			asset is ScriptableObject &&
			asset is not EditorWindow &&
			asset is not Editor;
		}

		protected static bool IsScriptableObject (Type type)
		{
			return
			type.IsClass &&
		   !type.IsNested &&
		   !type.IsAbstract &&
		   !type.IsGenericType &&
			type.IsSubclassOf(typeof(ScriptableObject)) &&
		   !type.IsSubclassOf(typeof(EditorWindow)) &&
		   !type.IsSubclassOf(typeof(Editor));
		}

		protected static bool IsAssemblyIncluded (Assembly assembly)
		{
			var root = GetAssemblyRootName(assembly);
			return !m_Settings.ExcludeAssemblies.Contains(root);
		}

		protected static string GetAssemblyRootName (Assembly assembly)
		{
			var name = assembly.GetName().Name;
			var index = name.IndexOf('.');
			return index < 0 ? name : name[..index];
		}

		protected static int CompareAlphaNumeric (string left, string right)
		{
			return string.Compare(left, right, StringComparison.InvariantCultureIgnoreCase);
		}

		protected abstract void Execute (string path);

		public sealed override void Action (int id, string path, string resource)
		{
			try
			{
				Execute(path);
			}
			catch (Exception e)
			{
				EditorUtility.DisplayDialog("Error", e.Message, "OK");
			}
		}
	}
}