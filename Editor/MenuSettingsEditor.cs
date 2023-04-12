using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectMenu
{
	[CustomEditor(typeof(MenuSettings))]
	internal sealed class MenuSettingsEditor : Editor
	{
		/// <summary>
		/// The menu settings target.
		/// </summary>
		private MenuSettings m_Settings;

		/// <summary>
		/// The validation error message.
		/// </summary>
		private string m_Message = string.Empty;

		private void OnEnable ()
		{
			m_Settings = (MenuSettings)target;
			m_Settings.hideFlags &= ~HideFlags.NotEditable;

			ValidateSettings();
		}

		private void OnDisable ()
		{
			m_Settings.Save();
			m_Settings.hideFlags |= HideFlags.NotEditable;
		}

		public sealed override void OnInspectorGUI ()
		{
			var property = serializedObject.GetIterator();

			if (!property.NextVisible(true))
			{
				return;
			}

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				serializedObject.UpdateIfRequiredOrScript();

				while (property.NextVisible(false))
				{
					EditorGUILayout.PropertyField(property, true);
				}

				serializedObject.ApplyModifiedProperties();

				if (check.changed)
				{
					ValidateSettings();
				}
			}

			if (!m_Settings.IsValid)
			{
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox($"Invalid settings:\n\n{m_Message}\nThe menu will be disabled.", MessageType.Warning);
			}
		}

		private void ValidateSettings ()
		{
			var messages = new List<string>();

			if (m_Settings.Validate(messages))
			{
				return;
			}

			m_Message = string.Empty;

			foreach (var message in messages)
			{
				m_Message += $"- {message}\n";
			}
		}
	}
}