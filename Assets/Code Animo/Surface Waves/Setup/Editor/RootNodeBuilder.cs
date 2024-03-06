// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using CodeAnimo.Support;
using CodeAnimo.SurfaceWaves.Support;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo.SurfaceWaves.Setup{
	
	public class RootNodeBuilder : ScriptableObject {
		
		public GameObject surfaceWavesRootNode;
		public Transform rootTransform;
		
		public SimulationFlow simFlow;
		public Dimensions simDimensions;
		public BuoyancyManager buoyancy;
		
		public SurfaceWavesFAQ faqComponent;
		
		public void Build(int waveLayer){
			CreateRootNode(waveLayer);
			
			// FIXME (Unity): Temporarily disable undoCrash workaround for componentAddition, further component additions should simply use the object creation undo step:
			UnityExtensionMethods.GameObjectExtensions.Unity4_3_4UndoCrashWorkaroundEnabled = false;
			
			// FIXME (Unity): Need to call regular AddComponent because of crash related to calling any kind of AddComponent from a reset method.
			
			CreateGettingStartedComponent();
			CreateMainSimFlowComponent();
			CreateDimensionsComponent();
			CreateBuoyancyComponent();
			CreateFAQComponent();
		}
		
		public void CreateRootNode(int waveLayer){
			GameObject surfaceWavesRootNode = new GameObject("Surface Waves");
			Undo.RegisterCreatedObjectUndo(surfaceWavesRootNode, "Surface Waves Object Creation");// In Unity 4.3.4, registering this combined with calling Undo.AddComponent from regular code crashes the editor when you perform an undo.
			surfaceWavesRootNode.layer = waveLayer;
			 
			Transform rootTransform = surfaceWavesRootNode.transform;
			if (Selection.activeTransform != null){
				SurfaceWavesBuilder.SetChildAtRoot(rootTransform, Selection.activeTransform);// Place it exactly over the center of the selected Game Object.
			}
			else rootTransform.localPosition = new Vector3(256,0,256);// Place corner at the 0,0, point, assuming default size of 512 by 512, to place it in the corner of a Unity Terrain.
			
			this.surfaceWavesRootNode = surfaceWavesRootNode;
			this.rootTransform = rootTransform;
		}
		
		protected void CreateGettingStartedComponent(){
			this.surfaceWavesRootNode.AddComponent<GettingStartedWithSurfaceWaves>();
		}
		
		protected void CreateMainSimFlowComponent(){
			var simFlow = surfaceWavesRootNode.AddComponent<SimulationFlow>();// Main Simulation Loop
			SurfaceWavesBuilder.ReplaceHelpFile<AboutDefaultMainFlow>(simFlow);// Retrieve default help file asset reference from AboutDefaultMainFlow instance.
			simFlow.loadStepsOnStart = true;
			simFlow.runStepsOnUpdate = true;
			
			this.simFlow = simFlow;
		}
		
		protected void CreateDimensionsComponent(){
			this.simDimensions = surfaceWavesRootNode.AddComponent<Dimensions>();// Simulation Dimensions
		}
		
		protected void CreateBuoyancyComponent(){
			this.buoyancy = surfaceWavesRootNode.AddComponent<BuoyancyManager>();// Buoyancy
			
			// Buoyancy component creates a collider, which is needed for simDimensions.
			this.simDimensions.TriggerValidation();
		}
		
		protected void CreateFAQComponent( ){
			SurfaceWavesFAQ faqComponent = this.surfaceWavesRootNode.AddComponent<SurfaceWavesFAQ>();
			this.faqComponent = faqComponent;
		}
		
		public void SetupBuoyancy(Dimensions simDimensions, WaveHeightCompute heightStep, WaveHeightOffsetCreator heightOffsetCreator){
			buoyancy.simPosition = simDimensions;
			buoyancy.waveData = heightStep;
			buoyancy.terrainData = heightOffsetCreator;
		}
		
		
	}
}