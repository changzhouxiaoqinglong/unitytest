// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo{
	
	[CustomPropertyDrawer(typeof(DescriptionAttribute))]
	public class DescriptionDrawer : PropertyDrawer {
		
		private GUIStyle m_textAreaStyle = null;
		
		protected DescriptionAttribute targetAttribute{
			get { return (DescriptionAttribute) attribute; }
		}
		
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (m_textAreaStyle == null){
				m_textAreaStyle = new GUIStyle(EditorStyles.textField);
				m_textAreaStyle.wordWrap = true;
				m_textAreaStyle.richText = false;
			}
			
			position.height = targetAttribute.height;
			int controlID = EditorGUIUtility.GetControlID(label, FocusType.Keyboard, position);
			Rect textFieldArea = EditorGUI.PrefixLabel(position, controlID, label);
			
			// FIXME: can't use GUI.BeginScrollView unfortunately, because Unity draws it outside of the drawn area.
			
			EditorGUI.BeginChangeCheck();
			string updatedText = EditorGUI.TextArea(textFieldArea, property.stringValue, m_textAreaStyle);
			
			if (EditorGUI.EndChangeCheck()){
//				EditorUtility.SetDirty(property.serializedObject);
				property.stringValue = updatedText;
			}
		}
		
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			return targetAttribute.height;
		}
		
	}
}