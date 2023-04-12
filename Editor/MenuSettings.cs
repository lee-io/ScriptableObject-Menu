using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScriptableObjectMenu
{
	[FilePath(PROJECT_SETTINGS_FILEPATH, FilePathAttribute.Location.ProjectFolder)]
	internal sealed class MenuSettings : ScriptableSingleton<MenuSettings>
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
		/// The path to the project settings file.
		/// </summary>
		private const string PROJECT_SETTINGS_FILEPATH = "ProjectSettings/ScriptableObjectMenuSettings.asset";

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

		private static T TryLoadAsset<T> (string name = null) where T : Object
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

		public void Reset ()
		{
			LogOnSuccess = true;
			SortItemsByName = true;
			GroupItemsByAssembly = true;
			IncludeSelectedTypes = true;
			m_ExcludeAssemblies = new(m_DefaultExcludeAssemblies);

			OpenAfterCreation = true;
			DefaultScriptName = DEFAULT_SCRIPT_NAME;
			TemplateIdentifierTag = DEFAULT_IDENTIFIER_TAG;
			TemplateAssetFile = TryLoadAsset<TextAsset>(DEFAULT_TEMPLATE_FILENAME);

			OnValidate();
			Save();
		}

		private void OnValidate ()
		{
			Validate();

			ExcludeAssemblies = new HashSet<string>(m_ExcludeAssemblies)
			{
				nameof(ScriptableObjectMenu)
			};

			IsDirty = true;
		}

		public bool Validate (ICollection<string> messages = null)
		{
			IsValid = true;

			if (string.IsNullOrWhiteSpace(DefaultScriptName))
			{
				IsValid = false;
				messages?.Add($"'{nameof(DefaultScriptName)}' is empty");
			}

			if (TemplateAssetFile == null)
			{
				IsValid = false;
				messages?.Add($"'{nameof(TemplateAssetFile)}' is null");
			}
			else if (string.IsNullOrWhiteSpace(TemplateIdentifierTag))
			{
				IsValid = false;
				messages?.Add($"'{nameof(TemplateIdentifierTag)}' is empty");
			}
			else if (!TemplateAssetFile.text.Contains(TemplateIdentifierTag))
			{
				IsValid = false;
				messages?.Add($"'{TemplateAssetFile.name}' does not contain tag '{TemplateIdentifierTag}'");
			}

			return IsValid;
		}

		public void Save ()
		{
			if (IsValid)
			{
				Save(true);
			}
		}
	}
}