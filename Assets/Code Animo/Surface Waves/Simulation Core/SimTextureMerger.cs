// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.SurfaceWaves{
	
	/// <summary>
	/// Takes SimulationStep output, which is a new RenderTexture every time, and puts it in one RenderTexture.
	/// This targetRenderTexture is easier to reference by materials.
	/// </summary>
	public class SimTextureMerger : SimulationOutput {
		
		public SimulationOutput textureSource{
			get { return m_textureSource; }
			set { if (value != m_textureSource) ChangeSource(value); }
		}
		[SerializeField] private SimulationOutput m_textureSource;
		
		public RenderTexture targetTexture{
			get { return m_targetTexture; }
			set { if (value != m_targetTexture) ChangeDestination(value); }
		}
		[TextureDebug][SerializeField] private RenderTexture m_targetTexture;
		
		
		private bool texturesMatched = false;
		
		
		public override void LoadData () {
			this.texturesMatched = false;
		}
		
		public override void RunStep(){
			if (m_targetTexture == null) return;
			if (m_textureSource == null) return;
			if (m_textureSource.isDataAvailable){
				
				if (!this.texturesMatched) MatchTextureSettings();// Should always work, if data is available
				Graphics.Blit(m_textureSource.outputData, m_targetTexture);
			}
		}
		
		private void ChangeSource(SimulationOutput source){
			m_textureSource = source;
			MatchTextureSettings();
		}
		
		private void ChangeDestination(RenderTexture destination){
			m_targetTexture = destination;
			MatchTextureSettings();
		}
		
		private void MatchTextureSettings(){
			//TODO: replace by moving this kind of code to CodeAnimo.GPGPU.SimulationTextureSettings
			if (textureSource.isDataAvailable){
				RenderTexture sourceTexture = textureSource.outputData;

				m_targetTexture.anisoLevel = sourceTexture.anisoLevel;
	//			m_targetTexture.antiAliasing = sourceTexture.antiAliasing;
	//			m_targetTexture.enableRandomWrite = sourceTexture.enableRandomWrite;
				m_targetTexture.enableRandomWrite = false;// This tool is meant to create textures are easy to read, not necessarily easy to write using dx11.
				m_targetTexture.filterMode = sourceTexture.filterMode;

                m_targetTexture.dimension = sourceTexture.dimension;
				m_targetTexture.mipMapBias = sourceTexture.mipMapBias;
				m_targetTexture.name = "Merged " + sourceTexture.name;
				m_targetTexture.useMipMap = sourceTexture.useMipMap;
				m_targetTexture.volumeDepth = sourceTexture.volumeDepth;

				m_targetTexture.wrapMode = sourceTexture.wrapMode;

				// Some things can't be changed on active RenderTextures:
				if (m_targetTexture.format != sourceTexture.format
				    || m_targetTexture.depth != sourceTexture.depth
				    || m_targetTexture.width != sourceTexture.width
				    || m_targetTexture.height != sourceTexture.height){

					m_targetTexture.Release();

					m_targetTexture.depth = sourceTexture.depth;
					m_targetTexture.format = sourceTexture.format;
					m_targetTexture.height = sourceTexture.height;
					m_targetTexture.width = sourceTexture.width;

					m_targetTexture.Create();
				}
				
				this.texturesMatched = true;
			}
			else this.texturesMatched = false;
		}
		
		/// <summary>
		/// Called when the script is loaded or a value is changed in the inspector (Called in the editor only).
		/// </summary>
		protected void OnValidate(){
			// Force validation through properties: 
			targetTexture = m_targetTexture;
			textureSource = m_textureSource;
		}
		
	}
}