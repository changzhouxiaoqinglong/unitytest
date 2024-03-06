// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

#if UNITY_EDITOR
using UnityEngine;

namespace CodeAnimo.Support{
	
	/// <summary>
	/// Class is used as a layer between Editor-only InteractiveArticle and non-editor-only Components.
	/// </summary>
	public class Article : ScriptableObject {
		
		public string buttonLabel;
		[Description]
		public string text;
		
		
		public virtual void DrawArticle(Component relatedComponent){
			
		}
		
		public override string ToString () {
			return buttonLabel;
		}
		
	}
	

	
}
#endif