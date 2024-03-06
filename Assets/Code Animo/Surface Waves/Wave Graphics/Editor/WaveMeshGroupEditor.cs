// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo.SurfaceWaves {
	
	[CustomEditor(typeof(WaveMeshGroup))]
	public class WaveMeshGroupEditor : GridMeshGroupInspector {
	
		public override void OnInspectorGUI () {
			base.OnInspectorGUI ();
		}
		
		
	}
}