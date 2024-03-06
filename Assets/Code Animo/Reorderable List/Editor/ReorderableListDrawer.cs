// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

namespace CodeAnimo {
	
	/// <summary>
	/// Property Drawer to add re-ordering functionality to generic Lists.
	/// Applying the ReorderableListAttribute to other classes is not yet support and might behave in unexpected ways.
	/// Only works for object lists.
	/// </summary>
	[CustomPropertyDrawer(typeof(ReorderableListAttribute))]
	public class ReorderableListDrawer : PropertyDrawer {
		
		/// <summary>
		/// Drop location, where the lowest index is drawn at the top.
		/// </summary>
		protected enum DropLocation{ Above, Below };
		
		private string notAListExceptionMessage{
			get{ return "No list found that the given property belongs to, are you trying to draw a non-list element?"; }
		}
		
		private IList m_targetList = null;
		protected IList targetList{
			get {
				if (m_targetList == null) throw new System.NullReferenceException("Trying to draw a non-list property, or list has not yet been cached.");
				else return m_targetList;
			}
		}
		private SerializedObject m_listOwner;
		protected SerializedObject listOwner{
			get { 
				if (m_listOwner == null) throw new System.NullReferenceException("Trying to access the serializedObject attached to the current property, before its reference was cached.");
				return m_listOwner;
			}
		}
		
		protected System.Type[] listArgumentTypes{
			get {
				System.Type listType = fieldInfo.FieldType;
				return listType.GetGenericArguments();
			}
		}
		
		protected System.Type listArgumentType{
			get {
				System.Type[] argumentTypes = listArgumentTypes;
				if (argumentTypes.Length < 1) throw new System.InvalidOperationException("The target field " + fieldInfo + " doesn't appear to be a generic list");
				if (argumentTypes.Length > 1) throw new System.InvalidOperationException("The target field " + fieldInfo + " has too many " + argumentTypes.Length + " generic arguments, 1 expected");
				
				return argumentTypes[0];// Assume the generic list has only one type.
			}
		}
		
		
		private float buttonWidth = 20f;
		private float dropGraphicHeight = 2f;
		
		private GUIContent removalButtonLabel = new GUIContent("X","Removes this item from the list");
		private GUIContent dragButtonLabel = new GUIContent("≡", "Drag to reorder");// use ≡ for its similarity to the three stripes used in other types of draggable controls.
		
		public static string dragDropIdentifier{
			get {
				if (_dragDropIdentifier == null)
					_dragDropIdentifier = typeof(ReorderableListDrawer).FullName + ".DragData";
				return _dragDropIdentifier;
			}
		}
		private static string _dragDropIdentifier;
		
		
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label){
			CacheReferences(property, fieldInfo);
			
			Rect dragIndicatorPosition = new Rect(position.xMin, position.yMin, buttonWidth, position.height);
			Rect removeButtonPosition = new Rect(position.xMax - buttonWidth, position.yMin, buttonWidth, position.height);
			float objectFieldWidth = removeButtonPosition.xMin - dragIndicatorPosition.xMax;// Remaining width
			Rect objectFieldPosition = new Rect(dragIndicatorPosition.xMax, position.yMin, objectFieldWidth, position.height);
			
			EditorGUIUtility.AddCursorRect(removeButtonPosition, MouseCursor.ArrowMinus);
			if(GUI.Button(removeButtonPosition, this.removalButtonLabel)){
				RemoveElementFromList(label.text);
				return;
			}
			
			// Indentation is already applied in the position, no need to indent further:
			int originalIndentationLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			
			GUIContent propertyLabel = new GUIContent();
			EditorGUI.PropertyField(objectFieldPosition, property, propertyLabel);
			
			EditorGUI.indentLevel = originalIndentationLevel;// reset indentation
			
			
			// Drag and Drop / Re-Ordering:
			GUI.Label(dragIndicatorPosition, dragButtonLabel);
			
			// Area used for dropping extends slightly above and below the given dropArea:
			Rect extendedDropArea = new Rect(
				dragIndicatorPosition.xMin, 
				dragIndicatorPosition.yMin - this.dropGraphicHeight, 
				dragIndicatorPosition.width, 
				dragIndicatorPosition.height + (2 * this.dropGraphicHeight));
			DragDropGUI(property, extendedDropArea, position, objectFieldPosition, label.text);
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label){
			return EditorGUI.GetPropertyHeight(property);
		}
		
