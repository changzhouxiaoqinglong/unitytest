// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace CodeAnimo.Support{
	
	public class HelpViewer : EditorWindow {
		
		public List<Article> articles = new List<Article>();
		private FieldFinder availableArticleSelector;
		private ExceptionPausedDrawer m_pausedDrawer;
		
		private SemiSerializedField m_selectedField;
		
		private Article m_lastDrawnArticle;
		
		private Vector2 articleListScrollPosition = Vector2.zero;
		
		protected void OnEnable(){
			if (this.availableArticleSelector == null){
				this.availableArticleSelector = CreateInstance<FieldFinder>();
				this.availableArticleSelector.hideFlags = HideFlags.HideAndDontSave;
			}
			this.availableArticleSelector.AddFieldSelectedListener(HandleFieldSelected);
			
			if (m_pausedDrawer == null){
				m_pausedDrawer = CreateInstance<ExceptionPausedDrawer>();
				m_pausedDrawer.hideFlags = HideFlags.HideAndDontSave;
			}
			
			availableArticleSelector.fieldType = typeof(Article);
			FindArticlesOnSelectedGameObject();
			
			this.minSize = new Vector2(400, 100);
			if (this.titleContent.text == GetType().FullName) this.titleContent.text = "Help Viewer";
		}
		
		protected void OnDestroy(){
			DestroyImmediate(this.availableArticleSelector);
			DestroyImmediate(m_pausedDrawer);
		}
		
		protected void OnGUI(){
			m_pausedDrawer.AttemptDrawing(DrawGUI);			
		}
		
		protected void DrawGUI(){
			GUILayout.BeginHorizontal();{
				
				GUILayout.BeginVertical(EditorStyles.objectFieldThumb, GUILayout.Width(200f));{
					this.articleListScrollPosition = GUILayout.BeginScrollView(this.articleListScrollPosition);
					
					if (this.availableArticleSelector.areFieldsAvailable){
						availableArticleSelector.DrawFieldSelectionButtons(m_selectedField);
					}
					GUILayout.EndScrollView();
				}GUILayout.EndVertical();
				
				
				// Default selection:
				if (m_selectedField == null){
					if (availableArticleSelector.areFieldsAvailable){
						m_selectedField = availableArticleSelector.availableFields[0];
					}
				}
				
				DrawSelectedArticle();
				
			}GUILayout.EndHorizontal();
		}
		
		protected void OnSelectionChange(){
			FindArticlesOnSelectedGameObject();
			Repaint();
		}
		
		protected void HandleFieldSelected(object sender, FieldSelectedEventArgs e){
			m_selectedField = e.selectedField;
		}
		
		protected void FindArticlesOnSelectedGameObject(){
			availableArticleSelector.selectedObject = Selection.activeGameObject;
		}
		
		protected void DrawSelectedArticle(){
			if (availableArticleSelector.selectedObject == null){
				EditorGUILayout.HelpBox("Select an object to view its help files", MessageType.Info);
				return;
			}
			
			SemiSerializedField selectedField = m_selectedField;
			if (selectedField == null){
				if (availableArticleSelector.areFieldsAvailable){
					EditorGUILayout.HelpBox("No Article Selected", MessageType.Info);
				}
				else{
					EditorGUILayout.HelpBox("No Help files available on the selected Object", MessageType.Info);
				}
				return;
			}
			
			if (selectedField.isFieldSelected == false){
				EditorGUILayout.HelpBox("SemiSerializedField lost field reference. Try selecting a different object.", MessageType.Warning);
				return;
			}
			
			Article selectedArticle = selectedField.GetValue() as Article;
			if (selectedArticle != m_lastDrawnArticle) GUI.FocusControl("");// Workaround for text not updating when there's a selection.
			
			if (selectedArticle == null){
				MonoScript scriptReference = MonoScript.FromMonoBehaviour((MonoBehaviour) selectedField.owner);// Make locating the problematic script easier.
				Debug.LogError("No article asset assigned to " + selectedField.fieldName + " on "  + selectedField.owner, scriptReference);
				Debug.LogWarning("Destroying invalid field reference");
				availableArticleSelector.DestroyFieldReference(selectedField);
				return;
			}
			
			selectedArticle.DrawArticle(selectedField.owner);
			m_lastDrawnArticle = selectedArticle;
			
			
		}
				
	}
}
#endif