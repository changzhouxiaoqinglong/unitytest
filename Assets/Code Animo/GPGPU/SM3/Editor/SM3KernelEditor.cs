// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	[CustomEditor(typeof(SM3Kernel))]
	public class SM3KernelEditor : Editor {
	
		public override void OnInspectorGUI ()
		{
			SM3Kernel targetComponent = target as SM3Kernel;
			
			KernelInspector.KernelSupportedProperty(targetComponent);
			
			DrawDefaultInspector();
		}
		
	}
}