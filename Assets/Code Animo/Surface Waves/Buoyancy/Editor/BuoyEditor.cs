// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo.SurfaceWaves{
	[CustomEditor(typeof(Buoy))]
	public class BuoyEditor : Editor {
		
	//	SerializedProperty radius;
		
		void OnEnable(){
	//		radius = serializedObject.FindProperty("radius");
		}
		
		override public void OnInspectorGUI(){
	//		serializedObject.Update();
			
	//		EditorGUILayout.PropertyField(radius);
			
	//		serializedObject.ApplyModifiedProperties();
			Buoy targetComponent = target as Buoy;
			
			GUIContent radiusLabel = new GUIContent("Radius", "This also affects the radius of the attached sphere collider");
			targetComponent.radius = EditorGUILayout.FloatField( radiusLabel, targetComponent.radius);
			
			
		}
		
		protected void OnSceneGUI(){
			Handles.BeginGUI();
			GUILayout.BeginVertical(EditorStyles.objectFieldThumb, GUILayout.Width(250));
			Buoy targetComponent = target as Buoy;
			GUILayout.Label("Buoy Debug Info: ");
			
			EditorGUILayout.ObjectField(new GUIContent("Affected Object"), targetComponent.affectedObject, typeof(Rigidbody), true);
			GUILayout.Label("Velocity Data: " + targetComponent.velocityData);
			GUILayout.Label("Position Data: " + targetComponent.position);
			GUILayout.Label("Last Applied Force: " + targetComponent.lastAppliedForce);
			GUILayout.EndVertical();
			Handles.EndGUI();
		}
		
	}
}