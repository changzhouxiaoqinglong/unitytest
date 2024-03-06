// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using CodeAnimo.UnityExtensionMethods;


namespace CodeAnimo{
	
	/// <summary>
	/// Used for storing a reference to a field on a specific object.
	/// Because FieldInfo isn't serialized by Unity, this class uses a string based work-around to reconstruct the field reference after an assembly reload
	/// </summary>
	[System.Serializable]
	public class SemiSerializedField : ScriptableObject{

		/// <summary>
		/// Indicates whether values might be lost due to things like assembly reload.
		/// This value is not serialized and defaults to true.
		/// Needs NonSerialized because private editor fields are apparently serialized.
		/// </summary>
		[NonSerialized] private bool ownerRequiresRestore = true;
		[NonSerialized] private bool fieldRequiresRestore = true;
		
		private Component m_owner;
		public Component owner{
			get {
				if (ownerRequiresRestore){
					m_owner = m_owner.FindPostAssemblyReloadComponent();
					ownerRequiresRestore = false;
				}
				return m_owner;
			}
			set {
				m_owner = value;
			}
		}
		private FieldInfo m_field; // FieldInfo isn't serialized by Unity.
		public FieldInfo field{
			get {
				if (fieldRequiresRestore) AttemptFieldReferenceRestore();
				return m_field;
			}
			set {
				if (m_field != value){
					m_field = value;
					m_fieldName = m_field.Name;
					this.name = m_fieldName;
				}
			}
		}
		public Type fieldType{
			get { return field.FieldType; }
		}
		
		[SerializeField] private string m_fieldName;
		
		public string label{
			get {
				if (field == null) return "no field selected";
				else return field.Name + " (" + field.FieldType + ")";
			}
		}
		
		public string fieldName{
			get { return field.Name; }
		}

		
		public bool isFieldSelected{
			get { return field != null && owner != null; }
		}
		
		private static BindingFlags allInstanceMembersFlag{
			get { return BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic; }
		}
		
		private static BindingFlags nonPublicInstanceMembersFlag{
			get { return BindingFlags.Instance | BindingFlags.NonPublic; }
		}
		
		public bool CanAssignTo<T>(){
			return typeof(T).IsAssignableFrom(field.FieldType);
		}
		
		public object GetValue(){
			return this.field.GetValue(owner);
		}
		
		/// <summary>
		/// Tries to return the value of the field.
		/// You need to pass the correct type, because the class itself needs to be non-generic, in order for it to be serialized by unity.
		/// </summary>
		/// <returns>
		/// The value of the field, or null if no field is selected.
		/// </returns>
		/// <typeparam name='T'>
		/// The type of the field.
		/// </typeparam>
		/// <exception cref='InvalidCastException'>
		/// Is thrown when the given type does not match the field type.
		/// </exception>
		public T GetValueOfType<T>() where T : UnityEngine.Object{
			if (!typeof(T).IsAssignableFrom(field.FieldType)) throw new InvalidCastException("Cannot assign fieldType: " + field.FieldType + " to " + typeof(T));
			else return (T) GetValue();
		}
		
		/// <summary>
		/// Attempts to restore field reference after assembly reload.
		/// Unity does not serialize FieldInfo objects.
		/// It does serialize strings, which can be used to restore the reference.
		/// </summary>
		public void AttemptFieldReferenceRestore(){
			Type ownerType = owner.GetType();
			
			if (m_fieldName != null){
				m_field = GetFieldOnComponent(m_fieldName, ownerType);
			}
			else throw new NullReferenceException("Can't restore field reference without a name");

			this.fieldRequiresRestore = false;
		}
		
		/// <summary>
		/// Creates an instance of SemiSerializedField and sets the required data.
		/// Similar to a Constructor, but using CreateInstance, which is required for Scriptable Objects.
		/// </summary>
		/// <returns>
		/// An instance of this class.
		/// </returns>
		/// <param name='field'>
		/// The field that you want the reference to.
		/// </param>
		/// <param name='owner'>
		/// Owner of the field
		/// </param>
		/// <exception cref="InvalidOperationException">
		/// Is thrown when the given field can not be found on the given component.
		/// </exception>
		public static SemiSerializedField CreateSemiSerializedField(FieldInfo field, Component owner){
			if (GetFieldOnComponent(field.Name, owner.GetType()) == null) throw new InvalidOperationException("The given field with the name '" + field.Name + "' does not exist on the component with type " +  owner.GetType());
			
			SemiSerializedField fieldData = CreateInstance<SemiSerializedField>();
			fieldData.fieldRequiresRestore = false;
			fieldData.ownerRequiresRestore = false;
			fieldData.field = field;
			fieldData.owner = owner;
			return fieldData;
		}
		
