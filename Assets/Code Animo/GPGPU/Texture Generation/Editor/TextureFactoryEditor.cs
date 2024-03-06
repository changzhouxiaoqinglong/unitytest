// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace CodeAnimo.GPGPU {
	
	[CustomEditor(typeof(TextureFactory))]
	public class TextureFactoryEditor : Editor {
		private TextureFactory targetComponent;
		private Material debugMaterial = null;
		
		public override void OnInspectorGUI () {
			targetComponent = target as TextureFactory;
			
			TextureSettingsProperty();
			DisplayResolutionOverride();
			
			// Latest Texture:
			DisplayTexture(targetComponent.LatestTexture);
			RenderTexture latestTexture = targetComponent.LatestTexture;
			if (latestTexture != null)	DisplayTextureSettings(latestTexture);
			
			
			// Older Textures:
			DisplayOldTextures(targetComponent.registeredTextures);
			
			// Older Textures settings:
			AllowedRecentTextureCountSetting();
			EditorGUILayout.BeginHorizontal();
			DisplayTextureCount();
			ButtonDestroyTextures();
			EditorGUILayout.EndHorizontal();
		}
		
		private void DisplayResolutionOverride(){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Resolution:");
			EditorGUIUtility.labelWidth = 16f;
			targetComponent.resolutionU = EditorGUILayout.IntField("U", targetComponent.resolutionU);
			targetComponent.resolutionV = EditorGUILayout.IntField("V", targetComponent.resolutionV);
			EditorGUIUtility.labelWidth = 0f;
			EditorGUILayout.EndHorizontal();
		}
		
		private void TextureSettingsProperty(){
			EditorGUI.BeginChangeCheck();
			
			var newSettings = EditorGUILayout.ObjectField(
				"Texture Settings",
				targetComponent.textureSettings,
				typeof(SimulationTextureSettings),
				false) as SimulationTextureSettings;
			
			if (EditorGUI.EndChangeCheck()){
				targetComponent.textureSettings = newSettings;
				EditorUtility.SetDirty(targetComponent);
			}
		}
		
		private void DisplayTextureSettings(RenderTexture texture){
			EditorGUILayout.LabelField("width: " + texture.width);
			EditorGUILayout.LabelField("height: " + texture.height);
			EditorGUILayout.LabelField("texture format: " + texture.format);
		}
		
		private void DisplayOldTextures(List<RenderTexture> allTextures){
			if (allTextures == null) return;
			for (int i = 1; i < allTextures.Count; i++){// Skip the recent Textures
				DisplayTexture(allTextures[i], 100,100, 4);
			}
		}
		
		private void DisplayTexture(Texture selectedTexture, int width, int height, int padding){
			if (width - 2 * padding <= 0 || height - 2 * padding <= 0) throw new System.ArgumentOutOfRangeException("No room for texture left");
			
			Rect totalArea = GUILayoutUtility.GetRect(width, height);
			
			if (totalArea.width < width || totalArea.height < height){
	//			Debug.LogError("No room left");	
				return;//throw new System.InvalidOperationException("It seems there wasn't enough room for this texture");
			}
			
			Rect background = totalArea;
			background.width = width;
			background.x = 0.5f * totalArea.width - 0.5f * width;
			Rect textureArea = background;
			textureArea.width -= padding * 2;
			textureArea.height -= padding * 2;
			textureArea.x += padding;
			textureArea.y += padding;
			
			bool textureAvailable = selectedTexture != null;
			
			string label = "No Texture";
			string tooltip = "No Texture";
			if (textureAvailable){
				label = selectedTexture.name;
	//			tooltip = createTextureTooltip(selectedTexture);
			}
			GUI.Box(background, new GUIContent(label, tooltip));// FIXME: when this is called After deleting a bunch of textures, this will not work well.
			
			if (!textureAvailable) return;
			EditorGUI.DrawPreviewTexture(textureArea, selectedTexture, this.debugMaterial,ScaleMode.ScaleToFit);
			
			HandleContextMenu(textureArea, selectedTexture);
		}
		
		private void DisplayTexture(Texture selectedTexture){
			DisplayTexture(selectedTexture, 220, 220, 20);	
		}
		
		private void AllowedRecentTextureCountSetting(){
			string label = "Allowed Recent Texture Count";
			string tooltip = "Should be 1 by default. The number of old textures that is allowed before starting to destroy old textures. If multiple components use the same texture and keep references, setting this higher might be a work around for Textures being destroyed too soon.";
			
			targetComponent.allowedRecentTextureCount = EditorGUILayout.IntSlider(
				new GUIContent(label, tooltip),
				targetComponent.allowedRecentTextureCount,
				1, 4);
		}
		
		private void DisplayTextureCount(){
			int numTextures = targetComponent.registeredTextureCount;
			if (numTextures < 1) EditorGUILayout.LabelField("No textures stored.");
			else if (numTextures == 1) EditorGUILayout.LabelField("One texture stored.");
			else EditorGUILayout.LabelField(numTextures + " textures stored.");	
		}
		
		private void ButtonDestroyTextures(){
			if (targetComponent.registeredTextureCount > 0){
				if(GUILayout.Button("Clean Textures")) targetComponent.DestroyAllTextures();
			}
		}
		
		private void DisplayDebugMaterialProperty(){
			EditorGUI.BeginChangeCheck();
			Material selectedMaterial = EditorGUILayout.ObjectField(this.debugMaterial, typeof(Material), false) as Material;
			if (EditorGUI.EndChangeCheck()){
				this.debugMaterial = selectedMaterial;	
			}
		}
		
		private void HandleContextMenu(Rect clickArea, Texture selectedTexture){
			if (Event.current.type != EventType.ContextClick) return;
			if (!clickArea.Contains(Event.current.mousePosition)) return;
			
			GenericMenu menu = new GenericMenu();
			GUIContent inspectTextureLabel = new GUIContent("Open in Texture Viewer");
			menu.AddItem(inspectTextureLabel, false, OpenTextureInViewer, selectedTexture);
			menu.ShowAsContext();
		}
		
		private void OpenTextureInViewer(System.Object selectedTextureObject){
			Texture selectedTexture = selectedTextureObject as Texture;
			if (selectedTexture == null) return;
			
			TextureAnalyserWindow targetWindow = EditorWindow.GetWindow<TextureAnalyserWindow>("Texture Debug");
			targetWindow.SetMainTexture(selectedTexture);
		}
		
	}
}