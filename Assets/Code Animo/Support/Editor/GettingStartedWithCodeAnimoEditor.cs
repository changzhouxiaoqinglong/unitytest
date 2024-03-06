// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CodeAnimo.Support{
	
	[CustomEditor(typeof(GettingStartedWithCodeAnimo))]
	public class GettingStartedWithCodeAnimoEditor : Editor {
		
		private GUIStyle m_wrappedTextStyle = null;
		
#if CODEANIMO_DEV
		[MenuItem("Assets/Create/Code Animo/Getting Started With Code Animo")]
		public static void CreateSimTextureSettingsAsset(){
			AssetCreation.CreateAsset<GettingStartedWithCodeAnimo>("Getting Started With Code Animo");
		}	
#endif
		public override void OnInspectorGUI (){			 
			if (m_wrappedTextStyle == null) m_wrappedTextStyle = CodeAnimoEditorStyles.wrappedTextAreaStyle;
			GettingStartedWithCodeAnimo targetComponent = (GettingStartedWithCodeAnimo) target;
			
			DrawAutoUpdateToggle(targetComponent);
			
			List<ProductData> foundProducts = targetComponent.foundProducts;
			if( foundProducts == null){
				if (GUILayout.Button("Search for installed products")) targetComponent.FindProductAttributes();
			}
			else{
				GUILayout.Label("The following products are installed: ");
				
				for (int i = 0; i < foundProducts.Count; i++) {
					ProductData helpInfo = foundProducts[i] as ProductData;
					
					DrawProductInfo(helpInfo);
				}
			}
			
			
			
			
			
			
			EditorGUILayout.HelpBox(targetComponent.aboutThisClass, MessageType.Info);
			
		}
		
		public void DrawAutoUpdateToggle(GettingStartedWithCodeAnimo targetComponent){
			EditorGUI.BeginChangeCheck();
			
			EditorGUIUtility.labelWidth = 160;
			bool autoUpdate = 
				EditorGUILayout.Toggle(
					new GUIContent(
						"Automatic product search",
						"If this is enabled, this asset will search for all occurrences of the GettingStartedAttribute, every time this component is enabled (such as after compiling). This is not necessary if you already know about every product"),
					targetComponent.automaticUpdateOnEnable
					); 
			EditorGUIUtility.labelWidth = 0;
			
			if (EditorGUI.EndChangeCheck()){
				EditorUtility.SetDirty(targetComponent);
				targetComponent.automaticUpdateOnEnable = autoUpdate;
				
				if (autoUpdate) targetComponent.FindProductAttributes();
			}
		}
		
		public void DrawProductInfo(ProductData helpInfo){
			if (helpInfo == null) return;
			
			EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label(helpInfo.productName, EditorStyles.largeLabel);
			if (helpInfo.productVersion != null) GUILayout.Label("version: " + helpInfo.productVersion, EditorStyles.miniBoldLabel);
			EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.LabelField(new GUIContent("Getting Started:"), EditorStyles.miniLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.TextArea(helpInfo.startupInfo, m_wrappedTextStyle);
			EditorGUI.indentLevel--;
//			EditorGUILayout.HelpBox(helpInfo.productName + " " + helpInfo.productVersion + "\n\n" + 
//				"How to get started: \n" + helpInfo.startupInfo
//				, MessageType.None);
//			
			if (GUILayout.Button(new GUIContent("Show Install Folder", "Selects the script asset where the product attribute was defined"), EditorStyles.toolbarButton)){
				Selection.activeObject = helpInfo.GetMonoScript();
			}
				
			EditorGUILayout.EndVertical();
		}
			
		
	}
}