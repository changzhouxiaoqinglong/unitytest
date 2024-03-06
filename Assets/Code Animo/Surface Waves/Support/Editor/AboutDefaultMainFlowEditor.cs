// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.SurfaceWaves.Support{
	
	[CustomEditor(typeof(AboutDefaultMainFlow))]
	public class AboutDefaultMainFlowEditor : Editor {
		
		public override void OnInspectorGUI () {
			AboutDefaultMainFlow targetComponent = (AboutDefaultMainFlow) target;
			if (targetComponent.helpFile == null) return;
			EditorGUILayout.HelpBox(targetComponent.helpFile.text, MessageType.Info);
		}
		
		
	}
}