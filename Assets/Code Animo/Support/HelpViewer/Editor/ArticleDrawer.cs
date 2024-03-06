// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.Support{
	
	[System.Serializable]
	public class ArticleDrawer : ScriptableObject{
		
		private Vector2 scrollposition = Vector2.zero;
		private GUIStyle m_wrappedTextStyle = null;
		
		public virtual void DrawArticle(Article targetArticle, Component relatedComponent){
			GUILayout.BeginVertical();
			this.scrollposition = GUILayout.BeginScrollView(this.scrollposition);
			
			if (m_wrappedTextStyle == null) m_wrappedTextStyle = CodeAnimoEditorStyles.wrappedTextAreaStyle;
						
			EditorGUILayout.TextArea(targetArticle.text, m_wrappedTextStyle);
			GUILayout.EndScrollView();
			
			#if CODEANIMO_DEV
			if (GUILayout.Button(new GUIContent("Ping Asset", "Selects the article asset that is being drawn. (Tool Development Button)"), EditorStyles.toolbarButton)){//TODO: remove this button before release.
				EditorGUIUtility.PingObject(targetArticle);
			}
			#endif
			GUILayout.EndVertical();
		}
		
	}
}