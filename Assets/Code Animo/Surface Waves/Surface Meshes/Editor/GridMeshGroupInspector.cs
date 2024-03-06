// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo {
	
	[CustomEditor(typeof(GridMeshGroup))]
	public class GridMeshGroupInspector : Editor {
		
		private GridMeshGroup targetComponent;
		
		protected virtual void OnEnable(){
			GridMeshGroup targetComponent = target as GridMeshGroup;
			if (targetComponent == null) return;
			
		}
		
		protected virtual void OnDisabled(){
			GridMeshGroup targetComponent = target as GridMeshGroup;
			if (targetComponent == null) return;
		}
		
		public override void OnInspectorGUI(){
			
			EditorGUI.BeginChangeCheck();
			
			DrawDefaultInspector();
			
			if(EditorGUI.EndChangeCheck()){
				SetTargetDirty();
			}
			
			DrawControls();
		}
		
		private void DrawControls(){
			this.targetComponent = target as GridMeshGroup;
			if (!targetComponent.isCreatingGroup){
				
				if (targetComponent.isCreatingGroup){
					EditorGUILayout.HelpBox("Creating Group...", MessageType.Info);
				}
				else{
					if (targetComponent.heightData == null){
						EditorGUILayout.HelpBox("No Heightdata, mesh will start flat. (Displacement only with shader.)", MessageType.Info);	
					}
					
					
				}
				
				if (targetComponent.containsSegments){
					if ( GUILayout.Button("Delete Group Members") )targetComponent.DeleteGroupMembers();	
				}
				else{
					if (GUILayout.Button("Create Group")) startCreatingMeshGroup();
				}
			}
			else{
				if (GUILayout.Button("Stop Creating Group")) targetComponent.StopCreatingGroup();
			}
		}
		
		private void startCreatingMeshGroup(){
			this.targetComponent.AddGroupCompleteHandler(this.HandleMeshGroupCreated);
			this.targetComponent.StartCreatingGroup();
		}
		
		private void HandleMeshGroupCreated(GridMeshGroup meshGroup){
			SetTargetDirty();
		}
		
		/// <summary>
		/// Marks targetcomponent as Dity so its values are stored.
		/// </summary>
		private void SetTargetDirty(){
			EditorUtility.SetDirty(targetComponent);
		}
		
	}
}