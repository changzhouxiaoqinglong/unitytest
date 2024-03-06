// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace CodeAnimo.UnityExtensionMethods {

	public static class GameObjectExtensions {
		
		public static bool Unity4_3_4UndoCrashWorkaroundEnabled = true;// Global state isn't ideal, but unfortunately crashes can occur through use of workaround in objects created after use of regular Undo.AddComponent, which is a global state issue.

		/// <summary>
		/// Tries to find a component of the given type, and adds it if it can't.
		/// </summary>
		/// <param name='gameObject'>
		/// Extension method Instance reference.
		/// </param>
		/// <typeparam name='T'>
		/// The type of component that should be found. Must be a Component subclass.
		/// </typeparam>
		/// <returns>
		/// The newly created component, or null if a component of the correct type already existed.
		/// </returns>
		public static T AddComponentIfMissing<T>(this GameObject gameObject) where T : Component{
			T existingComponent = gameObject.GetComponent<T>();
			if (existingComponent == null) return AddComponent<T>(gameObject);
			else return null;
		}
		/// <summary>
		/// Tries to find a component of type T. If it can't find that component, it will add a component of type U.
		/// This can be useful when a Component can handle multiple subclasses of a type that can't itself be added as a component.
		/// For Example, you might check for any collider, and add a SphereCollider if none was found.
		/// </summary>
		/// <param name='gameObject'>
		/// Extension method Instance reference.
		/// </param>
		/// <typeparam name='T'>
		/// The base type of Component that should be found. Must be a Component subclass.
		/// </typeparam>
		/// <typeparam name='U'>
		/// The type of Component that should be added if none of type T was found. Must be a Component subclass.
		/// </typeparam>
		/// <returns>
		/// The newly created component, or null if a component of the correct type already existed.
		/// </returns>
		public static U AddComponentIfMissing<T, U>(this GameObject gameObject) where T: Component where U : T{
			T existingComponent = gameObject.GetComponent<T>();
			if (existingComponent == null) return AddComponent<U>(gameObject);
			else return null;
		}
		
		#if UNITY_EDITOR
		private static string UndoClassName = "UnityEditor.Undo, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";// assembly-qualified name
		private static string AddComponentMethodName = "AddComponent";
		private static string GetCurrentGroupMethodName = "GetCurrentGroup";
		#endif		
		
		/// <summary>
		/// Adds the component to the given gameObject.
		/// This method is created to simplify a workaround
		/// for a crashed caused by performing undo/redo after AddComponent in the editor
		/// </summary>
		/// <returns>
		/// The created component
		/// </returns>
		/// <param name='targetObject'>
		/// The Gameobject to which the component should be added.
		/// </param>
		/// <typeparam name='T'>
		/// The Type of component that should be created
		/// </typeparam>
		public static T AddComponent<T>(this GameObject targetObject) where T : Component{
			// Workaround should only be applied in Editor mode:
			#if (UNITY_4_3 && UNITY_EDITOR)
			if (Unity4_3_4UndoCrashWorkaroundEnabled){
				Debug.Log("Applying AddComponent Undo-Crash workaround for Unity 4.3.4", targetObject);
				Type creationType = typeof(T);
				
				// Using reflection to get access to Undo.AddComponent:
				Type UndoType = GetUndoType();
				MethodInfo AddComponentMethod = UndoType.GetMethod(AddComponentMethodName, new Type[]{typeof(GameObject), typeof(Type)});
				if (AddComponentMethod == null) throw new MissingMethodException("Can't access method with the name " + AddComponentMethodName + " on class with the name " + UndoClassName + ". Perhaps it has been renamed or the parameters have changed");
				
				// call to static method Undo.AddComponent(GameObject, Type):
				return (T) AddComponentMethod.Invoke(null, new object[]{targetObject, creationType});
			}
			return targetObject.AddComponent<T>();
			#else
			// Regular version:
			return targetObject.AddComponent<T>();			
			#endif
		}
		
		private static int GetCurrentUndoGroup(){
			#if UNITY_EDITOR
				Type UndoType = GetUndoType();
				MethodInfo GetCurrentGroupMethod = UndoType.GetMethod(GetCurrentGroupMethodName);
				if (GetCurrentGroupMethod == null) throw new MissingMethodException("Can't access method with the name " + AddComponentMethodName + " on class with the name " + UndoClassName + ". Perhaps it has been renamed or the parameters have changed");
				return (int) GetCurrentGroupMethod.Invoke(null, null);
			#else
				throw new NotImplementedException("Must be called from Unity Editor");	
			#endif
		}
		
		
		private static Type GetUndoType(){
			#if UNITY_EDITOR
			// Using reflection to get access to Undo.AddComponent:
			Type UndoType = Type.GetType(UndoClassName);
			if (UndoType == null) throw new TypeLoadException("Can't access class with the name "+ UndoClassName + ". Perhaps this is not running in Unity3D's Editor Mode, or the class has been removed.");
			else return UndoType;
			#else
			throw new NotImplementedException("Must be called from Unity Editor");		
			#endif
		}
		
		/// <summary>
		/// Adds a component of the given type if it's not yet on the GameObject.
		/// If it is added, standard settings will be applied, based on the standardSettingsPrefab.
		/// </summary>
		/// <returns>
		/// The created component, null if one of the correct type already existed.
		/// </returns>
		/// <param name='gameObject'>
		/// Game object.
		/// </param>
		/// <param name='standardSettingsPrefab'>
		/// The fields of the appropriate component of this GameObject will be applied to component if created.
		/// </param>
		/// <typeparam name='T'>
		/// The type of the component that should be created.
		/// </typeparam>
		public static T AddComponentIfMissingAndCopySettings<T>(this GameObject gameObject, GameObject standardSettingsPrefab) where T : Component{
			T addedComponent = gameObject.AddComponentIfMissing<T>();
			if (addedComponent != null) addedComponent.ApplyPrefabSettings(standardSettingsPrefab);
			
			return addedComponent;
		}
		
	}
}