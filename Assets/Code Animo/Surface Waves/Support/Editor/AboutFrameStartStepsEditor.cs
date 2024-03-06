// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.SurfaceWaves.Support{
	
	[CustomEditor(typeof(AboutFrameStartSteps))]
	public class AboutFrameStartStepsEditor : Editor {
		
		public override void OnInspectorGUI () {
			AboutFrameStartSteps targetComponent = (AboutFrameStartSteps) target;
			string helpText;
			if (targetComponent.helpFile != null) helpText = targetComponent.helpFile.text;
			else helpText = "";
			EditorGUILayout.HelpBox(helpText, MessageType.Info);
		}
		
	}
}