// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace CodeAnimo {
	
	/// <summary>
	/// Used to find fields of a specific type on GameObjects, and to retrieve their values.
	/// </summary>
	[System.Serializable]
	public class FieldFinder : ScriptableObject {
		
		public event System.EventHandler<FieldSelectedEventArgs> fieldSelected;
		
		public System.Type fieldType = typeof(Object);
		
		public GameObject selectedObject{
			get { return m_selectedObject; }
			set{
				if (m_selectedObject != value){
					m_selectedObject = value;
					
					if (value != null) EnumerateAvailableFields();
					else{
						this.availableFields = null;// Use automatic cleanup code in the property.
					}
				}
			}
		}
		
		public bool areFieldsAvailable{
			get { return (this.availableFields != null && this.availableFields.Count > 0); }
		}
		
		[SerializeField] private GameObject m_selectedObject;
		[SerializeField] private List<SemiSerializedField> m_availableFields;
		
		private GUIStyle m_selectorButtonStyle = null;
		private GUIStyle m_selectorButtonStyleSelected = null;
		
		public List<SemiSerializedField> availableFields{
			get { return m_availableFields; }
			protected set {
				if (value != m_availableFields){
					
					if (m_availableFields != null){
						// Manual clean up of fields previously set as hideAndDontSave:
						for (int i = 0; i < m_availableFields.Count; i++) {
							DestroyImmediate( m_availableFields[i] );
						}
					}
					
					m_availableFields = value;
					
					if (m_availableFields != null){
						// Set objects to be ignored by Garbage Collection:
						for (int j = 0; j < m_availableFields.Count; j++) {
							m_availableFields[j].hideFlags = HideFlags.DontSave;
						}
					}
				}
			}
		}
		
		protected void OnDestroy(){
			// deselect everything to trigger cleanup:
			this.selectedObject = null;// the other parts of the selection are dependent on selectedObject, and set to null automatically.
		}
		
		/// <summary>
		/// Searches for a field with the given name on the given Component.
		/// If the given component is on a different GameObject than currently selected,
		/// it will do a search of all other available fields on that GameObject to allow switching to one of the others.
		/// </summary>
		/// <param name='fieldName'>
		/// Field name.
		/// </param>
		/// <param name='owner'>
		/// Component to which the field belongs.
		/// </param>
		public void FindFieldOnComponent(string fieldName, Component owner){
			this.selectedObject = owner.gameObject;
			SemiSerializedField selectedField = FindAvailableField(fieldName, owner);
			OnFieldSelected(selectedField);
		}
		
		/// <summary>
		/// Iterates over all fields to remove the given field from the list and destroy the reference.
		/// </summary>
		/// <param name='field'>
		/// Field that should be removed from the list and destroyed.
		/// </param>
		public void DestroyFieldReference(SemiSerializedField field){
			for (int i = 0; i < availableFields.Count; i++) {
				if (field == availableFields[i]){
					availableFields.RemoveAt(i);
					i--;// Stay at same index next loop iteration (counter i++)
				}
			}
			DestroyImmediate(field);
		}
		
		/// <summary>
		/// Creates a GenericMenu that contains the given fields, with lambda functions that make this class select the corresponding fields.
		/// </summary>
		/// <returns>
		/// A menu that contains the give fields and their labels.
		/// </returns>
		/// <param name='fields'>
		/// The fields that should be added as options for the menu.
		/// </param>
		protected GenericMenu AddFieldsToMenu(List<SemiSerializedField> fields, SemiSerializedField selectedField){
			GenericMenu menu = new GenericMenu();
			
			for (int i = 0; i < fields.Count; i++) {
				SemiSerializedField field = fields[i];
				
				// Lambda function that should be run when a menu item is clicked:
				GenericMenu.MenuFunction action = CreateSelectionLambda(field);
				
				bool currentlySelected = (selectedField == field) && (selectedField != null);
				
				GUIContent menuOptionLabel = new GUIContent(field.label);
				menu.AddItem(menuOptionLabel, currentlySelected, action);
			}
			return menu;
		}
		
		/// <summary>
		/// Creates a lambda function that selects the given field.
		/// </summary>
		/// <returns>
		/// The created lambda function
		/// </returns>
		/// <param name='field'>
		/// The field that should be selected.
		/// </param>
		protected GenericMenu.MenuFunction CreateSelectionLambda(SemiSerializedField field){
			return () => {
				OnFieldSelected(field);
			};
		}
		
		/// <summary>
		/// Draws the field for the selected object.
		/// </summary>
		public void DrawSelectedObjectField(){
			GUIContent targetObjectPropertyLabel = new GUIContent("", "The GameObject that contains the component with the tracked field.");
			GameObject newTargetObject = EditorGUILayout.ObjectField(targetObjectPropertyLabel, this.selectedObject,typeof(GameObject), true, GUILayout.ExpandWidth(false)) as GameObject;
			if (newTargetObject != selectedObject) selectedObject = newTargetObject;
		}
		
		private void EnumerateAvailableFields(){
			this.availableFields = SemiSerializedField.FindFields(this.selectedObject, this.fieldType);
		}
		
		/// <summary>
		/// Checks the fields available on the currently selected object for a field with the given name.
		/// </summary>
		/// <returns>
		/// The first field found with the given name, on the given component.
		/// null if no field was found.
		/// </returns>
		/// <param name='fieldName'>
		/// Name of the field that should be found.
		/// </param>
		protected SemiSerializedField FindAvailableField(string fieldName, Component owner){
			for (int i = 0; i < availableFields.Count; i++) {
				SemiSerializedField inspectedField = availableFields[i];
				if (inspectedField.fieldName == fieldName){
					
					// Need to compare InstanceID's because Unity might have replaced the instance:
					int inspectedId = inspectedField.owner.GetInstanceID();
					int ownerId = owner.GetInstanceID();
					
					if (inspectedId == ownerId){
						return inspectedField;
						
					}
				}
			}
			return null;
		}
		
		/// <summary>
		/// Draws a dropdown for all the available fields.
		/// </summary>
		public void DrawFieldSelectorDropDown(SemiSerializedField selectedField){
			if (this.selectedObject == null){
				GUILayout.Label("Select a GameObject that has the field you want to track.");
				return;
			}
				
			int fieldCount = availableFields.Count;
			
			if (fieldCount <= 0){// No fields:
				EditorGUILayout.HelpBox("No valid field found.", MessageType.Warning);
				return;
			}
			GUIContent label;
			if (selectedField != null) label = new GUIContent(selectedField.label, "The tracked field");
			else label = new GUIContent("No Field Selected", "No Field is being tracked yet");
			
			
			if (fieldCount == 1){// One Field:
				GUILayout.Label(label);
				return;
			}
			
			// Many available fields:
			if (GUILayout.Button(label, EditorStyles.popup, GUILayout.ExpandWidth(false))){
				GenericMenu menu = AddFieldsToMenu(availableFields, selectedField);
				menu.ShowAsContext();							
				return;
			}
		
		}
		
		public void DrawFieldSelectionButtons(SemiSerializedField selectedField){
			if (this.selectedObject == null){
				GUILayout.Label("Select a GameObject that has the field you want to track.");
				return;
			}
			int fieldCount = availableFields.Count;
			if (fieldCount <= 0){
				EditorGUILayout.HelpBox("No valid field found.", MessageType.Warning);
				return;
			}
			
			for (int i = 0; i < fieldCount; i++) {
				SemiSerializedField field = availableFields[i];
				if (field.isFieldSelected){
					Object fieldValue = (Object)field.GetValue();
					string buttonLabel;
					if (fieldValue != null) buttonLabel = fieldValue.ToString();
					else buttonLabel = field.label;
					
					if (m_selectorButtonStyle == null) m_selectorButtonStyle = CreateArticleButtonStyle();
					if (m_selectorButtonStyleSelected == null) m_selectorButtonStyleSelected = CreateSelectedArticleButtonStyle();
					
					GUIStyle buttonStyle = m_selectorButtonStyle;
					if (field == selectedField) buttonStyle = m_selectorButtonStyleSelected;
					
					if (GUILayout.Button(buttonLabel, buttonStyle)) OnFieldSelected(field);
				}
				else{
					EditorGUILayout.HelpBox("Field no longer holds field selection", MessageType.Error);
				}
			}
			
		}
		
		private GUIStyle CreateArticleButtonStyle(){
			var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
			buttonStyle.alignment = TextAnchor.MiddleLeft;
			return buttonStyle;
		}
		private GUIStyle CreateSelectedArticleButtonStyle(){
			var buttonStyle = CreateArticleButtonStyle();
			buttonStyle.normal = buttonStyle.active;
			return buttonStyle;
		}
		
		protected void OnFieldSelected(SemiSerializedField field){
			FieldSelectedEventArgs e = new FieldSelectedEventArgs(field);
			System.EventHandler<FieldSelectedEventArgs> handler = this.fieldSelected;
			
			if (handler != null){
				handler(this, e);
			}
		}
		
		
		public void AddFieldSelectedListener(System.EventHandler<FieldSelectedEventArgs> handler){
			this.fieldSelected -= handler;// No double subscriptions
			this.fieldSelected += handler;
		}
		
		
	}
	
	public class FieldSelectedEventArgs : System.EventArgs{
		public SemiSerializedField selectedField;
		public FieldSelectedEventArgs(SemiSerializedField selectedField){
			this.selectedField = selectedField;
		}
	}
	
	
}