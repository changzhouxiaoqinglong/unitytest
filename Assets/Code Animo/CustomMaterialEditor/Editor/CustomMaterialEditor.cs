// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo{
	
//	[CustomEditor(typeof(Material))]
	public class CustomMaterialEditor : Editor {
		public bool hideMainMaterial = false;// Used by TextureAnalyserWindow, which already displays main texture.
		
		/// <summary>
		/// Possible values for ShaderUtil.getRangeLimits
		/// </summary>
		public enum ShaderRangeComponent{
			defaultValue = 0,
			minimumValue = 1,
			maximumValue = 2
		}
		
		private Material m_selectedMaterial;

		public Material selectedMaterial {
			get { return m_selectedMaterial; }
			set { m_selectedMaterial = value; }
		}
		
		public override void OnInspectorGUI () {
			m_selectedMaterial = target as Material;
			DrawMaterialSettings();
//			EditorGUILayout.HelpBox("Succesfully overridden material editor", MessageType.Info);
		}
		
		public void DrawMaterialSettings(){
			if (m_selectedMaterial == null) return;
			
			Shader selectedShader = m_selectedMaterial.shader;
			int propertyCount = ShaderUtil.GetPropertyCount(selectedShader);
			
			for (int i = 0; i < propertyCount; i++) {
				ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(selectedShader, i);
				string propertyName = ShaderUtil.GetPropertyName(selectedShader, i);
				string propertyDescription = ShaderUtil.GetPropertyDescription(selectedShader, i);
				
				// Sometimes Properties are reported incorrectly:
				if (!m_selectedMaterial.HasProperty(propertyName)){
					DrawMissingPropertyError(propertyName);
					continue;	
				}
				
				switch (propertyType){
				case ShaderUtil.ShaderPropertyType.TexEnv:
					if (hideMainMaterial && propertyName == "_MainTex") break;
					DrawTextureProperty(selectedShader, i, propertyDescription);
					break;
				case ShaderUtil.ShaderPropertyType.Color:
					DrawShaderColorProperty(m_selectedMaterial, propertyName, propertyDescription);
					break;
				case ShaderUtil.ShaderPropertyType.Float:
					DrawShaderFloatProperty(m_selectedMaterial, propertyName, propertyDescription);
					break;
				case ShaderUtil.ShaderPropertyType.Range:
					DrawShaderRangeProperty(m_selectedMaterial, propertyName, propertyDescription, selectedShader, i);
					break;
				case ShaderUtil.ShaderPropertyType.Vector:
					DrawShaderVectorProperty(m_selectedMaterial, propertyName, propertyDescription);
					break;
				default:
					EditorGUILayout.HelpBox("Could not draw property '" + propertyName + "'", MessageType.Warning);
					break;
				}
				
				GUILayout.Space(2f);
				
			}
			
		}
		
		private void DrawMissingPropertyError(string propertyName){
			EditorGUILayout.HelpBox(
				"A property with the name '" 
				+ propertyName 
				+ "' is listed, but can not be edited.",
				MessageType.Error, 
				true);
		}
		
		private void DrawTextureProperty(Shader selectedShader, int propertyId, string propertyDescription){
			string propertyName = ShaderUtil.GetPropertyName(selectedShader, propertyId);
            UnityEngine.Rendering.TextureDimension textureType = ShaderUtil.GetTexDim(selectedShader, propertyId);
						
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
			EditorGUILayout.PrefixLabel(propertyDescription);
			DrawTextureTilingOffset();
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical(GUILayout.Width(75f), GUILayout.Height(75f));
			switch(textureType){
			case UnityEngine.Rendering.TextureDimension.Tex2D:
				DrawTexturePicker<Texture2D>(m_selectedMaterial, propertyName);
				break;
			case UnityEngine.Rendering.TextureDimension.Tex3D:
				DrawTexturePicker<Texture3D>(m_selectedMaterial, propertyName);
				break;
			case UnityEngine.Rendering.TextureDimension.Cube:
				DrawTexturePicker<Cubemap>(m_selectedMaterial, propertyName);
				break;
			}
			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
			
		}
		
		private void DrawTextureTilingOffset(){
			// Making the labels and fields a bit more narrow:
			EditorGUIUtility.labelWidth = 2f;// Not sure why it needs such extreme values
			EditorGUIUtility.fieldWidth = 2f;
			
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("");
			EditorGUILayout.LabelField("X");
			EditorGUILayout.LabelField("Y");
			EditorGUI.indentLevel--;
			GUILayout.EndVertical();
			
			
			GUILayout.BeginVertical();
			EditorGUILayout.LabelField("Tiling");
			EditorGUILayout.FloatField(0);
			EditorGUILayout.FloatField(0);
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical();
			EditorGUILayout.LabelField("Offset");
			EditorGUILayout.FloatField(0);
			EditorGUILayout.FloatField(0);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			// Reset to default:
			EditorGUIUtility.labelWidth = 0f;
			EditorGUIUtility.fieldWidth = 0f;
			
		}
		
		private void DrawTexturePicker<T>(Material targetMaterial, string propertyName) where T : Texture{
			T originalTexture = targetMaterial.GetTexture(propertyName) as T;
			GUILayout.BeginVertical();
			T newTexture = EditorGUILayout.ObjectField(
				originalTexture,
				typeof(T),
				true,
				GUILayout.Width(75),
				GUILayout.Height(75)) as T;
			GUILayout.EndVertical();
			if (newTexture != originalTexture){
				targetMaterial.SetTexture(propertyName, newTexture);
			}
		}
		
		private void DrawShaderColorProperty(Material targetMaterial, string propertyName, string propertyDescription){
            // Making the labels and fields a bit more narrow:
            EditorGUIUtility.labelWidth = 2f;// Not sure why it needs such extreme values
            EditorGUIUtility.fieldWidth = 2f;
            Color originalColor = targetMaterial.GetColor(propertyName);
			
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(propertyDescription);
			EditorGUILayout.Space();
			Color selectedColor = EditorGUILayout.ColorField(originalColor, GUILayout.Width(65));
			EditorGUILayout.EndHorizontal();
			if (originalColor != selectedColor){
				targetMaterial.SetColor(propertyName, selectedColor);
			}
            // Reset to default:
            EditorGUIUtility.labelWidth = 0f;
            EditorGUIUtility.fieldWidth = 0f;
        }
		
		private void DrawShaderFloatProperty(Material targetMaterial, string propertyName, string propertyDescription){
			float originalValue = targetMaterial.GetFloat(propertyName);
			EditorGUILayout.BeginHorizontal();
			
			EditorGUIUtility.fieldWidth = 65;// FIXME: for some reason FloatField does not obey fieldwidth settings.
			// Must use floatField with label to get click and drag controls:
			float newValue = EditorGUILayout.FloatField(new GUIContent(propertyDescription), originalValue);
			EditorGUILayout.EndHorizontal();
			
			if (newValue != originalValue){
				targetMaterial.SetFloat(propertyName, newValue);
			}
		}
		
		private void DrawShaderRangeProperty(Material targetMaterial, string propertyName, string propertyDescription, Shader selectedShader, int propertyIndex){
			float minimumValue = ShaderUtil.GetRangeLimits(selectedShader, propertyIndex, (int) ShaderRangeComponent.minimumValue);
			float maximumValue = ShaderUtil.GetRangeLimits(selectedShader, propertyIndex, (int) ShaderRangeComponent.maximumValue);
						
			float originalValue = targetMaterial.GetFloat(propertyName);
			float newValue = EditorGUILayout.Slider(propertyDescription, originalValue, minimumValue, maximumValue);
			if (newValue != originalValue){
				targetMaterial.SetFloat(propertyName, newValue);
			}
		}
		
		private void DrawShaderVectorProperty(Material targetMaterial, string propertyName, string propertyDescription){
			Vector4 originalValue = targetMaterial.GetVector(propertyName);
			Vector4 newValue = EditorGUILayout.Vector4Field(propertyDescription, originalValue);
			if (originalValue != newValue){
				targetMaterial.SetVector(propertyName, newValue);	
			}
		}
	}
}