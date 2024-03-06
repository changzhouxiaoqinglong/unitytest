// Copyright © 2017 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.UnityExtensionMethods;
using System.Collections;

namespace CodeAnimo.SurfaceWaves{
	
	[AddComponentMenu("Surface Waves/Simulation Steps/Wave Displace Renderer")]
	public class WaveDisplaceRenderer : SimulationOutput {
		
		[SerializeField] private LayerMask m_renderedLayers;
		public LayerMask renderedLayers{
			get { return m_renderedLayers; }
			set {
				m_renderedLayers = value;
				if (m_depthCamera != null){
					m_depthCamera.cullingMask = m_renderedLayers;
				}
			}
		}
		
		public Shader depthShader;
		
		public float m_cameraNearPlane = 0.3f;

		private GameObject m_cameraObject;
		private Camera m_depthCamera;

		private Texture m_clearTexture;
		private Material m_depthClearMaterial;
		
		public Shader m_depthClearShader;
		public Shader depthClearShader {
			get { return m_depthClearShader;}
			set {
				m_depthClearShader = value;
				if (m_depthClearMaterial != null) m_depthClearMaterial.shader = value;
			}
		}

		[SerializeField] private float m_cameraHeightOffset = -1f;

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
		private float m_latestCameraHeightOffset = -1f;
		/// <summary>
		/// Camera offset used for the latest Output
		/// </summary>
		public float CameraHeightOffset {
			get { return m_latestCameraHeightOffset; }
		}

		/// <summary>
		/// Clean up non-garbage collected assets.
		/// </summary>
		protected void OnDestroy(){
			DestroyImmediate(m_depthClearMaterial);
		}
		
		protected void OnValidate(){
			// Pass serialized values through their properties for validation:
			renderedLayers = m_renderedLayers;
		}
		
		public override void LoadData (){
			if (m_cameraObject == null){
				m_cameraObject = CreateDepthCamera();
				m_depthCamera = m_cameraObject.GetComponent<Camera>();
				PositionDepthCamera();
			}
			if (m_clearTexture == null){
				if (simTextureManager == null) FindTextureManager();
				m_clearTexture = simTextureManager.GetClearTexture();
			}
			
			if (m_depthClearMaterial == null) m_depthClearMaterial = new Material(m_depthClearShader);
			SetupDepthClearMaterial();

			RunStep();
		}
		
		/// <summary>
		/// Sets up the material used to clear the depth map to the near plane.
		/// Gives it the depthClearShader and sets its depth setting to 0.
		/// </summary>
		private void SetupDepthClearMaterial(){
			m_depthClearMaterial.shader = m_depthClearShader;
			m_depthClearMaterial.SetFloat("_Depth", 1);
		}

		public override void RunStep (){
			if (simTextureManager == null) FindTextureManager();

			RenderTexture streamBedDepth = simTextureManager.CreateOutputTexture("Displacement Depth", true);
			
			// Apply dynamic camera settings:
			this.m_depthCamera.targetTexture = streamBedDepth;
			this.m_depthCamera.farClipPlane = simulationSize.localSize.y - m_cameraHeightOffset;// Only render as far as the size of the simulation.
			this.m_depthCamera.nearClipPlane = m_cameraNearPlane;

			// Update Heightmap:
			Graphics.Blit(m_clearTexture, streamBedDepth, m_depthClearMaterial);// Clear to far plane to allow ZTest GEqual
			this.m_depthCamera.RenderWithShader(depthShader, "RenderType");
			UpdateOutput(streamBedDepth);

			// Store settings used for latest render;
			m_latestFarClipPlane = m_depthCamera.farClipPlane;
			m_latestNearClipPlane = m_depthCamera.nearClipPlane;
			m_latestCameraHeightOffset = m_cameraHeightOffset;
		}

		private GameObject CreateDepthCamera(){
			GameObject cameraObject = new GameObject("Wave Displacement Depth Camera", typeof(Camera));
			cameraObject.transform.parent = this.transform;
			
//			cameraObject.transform.Rotate(Vector3.right, 90);// Point it down. Terrain Height is farplane - (depth - offset)
			cameraObject.transform.Rotate(Vector3.right, -90);// Point it up. Stay close to the terrain, giving better LOD at low heights.
			
			cameraObject.hideFlags = HideFlags.DontSave;
			cameraObject.SetActive(false);// We don't want it rendering on its own.
			
			Camera depthCamera = cameraObject.GetComponent<Camera>();
			depthCamera.renderingPath = RenderingPath.Forward;// can only render depth in forward mode.
			depthCamera.orthographic = true;
			depthCamera.cullingMask = this.renderedLayers;
			depthCamera.backgroundColor = Color.white;// Maximum distance
			depthCamera.clearFlags = CameraClearFlags.Nothing;// Clear manually to near clip plane.
			depthCamera.useOcclusionCulling = false;
			depthCamera.SetReplacementShader(depthShader, "RenderType");// useful for previewing in editor to make it always use the replacement shader.

			depthCamera.orthographicSize = this.simulationSize.localExtends.z;// Set the vertical size of the camera.
			depthCamera.aspect = 1.0f;


			return cameraObject;
		}
		
		private void DestroyDepthCamera(){
			if (this.m_cameraObject != null){
				this.m_depthCamera = null;
				DestroyImmediate(this.m_cameraObject);
			}
		}

		private void PositionDepthCamera(){
			Transform cameraTransform = m_cameraObject.transform;
			cameraTransform.parent = this.transform;
			
			Vector3 floorCenter = this.simulationSize.center;
			floorCenter.y += m_cameraHeightOffset - this.simulationSize.localExtends.y;
			
			cameraTransform.position = floorCenter;
		}

	}
}