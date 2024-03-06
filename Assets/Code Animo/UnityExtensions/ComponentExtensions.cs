// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace CodeAnimo.UnityExtensionMethods{
	
	public static class ComponentExtensions {
		
		private static string OnValidateMethodName = "OnValidate";
		
		/// <summary>
		/// Searches for component of the same type on the given prefab (GameObject), and copies over the public field values.
		/// </summary>
		/// <param name='targetComponent'>
		/// Extension method instance reference
		/// </param>
		/// <param name='prefab'>
		/// Object that contains the component with the desired values.
		/// </param>
		/// <exception cref='System.NullReferenceException'>
		/// Is thrown when no compatible component is found on the given GameObject.
		/// </exception>
		public static void ApplyPrefabSettings(this Component targetComponent, GameObject prefab){
			Type ownType = targetComponent.GetType();
			Component prefabComponent = prefab.GetComponent(ownType);
			if (prefabComponent == null) throw new NullReferenceException("Component of type " + ownType + " is not available on prefab with the name " + prefab.name);
			
			List<FieldInfo> displayedFields = targetComponent.GetDisplayedFields();
			for (int i = 0; i < displayedFields.Count; i++) {
				FieldInfo field = displayedFields[i];
				
				object retrievedValue = field.GetValue(prefabComponent);
				if (retrievedValue == null) continue;
				field.SetValue(targetComponent, retrievedValue);
			}
			
			#if UNITY_EDITOR
			targetComponent.TriggerValidation();
			#endif
		}
		
		/// <summary>
		/// Returns an array of all the fields that would be visible in the inspector
		/// 
		/// </summary>
		/// <returns>
		/// The displayed fields.
		/// </returns>
		public static List<FieldInfo> GetDisplayedFields(this Component targetComponent){
			List<FieldInfo> displayedFields = new List<FieldInfo>();
			
			Type selectedType = targetComponent.GetType();
			do{
				FieldInfo[] fields = selectedType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				
				for (int i = 0; i < fields.Length; i++) {
					FieldInfo field = fields[i];
					
					// Fixme: prevent sharing things like lists with prefab.

					
					// only add to list if field is valid:
					if (isFieldVisibleInInspector(field)){
						displayedFields.Add(field);
					}
				}
				
				selectedType = selectedType.BaseType;
			} while (selectedType != null);
			
			return displayedFields;
		}
		
		/// <summary>
		/// Looks at the field information and its attributes to estimate whether Unity would show it or not.
		/// </summary>
		/// <returns>
		/// false for static fields
		/// false for fields marked with HideInInspector
		/// true for nonPublic fields marked with SerializeField
		/// true for all other public fields
		/// </returns>
		/// <param name='field'>
		/// If set to <c>true</c> field.
		/// </param>
		private static bool isFieldVisibleInInspector(FieldInfo field){
			object[] attributes = field.GetCustomAttributes(false);
			bool fieldIsSerialized = field.IsPublic;
//			Debug.Log("field: " +  field);
			if (field.IsStatic) return false;
			
			for (int i = 0; i < attributes.Length ; i++) {
				object attribute = attributes[i];
//				Debug.Log("Attribute: " + attribute);
				if (attribute is HideInInspector) return false;
				if (!fieldIsSerialized){
					if (attribute is SerializeField) fieldIsSerialized = true;
				}
				
			}
			
			return fieldIsSerialized;
		}
		
		/// <summary>
		/// Calls OnValidate if it exists on the target Component.
		/// Editor Only.
		/// </summary>
		/// <param name='targetComponent'>
		/// Target component.
		/// </param>
		public static void TriggerValidation(this Component targetComponent){
			#if UNITY_EDITOR
			Type targetType = targetComponent.GetType();
			MethodInfo validateMethod = targetType.GetMethod(OnValidateMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (validateMethod != null) validateMethod.Invoke(targetComponent, null);
			#else
				throw new InvalidOperationException("This method (" + OnValidateMethodName + ") is only available in the Unity Editor");
			#endif
		}
		
		/// <summary>
		/// Attempts to find the component that replaces the current component after an assembly reload.
		/// When an assembly is reloaded, new instances are created with matching instance ID's.
		/// Component references aren't restored correctly, but GameObjects are.
		/// This allows the new reference to be found.
		/// </summary>
		/// <returns>
		/// The component instance that replaces the given component after an assembly reload.
		/// </returns>
		/// <param name='originalComponent'>
		/// The component for which you want to find the replacement.
		/// </param>
		/// <exception cref='MissingComponentException'>
		/// Is thrown when none of the found components use the same instanceID.
		/// </exception>
		public static Component FindPostAssemblyReloadComponent(this Component originalComponent){
			int originalID = originalComponent.GetInstanceID();
			Type componentType = originalComponent.GetType();
			
			// There might be more components of the same type:
			Component[] matchingComponents;
			try{
				matchingComponents = originalComponent.gameObject.GetComponents(componentType);
			}
			catch(MissingReferenceException e){
				throw new NullReferenceException("The component can not be reconstructed. It probably really IS null, not just Unity's kind of null. It can't access its gameObject reference.", e);
			}
			for (int i = 0; i < matchingComponents.Length; i++) {
				int foundID = matchingComponents[i].GetInstanceID();
				if (foundID == originalID){ // A regular comparison between the instances would give 'False'
					return matchingComponents[i];
				}
			}
			throw new MissingComponentException("Can't find a component of same type with matching InstanceID");
		}
		
	}
}