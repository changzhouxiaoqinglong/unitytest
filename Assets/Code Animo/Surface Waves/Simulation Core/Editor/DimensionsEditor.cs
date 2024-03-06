// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.SurfaceWaves{
	
	[CustomEditor(typeof(Dimensions))]
	public class DimensionsEditor : Editor {
		
		private GUIContent sizeFieldLabel = new GUIContent("Size", "The size of the bounding box of the simulation. Automatically updates attached boxCollider too.");
		
		private GUIContent resolutionLabel = new GUIContent("Resolution", "The number of pixels used in the simulation. A higher number means more detail and higher cost");
		private GUIContent resolutionXLabel = new GUIContent("X", "The number of pixels in the x dimension. Should be a multiple of 32 (512, 768, 1024, etc.)");
		private GUIContent resolutionZLabel = new GUIContent("Z", "The number of pixels in the z dimension. Should be a multiple of 32 (512, 768, 1024, etc.)");
		
		Dimensions targetComponent;
		
		public override void OnInspectorGUI () {
			this.targetComponent = (Dimensions) target;
			EditorGUI.BeginChangeCheck();
			
			Undo.RecordObject(targetComponent, "Dimensions Size Change");
			targetComponent.localSize = EditorGUILayout.Vector3Field(sizeFieldLabel, targetComponent.localSize);
			
			DrawResolutionControls();
			
			if (EditorGUI.EndChangeCheck()){
				EditorUtility.SetDirty(targetComponent);
			}
			
		}
		
		protected void DrawResolutionControls(){
			Undo.RecordObject(targetComponent, "Dimensions Resolution Change");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(resolutionLabel);
//			EditorGUI.indentLevel++;
			
			EditorGUIUtility.labelWidth = 16f;// Match with size fields above.
			this.targetComponent.resolutionX = EditorGUILayout.IntField(resolutionXLabel, this.targetComponent.resolutionX);
			this.targetComponent.resolutionZ = EditorGUILayout.IntField(resolutionZLabel, this.targetComponent.resolutionZ);
			EditorGUILayout.EndHorizontal();
			
//			EditorGUI.indentLevel--;
		}
		
		
	}
}