// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.SurfaceWaves.Support;

namespace CodeAnimo.SurfaceWaves.Setup{
	
	public class RenderingStepsBuilder : ScriptableObject {
		
		public SimulationFlow renderingSteps;
		public WaveMeshOffsetCreator meshOffsetStep;
		
		public void Build(){
			SimulationFlow renderingSteps = SurfaceWavesBuilder.CreateComponentOnNewGameObject<SimulationFlow>(
				"Rendering Steps");
			SurfaceWavesBuilder.ReplaceHelpFile<AboutRenderingSteps>(renderingSteps);
			
			WaveMeshOffsetCreator meshOffsetStep = SurfaceWavesBuilder.CreateComponentOnNewGameObject<WaveMeshOffsetCreator>(
				"Wave Mesh Offset Step");
			
			SurfaceWavesBuilder.SetupSimSteps(renderingSteps.transform, renderingSteps,
				meshOffsetStep);
			
			this.renderingSteps = renderingSteps;
			this.meshOffsetStep = meshOffsetStep;
		}
		
		public void SetupMeshOffsetStep(
			Dimensions simDimensions, 
			WaveHeightCompute heightStep, 
			WaveHeightOffsetCreator heightOffsetCreator,
			WaveMeshGroup waveSurface)
		{
			
			meshOffsetStep.simulationSize = simDimensions;
			meshOffsetStep.waveData = heightStep;
			meshOffsetStep.terrainData = heightOffsetCreator;
			meshOffsetStep.selectedWave = waveSurface;
		}
		
	}
}