		protected void CacheReferences(SerializedProperty property, FieldInfo listFieldInfo){
			if (m_listOwner == null) m_listOwner = property.serializedObject;// cache reference. Used for finding list reference and undo system.
			if (m_targetList == null) m_targetList = findListReference(listFieldInfo);// cache list reference for as long as this propertyDrawer is on screen.
		}
		
		/// <summary>
		/// Uses reflection to get a reference to the Collection to which the given property belongs.
		/// </summary>
		/// <returns>ICollection if the collection can be found, otherwise null</returns>
		/// <param name="property">The property that belongs to the field described by fieldInfo</param>
		protected IList findListReference(FieldInfo listFieldInfo){
			// Find reference to list through a reference to the serializedObject that the property belongs to:
			Object containingClass = this.listOwner.targetObject;// reference to the instance that contains the field we're editing
			IList listReference = listFieldInfo.GetValue(containingClass) as IList;

			if (listReference == null) throw new System.MissingFieldException( notAListExceptionMessage );

			return listReference;
		}

		/// <summary>
		/// Find a serializedProperty that points to the list containing the given property.
		/// The returned serializedProperty can be used for basic array manipulation.
		/// List can not be retrieved from this serializedProperty using ObjectReference.
		/// </summary>
		/// <returns>The list property.</returns>
		/// <param name="childProperty">Child property.</param>
		protected SerializedProperty findListProperty(SerializedProperty childProperty){
			SerializedProperty listProperty = this.listOwner.FindProperty(fieldInfo.Name);

			if (listProperty == null) throw new System.MissingFieldException( notAListExceptionMessage );

			return listProperty;
		}
		
		/// <summary>
		/// Removes the first 8 characters of label, and uses the rest to reconstruct the index integer.
		/// Assumes the label is formatted as "Element [index number]"
		/// </summary>
		/// <returns>
		/// The number at the end of label.
		/// </returns>
		/// <param name='label'>
		/// The list element label.
		/// </param>
		/// <exception cref='System.ArgumentException'>
		/// Is thrown when there is a format problem.
		/// </exception>
		protected int ParseIndexFromLabel(string label){
			// Assume the label starts with "Element "
			try{
				return int.Parse(label.Substring(8));
			}
			catch(System.FormatException e){
				// Give the error some context.
				throw new System.ArgumentException( "Can't convert label into index because of a problem with its format.", e );	
			}
		}
		
		/// <summary>
		/// Removes the first occurance of the element from the current list.
		/// If there are multiple occurances of the given object in the list, this behavior can be unexpected.
		/// </summary>
		/// <param name='property'>
		/// The property that should be removed, and for which undo state should be set.
		/// </param>
		protected void RemoveElementFromList(SerializedProperty property){
			RegisterChange("List Element Removal");

			this.targetList.Remove(property.objectReferenceValue);
		}
		
		/// <summary>
		/// Removes the element from list, using the index parsed from its label to remove it from the exact index we're working on
		/// </summary>
		/// <param name='property'>
		/// The property for which an undo state should be set.
		/// It will automatically try to find the correct parent object.
		/// </param>
		/// <param name='indexLabel'>
		/// Index label.
		/// </param>
		protected void RemoveElementFromList(string indexLabel){
			RegisterChange("List Element Removal");
			int index = ParseIndexFromLabel(indexLabel);
			this.targetList.RemoveAt(index);
		}

		/// <summary>
		/// Marks the object that owns the given property as dirty and updates the undo system.
		/// </summary>
		/// <param name="property">Target Property.</param>
		/// <param name="undoMessage">Undo message.</param>
		protected void RegisterChange(string undoMessage){
			Object[] affectedObjects = this.listOwner.targetObjects;
			for (int i = 0; i < affectedObjects.Length; i++) {
				Object affectedObject = affectedObjects[i];
				Undo.RecordObject(affectedObject, undoMessage);
				EditorUtility.SetDirty(affectedObject);
			}
		}

