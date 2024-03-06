// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo{
	
	[CustomPropertyDrawer(typeof(TextureDebugAttribute))]
	public class TextureDebugDrawer : PropertyDrawer {
		
		public Material debugMaterial;
		
		private bool heightDetermined = false;
		
		private Rect basePosition;
		private Texture targetedTexture;
		private TextureDebugAttribute targetAttribute;
		private Event eventData;
		
		private float maximumTextureHeight = 512;
		
		private float propertyHeightCounter = 0;
		/// <summary>
		/// The number of pixels added to each extended rect.
		/// </summary>
		private float heightOffset = 4f;
		
		[HideInInspector] [SerializeField] private TextureAnalyserWindow m_createdWindow;
		
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label){
			int controlId = EditorGUIUtility.GetControlID(label, FocusType.Passive);
//			Debug.Log("Control ID for " + property.name + ": " + controlId, property.serializedObject.targetObject);
			
			TextureDebugDrawerState state = (TextureDebugDrawerState) EditorGUIUtility.GetStateObject(typeof(TextureDebugDrawerState), controlId);
			if (state.selectedMaterial != null) this.debugMaterial = state.selectedMaterial;
			
			propertyHeightCounter = 0;// Reset height counter.
			this.eventData = Event.current;
			
			position = GetControlLooksRect(position);//TODO: might be possible to use EditorStyles.defaultmargins
			
			// Setup shared state:
			this.basePosition = position;
			this.targetedTexture = GetTargetTexture(property);
			this.targetAttribute = attribute as TextureDebugAttribute;
			
			EditorGUI.PrefixLabel(position, controlId,label);
			Rect rightSideArea = GetRightHandBoxRect();	
			
			// Optional Texture Input:
			if (targetAttribute.inputBox) DrawTextureChooser(property, rightSideArea);
			else DrawTextureName(rightSideArea);
			
			// When a valid texture is selected
			if (this.targetedTexture != null){
				DrawTextureDisplay(property, state);
				if (this.targetAttribute.openInViewerButton) DrawWindowButton(property, state);
//				DrawSizeSlider();
				if (targetAttribute.materialSelector) DrawDebugMaterialChooser();
			}
			
			state.selectedMaterial = this.debugMaterial;
			this.heightDetermined = true;
		}
		
		/// <summary>
		/// Used by Unity Inspector GUI to reserve enough space.
		/// </summary>
		/// <returns>
		/// The height of the property
		/// </returns>
		/// <param name='property'>
		/// Reference of the drawn property.
		/// </param>
		/// <param name='label'>
		/// Label given to the inspector.
		/// </param>
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label){
			if (!this.heightDetermined){
				Rect fakePosition = new Rect(0,0,10,10);
				OnGUI(fakePosition, property, label);
			}
			return this.propertyHeightCounter;
		}
		
		/// <summary>
		/// Casts the object references by the given property to Texture, and returns that.
		/// </summary>
		/// <returns>
		/// The referenced object, cast to Texture
		/// Null if not a valid Texture.
		/// </returns>
		/// <param name='property'>
		/// The property currently being processed.
		/// </param>
		private Texture GetTargetTexture(SerializedProperty property){
			return property.objectReferenceValue as Texture;
		}
		
		/// <summary>
		/// Returns a rectangle below the previously requested rectangle
		/// Written as a replacement for GUILayoutUtility.GetRect, which doesn't seem to work in Custom Drawers.
		/// </summary>
		/// <returns>
		/// A rectangle based on the given rectangle, offset by previously requested heights.
		/// </returns>
		/// <param name='basePosition'>
		/// Base position.
		/// </param>
		/// <param name='width'>
		/// Width that the rectangle should have
		/// </param>
		/// <param name='height'>
		/// Height that the rectangle should have
		/// </param>
		private Rect GetExtendedRect(Rect basePosition, float width, float height){
			Rect extendedRect = basePosition;
			extendedRect.width = width;
			extendedRect.height = height;
			
			extendedRect.y += propertyHeightCounter;
			if (propertyHeightCounter > 0) extendedRect.y += this.heightOffset;
			
			this.propertyHeightCounter = extendedRect.yMax - basePosition.yMin;
			return extendedRect;
		}
		
		/// <summary>
		/// Processes input rect to make it allign properly as LookLikeControls, trimming 4 pixels of each edge
		/// </summary>
		/// <returns>
		/// The processed Rect
		/// </returns>
		/// <param name='inputRect'>
		/// Input rect.
		/// </param>
		private Rect GetControlLooksRect(Rect inputRect){
			Rect outputRect = inputRect;
			
			outputRect.xMin += 4f;
			outputRect.xMax -= 4f;
			return outputRect;
		}
		
		/// <summary>
		/// Used in combination with Prefix Labels. This method returns a rect for the area to the right of a prefix label.
		/// </summary>
		/// <returns>
		/// The Rect describing the area to the right of a prefix label
		/// </returns>
		private Rect GetRightHandBoxRect(){
			// Calculate offset for prefix label:
			Rect offsetArea = this.basePosition;
			float offset = 150f;
			offsetArea.x = this.basePosition.x + offset;
			float inputWidth = this.basePosition.width - offset;
			
			return GetExtendedRect(offsetArea, inputWidth, EditorGUIUtility.singleLineHeight);
		}
		
		
		private void DrawTextureChooser(SerializedProperty property, Rect rightSideArea){		
			property.objectReferenceValue = EditorGUI.ObjectField(rightSideArea, GUIContent.none, targetedTexture, typeof(Texture), true) as Texture;
		}
		
		/// <summary>
		/// Draws the name of the texture, or "No Texture selected" if no texture is selected
		/// </summary>
		/// <param name='rightSidearea'>
		/// The area to the right of a prefix label.
		/// </param>
		private void DrawTextureName(Rect rightSidearea){
			if (this.targetedTexture != null){
				EditorGUI.LabelField(rightSidearea, this.targetedTexture.name);
			}
			else{
				EditorGUI.LabelField(rightSidearea, "No Texture Selected");
			}
		}
		
		private void DrawWindowButton(SerializedProperty property, TextureDebugDrawerState state){
			float width = 100f;
			
			Rect buttonPosition = GetExtendedRect(this.basePosition, width, EditorGUIUtility.singleLineHeight);
			
			if (GUI.Button(buttonPosition, new GUIContent("Debug Window","Display the debug Window for the selected Texture"))){
				GetTrackFieldAction(property, state)();
			}
		}
		
		private void DrawMissingTextureWarning(){
			float warningBoxHeight = 20f;
			Rect warningArea = GetExtendedRect(this.basePosition, this.basePosition.width, warningBoxHeight);
			EditorGUI.HelpBox(warningArea, "No valid Texture selected", MessageType.Warning);
		}
		
		
		private void DrawTextureDisplay(SerializedProperty property, TextureDebugDrawerState state){
			float verticalTexturePadding = 10f;
			
			float textureAreaWidth = this.targetAttribute.previewWidth;
			
			Rect textureArea = GetExtendedRect(this.basePosition, this.basePosition.width, textureAreaWidth + verticalTexturePadding);
			textureArea = GUIPositioning.CenterRectInRect(new Rect(textureArea.x, textureArea.y, textureAreaWidth, textureAreaWidth), textureArea);
			
			Texture originalMaterialTexture = null;
			if (this != null && this.debugMaterial != null){// for some reason 'this' is not always available either.
				originalMaterialTexture = debugMaterial.mainTexture;// Backup material settings
			}
				
			EditorGUI.DrawPreviewTexture(textureArea, this.targetedTexture, debugMaterial, ScaleMode.ScaleToFit);
//			GUI.DrawTexture(textureArea, targetedTexture, ScaleMode.ScaleToFit, false);
			
			
			if (originalMaterialTexture != null){
				this.debugMaterial.mainTexture = originalMaterialTexture;// restore material settings
			}
				
			HandleContextMenu(textureArea, property, state);
		}
		
		private void HandleContextMenu(Rect clickArea, SerializedProperty property, TextureDebugDrawerState state){
			if (this.eventData.type != EventType.ContextClick) return;
			if (!clickArea.Contains(this.eventData.mousePosition)) return;
			
			GenericMenu menu = new GenericMenu();
			GUIContent inspectTextureLabel = new GUIContent("Open in Texture Viewer");
			
			// Lambda function that should be run when a menu item is clicked:
			GenericMenu.MenuFunction action = GetTrackFieldAction(property, state);
			
			menu.AddItem(inspectTextureLabel, false, action);
			menu.ShowAsContext();
		}
		
		/// <summary>
		/// Returns a Lambda function that opens the given property in a Texture Analyser Window.
		/// </summary>
		/// <returns>
		/// The lambda function that opens the given property in a Texture Analyser Window.
		/// </returns>
		/// <param name='property'>
		/// The property of the field that should be tracked.
		/// </param>
		protected GenericMenu.MenuFunction GetTrackFieldAction(SerializedProperty property, TextureDebugDrawerState state){
			return () => {
				if (state.openedWindow == null) state.openedWindow = EditorWindow.CreateInstance<TextureAnalyserWindow>();
				state.openedWindow.Show();
				state.openedWindow.TrackField(property.name, property.serializedObject.targetObject as Component);
			};
		}
		
		private void DrawSizeSlider(){
			Rect heightSettingPosition = GetExtendedRect(this.basePosition, this.basePosition.width, EditorGUIUtility.singleLineHeight);
			GUIContent sliderLabel = new  GUIContent("Debug Texture Size");
			this.maximumTextureHeight = EditorGUI.Slider(heightSettingPosition, sliderLabel, this.maximumTextureHeight, 10f, 500);
		}
		
		private void DrawDebugMaterialChooser(){
			Rect materialInputLocation = GetExtendedRect(this.basePosition, this.basePosition.width, EditorGUIUtility.singleLineHeight);
			GUIContent materialLabel = new GUIContent("Debug Material");
			this.debugMaterial = EditorGUI.ObjectField(materialInputLocation, materialLabel, this.debugMaterial, typeof(Material),true) as Material;
		}
		
	}
			
	[System.Serializable]
	public class TextureDebugDrawerState : Object {
		
		public Material selectedMaterial;
		public TextureAnalyserWindow openedWindow;
		
	}
		
			
}