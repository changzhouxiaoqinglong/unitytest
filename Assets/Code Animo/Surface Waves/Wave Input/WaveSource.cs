// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CodeAnimo.UnityExtensionMethods;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves {
	
	/// <summary>
	/// Creates a texture that indicates how much fluid should be added or removed.
	/// WaveSources are meant to be chained, to support multiple inputs and outputs.
	/// </summary>
	[AddComponentMenu("Surface Waves/Wave Sources/Wave Source")]
	public class WaveSource : SimulationOutput {
		public static int defaultWaveInputLayer = 10;
		
		private float m_inputSizeRatio;
		[SerializeField] private float m_inputWidth = 20f;
		public float inputWidth{
			get { return m_inputWidth; }
			set {
				m_inputWidth = value;
				CalculateRelativeSize();
			}
		}
		
		[RangeAttribute(-1,1)]
		public float inputIntensity = 0.5f;

		public SimulationOutput previousInput;// Wave Input source before this one.
		public Texture inputShape;
			
		protected float estimatedSimWidth{
			get { 
				float simulationWidth;
				if (this.simulationSize != null) simulationWidth = this.simulationSize.localExtends.x;
				else simulationWidth = 512f;// Default value for when this source hasn't found any dimensions yet.
				
				return simulationWidth;
			}
		}
		
		/// <summary>
		/// Used to disable wave input, without disabling the whole component. Example: MouseSource.cs
		/// </summary>
		protected bool forceUnchangedOutput = false;

		private RenderTexture emptyInput;// Used when there's no other input.

		private float m_inputGizmoScale = 100f;
		
		protected override void Reset () {
			gameObject.layer = defaultWaveInputLayer;
			base.Reset ();
		}
		
		protected override void AddMissingComponents (){
			base.AddMissingComponents();
			AddComponentIfMissingAndSetup<SM3Kernel>();
			
			Collider trigger = gameObject.AddComponentIfMissing<Collider, SphereCollider>();
			if (trigger != null) trigger.isTrigger = true;
			else trigger = GetComponent<Collider>();
			
			// Rigidbody required for collision with WaveMeshGroup.
			if (trigger.attachedRigidbody == null){
				Rigidbody physicsObject = gameObject.AddComponent<Rigidbody>();
				physicsObject.isKinematic = true;
				physicsObject.useGravity = false;
			}
		}
		
		protected void OnEnable(){
			CalculateRelativeSize();
		}
		
		protected void FeedSerializedDataThroughProperties(){
			inputWidth = m_inputWidth;
		}
		
		protected void OnValidate(){
			FeedSerializedDataThroughProperties();
		}
		
		public override void LoadData () {
			FindKernel();
			FindTextureManager();
		}
		
		protected void CalculateRelativeSize(){
			m_inputSizeRatio = inputWidth / estimatedSimWidth;
		}
		
		protected void OnDrawGizmos(){
			if (!forceUnchangedOutput){
				Color originalGizmoColor = Gizmos.color;
				
				SetupGizmoColor();
				
				Vector3 inputPosition = this.transform.position;
				
				// Draw a line to the bottom of the simulation:
				if (this.simulationSize != null){
					Vector3 lineBottom = inputPosition;
					lineBottom.y = this.simulationSize.center.y - this.simulationSize.localExtends.y;
					
					Gizmos.DrawLine(inputPosition , lineBottom);
				}
				
				
				float inputAmountGizmoHeight = Mathf.Abs(inputIntensity) * m_inputGizmoScale;
				Vector3 inputAmountGizmoPosition = inputPosition + new Vector3(0, 0.5f * inputAmountGizmoHeight, 0);
				Gizmos.DrawCube(inputAmountGizmoPosition, new Vector3(5f, inputAmountGizmoHeight, 5f));
				
				
				Gizmos.color = originalGizmoColor;
			}
			
			
		}
		
		protected void OnDrawGizmosSelected(){
			if (!forceUnchangedOutput){
				Color originalGizmoColor = Gizmos.color;
				SetupGizmoColor();
				
				Vector3 inputPosition = this.transform.position;
				
				Gizmos.DrawWireSphere(inputPosition, inputWidth);
				
				Gizmos.color = originalGizmoColor;
			}
		}
		
		protected void SetupGizmoColor(){
			if (inputIntensity > 0) Gizmos.color = Color.green;
			else if (inputIntensity < 0) Gizmos.color = Color.red;
		}

		protected void OnTriggerEnter(Collider other){
			Dimensions simSize = other.GetComponent<Dimensions>();
			if (simSize != null) this.simulationSize = simSize;
		}

		protected void OnTriggerExit(Collider other){
			Dimensions simSize = other.GetComponent<Dimensions>();
			if (this.simulationSize == simSize) this.simulationSize = null;
		}

		
		/// <summary>
		/// If clicking on some collider, a new input texture is created, based on any previous wave input component.
		/// If not clicking, the previous wave input is simply piped through.
		/// Pass along an empty input if you don't need any previous input component.
		/// </summary>
		/// <exception cref='System.NullReferenceException'>
		/// Is thrown when there is no previous input available.
		/// </exception>
		public override void RunStep () {
			RenderTexture waveHeight = null;
			if (this.previousInput != null){
				waveHeight = this.previousInput.outputData;
			}
			if (waveHeight == null){
				// This is probably the first input, initialize to float4(0,0,0,0) texture.
				if (this.emptyInput == null) SetupEmptyState(this.simTextureManager);
				waveHeight = this.emptyInput;
			}
			if (this.inputShape == null) throw new System.NullReferenceException("Input Shape Texture Missing");
			
			if (inputIntensity != 0 && this.simulationSize != null && !forceUnchangedOutput){
				Vector3 relativePosition = this.transform.position - this.simulationSize.firstCorner;

				relativePosition.x /= this.simulationSize.localSize.x;
				relativePosition.z /= this.simulationSize.localSize.z;

				if (relativePosition.x >= 0f 
				    && relativePosition.x <= 1f 
				    && relativePosition.z >= 0f 
				    && relativePosition.z <= 1f){
					waveHeight = AddWaves(waveHeight, relativePosition.x, relativePosition.z);
				}
			}
			UpdateOutput(waveHeight);
		}
		
		/// <summary>
		/// Adds waves,
		/// </summary>
		/// <returns>
		/// Wave Input/Output map
		/// </returns>
		/// <param name='previousInputTexture'>
		/// Previous input texture.
		/// </param>
		/// <param name='xLoc'>
		/// X location.
		/// </param>
		/// <param name='yLoc'>
		/// Y location.
		/// </param>
		private RenderTexture AddWaves(RenderTexture previousInputTexture, float xLoc, float yLoc){
			RenderTexture waveOut = this.simTextureManager.CreateOutputTexture("Wave Input Map");// Must write to different texture than used for reading.
			
			this.simKernel.SetFloat("xLoc", xLoc);
			this.simKernel.SetFloat("yLoc", yLoc);
			this.simKernel.SetFloat("SizeRatio", m_inputSizeRatio);
			this.simKernel.SetFloat("Intensity", this.inputIntensity);
			
			this.simKernel.SetTexture("WaveMapIn", previousInputTexture);
			this.simKernel.SetTexture("InputShape", this.inputShape);
			this.simKernel.SetTexture("WaveHeightOut", waveOut);
			
			this.simKernel.Dispatch();
			
			return waveOut;
		}
		
		
		private void SetupEmptyState(TextureFactory textureBuilder){
			Texture2D emptyTexture = textureBuilder.GetClearTexture();
			this.emptyInput = textureBuilder.CreateOutputTexture("No Wave Input/Output", false);
			Graphics.Blit(emptyTexture, this.emptyInput);
			Destroy(emptyTexture);
		}
		
		
	}
}