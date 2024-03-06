// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using CodeAnimo.SurfaceWaves.Support;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo.SurfaceWaves.Setup{
	
	public class RepeatableStepsBuilder : ScriptableObject {
		
		public SimulationFlow repeatableSteps;
		public WaveFlowCompute flowStep;
		public WaveHeightCompute heightStep;
		
		public void Build(Dimensions simDimensions, WaveHeightOffsetCreator heightOffsetCreator, WaveSourceList waveIO ){
			SimulationFlow repeatableSteps = 
				SurfaceWavesBuilder.CreateComponentOnNewGameObject<RepeatingSimulationFlow>("Repeatable Steps");
			
			SurfaceWavesBuilder.ReplaceHelpFile<AboutRepeatableSteps>(repeatableSteps);
			
			
			WaveFlowCompute flowStep = 
				SurfaceWavesBuilder.CreateComponentOnNewGameObject<WaveFlowCompute>("Wave Flow Step");
			WaveHeightCompute heightStep = 
				SurfaceWavesBuilder.CreateComponentOnNewGameObject<WaveHeightCompute>("Wave Height Step");
			
			flowStep.simulationSize = simDimensions;
			flowStep.heightOffsetData = heightOffsetCreator;
			flowStep.waveHeightData = heightStep;
			
			heightStep.simulationSize = simDimensions;
			heightStep.flowData = flowStep;
			heightStep.waveInputData = waveIO;
			
			SurfaceWavesBuilder.SetupSimSteps(repeatableSteps.transform, repeatableSteps,  
				flowStep, 
				heightStep);
			
			
			this.repeatableSteps = repeatableSteps;
			this.flowStep = flowStep;
			this.heightStep = heightStep;
		}
		
	}
}