// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.SurfaceWaves.Support{
	
	[CustomEditor(typeof(SurfaceWavesFAQ))]
	public class SurfaceWavesFAQEditor : Editor {
		
		public override void OnInspectorGUI (){
			EditorGUILayout.HelpBox("This editor-only component only holds some help files for the Help Viewer.", MessageType.Info);
			if (GUILayout.Button("Open Help Viewer", EditorStyles.toolbarButton)) EditorWindow.GetWindow<CodeAnimo.Support.HelpViewer>();
		}
		
	}
}