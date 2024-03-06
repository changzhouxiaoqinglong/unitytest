// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	[CustomEditor(typeof(ComputeKernel1D))]
	public class ComputeKernel1DEditor : Editor {
		
		private ComputeKernel1D targetComponent;
		
		public override void OnInspectorGUI () {
			this.targetComponent = target as ComputeKernel1D;
			
			KernelInspector.KernelSupportedProperty(targetComponent);
			
	//		targetComponent.initializeKernel();//Uncomment this if you want to constantly check if the kernel is available. (For example, a kernel might go missing after recompiling the shader, if the computeKernel2D isn't reinitialized)
			
//			if (targetComponent.kernelFound) EditorGUIUtility.LookLikeInspector();
//			else EditorGUIUtility.LookLikeControls();
			
			// Element Count property not necessary, it should be set by the object using the kernel.
			
			ComputeKernelEditor.KernelNameProperty(this.targetComponent);
			ComputeKernelEditor.SimulationShaderProperty(this.targetComponent);
			
			ComputeKernelEditor.KernelIdNotification(this.targetComponent);
			
			ComputeKernelEditor.WarpWidthProperty(this.targetComponent);
			ComputeKernelEditor.WarpHeightProperty(this.targetComponent);
			
			
		}
	
	}
}