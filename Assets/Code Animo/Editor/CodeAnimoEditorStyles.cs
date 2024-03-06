// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo{
	
	public class CodeAnimoEditorStyles {
		
		public static GUIStyle wrappedTextAreaStyle{
			get { 
				GUIStyle style = new GUIStyle(EditorStyles.textField);
				style.wordWrap = true;
				style.richText = true;
				
				return style;
			}
		}
		
	}
}