// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves{

	public class StepStateManager : MonoBehaviour {
	
		[SerializeField] private Texture2D m_startMap;
		private Texture2D clearTexture;
		
		
		protected bool m_savedStateAvailable = false;
		
		
		protected void OnDisable(){
			destroyClearTexture();
		}
		
		protected void OnDestroy(){
			destroyClearTexture();
		}
		
		
		
		public virtual RenderTexture LoadState(TextureFactory destination){
//			if (!m_savedStateAvailable)
			return initializeTextures(destination);
		}
		
		public RenderTexture initializeTextures(TextureFactory textureBuilder){
			Texture2D textureSource = m_startMap;
			
			// Force textures to be completely 0, in stead of null, otherwise they might end up as anything:
			if (textureSource == null){
				if (clearTexture == null) this.clearTexture = textureBuilder.GetClearTexture();
				textureSource = this.clearTexture;
			}
			
			string initialTextureName = "Initial Texture for " + textureBuilder.name;
			
			RenderTexture resultTexture = textureBuilder.CreateOutputTexture(initialTextureName);
			Graphics.Blit(textureSource, resultTexture);
			
			return resultTexture;
		}
		
		private void destroyClearTexture(){
			if (this.clearTexture == null) return;
			DestroyImmediate(this.clearTexture);
		}
		
		
	}
}