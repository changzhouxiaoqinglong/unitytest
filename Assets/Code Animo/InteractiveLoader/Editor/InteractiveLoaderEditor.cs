// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo{

	[CustomEditor(typeof(InteractiveLoader))]
	public class InteractiveLoaderEditor : Editor {
		
		private InteractiveLoader targetComponent;
		
		private GUIContent endUpdateTimeLabel = new GUIContent("End Update Time (ms)", "The amount of time in miliseconds, that is allowed to pass before ending an update");
		
		public override void OnInspectorGUI () {
//			base.OnInspectorGUI ();
			this.targetComponent = target as InteractiveLoader;
			DrawEndUpdateTime();
			DrawLoadOnNewData();
			
	//		DisplayLoadingControls();		
	//		DisplayElementCount();
				
		}
		
		protected void OnEnable(){
			this.targetComponent = target as InteractiveLoader;
			
			EditorApplication.update -= this.targetComponent.EditorUpdate;
			EditorApplication.update += this.targetComponent.EditorUpdate;
		}
		protected void OnDisable(){
			this.targetComponent = target as InteractiveLoader;
			EditorApplication.update -= this.targetComponent.EditorUpdate;
		}
		
		protected void DrawEndUpdateTime(){
			this.targetComponent.endUpdateTime = EditorGUILayout.IntField(endUpdateTimeLabel, this.targetComponent.endUpdateTime);
		}
		
		protected void DrawLoadOnNewData(){
			this.targetComponent.loadOnNewData = EditorGUILayout.Toggle("Load on new data", this.targetComponent.loadOnNewData);
		}
		
		protected void DisplayLoadingControls(){
			if (targetComponent.Loading){
				if (GUILayout.Button("Stop Loading")) this.targetComponent.StopLoading();	
			}
			else{
				if (targetComponent.ElementCount > 0){
					if (GUILayout.Button("Start Loading")) this.targetComponent.StartLoading();
				}
			}
		}
		
		protected void DisplayElementCount(){
			EditorGUILayout.IntField(new GUIContent("Number of Elements"), this.targetComponent.ElementCount);
		}
		
	}
}