// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CodeAnimo.UnityExtensionMethods;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves{
	
	/// <summary>
	/// Keeps track of all buoys within bounds of a trigger attached to the same GameObject.
	/// </summary>
	[AddComponentMenu("Surface Waves/Buoyancy/Buoyancy Manager")]
	public class BuoyancyManager : MonoBehaviour {
		
		/// <summary>
		/// A prefab that contains all components normally associated with this step, setup with the settings for this context.
		/// This reference should be set from the defaultReferences of the script,
		/// and is stored in MetaData.
		/// </summary>
		[SerializeField][HideInInspector] protected GameObject standardSettingsPrefab;// Not hidden from defaultReferences GUI in editor.
		
		#if UNITY_EDITOR
		[HideInInspector] public CodeAnimo.Support.Article aboutBuoyancy;
		#endif
		
		public Dimensions simPosition;
		public SimulationOutput waveData;
		public SimulationOutput terrainData;
		
		private List<Buoy> nextFrameBuoys = new List<Buoy>();
		private ComputeKernel1D simKernel;

		public void Reset(){
			this.ApplyPrefabSettings(standardSettingsPrefab);
			AddMissingComponents();
		}
		public void Awake(){
			#if UNITY_EDITOR
				if (Application.isPlaying) AddMissingComponents();
			#endif
		}	
		
		protected void AddMissingComponents(){
			gameObject.AddComponentIfMissingAndCopySettings<ComputeKernel1D>(standardSettingsPrefab);
			BoxCollider newCollider = gameObject.AddComponentIfMissing<Collider, BoxCollider>();
			if (newCollider != null && this.standardSettingsPrefab != null){
				newCollider.ApplyPrefabSettings(this.standardSettingsPrefab);
				newCollider.isTrigger = true;// FIXME: Work-around for ApplyPrefabSettings not finding fields for BoxColliders.
			}
		}
		
		/// <summary>
		/// Buoyancy is calculated at the same intervals as physics.
		/// </summary>
		protected void FixedUpdate(){
			calculateBuoyancy();
		}
		
		protected void OnEnable(){
			findKernelReference();
			if (!enabled) return;// Might be disabled after attempting to find kernel reference.
			ReSubscribe();
		}
		
		/// <summary>
		/// Add Buoys that are in the area where the waves might be.
		/// It might be a good idea to have the buoyancy manager collide only with buoys.
		/// </summary>
		/// <param name='other'>
		/// The collider that touched this trigger.
		/// </param>
		protected void OnTriggerEnter(Collider other){
			Buoy possibleBuoy = other.GetComponent<Buoy>();
			if (possibleBuoy == null) return;
			
			nextFrameBuoys.Add(possibleBuoy);
			possibleBuoy.addWillBeDestroyedHandler(this.HandleBuoyDestruction);
		}
		
		/// <summary>
		/// Remove rigidbodies that are outside the influence of the waves.
		/// Raises the trigger exit event.
		/// </summary>
		/// <param name='other'>
		/// The collider that used to touch this trigger.
		/// </param>
		protected void OnTriggerExit(Collider other){
			Buoy possibleBuoy = other.GetComponent<Buoy>();
			if (possibleBuoy != null) nextFrameBuoys.Remove(possibleBuoy);
		}
		
		/// <summary>
		/// If code is compiled during playmode, event handlers tend to get lost.
		/// ReSubscribe to any needed events.
		/// </summary>
		private void ReSubscribe(){
			foreach (Buoy buoy in nextFrameBuoys){
				buoy.addWillBeDestroyedHandler(this.HandleBuoyDestruction);	
			}
		}
		
		/// <summary>
		/// Tries to find a reference to a kernel that can be used.
		/// Disables the component if none is found.
		/// </summary>
		private void findKernelReference(){
			try{
				this.simKernel = Kernel.FindCompatibleKernelOnGameObject(gameObject) as ComputeKernel1D;
			}
			catch (MissingComponentException){
				Debug.LogWarning("No supported kernel found, disabling buoyancy. Is DirectX 11 mode enabled? (Alternatively, disable or remove BuoyancyManager)", this);
				this.enabled = false;
			}
		}
		
		private void calculateBuoyancy(){
			if (this.nextFrameBuoys.Count > 0){
				Buoy[] buoyantObjects = this.nextFrameBuoys.ToArray();// need to guarantee that the list is ordered in the same way when we check back.
				
				Vector4[] buoyancyData = getPositionData(buoyantObjects);
				Vector4[] velocityData = getVelocityData(buoyantObjects);
				
				Vector3[] buoyForces = computeForces(buoyancyData, velocityData);
				
				applyBuoyForces(buoyantObjects, buoyForces);
			}
		}	
		
		/// <summary>
		/// Puts position and size data into a Vector4 array so it can be placed in a ComputeBuffer
		/// </summary>
		/// <returns>
		/// Position and radius data.
		/// </returns>
		/// <param name='buoys'>
		/// The buoys of which Vector4s will be created
		/// </param>
		private Vector4[] getPositionData(Buoy[] buoys){
			int numElements = buoys.Length;
			
			// Resolution might not match size:
			float positionScaleX = simPosition.resolutionX / simPosition.localSize.x;
			float positionScaleZ = simPosition.resolutionZ / simPosition.localSize.z;
			
			Vector4[] positionData = new Vector4[numElements];
			
			for (int i = 0; i < numElements; i++) {
				Buoy currentBuoy = buoys[i];
				
				Vector3 relativePosition = currentBuoy.position - simPosition.firstCorner;
				
				positionData[i] = new Vector4(relativePosition.x * positionScaleX, relativePosition.y, relativePosition.z * positionScaleZ, currentBuoy.radius * 2);
			}
			return positionData;
		}
		
		/// <summary>
		/// Places the velocity data in a Vector4 array for use in a computeBuffer.
		/// The last parameter is currently unused.
		/// </summary>
		/// <returns>
		/// The velocity data, w component is unused.
		/// </returns>
		/// <param name='buoys'>
		/// The buoys of which Vector4s will be created.
		/// </param>
		private Vector4[] getVelocityData(Buoy[] buoys){
			int numElements = buoys.Length;
			
			Vector4[] velocityArray = new Vector4[numElements];
			
			for (int i = 0; i < numElements; i++) {
				Buoy currentBuoy = buoys[i];
				
				Vector3 buoyVelocity = currentBuoy.velocityData;
				velocityArray[i] = new Vector4(buoyVelocity.x, buoyVelocity.y, buoyVelocity.z, 0);
			}
			return velocityArray;
		}
		
		private void applyBuoyForces(Buoy[] buoys, Vector3[] buoyForces){
			int numBuoys = buoys.Length;
			for (int i = 0; i < numBuoys; i++) {
				Buoy currentBuoy = buoys[i];
				currentBuoy.applyBuoyancy(buoyForces[i]);
			}
		}
		
		/// <summary>
		/// Stop processing buoys that have been destroyed
		/// </summary>
		/// <param name='victim'>
		/// The buoy that will soon be destroyed.
		/// </param>
		private void HandleBuoyDestruction(Buoy victim){
			nextFrameBuoys.Remove(victim);
		}
		
		/**
		 * Run compute shader to calculate the forces that should be applied to all buoys.
		 **/
		private Vector3[] computeForces(Vector4[] inputArray, Vector4[] velocityArray){
			int elementCount = inputArray.Length;
			this.simKernel.elementCount = elementCount;
			int warpGroupCount = this.simKernel.CalculateWarpGroupCount();
			
			ComputeBuffer positionBuffer = new ComputeBuffer(elementCount, 16);// stride of 4 floats
			ComputeBuffer velocityBuffer = new ComputeBuffer(elementCount, 16);
			ComputeBuffer forceBuffer = new ComputeBuffer(elementCount, 12);// stride of 3 floats
			
			positionBuffer.SetData(inputArray);
			velocityBuffer.SetData(velocityArray);
			
			this.simKernel.SetBuffer("buoyPosition", positionBuffer);
			this.simKernel.SetBuffer("buoyVelocity", velocityBuffer);
			this.simKernel.SetBuffer("ForceOut", forceBuffer);
			
			this.simKernel.SetTexture("WaveHeightIn", this.waveData.outputData);
			this.simKernel.SetTexture("TerrainHeightIn", this.terrainData.outputData);
			
			this.simKernel.SetFloat("waveHeightScale", 10);
			this.simKernel.SetInt("groupCountX", warpGroupCount);
			
			
			this.simKernel.Dispatch();
			
			
			Vector3[] forceData = new Vector3[elementCount];
			forceBuffer.GetData(forceData);// Remember, this might be 'really slow'
			
//			Debug.Log(forceData[0]);
			
			forceBuffer.Release();
			velocityBuffer.Release();
			positionBuffer.Release();
			
			
			// I think Dispose is deprecated, Release() seems to work too.
	//		forceBuffer.Dispose();
	//		velocityBuffer.Dispose();
	//		positionBuffer.Dispose();
			
			return forceData;
		}
	}
}