// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeAnimo.UnityExtensionMethods;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves {
	
	/// <summary>
	/// Creates a texture to influence the height of waves,
	/// based on things like terrain.
	/// </summary>
	[AddComponentMenu("Surface Waves/Simulation Steps/Wave Height Offset Creator")]
	public class WaveHeightOffsetCreator : SimulationOutput {	
		public TerrainHeightRenderer groundDepthData;
		public WaveDisplaceRenderer displaceDepthData;
		public WaveHeightCompute waveMapSource;
		public float WaveHeightScale = 10f;
		
		protected override void AddMissingComponents () {
			base.AddMissingComponents();
			AddComponentIfMissingAndSetup<ComputeKernel2D>();
			AddComponentIfMissingAndSetup<SM3Kernel>();
			AddComponentIfMissingAndSetup<StepStateManager>();
		}
		
		public override void LoadData (){
			FindTextureManager();
			FindKernel();
		}

		public override void RunStep () {
			RenderTexture waveHeightOffset = this.simTextureManager.CreateOutputTexture("waveHeightOffset");
			RenderTexture groundDepth = groundDepthData.outputData;
			RenderTexture displaceDepth = displaceDepthData ? displaceDepthData.outputData : null;
			RenderTexture waveMap = waveMapSource ? waveMapSource.outputData : null;
			
			if (waveHeightOffset == null) throw new System.NullReferenceException();
			if (groundDepth == null) throw new System.NullReferenceException();
			
			CalculateWaveHeightOffset(waveHeightOffset, groundDepth, displaceDepth, waveMap, this.WaveHeightScale);
			
			UpdateOutput(waveHeightOffset);
		}

		private RenderTexture CalculateWaveHeightOffset(RenderTexture waveHeightOffset, RenderTexture groundDepth, RenderTexture displaceDepth, RenderTexture waveMap, float waveHeightScale) {
			this.simKernel.SetTexture("GroundDepth", groundDepth);

			if (displaceDepth != null) { this.simKernel.SetTexture("DisplaceDepth", displaceDepth); }
			else { this.simKernel.SetTexture("DisplaceDepth", this.simTextureManager.GetClearTexture()); }

			if (waveMap != null) { this.simKernel.SetTexture("WaveMapIn", waveMap); }
			else { this.simKernel.SetTexture("WaveMapIn", this.simTextureManager.GetClearTexture()); }

			this.simKernel.SetFloat("customFarClip", this.groundDepthData.FarClipPlane);
			this.simKernel.SetFloat("customNearClip", this.groundDepthData.NearClipPlane);
			this.simKernel.SetFloat("groundDepthOffset", this.groundDepthData.CameraHeightOffset);
			this.simKernel.SetFloat("groundDepthScale", waveHeightScale);
			
			this.simKernel.SetTexture("DisplacementOut", waveHeightOffset);
			
			this.simKernel.Dispatch();
			
			return waveHeightOffset;
		}
		
	}
}