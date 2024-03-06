// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeAnimo.UnityExtensionMethods;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves {
	
	public abstract class SimulationOutput : SimulationStep {	
		
		/// <summary>
		/// A prefab that contains all components normally associated with this step, setup with the settings for this context.
		/// This reference should be set from the defaultReferences of the script,
		/// and is stored in MetaData.
		/// </summary>
		[SerializeField][HideInInspector] protected GameObject standardSettingsPrefab;// Not hidden from defaultReferences GUI in editor.
		
		public RenderTexture outputData{
			get { return m_outputData; }
		}
		public bool isDataAvailable{
			get { return m_outputData != null; }	
		}
		
		public Dimensions simulationSize;
		
		/// <summary>
		/// The simulation data. Should be marked as private.
		/// </summary>/
		[SerializeField][TextureDebug(inputBox=false)]
		protected RenderTexture m_outputData;// Only protected to make it show up in the inspector. Should be private.
		
		protected StepStateManager stateManager;
		protected Kernel simKernel;
		protected TextureFactory simTextureManager;

		protected virtual void Reset(){
			if (standardSettingsPrefab != null) this.ApplyPrefabSettings(standardSettingsPrefab);
			else throw new MissingReferenceException("Somehow, the standard Settings Prefab isn't available, make sure it's set on the script's default references");
			
			AddMissingComponents();
		}
		protected virtual void Awake(){
			#if UNITY_EDITOR
				if (Application.isPlaying) AddMissingComponents();
			#endif
		}
		
		/// <summary>
		/// Override this method to add components that might be required.
		/// This method is called by Reset, and Awake, during playmode, in case objects are added in playmode.
		/// Reset and Awake should only call this method in the editor.
		/// 
		/// This baseClass adds and sets up a TextureFactory component.
		/// </summary>
		protected virtual void AddMissingComponents(){
			if (Application.isPlaying) gameObject.AddComponentIfMissing<TextureFactory>();// Default References are not available in play mode.
			else AddComponentIfMissingAndSetup<TextureFactory>();
		}
		
		/// <summary>
		/// Adds a component of the given type if it's not yet on the GameObject.
		/// If it is added, standard settings will be applied, based on the standardSettingsPrefab.
		/// </summary>
		/// <returns>
		/// The created component, null if one of the correct type already existed.
		/// </returns>
		/// <typeparam name='T'>
		/// The type of the component that should be created.
		/// </typeparam>
		protected T AddComponentIfMissingAndSetup<T>() where T : Component{
			return gameObject.AddComponentIfMissingAndCopySettings<T>(this.standardSettingsPrefab);
		}
		
		protected void FindKernel(){
			if (this.simKernel == null){
				try{
					this.simKernel = Kernel.FindCompatibleKernelOnGameObject(this.gameObject);
				}
				catch(MissingComponentException e){
					Debug.LogException(e,this);
				}
			}	
		}
		
		protected void FindTextureManager(){
			if (this.simTextureManager == null) this.simTextureManager = GetComponent<TextureFactory>();
			if (this.simTextureManager == null) throw new MissingComponentException("Missing Texture Factory");
			
			if (this.simulationSize == null) throw new MissingReferenceException("Missing Dimensions");
			this.simTextureManager.resolutionU = this.simulationSize.resolutionX;
			this.simTextureManager.resolutionV = this.simulationSize.resolutionZ;
		}
		
		protected void LoadState(){
			if (this.simTextureManager == null) FindTextureManager();
			try{
				this.stateManager = GetComponent<StepStateManager>();
				if (this.stateManager == null) throw new MissingComponentException("StepStateManager Component is missing");
				
				m_outputData = this.stateManager.LoadState(this.simTextureManager);
			}catch(MissingComponentException e){
				Debug.LogException(e, this);
			}
		}
		
		/// <summary>
		/// Sets the new data as latest Simulation data map
		/// and triggers the DataUpdated event.
		/// </summary>
		/// <param name='newData'>
		/// Latest DataMap
		/// </param>
		protected void UpdateOutput(RenderTexture newData){
			m_outputData = newData;
		}
		
	}
}