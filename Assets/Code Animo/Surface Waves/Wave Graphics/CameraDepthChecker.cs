// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com
using UnityEngine;
using System.Collections.Generic;

namespace CodeAnimo{
	public class CameraDepthChecker : MonoBehaviour {
		
		public List<Camera> depthlessCameras;
		
		public bool automaticallyEnableDepthOnStart = true;
		public DepthTextureMode automaticDepthTextureMode = DepthTextureMode.Depth;
		
		protected void Start(){
			FindDepthlessCameras();
			if (automaticallyEnableDepthOnStart) EnableAllCameraDepthRendering();
		}
		
		
		/// <summary>
		/// Stores a list of the cameras that don't seem to write any depth texture.
		/// </summary>
		public void FindDepthlessCameras(){
			this.depthlessCameras = ListDepthlessCameras();
			
			if (this.automaticallyEnableDepthOnStart) return;// Throw warning in the code that changes the depth rendering mode instead.
			
			if (this.depthlessCameras.Count < 1) return;// No issues with depth textures found.
			if (this.depthlessCameras.Count == 1){// One camera is not rendering a depth texture.
				Debug.LogWarning("One of your cameras is not set up for use with depth-based effects. Click this error message once to see the component with more information.", this);
			}
			else{// Multiple cameras are not rendering a depth texture.
				Debug.LogWarning("Several of your cameras are not set up for use with depth-based effects. Click this error message once to see the component with more information.", this);
			}	
		}
		
		/// <summary>
		/// Applies a default texture to the camera depth texture, to make depth based objects appear when they would normally be invisible.
		/// </summary>
		/// <param name='someDefaultTexture'>
		/// The texture that will be written to the global shader property _CameraDepthTexture.
		/// </param>
		protected void ApplyDefaultCameraDepthTexture(Texture someDefaultTexture){
			// Setup default camera depth:
			if(someDefaultTexture != null) Shader.SetGlobalTexture("_CameraDepthTexture", someDefaultTexture);// Works if nothing renders to the texture after this.
		}
		
		protected void EnableAllCameraDepthRendering(){
			for (int i = 0; i < this.depthlessCameras.Count; i++) {
				Camera targetCamera = this.depthlessCameras[i];
				Debug.LogWarning("Depth rendering automatically enabled for camera '" +  targetCamera.name + "', by CameraDepthChecker to allow for depth-based effects.", this);
				this.depthlessCameras[i].depthTextureMode = this.automaticDepthTextureMode;
			}
		}
		
		protected List<Camera> ListDepthlessCameras(){
			List<Camera> depthlessCameras = new List<Camera>();
			Camera[] cameraList = Camera.allCameras;
			
			for (int i = 0; i < cameraList.Length; i++) {
				Camera targetCamera = cameraList[i];
				if (targetCamera.actualRenderingPath == RenderingPath.DeferredLighting) continue;
				
				DepthTextureMode mode = targetCamera.depthTextureMode;
				if (mode == DepthTextureMode.Depth || mode == DepthTextureMode.DepthNormals) continue;
				depthlessCameras.Add(cameraList[i]);
			}
			
			return depthlessCameras;
		}
	}
}
