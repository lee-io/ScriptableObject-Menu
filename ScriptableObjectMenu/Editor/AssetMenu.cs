using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectMenu
{
	internal sealed class AssetMenu : BaseMenu
	{
		private static GenericMenu m_AssetPopupMenu;

		[MenuItem(EDITOR_ASSET_MENU_PATH + "Asset", true, EDITOR_ASSET_MENU_PRIORITY)]
		internal static bool Validate ()
		{
			// Disabled during domain reload
			return !EditorApplication.isCompiling;
		}

		[MenuItem(EDITOR_ASSET_MENU_PATH + "Asset", false, EDITOR_ASSET_MENU_PRIORITY)]
		internal static void Initiate ()
		{
			// Populate Asset menu
			if (m_AssetPopupMenu == null)
			{
				m_AssetPopupMenu = new GenericMenu();

				// Traverse domain assemblies
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					// Traverse assembly types
					foreach (var type in asm.GetTypes())
					{
						// Filter by type
						if (!type.IsValueType &&
							!type.IsInterface &&
							!type.IsAbstract)
						{
							// Filter by namespace
							if (type.Namespace != null)
							{
								var space = type.Namespace.Split('.')[0];
								if (space == "UnityEngine" ||
									space == "UnityEditor" ||
									space == "UnityEditorInternal")
								{
									continue;
								}
							}

							// Filter by class
							if (IsScriptableObject(type))
							{
								// Convert namespace to path
								var path = type.FullName.Replace('.', '/');

								// Append type to menu
								m_AssetPopupMenu.AddItem(new GUIContent(path), false, () =>
								{
									CreateAsset(type);
								});
							}
						}
					}
				}
			}

			// Display when populated
			if (m_AssetPopupMenu.GetItemCount() > 0)
			{
				var window = CreateInstance<AssetMenu>();
				window.position = new Rect(0f, 0f, 2f, 2f);
				window.ShowPopup();
			}

			// Otherwise alert user
			else if (EditorUtility.DisplayDialog("Error", "No Scriptable Object Found", "OK"))
			{
				// And display the Script menu
				ScriptMenu.Initiate();
			}
		}

		private static void CreateAsset (Type type)
		{
			// Create asset instance
			var asset = CreateInstance(type);

			if (asset != null)
			{
				// Display save dialog
				var path = EditorUtility.SaveFilePanelInProject("Save Asset", type.Name, "asset", string.Empty);

				if (path.Length > 0)
				{
					// Save asset to file
					AssetDatabase.CreateAsset(asset, path);
					AssetDatabase.SaveAssets();

					UpdateAssetDatabase(type, path);

					// Log on complete
					Debug.Log($"Asset created at \"{path}\"");
				}
			}
			else
			{
				// Alert on type error
				EditorUtility.DisplayDialog("Error", $"Invalid Asset\n\n\"{type.Name}\"", "OK");
			}
		}

		private static bool IsScriptableObject (Type type)
		{
			// Get base
			type = type.BaseType;

			// Match type
			if (type != null &&
				type != typeof(Editor) &&
				type != typeof(EditorWindow))
			{
				if (type == typeof(ScriptableObject))
				{
					return true;
				}
				else
				{
					// Otherwise recurse
					return IsScriptableObject(type);
				}
			}

			return false;
		}

		private void OnGUI ()
		{
			// Display popup
			m_AssetPopupMenu.ShowAsContext();

			// Close this Window
			Close();
		}
	}
}