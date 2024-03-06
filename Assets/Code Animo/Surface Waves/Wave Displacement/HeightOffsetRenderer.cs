// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo.SurfaceWaves{
	
	/// <summary>
	/// I think this is the stub for a class that would be able to make objects form waves.
	/// It could use a rename.
	/// </summary>
	public class HeightOffsetRenderer : SimulationOutput {
		public Shader depthShader;
		
		public LayerMask renderedLayers;
		public float cameraYOffset = 0;
		
		private float m_latestFarClipPlane;
		/// <summary>
		/// Far Clip plane distance used for latest Output
		/// </summary>
		public float FarClipPlane{
			get { return m_latestFarClipPlane; }	
		}
		private float m_latestNearClipPlane;
		/// <summary>
		/// Near Clip plane distance used for latest Output
		/// </summary>
		public float NearClipPlane{
			get { return m_latestNearClipPlane; }	
		}

		private Camera depthCamera;
		private RenderTexture displacementDepth;

		
		protected void OnDestroy(){
			DestroyDepthCamera();	
		}
		
		public override void LoadData () {
			FindTextureManager();
		}
		
		private Camera CreateDepthCamera(){
			GameObject cameraObject = new GameObject("Wave Depth Camera", typeof(Camera));
			cameraObject.transform.parent = this.transform;
			
			cameraObject.transform.Rotate(Vector3.right, -90);// Point it upwards
			
			cameraObject.hideFlags = HideFlags.DontSave;
			cameraObject.SetActive(false);// We don't want it rendering on its own.
			
			Camera depthCamera = cameraObject.GetComponent<Camera>();
			depthCamera.orthographic = true;
			depthCamera.cullingMask = renderedLayers;
			depthCamera.backgroundColor = Color.white;// Maximum distance
			depthCamera.clearFlags = CameraClearFlags.SolidColor;
			
			return depthCamera;
		}
		
		private void DestroyDepthCamera(){
			if (this.depthCamera != null){
				GameObject cameraObject = this.depthCamera.gameObject;
				DestroyImmediate(cameraObject);
			}	
		}
		
		public override void RunStep(){
			if (this.depthCamera == null) this.depthCamera = CreateDepthCamera();
			
			RenderTexture displacementDepth = this.simTextureManager.CreateOutputTexture("Displacing Objects Depth");
			
			float cameraWidth = (float)this.simTextureManager.resolutionU;
			float cameraHeight = (float)this.simTextureManager.resolutionV;
			
			this.depthCamera.targetTexture = displacementDepth;
			this.depthCamera.orthographicSize = cameraWidth / 2;
			this.depthCamera.aspect = cameraWidth / cameraHeight;
			
			Vector3 cameraPosition = new Vector3(0.5f * cameraWidth, cameraYOffset, 0.5f * cameraHeight) + this.transform.position;
			this.depthCamera.transform.position = cameraPosition;
			
			this.depthCamera.RenderWithShader(depthShader, "RenderType");
			
			UpdateOutput(displacementDepth);
			
			// Store far and near clip plane used for latest output:
			m_latestFarClipPlane = this.depthCamera.farClipPlane;
			m_latestNearClipPlane = this.depthCamera.nearClipPlane;
		}
	}
}