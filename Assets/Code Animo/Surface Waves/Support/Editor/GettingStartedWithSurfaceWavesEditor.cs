// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.Support{
	
	[CustomEditor(typeof(GettingStartedWithSurfaceWaves))]
	public class GettingStartedWithSurfaceWavesEditor : Editor {
		
		protected GUIContent gettingStartedButton = new GUIContent("Click here to learn more.", "Click here to open a window with information on how to get started using Surface Waves.");
		
		protected GettingStartedWithSurfaceWaves targetComponent{
			get { return (GettingStartedWithSurfaceWaves) target; }
		}
		
		public override void OnInspectorGUI () {
			EditorGUILayout.HelpBox("Welcome to Surface Waves by Code Animo! \n\nIf you press play now, the simulation should run. For the best effect, add a terrain object for the simulation to run on. \n\nTo learn about the components that make up Surface Waves, click the button below", MessageType.Info);
			
			if (GUILayout.Button(this.gettingStartedButton, EditorStyles.toolbarButton)) OpenWindow();
		}
		
		protected HelpViewer OpenWindow(){
			HelpViewer helpWindow = EditorWindow.GetWindow<HelpViewer>("Getting Started");
//			helpWindow.articles.AddRange(targetComponent.defaultArticles);
			return helpWindow;
		}
		
	}
}