		protected void DragDropGUI(SerializedProperty property, Rect dropArea, Rect propertyArea, Rect objectFieldArea, string currentElementIndexLabel){
			Event currentEvent = Event.current;
			EventType currentEventType = currentEvent.type;
			
			// Mouse Cursor to indicate you can drag.
			if (DragAndDrop.visualMode == DragAndDropVisualMode.None){// Don't try to fight with the drag/drop graphics.
				EditorGUIUtility.AddCursorRect(dropArea, MouseCursor.ResizeVertical);
			}
			
			// The DragExited event does not have the same mouse position data as the other events.
			if ( currentEventType == EventType.DragExited ){
				// Handle cancelling of drag, for example when user presses escape.
				DragAndDrop.PrepareStartDrag();
			}
			
			if (!dropArea.Contains(currentEvent.mousePosition))	return;
			
			switch (currentEventType){
			case EventType.MouseDown:
				DragAndDrop.PrepareStartDrag();// Reset Data
				
				ReorderableListDragData dragData = new ReorderableListDragData();
				dragData.originalIndex = ParseIndexFromLabel(currentElementIndexLabel);
				dragData.originalList = this.targetList;
				
				DragAndDrop.SetGenericData(dragDropIdentifier, dragData);
				
				if (property.propertyType == SerializedPropertyType.ObjectReference){
					Object objectToDrag = property.objectReferenceValue;
					if (objectToDrag != null){
						AddObjectToDrag(objectToDrag);
					}
					else SetEmptyDragReferences();					
				}
				else SetEmptyDragReferences();
				//TODO: Find non-Object Reference value and box it for transport through DragAndDrop.
				
				currentEvent.Use();
				
				break;
			case EventType.MouseDrag:
				// If drag was started here:
				ReorderableListDragData existingDragData = FindGenericDragData();
				if (existingDragData != null){
					DragAndDrop.StartDrag("Dragging List ELement");
					currentEvent.Use();
				}
				break;
			case EventType.DragUpdated:
				if (IsDragTargetValid( DragAndDrop.objectReferences, this.listArgumentType )) DragAndDrop.visualMode = DragAndDropVisualMode.Link;
				else DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
				currentEvent.Use();
				break;
			case EventType.Repaint:
				DrawDropGraphic(DragAndDrop.visualMode, propertyArea, currentEvent.mousePosition, objectFieldArea);
				break;
			case EventType.DragPerform:
				PerformDrag(currentEvent, currentElementIndexLabel, dropArea);
				break;		
			case EventType.MouseUp:
				DragAndDrop.PrepareStartDrag();
				break;
			}

		}
		
