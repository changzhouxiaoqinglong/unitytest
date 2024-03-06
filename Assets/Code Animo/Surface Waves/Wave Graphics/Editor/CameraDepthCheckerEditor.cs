// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CodeAnimo{
	[CustomEditor(typeof(CameraDepthChecker))]
	public class CameraDepthCheckerEditor : Editor {
		
		protected GUIContent automaticDepthToggleLabel = new GUIContent("Auto-enable depth", "If this is enabled, all cameras without depth rendering enabled with automatically have depth rendering enabled on Start.");
		
		public void OnEnable(){
			CameraDepthChecker targetComponent = (CameraDepthChecker) target;
			
			// Situation might have changed, update the list:
			targetComponent.FindDepthlessCameras();
		}
		
		public override void OnInspectorGUI (){
			CameraDepthChecker targetComponent = (CameraDepthChecker) target;
			
			List<Camera> listedCameras = targetComponent.depthlessCameras;
			
			DrawComponentInformation(targetComponent, listedCameras);// Show what this component is about.
			
			// If there are no problems, show all cameras:
			if (listedCameras.Count <= 0){
				listedCameras = new List<Camera>(Camera.allCameras);
			}
			
			DrawCameraSettingsList(targetComponent, listedCameras);// Show a list of cameras and their settings.
			DrawRefreshButton(targetComponent);// Refresh Button.
			DrawAutomaticDepthToggle(targetComponent);// Automatic Depth Toggle.
			
		}
		
		protected void DrawComponentInformation(CameraDepthChecker targetComponent, List<Camera> listedCameras){
			if (listedCameras == null){
				EditorGUILayout.HelpBox("This component exists to remind you when cameras don't write a depth texture.", MessageType.Info);
				
				if (GUILayout.Button(new GUIContent("Find Depthless Cameras"))) targetComponent.FindDepthlessCameras();
				
				return;
			}
			
			if (listedCameras.Count <= 0){
				EditorGUILayout.HelpBox("Good news everyone! It appears that all cameras write depth. Showing all cameras:", MessageType.Info);
			}
			else{
				EditorGUILayout.HelpBox("The following cameras appear not to write depth. You can try switching to deferred lighting, or setting their depthTextureMode (setting depthTextureMode here is just temporary).", MessageType.Warning);
			}
		}
		
		protected void DrawCameraSettingsList(CameraDepthChecker targetComponent, List<Camera> listedCameras){
			for (int i = 0; i < listedCameras.Count; i++) {
				Camera displayedCamera = listedCameras[i];
				
				EditorGUILayout.BeginHorizontal();
				
				EditorGUILayout.ObjectField(displayedCamera, typeof(Camera), true);
				EditorGUI.BeginChangeCheck();
				
				RenderingPath selectedPath = (RenderingPath) EditorGUILayout.EnumPopup(displayedCamera.renderingPath);
				DepthTextureMode selectedDepthMode = (DepthTextureMode) EditorGUILayout.EnumPopup(displayedCamera.depthTextureMode);
				
				bool cameraUpdated = false;
				if (EditorGUI.EndChangeCheck()){
					if (displayedCamera.renderingPath != selectedPath){
						displayedCamera.renderingPath = selectedPath;
						cameraUpdated = true;
					}
					if (displayedCamera.depthTextureMode != selectedDepthMode){
						displayedCamera.depthTextureMode = selectedDepthMode;
						cameraUpdated = true;
					}
					if (cameraUpdated){
						EditorUtility.SetDirty(displayedCamera);
						targetComponent.FindDepthlessCameras();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		
		protected void DrawRefreshButton(CameraDepthChecker targetComponent){
			if (GUILayout.Button(new GUIContent("Refresh list"))) targetComponent.FindDepthlessCameras();
			
			GameObject targetGameObject = targetComponent.gameObject;
			
			int depthCheckerEnabled = EditorUtility.GetObjectEnabled(targetComponent);
			if (depthCheckerEnabled == 1 && targetGameObject.activeInHierarchy){
				GUILayout.Label("List is automatically updated on Game Start.");
			}
			else{
				GUILayout.Label("To make this list update automatically, enable this component.");
			}
		}
		
		protected void DrawAutomaticDepthToggle(CameraDepthChecker targetComponent){
			bool newSetting = EditorGUILayout.Toggle(this.automaticDepthToggleLabel, targetComponent.automaticallyEnableDepthOnStart);
			if (newSetting != targetComponent.automaticallyEnableDepthOnStart){
				targetComponent.automaticallyEnableDepthOnStart = newSetting;
				EditorUtility.SetDirty(targetComponent);
			}
		}
		
	}
}