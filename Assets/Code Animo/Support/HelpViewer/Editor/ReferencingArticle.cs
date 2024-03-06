// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.Support{
	
	public class ReferencingArticle : InteractiveArticle {
		
		public MonoScript referencedObject;
		public GUIContent referenceButtonLabel;
		
		override protected void OnEnable(){
			if (drawerScript == null) drawerScript = MonoScript.FromScriptableObject(CreateInstance<ReferencingArticleDrawer>());
		}
		
#if CODEANIMO_DEV
		[MenuItem("Assets/Create/Code Animo/Help/Referencing Article")]
		public static void CreateReferencingArticleAsset(){
			AssetCreation.CreateAsset<ReferencingArticle>("Referencing Article");
		}
#endif
	}
}