// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	[CustomEditor(typeof(ComputeKernel2D))]
	public class ComputeKernel2DEditor : Editor {
		
		private ComputeKernel2D targetComponent;
		
		
		public override void OnInspectorGUI () {
			targetComponent = target as ComputeKernel2D;
			
			bool kernelSupported = KernelInspector.KernelSupportedProperty(targetComponent);
			
	//		targetComponent.initializeKernel();//Uncomment this if you want to constantly check if the kernel is available. (For example, a kernel might go missing after recompiling the shader, if the computeKernel2D isn't reinitialized)
			
			EditorGUI.BeginDisabledGroup(!kernelSupported);
			
			ComputeKernelEditor.KernelNameProperty(this.targetComponent);
			ComputeKernelEditor.SimulationShaderProperty(this.targetComponent);
			
			ComputeKernelEditor.KernelIdNotification(this.targetComponent);
			
			
			EditorGUILayout.Space();
			
			ComputeKernelEditor.WarpWidthProperty(this.targetComponent);
			ComputeKernelEditor.WarpHeightProperty(this.targetComponent);
			
			EditorGUILayout.Space();
			
			targetComponent.forceCustomResolution = EditorGUILayout.Toggle("Force Custom Resolution", targetComponent.forceCustomResolution);
			if (targetComponent.willUseCustomResolution){
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Resolution");
				EditorGUIUtility.labelWidth = 16;
				targetComponent.resolutionU = EditorGUILayout.IntField("U", targetComponent.resolutionU);
				targetComponent.resolutionV = EditorGUILayout.IntField("V", targetComponent.resolutionV);
				EditorGUIUtility.labelWidth = 0;
					
				EditorGUILayout.EndHorizontal();
			}
			
			EditorGUI.EndDisabledGroup();
			
	//		ComputeKernelEditor.InitializeKernelButton(this.targetComponent);
		}
	
	}
}