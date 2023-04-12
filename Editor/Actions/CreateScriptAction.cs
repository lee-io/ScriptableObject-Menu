using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace ScriptableObjectMenu
{
	internal sealed class CreateScriptAction : MenuActionBase
	{
		/// <summary>
		/// The Regex pattern for identifiers.
		/// </summary>
		private const string REGEX_IDENTIFIER_PATTERN = "^[A-Z_][\\w]*$";

		[MenuItem(EDITOR_CREATE_MENU_PATH + "Script", true, EDITOR_CREATE_MENU_PRIORITY + 1)]
		private static bool Validate ()
		{
			return !EditorApplication.isCompiling && m_Settings.IsValid;
		}

		[MenuItem(EDITOR_CREATE_MENU_PATH + "Script", false, EDITOR_CREATE_MENU_PRIORITY + 1)]
		public static void Initiate ()
		{
			var path = Path.ChangeExtension(m_Settings.DefaultScriptName, EDITOR_SCRIPT_FILE_EXTENSION);
			InvokeAction<CreateScriptAction>(path, EDITOR_SCRIPT_ICON_RESOURCE);
		}

		private static bool TypeExistsInAssembly (string path, string name)
		{
			var pasm = CompilationPipeline.GetAssemblyNameFromScriptPath(path);
			var qasm = Assembly.CreateQualifiedName(pasm, name);
			return Type.GetType(qasm, false, true) != null;
		}

		private static void CreateScriptFromTemplate (string path, string name)
		{
			try
			{
				// Set identifier
				var source = m_Settings.TemplateAssetFile.text;
				var script = source.Replace(m_Settings.TemplateIdentifierTag, name);

				// Save file
				var utf8 = new UTF8Encoding(true);
				File.WriteAllText(path, script, utf8);
			}
			catch
			{
				// Clean up
				if (File.Exists(path))
				{
					File.Delete(path);
				}

				throw;
			}
		}

		protected sealed override void Execute (string path)
		{
			// Get name from path
			var name = Path.GetFileNameWithoutExtension(path);

			// Validate identifier
			if (!Regex.IsMatch(name, REGEX_IDENTIFIER_PATTERN))
			{
				throw new Exception($"Invalid script name '{name}'.\n\n" +
					"Names must begin with a capital letter and contain only letters, digits or underscores.");
			}

			// Deconflict type
			if (TypeExistsInAssembly(path, name))
			{
				throw new Exception($"Type '{name}' already exists.");
			}

			CreateScriptFromTemplate(path, name);

			var script = UpdateAssetDatabase<MonoScript>(path);

			if (script == null)
			{
				throw new Exception($"Failed to create script at path '{path}'.");
			}

			PingProjectWindow(script);

			if (m_Settings.LogOnSuccess)
			{
				Debug.Log($"Script created at path '{path}'");
			}

			if (m_Settings.OpenAfterCreation)
			{
				AssetDatabase.OpenAsset(script);
			}
		}
	}
}