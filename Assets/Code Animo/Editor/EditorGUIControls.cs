// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace CodeAnimo {

	public class EditorGUIControls {
		
		public static void OrderedList<T>(List<T> orderedList, float elementControlButtonWidth) where T : Object {
			int stepCount = orderedList.Count;
			for (int i = 0; i < stepCount; i++) {
				EditorGUILayout.BeginHorizontal();
				{
					T currentStep = orderedList[i];
					
					GUIContent removalButtonLabel = new GUIContent("X","Removes this item from the list");
					if(GUILayout.Button(removalButtonLabel, GUILayout.Width(elementControlButtonWidth))){
						orderedList.RemoveAt(i);
						break;
					}
					
					orderedList[i] = EditorGUILayout.ObjectField(currentStep, typeof(T), true) as T;
					
					
					
					GUIContent downwardSwapButton = new GUIContent("⋁", "Swaps this step with the one below it");
					if (stepCount - 1 > i){// for all except last element
						if(GUILayout.Button(downwardSwapButton, GUILayout.Width(elementControlButtonWidth))){
							orderedList[i] = orderedList[i + 1];
							orderedList[i + 1] = currentStep;
							break;
						}
					}
					else { GUILayout.Space(elementControlButtonWidth + 4); }
					
					GUIContent upwardSwapButton = new GUIContent("⋀", "Swaps this step with the one above it");
					if (i > 0){// for all except first element
						if (GUILayout.Button(upwardSwapButton, GUILayout.Width(elementControlButtonWidth))){
							orderedList[i] = orderedList[i - 1];
							orderedList[i - 1] = currentStep;
							break;
						}
					}
					else { GUILayout.Space(elementControlButtonWidth + 4); }
					
				}
				EditorGUILayout.EndHorizontal();
				
			}

			DisplayNewOrderedListElement<T>(orderedList);
		}

		public static void OrderedList<T>(List<T> orderedList) where T : Object{
			OrderedList<T>(orderedList, 20f);
		}

		private static void DisplayNewOrderedListElement<T>(List<T> orderedList) where T : Object{
			T selectedStep = EditorGUILayout.ObjectField("Add new Step", null, typeof(T), true) as T;
			if (selectedStep != null) orderedList.Add(selectedStep);
		}
		
	}
}