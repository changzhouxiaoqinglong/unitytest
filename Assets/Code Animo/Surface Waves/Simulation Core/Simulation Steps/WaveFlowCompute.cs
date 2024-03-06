// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeAnimo.UnityExtensionMethods;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves {
	
	/// <summary>
	/// Handles the simulation Kernel that determines how much fluid should be moved to neighbouring cells (pixels)
	/// </summary>
	[AddComponentMenu("Surface Waves/Simulation Steps/Flow Compute")]
	public class WaveFlowCompute : SimulationOutput {
		
		public SimulationOutput heightOffsetData;
		public SimulationOutput waveHeightData;
		
		[Range(0.0f,1.0f)]
		public float flowDamping = 0.9999f;
		[Range(0.001f, 0.45f)]
		public float deltaTime = 0.08f;
		
		protected override void AddMissingComponents(){
			base.AddMissingComponents();
			
			AddComponentIfMissingAndSetup<ComputeKernel2D>();
			AddComponentIfMissingAndSetup<SM3Kernel>();
			AddComponentIfMissingAndSetup<StepStateManager>();
		}
		
		
		public override void LoadData (){
			FindKernel();
			LoadState();
		}

		public override void RunStep () {
			RenderTexture oldFlow = this.outputData;// Warning: if another component works with the same texture factory, this texture might have already been destroyed.	
			RenderTexture flowOut = this.simTextureManager.CreateOutputTexture("FlowMap");// Output texture.
			RenderTexture waveHeight = this.waveHeightData.outputData;
			RenderTexture waveHeightOffset = this.heightOffsetData.outputData;
			
			if (flowOut == null) throw new System.NullReferenceException("Flow Output Texture not successfully created");
			if (oldFlow == null) throw new System.NullReferenceException("Old Flow texture missing");
			if (waveHeight == null) throw new System.NullReferenceException("Wave Height texture missing");
			if (waveHeightOffset == null) throw new System.NullReferenceException("Wave Height Offset texture missing");
			
			
			RenderTexture flowMap = ComputeFlow(flowOut, oldFlow, waveHeight, waveHeightOffset);
			UpdateOutput(flowMap);
		}

		/// <summary>
		/// Compute pressures between cells (pixels)
		/// </summary>
		/// <returns>
		/// The updated FlowData
		/// </returns>
		/// <param name='flowOut'>
		/// Output Data
		/// </param>
		/// <param name='oldFlow'>
		/// Previous Flow Data
		/// </param>
		/// <param name='waveHeight'>
		/// Wave Height Data
		/// </param>
		/// <param name='waveOffset'>
		/// Wave Height Offset Data
		/// </param>
		private RenderTexture ComputeFlow(RenderTexture flowOut, RenderTexture oldFlow, RenderTexture waveHeight, RenderTexture waveOffset){
			// Setup:
			this.simKernel.SetFloat("TimeStep", this.deltaTime);
			this.simKernel.SetFloat("FlowDamping", this.flowDamping);
			
			this.simKernel.SetTexture("FlowMapIn", oldFlow);
			this.simKernel.SetTexture("WaveMapIn", waveHeight);
			this.simKernel.SetTexture("HeightOffsetIn", waveOffset);
			this.simKernel.SetTexture("FlowMapOut", flowOut);
			
			this.simKernel.Dispatch();
			
			return flowOut;
		}
	}
}