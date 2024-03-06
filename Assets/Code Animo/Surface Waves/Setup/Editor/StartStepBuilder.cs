// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.SurfaceWaves;
using CodeAnimo.SurfaceWaves.Support;

namespace CodeAnimo.SurfaceWaves.Setup{
	
	public class StartStepBuilder : ScriptableObject {
		
		public SimulationFlow simFlow;
		public TerrainHeightRenderer groundRenderer;
		
		public GameObject sourceListNode;
		public WaveSourceList waveIO;
		public GameObject waveSourceNode;
		public GameObject mouseInputNode;
		public WaveHeightOffsetCreator heightOffsetCreator;
		
		
		public void Build(Dimensions simSize, int waveLayer){
			SimulationFlow frameStartSteps = SurfaceWavesBuilder.CreateComponentOnNewGameObject<SimulationFlow>(
				"Frame Start Steps");
			
			SurfaceWavesBuilder.ReplaceHelpFile<AboutFrameStartSteps>(frameStartSteps);			
			
			Dimensions simDimensions = simSize;
			
			CreateGroundRendererComponent(simDimensions);
			CreateWaveHeightOffsetCreator(simDimensions, this.groundRenderer);
			CreateSourceList(simDimensions);
			
			CreateInput(simDimensions, this.sourceListNode);
			CreateMouseInput(simDimensions, waveLayer, this.sourceListNode);
			
			
			
			SurfaceWavesBuilder.SetupSimSteps(frameStartSteps.transform, frameStartSteps,
				this.waveIO,
				this.groundRenderer,
				this.heightOffsetCreator);
			
			this.simFlow = frameStartSteps;
		}
		
		protected void CreateGroundRendererComponent(Dimensions simDimensions){
			TerrainHeightRenderer groundRenderer = SurfaceWavesBuilder.CreateComponentOnNewGameObject<TerrainHeightRenderer>(
				"Terrain Height Renderer");
			groundRenderer.simulationSize = simDimensions;
			groundRenderer.renderedLayers = 1;
			
			this.groundRenderer = groundRenderer;
		}
		
		protected void CreateSourceList(Dimensions simSize){
			GameObject sourceList = new GameObject("Wave Input/Output");
			WaveSourceList ioList = sourceList.AddComponent<WaveSourceList>();
			ioList.simulationSize = simSize;
			
			this.sourceListNode = sourceList;
			this.waveIO = ioList;
		}
		
		protected void CreateInput(Dimensions simSize, GameObject sourceListNode){
			GameObject waveSourceNode = new GameObject("Wave Source");
			waveSourceNode.transform.parent = sourceListNode.transform;
			WaveSource waveSource = waveSourceNode.AddComponent<WaveSource>();
			waveSource.simulationSize = simSize;
			
			this.waveIO.AddStep(waveSource);
			
			this.waveSourceNode = waveSourceNode;
		}
		
		protected void CreateMouseInput(Dimensions simSize, int waveLayer, GameObject sourceListNode){
			GameObject mouseInputNode = new GameObject("Wave Mouse Input");
			mouseInputNode.transform.parent =  sourceListNode.transform;
			MouseSource mouseInput = mouseInputNode.AddComponent<MouseSource>();
			mouseInput.simulationSize = simSize;
			this.waveIO.AddStep(mouseInput);
			mouseInput.playerMouseTracker.activeLayers -= 1 << waveLayer;
			
			this.mouseInputNode = mouseInputNode;
		}
		
		protected void CreateWaveHeightOffsetCreator(Dimensions simDimensions, TerrainHeightRenderer groundRenderer){
			WaveHeightOffsetCreator heightOffsetCreator = SurfaceWavesBuilder.CreateComponentOnNewGameObject<WaveHeightOffsetCreator>("Wave Height Offset Step");
			
			heightOffsetCreator.simulationSize = simDimensions;
			heightOffsetCreator.groundDepthData = groundRenderer;
			
			this.heightOffsetCreator = heightOffsetCreator;
		}
		
		
		
	}
}