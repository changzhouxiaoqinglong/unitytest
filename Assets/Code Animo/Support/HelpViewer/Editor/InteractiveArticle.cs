// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System;

namespace CodeAnimo.Support{
	
	public class InteractiveArticle : Article {
		
		
		public MonoScript drawerScript;
		
		private ArticleDrawer m_drawer;
		protected ArticleDrawer drawer{
			get {
				if (m_drawer == null || m_drawer.GetType() != drawerScript.GetClass()){
					if (drawerScript!= null){
						if (m_drawer != null) DestroyImmediate(m_drawer);// Destroy old one.
						m_drawer = InstantiateDrawer();
						m_drawer.hideFlags = HideFlags.HideAndDontSave;
					}
				}
				return m_drawer;
			}
		}
		
		protected virtual void OnEnable(){
			if (drawerScript == null) drawerScript = MonoScript.FromScriptableObject(CreateInstance<ArticleDrawer>());
		}
		
		protected void OnDestroy(){
			if (m_drawer != null) DestroyImmediate(m_drawer);
		}
		
		public override void DrawArticle(Component relatedComponent){
			if (drawer != null)	drawer.DrawArticle(this, relatedComponent);
			else{
				EditorGUILayout.HelpBox("No drawer script selected for article " + this.buttonLabel, MessageType.Error);
				if (GUILayout.Button("Ping Article Asset")) EditorGUIUtility.PingObject(this);
			}
		}
		
		
		protected ArticleDrawer InstantiateDrawer(){
			if (drawerScript == null) throw new MissingReferenceException("No drawer script assigned");
			
			Type requiredType = typeof(ArticleDrawer);
			Type referencedType = drawerScript.GetClass();
			
			if (!requiredType.IsAssignableFrom(referencedType)) throw new InvalidCastException("The provided script defines the wrong type: " + referencedType.Name + " . It needs to derive from " + requiredType.Name);
			if (referencedType.IsAbstract) throw new InvalidOperationException("The drawer script implementation has to be a non-abstract class.");
			
			return (ArticleDrawer) CreateInstance(referencedType);
		}
		
#if CODEANIMO_DEV
		[MenuItem("Assets/Create/Code Animo/Help/Interactive Article")]
		public static void CreateArticleAsset(){
			AssetCreation.CreateAsset<InteractiveArticle>("Interactive Article");
		}
#endif
	}
}