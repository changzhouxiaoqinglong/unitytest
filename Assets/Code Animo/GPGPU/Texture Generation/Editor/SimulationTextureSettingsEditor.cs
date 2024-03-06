// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SimulationTextureSettings))]
	public class SimulationTextureSettingsEditor : Editor {
		private SimulationTextureSettings targetComponent;
		
		private SerializedProperty dataPrecision;
		private SerializedProperty wrapMode;
		private SerializedProperty enableRandomWrite;
		private SerializedProperty anisoLevel;
		private SerializedProperty filterMode;
		private SerializedProperty readWriteMode;
		private SerializedProperty textureDepth;
		
		protected void OnEnable(){
			findProperties();	
		}
		
		public override void OnInspectorGUI () {
			targetComponent = target as SimulationTextureSettings;
			
			EditorGUIUtility.labelWidth = 150f;

			EditorGUI.BeginChangeCheck();
			
			
			
			serializedObject.Update();

			EditorGUILayout.PropertyField(dataPrecision);
			EditorGUILayout.PropertyField(this.wrapMode);
			EditorGUILayout.PropertyField(enableRandomWrite);
			EditorGUILayout.IntSlider(anisoLevel, 0, 9);
			EditorGUILayout.PropertyField(filterMode);
			EditorGUILayout.PropertyField(readWriteMode);
			EditorGUILayout.IntSlider(this.textureDepth,0, 24);
			
			serializedObject.ApplyModifiedProperties();
			
			
			
			if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(targetComponent);
			
			DrawSupportOnSystemInfo();
			
			EditorGUIUtility.labelWidth = 0f;// Reset field width
		}
		
		
		private void findProperties(){
			this.dataPrecision = serializedObject.FindProperty("dataPrecision");
			this.wrapMode = serializedObject.FindProperty("wrapMode");
			this.enableRandomWrite = serializedObject.FindProperty("enableRandomWrite");
			this.anisoLevel = serializedObject.FindProperty("anisoLevel");
			this.filterMode = serializedObject.FindProperty("filterMode");
			this.readWriteMode = serializedObject.FindProperty("readWriteMode");
			this.textureDepth = serializedObject.FindProperty("m_textureDepth");
		}
		
		
		/// <summary>
		/// Draws a message to indicate support for the selected Texture Settings Asset(s).
		/// Supports having multiple of these assets selected.
		/// </summary>
		private void DrawSupportOnSystemInfo(){
			string supportDisplayMessage = "No support check written yet";// To be overwritten with appropriate support message
			
			// Check support:
			if (serializedObject.isEditingMultipleObjects){
				bool allSupported = true;
				bool atLeastOneSupported = false;
				foreach (Object targetObject in serializedObject.targetObjects){
					SimulationTextureSettings settingsObject = targetObject as SimulationTextureSettings;
					if (settingsObject == null) continue;
					
					if (!settingsObject.supportedOnCurrentSystem()) allSupported = false;
					else atLeastOneSupported = true;
				}
				if (allSupported) supportDisplayMessage = "All Selected objects are supported on this system.";
				else if (atLeastOneSupported) supportDisplayMessage = "Not all selected objects are supported on this system.";
				else supportDisplayMessage = "None of the selected objects are supported on this system.";
			}
			else{
				SimulationTextureSettings settingsObject = serializedObject.targetObject as SimulationTextureSettings;
				if (settingsObject == null) supportDisplayMessage = "I have no idea how this happened, but somehow I can't check for support on this object.";
				
				if (settingsObject.supportedOnCurrentSystem()) supportDisplayMessage = "Selected object is supported on this system.";
				else supportDisplayMessage = "Selected object is not supported on this system";
			}
			
			// Draw debug message:
			EditorGUILayout.LabelField(supportDisplayMessage);
		}
		
		
	}
}