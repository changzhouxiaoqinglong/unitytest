// Copyright © 2017 Laurens Mathot
// Code Animo™ http://codeanimo.com
/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeAnimo.UnityExtensionMethods;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves {
	
	[AddComponentMenu("Surface Waves/Simulation Steps/Height Initialize")]
	public class WaveHeightInitialize : SimulationOutput {
			
		public SimulationOutput heightData;
		
		protected override void AddMissingComponents() {
			base.AddMissingComponents();
			
			AddComponentIfMissingAndSetup<ComputeKernel2D>();
			AddComponentIfMissingAndSetup<SM3Kernel>();
		}

		public override void LoadData (){
			FindKernel();
			
		}
		
		public override void RunStep () {
			RenderTexture oldWaveHeight = this.outputData;
			oldWaveHeight.filterMode = FilterMode.Point;// Switch from display to Simulation mode.
			RenderTexture flowMap = this.flowData.outputData;
			RenderTexture inputMap = this.waveInputData.outputData;
			RenderTexture waveOut = this.simTextureManager.CreateOutputTexture("WaveMap");// Must write to different texture than used for reading.
			
			if (waveOut == null) throw new System.NullReferenceException("Wave Height Output Texture was not successfully created.");
			if (inputMap == null) throw new System.NullReferenceException("Wave Input Data missing. At least one WaveInput is required");
			if (flowMap == null) throw new System.NullReferenceException("Flow texture missing.");
			if (oldWaveHeight == null) throw new System.NullReferenceException("Previous waveHeight texture missing.");
			
			RenderTexture waveHeight = computeWaveHeight(flowMap, waveOut, oldWaveHeight, inputMap);
			this.UpdateOutput(waveHeight);
		}	
	
		private RenderTexture initializeWaveHeight(RenderTexture waveOut) {
			this.simKernel.SetTexture("WaveHeightOut", waveOut);
			this.simKernel.SetFloat("WaveHeight", 

		}


		/// <summary>
		/// Compute new wave levels for each cell
		/// </summary>
		/// <returns>
		/// The new Wave HeightMap
		/// </returns>
		/// <param name='flowMap'>
		/// Flow map.
		/// </param>
		/// <param name='waveOut'>
		/// Wave output texture
		/// </param>
		/// <param name='oldWaveHeight'>
		/// Wave Height in previous frame
		/// </param>
		/// <param name='inputMap'>
		/// Map of Wave Input and Output. Red channel is a source, green channel is a sink.
		/// </param>
		private RenderTexture computeWaveHeight(RenderTexture flowMap, RenderTexture waveOut ,RenderTexture oldWaveHeight, RenderTexture inputMap){			
			this.simKernel.SetTexture("FlowMapIn", flowMap);
			this.simKernel.SetTexture("WaveMapIn", oldWaveHeight);
			this.simKernel.SetTexture("AddedWavesMap", inputMap);
			
			this.simKernel.SetTexture("WaveHeightOut", waveOut);
			
			this.simKernel.SetFloat("FoamMultiplier", this.foamMultiplier);
			this.simKernel.SetFloat("FoamDecay", this.foamDecay);
			
			this.simKernel.Dispatch();
			
			return waveOut;
		}
	}
}*/