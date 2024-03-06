// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections.Generic;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo.SurfaceWaves{
	
	/// <summary>
	/// Creates a depth texture for depth based effect in water.
	/// Created to handle the situation where a custom depth rendering shader is required
	/// For example, when meshes in the scene use non-standard vertex manipulation.
	/// </summary>
	[AddComponentMenu("Surface Waves/Graphics/Water Depth Renderer")]
	public class WaveDepthRenderer : SimulationOutput {
	
		private RenderTexture depthTexture;
		public bool renderDepthMap = true;
		private bool renderingDepth = false;
		
		public Shader depthShader;
		private string defaultShaderName = "Hidden/Camera-DepthTexture";
		
		public WaveMeshGroup selectedWater;

		public List<Camera> selectedCameras = new List<Camera>();
		private Camera lastRenderingCamera;
		[SerializeField][HideInInspector] private Camera depthCamera;
		
		
		public override void LoadData () {
			FindTextureManager();
			
			if (this.depthShader == null){
				this.depthShader = Shader.Find(this.defaultShaderName);
				if (this.depthShader == null){
					throw new MissingReferenceException("No depth rendering shader detected, and default shader can't be found");	
				}
			}
			
			if (this.depthCamera == null) this.depthCamera = CreateDepthCamera();
		}
		
		
		public override void RunStep () {
			this.lastRenderingCamera = null;
		}
		
		/// <summary>
		/// Called by objects that need the water depth texture, when they're about to be rendered.
		/// Those objects should pass Camera.current
		/// </summary>
		/// <param name='currentCamera'>
		/// Reference to Camera.current
		/// </param>
		public void CalculateDepth(Camera currentCamera){
			if (!enabled || !renderDepthMap) return;
			if (renderingDepth) return;// Prevent recursion. Don't run this if it is our own depth renderer rendering this.
			if (object.ReferenceEquals(currentCamera,lastRenderingCamera)) return;// Don't render for twice camera twice
			
			if( !CameraIsSelected(currentCamera)) return;
			
			this.lastRenderingCamera = currentCamera;
			GenerateDepthTexture(currentCamera);
		}
		
		private void GenerateDepthTexture(Camera currentCamera){
			if (this.depthCamera == null) return;//this.depthCamera = createDepthCamera();//throw new MissingReferenceException("Depth Camera Missing. Disabling and then enabling should create a new one.");
	//		if (!UnityEditor.EditorApplication.isPlaying) return;
			if (depthShader == null) throw new MissingReferenceException("Depth Rendering Shader missing");
			
			this.renderingDepth = true;// update state
			
			this.depthCamera.CopyFrom(currentCamera);
			
			int viewportWidth = (int) currentCamera.pixelWidth;
			int viewportHeight = (int) currentCamera.pixelHeight;
			
			this.simTextureManager.resolutionU = viewportWidth;
			this.simTextureManager.resolutionV = viewportHeight;
			
			this.depthTexture = this.simTextureManager.CreateOutputTexture(currentCamera.name + " Water Depth",true);

			// Not ALL variables should be exactly the same as the current camera:
			depthCamera.renderingPath = RenderingPath.Forward;// Workaround for depth rendering not working in the deferred pipeline.
			depthCamera.targetTexture = depthTexture;
			depthCamera.clearFlags = CameraClearFlags.SolidColor;
			depthCamera.backgroundColor = Color.white;// 0 is close, 1 is far.
			depthCamera.RenderWithShader(depthShader, "RenderType");
			
			
			UpdateOutput(depthTexture);
			Material selectedMaterial = selectedWater.selectedMaterial;
			selectedMaterial.SetTexture("_DepthTex", this.outputData);// Currently support for automatically applying the texture on a single material.
			
			renderingDepth = false;// update state
		}
		
		/// <summary>
		/// Checks if the given camera is selected for depth rendering.
		/// </summary>
		/// <returns>
		/// Returns <c>true</c> if the given camera is selected for depthRendering.
		/// </returns>
		/// <param name='currentCam'>
		/// The camera of which we want to know if it is selected for depth rendering
		/// </param>
		private bool CameraIsSelected(Camera currentCam){
			return selectedCameras.Contains(currentCam);
		}
		
		private Camera CreateDepthCamera(){
			GameObject depthCameraObject = new GameObject("Water Mesh Depth Camera", typeof(Camera));
			depthCameraObject.transform.parent = this.transform;
			depthCameraObject.hideFlags = HideFlags.DontSave;
			depthCameraObject.SetActive(false);// We don't want it rendering on its own.

			Camera depthCamera = depthCameraObject.GetComponent<Camera>();
			depthCamera.SetReplacementShader(depthShader, "RenderType");

			return depthCamera;
		}
		
		private void DestroyDepthCamera(){
			if (this.depthCamera != null){
				GameObject cameraObject = this.depthCamera.gameObject;
				DestroyImmediate(cameraObject);
			}
		}
		
	}
}