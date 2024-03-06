// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	public class ComputeKernelEditor : Editor {
		
		public static void InitializeKernelButton(ComputeKernel targetComponent){
			if (GUILayout.Button("Initialize Kernel")){
				targetComponent.InitializeKernel();
			}
		}
		
		public static void KernelIdNotification(ComputeKernel targetComponent){
			if (targetComponent.kernelFound){
	//			EditorGUILayout.HelpBox("Kernel Found", MessageType.None);
			}
			else{
				EditorGUILayout.HelpBox("No Kernel Found with that name, in the selected shader.", MessageType.Warning);	
			}
		}
		
		public static void KernelNameProperty(ComputeKernel targetComponent){		
			EditorGUI.BeginChangeCheck();
			
			string label = "Kernel Name";
			string tooltip = "The name of the kernel function you want to use in the compute shader.";
			
			string updatedName = EditorGUILayout.TextField(new GUIContent(label, tooltip), targetComponent.kernelName);
			
			if (EditorGUI.EndChangeCheck()){
				Undo.RecordObject(targetComponent, "Kernel Name Change");
				targetComponent.kernelName = updatedName;
				EditorUtility.SetDirty(targetComponent);
			}
		}
		
		public static void SimulationShaderProperty(ComputeKernel targetComponent){
			EditorGUI.BeginChangeCheck();
			
			string label = "Simulation Shader";
			string tooltip = "In this shader there should be kernel with the given kernel name.";
			
			ComputeShader simShader = EditorGUILayout.ObjectField(
				new GUIContent(label, tooltip),
				targetComponent.simulationShader,
				typeof(ComputeShader),
				false
				) as ComputeShader;
			
			
			if (EditorGUI.EndChangeCheck()){
				Undo.RecordObject(targetComponent, "Compute Shader Selection");
				targetComponent.simulationShader = simShader;
				targetComponent.InitializeKernel();	
				EditorUtility.SetDirty(targetComponent);
			}
		}
		
		
		private static string warpDimensionTooltip = "You can find the required value for this setting inside the compute shader file as [numthreads(width, height, depth)]";
		
		public static void WarpWidthProperty(ComputeKernel targetComponent){		
			EditorGUI.BeginChangeCheck();
			string label = "Warp Width";
			
			int targetWidth = EditorGUILayout.IntField(
				new GUIContent(label, warpDimensionTooltip),
				targetComponent.warpWidth
				);
			if (EditorGUI.EndChangeCheck()){
				Undo.RecordObject(targetComponent, "Warp Width Change");
				targetComponent.warpWidth = targetWidth;
				EditorUtility.SetDirty(targetComponent);
			}
			
		}
		
		public static void WarpHeightProperty(ComputeKernel targetComponent){
			EditorGUI.BeginChangeCheck();
			string label = "Warp Height";
			
			int targetHeight = EditorGUILayout.IntField(
				new GUIContent(label, warpDimensionTooltip),
				targetComponent.warpHeight
				);
			if (EditorGUI.EndChangeCheck()){
				Undo.RecordObject(targetComponent, "Warp Height Change");
				targetComponent.warpHeight = targetHeight;
				EditorUtility.SetDirty(targetComponent);
			}
			
		}
	
	}
}