		/// <summary>
		/// Creates an instance of SemiSerializedField and sets the required data.
		/// Similar to a Constructor, but using CreateInstance, which is required for Scriptable Objects.
		/// Tries to find a field with the given name on the given component.
		/// </summary>
		/// <returns>
		/// An instance of this class.
		/// </returns>
		/// <param name='fieldName'>
		/// Field name.
		/// </param>
		/// <param name='owner'>
		/// Owner.
		/// </param>
		/// <exception cref='System.NullReferenceException'>
		/// Is thrown when given owner is null.
		/// </exception>
		/// <exception cref='System.ArgumentException'>
		/// Is thrown when no field with the given name can be found on the given component.
		/// </exception>
		public static SemiSerializedField CreateSemiSerializedField(string fieldName, Component owner){
			if (owner == null) throw new System.NullReferenceException("Can't track field on a null object");
			FieldInfo field = GetFieldOnComponent(fieldName, owner.GetType());
			if (field == null) throw new System.ArgumentException("Could not find field with the name " + fieldName + ", on Object named " + owner.name);
			
			SemiSerializedField fieldData = CreateInstance<SemiSerializedField>();
			fieldData.field = field;
			fieldData.owner = owner;
			return fieldData;
		}
		
		/// <summary>
		/// Uses Reflectiont to find fields of the given type on the given Game Object.
		/// </summary>
		/// <returns>
		/// Fields of the given type on the given GameObject.
		/// </returns>
		/// <param name='selectedGameObject'>
		/// The GameObject on which the fields should be found.
		/// </param>
		public static List<SemiSerializedField> FindFields(GameObject selectedGameObject, Type fieldType){
			if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldType)) throw new InvalidCastException("fieldType needs to extend UnityEngine.Object. " + fieldType);
			if (selectedGameObject == null) throw new System.NullReferenceException("No GameObject Selected");
			List<SemiSerializedField> foundFields = new List<SemiSerializedField>();
			
			// Search for valid fields on every component of the given GameObject:
			Component[] components = selectedGameObject.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++) {
				Component targetComponent = components[i];
		
				foundFields.AddRange(FindFields(targetComponent, fieldType));
			}
			return foundFields;
			
		}
		
		/// <summary>
		/// Uses Reflection to find fields of the given type on the given component.
		/// </summary>
		/// <returns>
		/// Fields of the given type on the given component.
		/// </returns>
		/// <param name='targetComponent'>
		/// Target component.
		/// </param>
		public static List<SemiSerializedField> FindFields(Component targetComponent, Type fieldType){
			if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldType)) throw new InvalidCastException("fieldType needs to extend UnityEngine.Object. " + fieldType);
			List<SemiSerializedField> foundFields = new List<SemiSerializedField>();
			
			bool mostDerivedType = true;
			System.Type selectedType = targetComponent.GetType();
			do{
				BindingFlags flags = allInstanceMembersFlag;
				
				if (mostDerivedType) flags = allInstanceMembersFlag;
				else flags = nonPublicInstanceMembersFlag;// For base types, don't search for public fields.
				
				FieldInfo[] fields = selectedType.GetFields(flags);
				for (int j = 0; j < fields.Length; j++) {
					FieldInfo field = fields[j];
					
					if (fieldType.IsAssignableFrom(field.FieldType)){
						
						if (mostDerivedType || field.IsPrivate){// For base types, only add private fields. The other fields were already seen in the derived types.
							SemiSerializedField validField = CreateSemiSerializedField(field, targetComponent);
							foundFields.Add(validField);
						}
					}
					
				}
				
				selectedType = selectedType.BaseType;
				mostDerivedType = false;
			}
			while (selectedType != null);
			
			return foundFields;
		}
		
		/// <summary>
		/// Tries to find the field with the given name on the given component.
		/// Search includes baseclasses.
		/// </summary>
		/// <returns>
		/// The first field with the given name found on the component
		/// </returns>
		/// <param name='fieldName'>
		/// Field name.
		/// </param>
		/// <param name='targetComponent'>
		/// Target component.
		/// </param>
		protected static FieldInfo GetFieldOnComponent(string fieldName, Type componentType){
			FieldInfo foundField;
			do {
				foundField = componentType.GetField(fieldName, allInstanceMembersFlag);
				if (foundField != null) break;
				componentType = componentType.BaseType;
			}
			while (componentType != null);
			
			return foundField;
		}
		
	}
}