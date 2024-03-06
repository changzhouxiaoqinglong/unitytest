// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System;

namespace CodeAnimo.Support{
	
	public class ReferencingArticleDrawer : ArticleDrawer {
			
		public override void DrawArticle (Article targetArticle, Component relatedComponent){
			GUILayout.BeginVertical();
			base.DrawArticle (targetArticle, relatedComponent);
			
			ReferencingArticle articleData = (ReferencingArticle) targetArticle;
			
			if (GUILayout.Button( articleData.referenceButtonLabel, EditorStyles.toolbarButton )){
				var referencedObject = articleData.referencedObject;
				if (referencedObject == null) return;
				Type searchedType = referencedObject.GetClass();
				if (searchedType == null) return;
				
				var foundReference = FindObjectOfType(searchedType);
				if (foundReference == null) return;
				Selection.activeObject = foundReference;
				EditorGUIUtility.PingObject(foundReference);				
			}
			
			GUILayout.EndVertical();
		}
		
		
	}
}