		/// <summary>
		/// Determines whether dragged items can be dropped here.
		/// Either because the drag started here, or because any of the dragged Objects have a type that is accepted by the current list.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the dragged Objects can be added to the current list; otherwise, <c>false</c>.
		/// </returns>
		protected bool IsDragTargetValid( Object[] draggedObjects, System.Type requiredType){
			ReorderableListDragData dragData = FindGenericDragData();
			if (dragStartedHere(dragData)) return true;
			
			if (requiredType.IsValueType) return false;// Don't allow value type values from other sources. (Workaround for this drawer not passing value type data to dragdrop system anyway)
			
			// If it didn originate from here, check type:
			for (int i = 0; i < draggedObjects.Length; i++) {
				Object draggedObject = FindListCompatibleObject( draggedObjects[i], requiredType );
				
				if (draggedObject != null){
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Determines the drop location. Based on whether the mouse is above or below the center.
		/// </summary>
		/// <returns>
		/// Where the drop location should be displayed.
		/// </returns>
		/// <param name='dropArea'>
		/// Total drop area.
		/// </param>
		/// <param name='mouseLocation'>
		/// Mouse location.
		/// </param>
		protected DropLocation DetermineDropLocation(Rect dropArea, Vector2 mouseLocation){
			float halfHeight = dropArea.yMin + dropArea.height * 0.5f;
			if (mouseLocation.y > halfHeight){
				return DropLocation.Below;
			}
			else return DropLocation.Above;
		}
		
		/// <summary>
		/// Draws a line where the object will be added, if it's not obscured by the accompanying object field.
		/// </summary>
		/// <param name='visualMode'>
		/// The currently used VisualMode by DragAndDrop class.
		/// </param>
		/// <param name='propertyArea'>
		/// The area used by the property for which the graphic is shown.
		/// </param>
		/// <param name='mousePosition'>
		/// Current mouse position
		/// </param>
		/// <param name='objectFieldArea'>
		/// Area used by the object field.
		/// </param>
		/// <exception cref='System.InvalidOperationException'>
		/// Is thrown drop location can't be determined.
		/// </exception>
		protected void DrawDropGraphic(DragAndDropVisualMode visualMode, Rect propertyArea, Vector2 mousePosition, Rect objectFieldArea){
			// Don't draw if not dragging.
			switch(visualMode){
			case DragAndDropVisualMode.None:
			case DragAndDropVisualMode.Rejected:
//			case DragAndDropVisualMode.Generic:// potential fix for graphic showing up right after opening the editor.
				return;
			}
			if (objectFieldArea.Contains(mousePosition)) return;// Don't draw the graphic when it's being obscured by the object field.
			
			// Determine location on the y axis:
			float dropGraphicY = 0;			
			switch(DetermineDropLocation(propertyArea, mousePosition)){
			case DropLocation.Above:
				dropGraphicY = propertyArea.yMin - this.dropGraphicHeight;
				break;
			case DropLocation.Below:
				dropGraphicY = propertyArea.yMax;
				break;
			default:
				throw new System.InvalidOperationException("Drop location could not be determined.");
			}
			
			Rect graphicShape = new Rect(propertyArea.xMin, dropGraphicY, propertyArea.width, this.dropGraphicHeight);
						
			EditorGUI.DrawRect(graphicShape, Color.grey);
		}
		
		protected void PerformDrag(Event currentEvent, string elementIndexLabel, Rect dropArea){
			DragAndDrop.AcceptDrag();
				
			// Add acceptable objects in the location indicated by the dropGraphic:
			int targetIndex = ParseIndexFromLabel(elementIndexLabel);// Find index of current Element.
			DropLocation relativePosition = DetermineDropLocation(dropArea, currentEvent.mousePosition);
			if ( relativePosition == DropLocation.Below) targetIndex++;
			
			// Choose between reordering and adding:
			ReorderableListDragData dragData = FindGenericDragData();
			if (dragStartedHere(dragData)) ReorderObject(dragData.originalIndex, targetIndex);
			else AddDraggedObjectsToList(targetIndex);

			currentEvent.Use();
		}
		
		/// <summary>
		/// Moves the object at originalIndex to destinationIndex
		/// </summary>
		/// <param name='originalIndex'>
		/// The index of the starting position.
		/// </param>
		/// <param name='destinationIndex'>
		/// The index of the intended position
		/// </param>
		protected void ReorderObject(int originalIndex, int destinationIndex){
			RegisterChange("List Order Change");
			if (originalIndex == destinationIndex) return;
			
			IList currentList = this.targetList;
			var originalElement = currentList[originalIndex];
			
			if (originalIndex < destinationIndex) destinationIndex--;// index will change after element removal.
			
			currentList.RemoveAt(originalIndex);
			currentList.Insert(destinationIndex, originalElement);
		}
		
		protected void AddObjectToDrag(Object dragObject){
			Object[] objectReferences = new Object[1]{dragObject};
			DragAndDrop.objectReferences = objectReferences;
		}
		
		/// <summary>
		/// Sets the empty drag references.
		/// This may be necessary when there's no data (null) to share.
		/// DragAndDrop.objectReferences should never have null data, because it causes exceptions in built-in editor code.
		/// </summary>
		protected void SetEmptyDragReferences(){
			DragAndDrop.objectReferences = new Object[0];
		}
		
		protected void AddDraggedObjectsToList(int destinationIndex){
			RegisterChange("List Element Addition");
			
			Object[] draggedObjects = DragAndDrop.objectReferences;
			
			for (int i = draggedObjects.Length - 1; i >= 0 ; i--) {// adding the last ones first ensures the order ends up the same.
				Object draggedObject = FindListCompatibleObject( draggedObjects[i], this.listArgumentType );
				
				if (draggedObject != null){
					this.targetList.Insert(destinationIndex, draggedObject);
				}
			}
		}
		
		protected ReorderableListDragData FindGenericDragData(){
			return DragAndDrop.GetGenericData(dragDropIdentifier) as ReorderableListDragData;
		}
		
		protected bool dragStartedHere(ReorderableListDragData dragData ){
			return (dragData != null && dragData.originalList == this.targetList);
		}

		/// <summary>
		/// Returns an object compatible with the current list, or null if it can't find one.
		/// The compatible object can be a component on a given GameObject.
		/// </summary>
		/// <returns>
		/// The compatible object, or null if none was found.
		/// </returns>
		/// <param name='targetObject'>
		/// The object which is, or contains the valid object.
		/// </param>
		protected Object FindListCompatibleObject(Object targetObject, System.Type requiredType){
			if (targetObject == null) throw new System.NullReferenceException("Dragging null object");
			System.Type objectType = targetObject.GetType();
			
			if (requiredType.IsAssignableFrom(objectType)) return targetObject;
			else {
				// If it is a GameObject, try to find the relevant component:
				GameObject parentObject = targetObject as GameObject;
				if (parentObject != null){
					return parentObject.GetComponent(requiredType);
				}
				else {
					return null;
				}
			}
		}
		
		
	}
		
	public class ReorderableListDragData{
		public int originalIndex;
		public IList originalList;
	}
		
}