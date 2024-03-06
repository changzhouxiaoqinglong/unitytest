// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	/// <summary>
	/// This class describes an asset used to store information needed for creating textures used in Surface Waves Simulation.
	/// </summary>
	[System.Serializable]
	public class SimulationTextureSettings : ScriptableObject {
		
		public RenderTextureFormat dataPrecision = RenderTextureFormat.ARGBFloat;
		public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
		public bool enableRandomWrite = true;
		public int anisoLevel = 0;
		public FilterMode filterMode = FilterMode.Point;
		public RenderTextureReadWrite readWriteMode = RenderTextureReadWrite.Linear;
		[SerializeField] private int m_textureDepth = 0;
		public int textureDepth{
			get { return m_textureDepth; }
			set { 
				if (value > 8){
					if (value > 20) m_textureDepth = 24;
					else m_textureDepth = 16;
				}
				else m_textureDepth = 0;
			}
		}
		
		public void MatchTexture(RenderTexture texture){
			this.dataPrecision = texture.format;
			this.wrapMode = texture.wrapMode;
			this.enableRandomWrite = texture.enableRandomWrite;
			this.anisoLevel = texture.anisoLevel;
			this.filterMode = texture.filterMode;
			if (texture.sRGB) this.readWriteMode = RenderTextureReadWrite.sRGB;
			else this.readWriteMode = RenderTextureReadWrite.Linear;
			this.textureDepth = texture.depth;
		}

		protected void OnValidate(){
			// apply values through properties:
			textureDepth = m_textureDepth;
		}
		
		public bool supportedOnCurrentSystem(){
			return SystemInfo.SupportsRenderTextureFormat(this.dataPrecision);
		}
		
	}
}