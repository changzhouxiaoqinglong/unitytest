// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using CodeAnimo.Support;
using CodeAnimo.SurfaceWaves.Support;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo.SurfaceWaves.Setup{
	[ProductInfo("Surface Waves", 
		"From the GameObject creation menu: \n3D Object > Wave Simulation, and read the 'Getting Started' component.",
		productVersion="v_15")]
	public class SurfaceWavesBuilder : ScriptableObject {
		
		public int defaultWaveLayer = 4;
		
		public RootNodeBuilder rootBuilder;
		public StartStepBuilder startStep;
		public RepeatableStepsBuilder repeatableSteps;
		public RenderingStepsBuilder renderingSteps;
		public WaveSurfaceBuilder waveSurface;
		
		[MenuItem("GameObject/3D Object/Wave Simulation")]
		public static void CreateWaveSimulationObject(){						
			SurfaceWavesBuilder builder = CreateInstance<SurfaceWavesBuilder>();
			
			builder.Build();		
			
			// Upon completion:
			GameObject rootNode = builder.rootBuilder.surfaceWavesRootNode;
			
			EditorUtility.FocusProjectWindow();
			EditorGUIUtility.PingObject(rootNode);
			Selection.activeObject = rootNode;
			
			DestroyImmediate(builder);
		}
		
		protected void OnDestroy(){
			DestroyImmediate(this.rootBuilder);
			DestroyImmediate(this.startStep);
			DestroyImmediate(this.repeatableSteps);
			DestroyImmediate(this.renderingSteps);
			DestroyImmediate(this.waveSurface);
		}
		
		public void Build(){
			Undo.IncrementCurrentGroup();
			int undoGroup = Undo.GetCurrentGroup();
			
			CreateBuilders();
			
			// Root:
			this.rootBuilder.Build(defaultWaveLayer);
			
			Dimensions simDimensions = rootBuilder.simDimensions;
			
			// Start Frame Steps:
			this.startStep.Build(
				simDimensions, 
				defaultWaveLayer);
			
			// Repeatable Steps:
			this.repeatableSteps.Build(
				simDimensions, 
				startStep.heightOffsetCreator, 
				startStep.waveIO);
			
			// Rendering Steps:
			this.renderingSteps.Build();
			
			// Wave Surface:
			this.waveSurface.Build(simDimensions);
			
			// Buoyancy
			this.rootBuilder.SetupBuoyancy(
				simDimensions, 
				this.repeatableSteps.heightStep, 
				this.startStep.heightOffsetCreator);
			
			this.renderingSteps.SetupMeshOffsetStep(
				simDimensions,
				this.repeatableSteps.heightStep,
				this.startStep.heightOffsetCreator,
				this.waveSurface.waveSurface);			
			
			
			// Parenting:
			SurfaceWavesBuilder.SetChildAtRoot(waveSurface.rootTransform, rootBuilder.rootTransform);
			
			SetupSimSteps(rootBuilder.rootTransform, rootBuilder.simFlow,
				startStep.simFlow,
				repeatableSteps.repeatableSteps,
				renderingSteps.renderingSteps);
			
			UnityExtensionMethods.GameObjectExtensions.Unity4_3_4UndoCrashWorkaroundEnabled = true;// Reset workaround.
			
			Undo.CollapseUndoOperations(undoGroup);
			
			waveSurface.waveSurface.StartCreatingGroup();
		}
		
		protected void CreateBuilders(){
			this.rootBuilder		= CreateInstance<RootNodeBuilder>();
			this.startStep			= CreateInstance<StartStepBuilder>();
			this.repeatableSteps	= CreateInstance<RepeatableStepsBuilder>();
			this.renderingSteps		= CreateInstance<RenderingStepsBuilder>();
			this.waveSurface		= CreateInstance<WaveSurfaceBuilder>();
			
			this.rootBuilder.hideFlags = HideFlags.HideAndDontSave;
			this.startStep.hideFlags = HideFlags.HideAndDontSave;
			this.repeatableSteps.hideFlags = HideFlags.HideAndDontSave;
			this.renderingSteps.hideFlags = HideFlags.HideAndDontSave;
			this.waveSurface.hideFlags = HideFlags.HideAndDontSave;
		}
		
		/// <summary>
		/// Replaces the help file with one from an AboutComponent.
		/// It instantiates the component to get access to its default references.
		/// When it has the information it needs, the created instance is destroyed.
		/// </summary>
		/// <param name='step'>
		/// Simulation Step.
		/// </param>
		/// <typeparam name='T'>
		/// An AboutComponent, which has a default reference set for 'helpFile'
		/// </typeparam>
		public static void ReplaceHelpFile<T>(SimulationStep step) where T : AboutComponent{
			GameObject dummyObject = new GameObject("Dummy Object", typeof(T));
			AboutComponent dummyComponent = dummyObject.GetComponent<T>();
			step.componentHelp = dummyComponent.helpFile;
			DestroyImmediate(dummyObject);
		}
		
		public static void SetChildAtRoot(Transform child, Transform parent){
			child.parent = parent;
			child.localPosition = new Vector3(0,0,0);
		}
		
		public static T CreateComponentOnNewGameObject<T>(string name) where T : Component{
			GameObject stepNode = new GameObject(name);
			T step = stepNode.AddComponent<T>();
			return step;
		}
		
		public static void SetupSimSteps(Transform parent, SimulationFlow targetLoop, params SimulationStep[] steps){
			for (int i = 0; i < steps.Length; i++) {
				SimulationStep step = steps[i];
				
				SetChildAtRoot(step.transform, parent);
				targetLoop.AddStep(step);
			}
		}
		
		
		
	}
}