// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo{
	
	/// <summary>
	/// This class is designed to maintain Editor Window interactivity after an exception occurs.
	/// Normally when repeating code throws an exception, you lose control until the cause of the exception is fixed.
	/// This can make it difficult to view the console, and makes it impossible to close the window.
	/// </summary>
	public class ExceptionPausedDrawer : ScriptableObject {
		public delegate void DrawerMethod();
		private bool m_pausedForException = false;
		
		/// <summary>
		/// Calls the method that potentially throws an exception,
		/// unless this drawers is already paused by an exception.
		/// If an exception occurs in the given code, 
		/// execution is paused and exception rethrown to preserve stack.
		/// Custom notification GUI is drawn instead.
		/// Includes a button for re-enabling drawing,
		/// for if the user expects the cause of the exception to be removed.
		/// </summary>
		/// <returns>
		/// Wheter the 
		/// </returns>
		/// <param name='potentialExceptionCode'>
		/// If set to <c>true</c> potential exception code.
		/// </param>
		public bool AttemptDrawing(DrawerMethod potentialExceptionCode){		
			if (m_pausedForException){
				EditorGUILayout.HelpBox("An exception has occurred in the window drawing code.", MessageType.Warning);
				EditorGUILayout.HelpBox("Window drawing paused, to maintain interactivity. Check the console for exception messages. Try re-enabling drawing, or closing this window.",MessageType.Info);
				if (GUILayout.Button("Re-enable Drawing")) m_pausedForException = false;
				return false;
			}
			else{
				//TODO: only pause when unhandled exception occurs.
				try{
					potentialExceptionCode();
				}
				catch(System.Exception e){
					if (!(e is UnityEngine.ExitGUIException)){
						m_pausedForException = true;
//						Debug.LogException(e);
					}
					throw;
				}
				return true;
			}
		}
		
		public void UnPause(){
			m_pausedForException = false;
		}
		
	}
}