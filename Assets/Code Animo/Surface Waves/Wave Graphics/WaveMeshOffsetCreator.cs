// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.UnityExtensionMethods;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves {
	
	/// <summary>
	/// Creates a texture that is used to offset a wave mesh.
	/// </summary>
	[AddComponentMenu("Surface Waves/Graphics/Wave Mesh Offset Creator")]
	public class WaveMeshOffsetCreator : SimulationOutput {
		
		public SimulationOutput waveData;
		public SimulationOutput terrainData;

		public WaveMeshGroup selectedWave;
		
		protected override void AddMissingComponents () {
			base.AddMissingComponents();
			AddComponentIfMissingAndSetup<ComputeKernel2D>();
			AddComponentIfMissingAndSetup<SM3Kernel>();
		}
		
		public override void LoadData () {
			FindKernel();
			FindTextureManager();
		}
		
		public override void RunStep (){
			updateGraphics();
		}
		
		private void updateGraphics(){
			if (this.waveData == null || !waveData.isDataAvailable) return; //TODO: Disable update graphics in editor if missing stuff. Throw exception if called anyway.
			if (this.terrainData == null || ! terrainData.isDataAvailable) return;
			
			// This might be using data from last frame:
			RenderTexture waveHeightMap = waveData.outputData;

			updateHeightMap(waveHeightMap, terrainData.outputData, waveHeightMap.width, waveHeightMap.height);
			waveHeightMap.filterMode = FilterMode.Trilinear;// Softer sampling for foam.

			Material waveSurface = selectedWave.selectedMaterial;

			waveSurface.SetTexture("_HeightTex", this.outputData);
			waveSurface.SetTexture("_WaterData", waveHeightMap);
//			waveSurface.SetTexture("_DepthTex", depthTexture);
		}
		
		
		
		/// <summary>
		/// Creates a displacementmap for wave surface.
		/// </summary>
		/// <param name='waveHeight'>
		/// Wave Height Simulation Texture
		/// </param>
		/// <param name='terrainHeight'>
		/// Terrain height simulation Texture
		/// </param>
		/// <param name='textureWidth'>
		/// Width of the created displacement map.
		/// </param>
		/// <param name='textureHeight'>
		/// Height of the created displacement map.
		/// </param>
		private void updateHeightMap(RenderTexture waveHeight, RenderTexture terrainHeight, int textureWidth, int textureHeight){
			RenderTexture waveDisplacement = this.simTextureManager.CreateOutputTexture("WaveMesh Displacement", true);
			
			this.simKernel.SetTexture("DisplacementTextureOut", waveDisplacement);
			this.simKernel.SetTexture("WaveMapIn", waveHeight);
			this.simKernel.SetTexture("TerrainMapIn", terrainHeight);
			
			this.simKernel.Dispatch();
			
			UpdateOutput(waveDisplacement);
		}

		
	}
}