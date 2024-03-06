// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	public class KernelInspector : Editor {
		
		
		/// <summary>
		/// Displays whether the kernel is supported on the current system or not.
		/// </summary>
		/// <returns>
		/// Returns true when the kernel is supported on the current system.
		/// </returns>
		/// <param name='targetComponent'>
		/// If set to <c>true</c> target component.
		/// </param>
		public static bool KernelSupportedProperty(Kernel targetComponent){
			if (targetComponent.SupportedBySystem()){
				EditorGUILayout.LabelField("Supported by OS and GPU.");
				return true;
			}
			else{
				EditorGUILayout.LabelField("Not Supported by OS, GPU, or Unity Editor mode.");
				return false;
			}
		}
		
	}
}