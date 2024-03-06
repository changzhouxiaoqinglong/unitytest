// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CodeAnimo {
	
	[CustomEditor(typeof(GridMesh))]
	public class GridMeshInspector : Editor {
		
		public override void OnInspectorGUI(){
			DrawDefaultInspector();
			
			GridMesh targetComponent = target as GridMesh;
			
			if (!targetComponent.hasMesh){
				if (GUILayout.Button("Create Grid")) { targetComponent.GenerateGrid(); }
			}
			else{
				if (GUILayout.Button("Destroy Mesh")) { targetComponent.DestroyGeneratedMesh(); }
		//		if (GUILayout.Button("Duplicate U")){ targetComponent.duplicate(1, 0); }
		//		if (GUILayout.Button("Duplicate V")){ targetComponent.duplicate(0, 1); }
			}
		}
	}
}