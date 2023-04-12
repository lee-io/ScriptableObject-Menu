using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScriptableObjectMenu
{
	internal sealed class CreateAssetAction : MenuActionBase
	{
		/// <summary>
		/// The object HideFlags that are incompatible with creation.
		/// </summary>
		private const HideFlags INVALID_HIDE_FLAGS = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

		/// <summary>
		/// The Asset Database selection find filter.
		/// </summary>
		private const string SELECTION_FIND_FILTER = "t:" + nameof(ScriptableObject) + " t:" + nameof(MonoScript);

		/// <summary>
		/// The asset menu item cache.
		/// </summary>
		private static List<ContextMenuItem> m_ItemCache;

		/// <summary>
		/// The asset type to be created.
		/// </summary>
		private Type m_AssetType;

		[MenuItem(EDITOR_CREATE_MENU_PATH + "Asset", true, EDITOR_CREATE_MENU_PRIORITY)]
		private static bool Validate ()
		{
			return !EditorApplication.isCompiling && m_Settings.IsValid;
		}

		[MenuItem(EDITOR_CREATE_MENU_PATH + "Asset", false, EDITOR_CREATE_MENU_PRIORITY)]
		private static void Initiate ()
		{
			if (m_ItemCache == null || m_Settings.IsDirty)
			{
				m_Settings.IsDirty = false;
				GenerateItemCache();
			}

			if (m_ItemCache.Count > 0)
			{
				var menu = CreateContextMenu();
				ContextMenuWindow.Display(menu);
			}
			else if (EditorUtility.DisplayDialog("Error", "No Scriptable Object(s) found.\n\n" +
				"Would you like to create a new script?", "Create", "Cancel"))
			{
				CreateScriptAction.Initiate();
			}
		}

		private static void GenerateItemCache ()
		{
			m_ItemCache = new List<ContextMenuItem>();

#if UNITY_2019_2_OR_NEWER

			// From TypeCache
			foreach (var type in TypeCache.GetTypesDerivedFrom<ScriptableObject>())
			{
				if (IsScriptableObject(type) && IsAssemblyIncluded(type.Assembly))
				{
					var path = CreateMenuPath(type);
					var item = CreateMenuItem(type, path);
					m_ItemCache.Add(item);
				}
			}
#else
			// From Domain
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (IsAssemblyIncluded(asm))
				{
					foreach (var type in asm.GetTypes())
					{
						if (IsScriptableObject(type))
						{
							var path = CreateMenuPath(type);
							var item = CreateMenuItem(type, path);
							m_ItemCache.Add(item);
						}
					}
				}
			}
#endif
			if (m_Settings.SortItemsByName)
			{
				m_ItemCache.Sort((x, y) => CompareAlphaNumeric(x.Path, y.Path));
			}
		}

		private static GenericMenu CreateContextMenu ()
		{
			var menu = new GenericMenu { allowDuplicateNames = !m_Settings.GroupItemsByAssembly };

			// From selection
			if (m_Settings.IncludeSelectedTypes && TryGetSelectedTypes(out var types))
			{
				foreach (var type in types)
				{
					var item = CreateMenuItem(type, type.Name);
					menu.AddItem(item.Content, false, item.Function);
				}

				menu.AddSeparator(string.Empty);
			}

			// From cache
			foreach (var item in m_ItemCache)
			{
				menu.AddItem(item.Content, false, item.Function);
			}

			return menu;
		}

		private static string CreateMenuPath (Type type)
		{
			// Convert namespace to path
			var path = type.FullName.Replace('.', '/');

			// Prefix assembly name
			if (m_Settings.GroupItemsByAssembly)
			{
				path = $"{GetAssemblyRootName(type.Assembly)}/{path}";
			}

			return path;
		}

		private static ContextMenuItem CreateMenuItem (Type type, string path)
		{
			return new ContextMenuItem(path, new GUIContent(path), () =>
			{
				var name = Path.ChangeExtension(type.Name, EDITOR_ASSET_FILE_EXTENSION);
				InvokeAction<CreateAssetAction>(name, EDITOR_ASSET_ICON_RESOURCE).m_AssetType = type;
			});
		}

		private static bool TryGetSelectedTypes (out List<Type> types)
		{
			var objects = Selection.objects;

			if (objects.Length == 0)
			{
				types = null;
				return false;
			}

			types = new List<Type>();

			foreach (var obj in objects)
			{
				var path = AssetDatabase.GetAssetPath(obj);

				if (string.IsNullOrEmpty(path))
				{
					continue;
				}

				if (!AssetDatabase.IsValidFolder(path))
				{
					TryAdd(types, obj);
					continue;
				}

				foreach (var guid in AssetDatabase.FindAssets(SELECTION_FIND_FILTER, new[] { path }))
				{
					path = AssetDatabase.GUIDToAssetPath(guid);

					if (!string.IsNullOrEmpty(path))
					{
						TryAdd(types, AssetDatabase.LoadMainAssetAtPath(path));
					}
				}
			}

			if (types.Count == 0)
			{
				return false;
			}

			if (m_Settings.SortItemsByName)
			{
				types.Sort((x, y) => CompareAlphaNumeric(x.Name, y.Name));
			}

			return true;

			static void TryAdd (List<Type> types, Object obj)
			{
				Type type;

				if (obj is ScriptableObject asset)
				{
					type = asset.GetType();
				}
				else if (obj is MonoScript script)
				{
					type = script.GetClass();
				}
				else
				{
					return;
				}

				if (type != null &&
				   !types.Contains(type) &&
					IsScriptableObject(type) &&
					IsAssemblyIncluded(type.Assembly))
				{
					types.Add(type);
				}
			}
		}

		protected sealed override void Execute (string path)
		{
			var asset = CreateInstance(m_AssetType);

			if (asset == null)
			{
				throw new Exception($"Failed to create asset at path '{path}'.");
			}

			// Reject invalid HideFlags
			if ((asset.hideFlags & INVALID_HIDE_FLAGS) != 0)
			{
				DestroyImmediate(asset, true);
				throw new Exception($"Asset of type '{m_AssetType.Name}'" +
					" cannot be saved due to its 'HideFlags' settings.");
			}

			AssetDatabase.CreateAsset(asset, path);
			PingProjectWindow(asset);

			if (m_Settings.LogOnSuccess)
			{
				Debug.Log($"Asset created at path '{path}'");
			}
		}
	}
}