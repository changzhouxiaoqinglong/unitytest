// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo {
	
	[System.Serializable]
	public class TextureDisplay {
		
		public bool zoomLimited = true;
		public bool continuousUpdate = false;
		
		public float scrollSpeed = 0.05f;
		
		private Rect sampleArea = new Rect(0,0,1,1);
		private float textureAreaBorderWidth = 8f;
		private float coordinateWidth = 1f;// Width of the area visible on screen, in normalized texture space.	
		
		public bool scaleChanged;
		private bool dragPanning = false;
		
		private Vector2 dragAnchorPosition;
		
		private GenericMenu contextMenu;
		
		public bool requiresRedraw{
			get { return continuousUpdate || scaleChanged || dragPanning; }
		}
		
		/// <summary>
		/// Draws the texture view.
		/// </summary>
		public void DrawTextureView(Texture selectedTexture, Material selectedMaterial, Event currentEvent, Rect drawArea, bool textureDimensionsMatch ){
			if (currentEvent.type == EventType.Layout){
				this.scaleChanged = false;// Reset state
			}
			
			// Slight margin for background:
			Rect backgroundArea = new Rect(
				drawArea.x + 1, 
				drawArea.y + 1,
				drawArea.width - 2, 
				drawArea.height - 2);
			
			GUI.Box(backgroundArea, GUIContent.none);
			
			Rect texturePosition = new Rect(
				drawArea.x + this.textureAreaBorderWidth, 
				drawArea.y + this.textureAreaBorderWidth, 
				drawArea.width - 2 * this.textureAreaBorderWidth,
				drawArea.height - 2 * this.textureAreaBorderWidth);
			
			HandleZoomToMouse(texturePosition, currentEvent);
			HandlePanning(texturePosition, currentEvent);
			HandleTextureAreaContextMenu(texturePosition, currentEvent);
			
			if (selectedTexture == null){
				EditorGUI.HelpBox(drawArea, "No Texture Selected", MessageType.Warning);
				return;
			}
			
			// Work-around for crash due to GrabPass:
			if (selectedMaterial.passCount > 1){
				EditorGUI.HelpBox(drawArea, "Not drawing texture for multipass shaders. Just in case it is a GrabPass shader, which would crash the editor (Unity 4.2.2).",MessageType.Warning);
				return;
			}
			
			if (!textureDimensionsMatch){
				EditorGUI.HelpBox(drawArea, "Missmatch between Selected Texture, and Material MainTexture property type. Are you using the right shader?", MessageType.Error);
				return;
			}
			
			// Only use Graphics.DrawTexture during repaint:
			if (currentEvent.type == EventType.Repaint){
				// Display Texture:
				FilterMode originalFilterMode = selectedTexture.filterMode;
				selectedTexture.filterMode = FilterMode.Point;
				
//				EditorGUI.DrawTextureTransparent(texturePosition, selectedTexture);
//				EditorGUI.DrawPreviewTexture(texturePosition, selectedTexture, null);
				Graphics.DrawTexture(texturePosition, selectedTexture, this.sampleArea, 0,0,0,0, selectedMaterial);
				selectedTexture.filterMode = originalFilterMode;
			}
		}
		
		public void DrawOffsetControls(){
			GUIContent label = new GUIContent("Texture offset:");
		
			EditorGUILayout.BeginHorizontal();
			
			EditorGUIUtility.labelWidth = 100f;
			EditorGUILayout.PrefixLabel(label);
			
			EditorGUIUtility.labelWidth = 25f;
			EditorGUIUtility.fieldWidth = 25f;
			this.sampleArea.x = EditorGUILayout.FloatField("X:", this.sampleArea.x, GUILayout.Width(100f));
			this.sampleArea.y = EditorGUILayout.FloatField("Y:", this.sampleArea.y, GUILayout.Width(100f));
			EditorGUIUtility.labelWidth = 0f;// Reset to default
			EditorGUIUtility.fieldWidth = 0f;
			
			EditorGUILayout.EndHorizontal();

		}
		
		
		private void HandleTextureAreaContextMenu(Rect textureArea, Event currentEvent){
			if (!textureArea.Contains(currentEvent.mousePosition)) return;
			if(currentEvent.type == EventType.ContextClick){
				this.contextMenu = new GenericMenu();
				this.contextMenu.AddItem(new GUIContent("Reset View", "Reset Offset and Scaling"), false, ResetOffsets);
				this.contextMenu.AddItem(new GUIContent("Limit Zoom"), this.zoomLimited, ToggleZoomLimit);
				this.contextMenu.AddItem(new GUIContent("Continuous Update", "Redraw Every Update (expensive)"), this.continuousUpdate, ToggleContinuousUpdate);
				
				this.contextMenu.ShowAsContext();
			}	
		}
		
		
		private void HandleZoomToMouse(Rect textureArea, Event currentEvent){		
			if (currentEvent.type != EventType.ScrollWheel) return;// Only handle ScrollWheel event
			
			Vector2 mousePosition = currentEvent.mousePosition;
			if (!textureArea.Contains(mousePosition)) return;// Ignore scrolling ourside texture area
			currentEvent.Use();// Making objects outside this area ignore this event
			
			Vector2 originalMouseTextureCoords = WindowPositionToTexturePosition(mousePosition, textureArea);
			
			// Calculate new coordinate width:
			float originalWidth = this.coordinateWidth;// Store for change detection
			
			this.coordinateWidth += currentEvent.delta.y * scrollSpeed * this.coordinateWidth;// Speed up scrolling when zoomed out, slow down scrolling when zoomed in
			if (zoomLimited) this.coordinateWidth = Mathf.Clamp01(coordinateWidth);
			
			// Detect changes:
			if (Mathf.Abs(originalWidth - this.coordinateWidth) > 0.0001) this.scaleChanged = true;
			
			if (this.scaleChanged){
				// Apply sample Area size changes:
				this.sampleArea.width = this.coordinateWidth;
				this.sampleArea.height = this.coordinateWidth;
				
				// Move center so mouse ends up over the same region:
				Vector2 newMouseLocation = WindowPositionToTexturePosition(mousePosition, textureArea);
				Vector2 textureDistanceMoved = newMouseLocation - originalMouseTextureCoords;
				
				this.sampleArea.x -= textureDistanceMoved.x;
				this.sampleArea.y -= textureDistanceMoved.y;
			}
			
			
		}
		
		private void HandlePanning(Rect textureArea, Event currentEvent){
			if (currentEvent.type == EventType.MouseDown && currentEvent.button == (int)MouseButton.Middle){
				if (!this.dragPanning && textureArea.Contains(currentEvent.mousePosition)){
					// Start Dragging:
					this.dragPanning = true;
					this.dragAnchorPosition = WindowPositionToTexturePosition( currentEvent.mousePosition, textureArea);
				}
			}
			else if (currentEvent.rawType == EventType.MouseUp){ // RawType, because we also want to detect mouseUp from outside.
				if (this.dragPanning){
					// Cancel dragging:
					this.dragPanning = false;
				}
			}
			else{
				// Handle dragging:
				if (this.dragPanning){
					if (currentEvent.type == EventType.Layout) return;// During layout event, the texture area is incorrect.
					
					Vector2 newMousePosition = WindowPositionToTexturePosition( currentEvent.mousePosition, textureArea);
					Vector2 mouseOffset = newMousePosition - this.dragAnchorPosition;
					
					this.sampleArea.x -= mouseOffset.x;
					this.sampleArea.y -= mouseOffset.y;
				}
			}
			
		}
		
		private Vector2 WindowPositionToTexturePosition(Vector2 relativeToWindow, Rect textureArea){
			Vector2 relativeToArea = relativeToWindow - new Vector2(textureArea.x, textureArea.y);
			// Normalize coordinates:
			relativeToArea.x /= textureArea.width;
			relativeToArea.y /= textureArea.height;
			
			// Apply current texture transform:
			float xPosition = (relativeToArea.x * this.sampleArea.width) + this.sampleArea.x;
			relativeToArea.y = 1 - relativeToArea.y;// reverse coordinates (bottom to top)
			float yPosition = (relativeToArea.y * this.sampleArea.height) + this.sampleArea.y;
			return new Vector2(xPosition,yPosition);
		}
		
		
		private void ResetOffsets(){
			this.coordinateWidth = 1f;
			this.sampleArea = new Rect( 0f, 0f, 1f, 1f);
		}
		
		public void ToggleZoomLimit(){
			this.zoomLimited = !this.zoomLimited;
			
			if (zoomLimited){
				this.coordinateWidth = Mathf.Clamp01(coordinateWidth);
				this.sampleArea.width = this.coordinateWidth;
				this.sampleArea.height = this.coordinateWidth;
				
				this.scaleChanged = true;
			}
		}
		public void ToggleContinuousUpdate(){
			this.continuousUpdate = !this.continuousUpdate;
		}
		
	}
}