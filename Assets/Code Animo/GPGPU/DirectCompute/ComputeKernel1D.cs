// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	[AddComponentMenu("GPGPU/Compute Kernel 1D")]
	public class ComputeKernel1D : ComputeKernel {
		
		[HideInInspector] public int elementCount = 1;// Only set through script.
		
		#if UNITY_EDITOR
		[HideInInspector] public Support.Article componentHelp;// Only set through default references.
		#endif
		
		/// <summary>
		/// Calculates the number of required threadGroups required and dispatches the kernel.
		/// </summary>
		public override void Dispatch () {
			if (!kernelFound) LogKernelNotFoundWarning();
			else{
				int warpGroups = CalculateWarpGroupCount();
				
				simulationShader.Dispatch(kernelIndex, warpGroups, 1, 1);
			}
		}
		
		/// <summary>
		/// Calculates the required number of warp groups for the set amount of elements, warpWidth, warpHeight and warpDepth.
		/// </summary>
		/// <returns>
		/// The required number of warpGroups
		/// </returns>
		public int CalculateWarpGroupCount(){
			int warpCount = this.warpWidth * this.warpHeight * this.warpDepth;
			
			// Calculate the required number of warpGroups:
			// Don't test if warpCount is higher than zero.
			// The divisionByZero possible exception already gives the useful stacktrace.
			return Mathf.CeilToInt((float)this.elementCount / (float)warpCount);
		}
				
	}
}