using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScriptableObjectMenu
{
	internal sealed class MenuSettings : ScriptableObject
	{
		/// <summary>
		/// The default name for new scripts.
		/// </summary>
		private const string DEFAULT_SCRIPT_NAME = "NewScriptableObject";

		/// <summary>
		/// The default template's identifier tag.
		/// </summary>
		private const string DEFAULT_IDENTIFIER_TAG = "%IDENTIFIER_TAG%";

		/// <summary>
		/// The default template's filename.
		/// </summary>
		private const string DEFAULT_TEMPLATE_FILENAME = "ScriptableObjectTemplate.cs";

		/// <summary>
		/// The default assembly exclusion list.
		/// </summary>
		private static readonly List<string> m_DefaultExcludeAssemblies = new List<string>
		{
			"Unity", "UnityEngine", "UnityEditor", "System", "Mono"
		};

		[field: Header("General Settings")]

		[field: Tooltip("Determines whether successful actions should be logged to the console.")]
		[field: SerializeField] public bool LogOnSuccess { get; private set; }

		[field: Header("Asset Settings")]

		[field: Tooltip("Determines whether asset menu items should be alpha-numerically sorted.")]
		[field: SerializeField] public bool SortItemsByName { get; private set; }

		[field: Tooltip("Determines whether asset menu items should be grouped by assembly.")]
		[field: SerializeField] public bool GroupItemsByAssembly { get; private set; }

		[field: Tooltip("Determines whether selected types in the Project Window should be included in the asset menu.")]
		[field: SerializeField] public bool IncludeSelectedTypes { get; private set; }

		[field: Tooltip("Specifies assemblies to exclude from type discovery.")]
		[field: SerializeField] private List<string> m_ExcludeAssemblies;

		[field: Header("Script Settings")]

		[field: Tooltip("Determines whether new scripts should be opened with associated application.")]
		[field: SerializeField] public bool OpenAfterCreation { get; private set; }

		[field: Tooltip("Specifies the default name for new scripts.")]
		[field: SerializeField] public string DefaultScriptName { get; private set; }

		[field: Tooltip("Specifies the script template asset file.")]
		[field: SerializeField] public TextAsset TemplateAssetFile { get; private set; }

		[field: Tooltip("Specifies the script template's identifier tag.")]
		[field: SerializeField] public string TemplateIdentifierTag { get; private set; }

		/// <summary>
		/// The assembly exclusion set for O(1) lookup.
		/// </summary>
		public HashSet<string> ExcludeAssemblies { get; private set; }

		/// <summary>
		/// Specifies whether the state is valid.
		/// </summary>
		public bool IsValid { get; private set; }

		/// <summary>
		/// Specifies whether the state has changed.
		/// </summary>
		public bool IsDirty { get; set; }

		/// <summary>
		/// The singleton instance.
		/// </summary>
		private static MenuSettings m_Instance;

		/// <summary>
		/// Lazy loads or creates the singleton instance.
		/// </summary>
		public static MenuSettings Instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = TryLoadInstance<MenuSettings>();

					if (m_Instance == null)
					{
						CreateInstance();
					}
				}

				return m_Instance;
			}
		}

		private static T TryLoadInstance<T> (string name = null) where T : Object
		{
			var guid = AssetDatabase.FindAssets($"{name} t:{typeof(T).Name}");

			if (guid.Length > 0)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid[0]);

				if (!string.IsNullOrEmpty(path))
				{
					return AssetDatabase.LoadAssetAtPath<T>(path);
				}
			}

			return null;
		}

		private static void CreateInstance ()
		{
			m_Instance = CreateInstance<MenuSettings>();
			var path = $"Assets/{nameof(ScriptableObjectMenu)}Settings.asset";
			AssetDatabase.CreateAsset(m_Instance, path);
		}

		private void Reset ()
		{
			LogOnSuccess = true;
			SortItemsByName = true;
			GroupItemsByAssembly = true;
			IncludeSelectedTypes = true;
			m_ExcludeAssemblies = m_DefaultExcludeAssemblies;
			OpenAfterCreation = true;
			DefaultScriptName = DEFAULT_SCRIPT_NAME;
			TemplateIdentifierTag = DEFAULT_IDENTIFIER_TAG;
			TemplateAssetFile = TryLoadInstance<TextAsset>(DEFAULT_TEMPLATE_FILENAME);

			OnValidate();
		}

		private void OnValidate ()
		{
			IsValid = true;
			var message = string.Empty;

			if (string.IsNullOrWhiteSpace(DefaultScriptName))
			{
				IsValid &= false;
				message += $" '{nameof(DefaultScriptName)}' is empty.";
			}

			if (TemplateAssetFile == null)
			{
				IsValid &= false;
				message += $" '{nameof(TemplateAssetFile)}' is null.";
			}
			else if (string.IsNullOrWhiteSpace(TemplateIdentifierTag))
			{
				IsValid &= false;
				message += $" '{nameof(TemplateIdentifierTag)}' is empty.";
			}
			else if (!TemplateAssetFile.text.Contains(TemplateIdentifierTag))
			{
				IsValid &= false;
				message += $" '{TemplateAssetFile.name}' does not contain tag '{TemplateIdentifierTag}'.";
			}

			if (!IsValid)
			{
				Debug.LogWarning($"Invalid Scriptable Object Menu settings:{message} The Menu will be disabled.");
			}

			ExcludeAssemblies = new HashSet<string>(m_ExcludeAssemblies)
			{
				nameof(ScriptableObjectMenu)
			};

			IsDirty = true;
		}
	}
}