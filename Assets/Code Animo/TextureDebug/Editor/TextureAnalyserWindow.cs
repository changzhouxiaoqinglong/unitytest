// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo{
	[Support.ProductInfo("Texture Viewer", "From the top menu: Window > Texture Viewer")]
	public class TextureAnalyserWindow : EditorWindow {
		
		/// <summary>
		/// Fast update gives smoother animations, but also uses processor power in the background.
		/// </summary>
		public bool fastUpdate = false;
		
		private bool m_trackingEnabled = false;
		public bool trackingEnabled{
			get { return m_trackingEnabled; }
			set {
				if (value != m_trackingEnabled){// value changed
					m_trackingEnabled = value;
					if(value){
						if (m_TextureView != null)m_TextureView.continuousUpdate = true;// When tracking, you expect realtime updates, so enable it.
					}
					else redrawRequired = true;// No Texture will be available after setting this to false.
				}
			}
		}
		
		private SemiSerializedField m_selectedField;
		
		private bool m_redrawRequired = false;
		protected bool redrawRequired{
			get { return this.fastUpdate && (m_redrawRequired || m_TextureView.requiresRedraw); }
			set { m_redrawRequired = value; }
		}
		
		private GUIContent trackingToggleLabel = new GUIContent();
		
		private Texture m_mainTexture;
		public bool MainTextureSelected{
			get { return m_mainTexture != null; }	
		}
		
		private Shader m_selectedShader;
		public Shader SelectedShader{
			get { return m_selectedShader; }
			set {
				if (value != m_selectedShader){
					m_selectedShader = value;
					if (m_analysisMaterial != null){
						m_analysisMaterial.shader = value;
						CheckMainTextureDimensionMatch();
					}
				}
			}
		}
		private Material m_analysisMaterial;
		private bool m_mainTextureDimensionMatch = false;
		
		private TextureDisplay m_TextureView;
		[SerializeField] private FieldFinder m_textureSelector;
		
		private ExceptionPausedDrawer m_GUIDrawer;
		
		private Vector2 windowScrollSettings = new Vector2(0,0);
		
		protected bool m_sideControls = false;
		/// <summary>
		/// Gets a value indicating whether this <see cref="CodeAnimo.TextureAnalyserWindow"/> displays its controls to the side of its texture display or not.
		/// </summary>
		/// <value>
		/// <c>true</c> if using side controls; otherwise, <c>false</c>.
		/// </value>
		public bool sideControls{
			get { return m_sideControls; }
		}
		
		private float minimumControlsWidth = 320f;
		private float minimumControlsHeight = 150f;
		
		private Event eventData;
		
		private CustomMaterialEditor m_materialEditor;
		
		[MenuItem("Window/TextureViewer")]
		private static void OpenFromMenu(){
			TextureAnalyserWindow openedWindow = EditorWindow.CreateInstance<TextureAnalyserWindow>();
			openedWindow.Show();
//			TextureAnalyserWindow openedWindow = EditorWindow.GetWindow<TextureAnalyserWindow>("Texture Analyser");
			openedWindow.SetupMaterial();
			if (!openedWindow.MainTextureSelected){
				if (Selection.activeObject is Texture){
					openedWindow.SetMainTexture(Selection.activeObject as Texture);
				}
			}
		}

		protected void OnEnable(){
			this.titleContent.text = "Texture Analyser";
			if (m_materialEditor == null){
				m_materialEditor = CreateInstance<CustomMaterialEditor>();
				m_materialEditor.hideFlags = HideFlags.HideAndDontSave;
				m_materialEditor.hideMainMaterial = true;
			}
			if (m_TextureView == null){
				m_TextureView = new TextureDisplay();
			}
			if (m_textureSelector == null){
				m_textureSelector = CreateInstance<FieldFinder>();
				m_textureSelector.fieldType = typeof(Texture);
				m_textureSelector.hideFlags = HideFlags.HideAndDontSave;
			}
			m_textureSelector.AddFieldSelectedListener(HandleFieldSelected);
			if (m_GUIDrawer == null){
				m_GUIDrawer = CreateInstance<ExceptionPausedDrawer>();
				m_GUIDrawer.hideFlags = HideFlags.HideAndDontSave;
			}
			
			this.minSize = new Vector2(165, 165);
			
			Repaint();// Force first OnGUI. Necessary to start updating when entering play mode.
		}
		
		
		protected void OnDestroy(){
			DestroyImmediate(m_materialEditor);
			DestroyImmediate(m_analysisMaterial);
			DestroyImmediate(m_textureSelector);
		}
		
		protected void OnInspectorUpdate(){
			// Update graphics again if required:
			if (!fastUpdate && m_TextureView.requiresRedraw) Repaint();
		}
		
		protected void OnFocus(){
			fastUpdate = true;
		}
		
		protected void OnLostFocus(){
			fastUpdate = false;
		}
		
		protected void OnGUI(){	
			this.eventData = Event.current;
			m_GUIDrawer.AttemptDrawing(DrawGUI);
		}
		
		protected void DrawGUI(){
			m_redrawRequired = false;
			
			if (m_analysisMaterial == null) SetupMaterial();
			if (trackingEnabled){
				
				// Default Selection:
				if (m_selectedField == null){
					if (m_textureSelector.areFieldsAvailable) m_selectedField = m_textureSelector.availableFields[0];
				}
				
				// Retrieve texture from tracked field:
				if (m_selectedField != null){
					object fieldValue = m_selectedField.GetValue();
					if (!typeof(Texture).IsAssignableFrom(fieldValue.GetType())){
						m_textureSelector.DestroyFieldReference(m_selectedField);
					}
					
					SetMainTexture( (Texture) fieldValue);
				}
			}
			
			float areaWidth = position.width;
			float areaHeight = EditorGUIUtility.singleLineHeight * 2;
			
			// Determine required scaling for making texture fit:
			if (m_mainTexture != null){
				float widthScale = position.width / m_mainTexture.width;
				float heightScale = position.height / m_mainTexture.height;
				
				float fittingScale = Mathf.Min(widthScale, heightScale);
				
				areaWidth = m_mainTexture.width * fittingScale;
				areaHeight = m_mainTexture.height * fittingScale;
				
				// Determine required scaling for making controls fit:
				float controlsWidthFitScale = (position.width - minimumControlsWidth) / areaWidth;
				float controlsHeightFitScale = (position.height - minimumControlsHeight) / areaHeight;
				
				// Controls should be where least downscaling is required:
				m_sideControls = controlsWidthFitScale > controlsHeightFitScale;
				float fittingControlsScale = Mathf.Max(controlsWidthFitScale, controlsHeightFitScale);
				fittingControlsScale = Mathf.Clamp01(fittingControlsScale);// Prevent Scaling Up and negative scaling
				
				areaWidth *= fittingControlsScale;
				areaHeight *= fittingControlsScale;
			}
			else {
				m_sideControls = false;
			}
			Rect textureArea = new Rect(0, 0, areaWidth, areaHeight);
			
			DrawTextureArea(textureArea);
			
			Rect controlsArea;
			if (this.sideControls) controlsArea = new Rect(textureArea.width, 0, position.width - textureArea.width, position.height);
			else controlsArea = new Rect(0, textureArea.height, position.width, position.height - textureArea.height);
			
			GUI.Box(controlsArea, GUIContent.none);
			GUILayout.BeginArea(controlsArea);
			
			windowScrollSettings = EditorGUILayout.BeginScrollView(windowScrollSettings);
			
			EditorGUILayout.BeginVertical();
			m_TextureView.DrawOffsetControls();
						
			EditorGUILayout.BeginHorizontal();
			DrawTrackingToggle();
			if (trackingEnabled){
				m_textureSelector.DrawSelectedObjectField();
				EditorGUILayout.EndHorizontal();
				
				m_textureSelector.DrawFieldSelectorDropDown(m_selectedField);
				
			}else EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.Space();
			
			
			DrawMainTextureSelector();
			
			
			
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.BeginVertical();
				DrawShaderSelector();
				
				DrawMaterialSettings();
			EditorGUILayout.EndVertical();
			
			
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
			
			if (this.redrawRequired) Repaint();
		}
		
		protected void HandleFieldSelected(object sender, FieldSelectedEventArgs e){
			m_selectedField = e.selectedField;
		}
		
		public void SetMainTexture(Texture mainTexture){
			if (m_analysisMaterial == null) SetupMaterial();
			m_mainTexture = mainTexture;
			CheckMainTextureDimensionMatch();
		}
		
		public void TrackField(string fieldName, Component fieldOwner){
			trackingEnabled = true;
			m_textureSelector.FindFieldOnComponent(fieldName, fieldOwner);
		}
		
		private void SetupMaterial(){
			if (m_analysisMaterial != null) return;
			
			if (this.SelectedShader == null){
				ResetToDefaultShader();
			}
			
			m_analysisMaterial = new Material(this.SelectedShader);
			m_analysisMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		
		private void ResetToDefaultShader(){
			this.SelectedShader = Shader.Find("Hidden/Debug/TextureAnalysis");	
		}
		
		protected void DrawTextureArea(Rect textureArea){
			if (m_selectedShader != null) m_TextureView.DrawTextureView(m_mainTexture, m_analysisMaterial, this.eventData, textureArea, m_mainTextureDimensionMatch);
			else EditorGUI.HelpBox(textureArea, "No Shader Selected", MessageType.Warning);
		}
		
		protected void DrawTrackingToggle(){
			if (this.trackingEnabled) trackingToggleLabel.tooltip = "Toggle to stop tracking a texture field for updates";
			else this.trackingToggleLabel.tooltip = "Toggle to start tracking a texture field for updates";
			
			this.trackingToggleLabel.text = "Track Field";
			
			this.trackingEnabled = GUILayout.Toggle(trackingEnabled, this.trackingToggleLabel, GUILayout.ExpandWidth(false));
		}
		
		private void DrawMainTextureSelector(){
			string textureName = "";
			string textureResolutionInfo = "";
			if (m_mainTexture != null){
				textureName = "(" + m_mainTexture.name + ")";
				textureResolutionInfo = m_mainTexture.width + "x" + m_mainTexture.height;
			}
			GUIContent label = new GUIContent("Main Texture " +  textureName, textureResolutionInfo);
			
			EditorGUILayout.BeginHorizontal();{
				
				EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false));{
					EditorGUILayout.LabelField(label);
					EditorGUILayout.LabelField(textureResolutionInfo);
					
				}EditorGUILayout.EndVertical();
				
				
				EditorGUIUtility.labelWidth = 2f;// Remove standard label.
				Texture selectedTexture = EditorGUILayout.ObjectField(
					GUIContent.none,// use label version to make it a square object field, like in material displays.
					m_mainTexture,
					typeof(Texture),
					true
					) as Texture;
				if (selectedTexture != m_mainTexture){
					SetMainTexture(selectedTexture);
				}
				EditorGUIUtility.labelWidth = 0f;
			}EditorGUILayout.EndHorizontal();
		}
		
		private void DrawShaderSelector(){
			GUIContent label = new GUIContent("Analysis Shader:");
			Shader newShader = EditorGUILayout.ObjectField(
				label,
				this.SelectedShader,
				typeof(Shader),
				true) as Shader;
			if (newShader != this.SelectedShader) this.SelectedShader = newShader;
		}
		
		private void DrawMaterialSettings(){
			if (m_analysisMaterial == null) return;
			m_materialEditor.selectedMaterial = m_analysisMaterial;
			m_materialEditor.DrawMaterialSettings();
		}
		
		/// <summary>
		/// Loops through all properties on the given shader to find a property with the given name.
		/// Not the same as Shader.PropertyToId which returns a unique ID.
		/// </summary>
		/// <returns>
		/// The index of the property with the given targetName.
		/// -1 if none of the properties have targetName as name.
		/// </returns>
		/// <param name='selectedShader'>
		/// Selected shader.
		/// </param>
		/// <param name='targetName'>
		/// Target name.
		/// </param>
		private int FindMaterialPropertyIndex(Shader selectedShader, string targetName){
			if (selectedShader != null){
				int propertyCount = ShaderUtil.GetPropertyCount(selectedShader);
				
				for (int i = 0; i < propertyCount; i++) {
					string propertyName = ShaderUtil.GetPropertyName(selectedShader, i);
					if (propertyName == targetName) return i;
				}
			}
			return -1;
		}
		
		/// <summary>
		/// Checks if the MainTexture property type matches the type of the selected main texture.
		/// The result is stored in m_mainTextureDimensionMatch.
		/// This method should be called whenever the shader or texture changes.
		/// </summary>
		private void CheckMainTextureDimensionMatch(){
			int propertyId = FindMaterialPropertyIndex(m_selectedShader, "_MainTex");
			if (propertyId >= 0){
				UnityEngine.Rendering.TextureDimension texturePropertyType = ShaderUtil.GetTexDim(m_selectedShader, propertyId);
			
				if (texturePropertyType == UnityEngine.Rendering.TextureDimension.Any){
					m_mainTextureDimensionMatch = true;
					return;
				}
			
				if (m_mainTexture is Texture2D){
					m_mainTextureDimensionMatch = (texturePropertyType == UnityEngine.Rendering.TextureDimension.Tex2D);
				}
				else if (m_mainTexture is Cubemap){
					m_mainTextureDimensionMatch = (texturePropertyType == UnityEngine.Rendering.TextureDimension.Cube);
				}
				else if (m_mainTexture is RenderTexture){
					RenderTexture mainRenderTexture = m_mainTexture as RenderTexture;
					if (mainRenderTexture == null) m_mainTextureDimensionMatch = false;
					else{
						m_mainTextureDimensionMatch = (texturePropertyType == mainRenderTexture.dimension);	
					}
				}
			}
			else{
				m_mainTextureDimensionMatch = false;	
			}
		}
		
	}
}