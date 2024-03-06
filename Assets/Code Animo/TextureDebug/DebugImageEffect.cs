// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;

namespace CodeAnimo{
	
	/// <summary>
	/// Directly copies over the texture, but stores and displays the source image.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Debug Image Effect")]
	public class DebugImageEffect : MonoBehaviour {
		
		public bool outputDepth = false;
		
		[HideInInspector][SerializeField] private Shader m_DepthCopyShader;
		private Material m_depthCopyMaterial;
		protected Material depthCopyMaterial{
			get {
				if (m_depthCopyMaterial == null) m_depthCopyMaterial = new Material(m_DepthCopyShader);
				return m_depthCopyMaterial;
			}
		}
		
		[TextureDebug]
		public RenderTexture m_sourceCopy;
		
		protected void OnRenderImage(RenderTexture source, RenderTexture destination){
			DestroyRenderTexture(m_sourceCopy);			
			
			MakeDebugCopy(source, outputDepth);// Make copy for studying.
			PassThrough(source, destination);// Send regular data through to the next image effect or output.
		}
		
		protected void MakeDebugCopy(RenderTexture source, bool copyDepth){
			RenderTextureFormat copyFormat;
			if (copyDepth) copyFormat = RenderTextureFormat.ARGBFloat;
			else copyFormat = source.format;
			RenderTextureReadWrite readWriteMode = (source.sRGB == true) ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;
			
			m_sourceCopy = new RenderTexture(source.width, source.height, source.depth, copyFormat, readWriteMode);
			
			if (copyDepth) Graphics.Blit(source, m_sourceCopy, this.depthCopyMaterial);
			else Graphics.Blit(source, m_sourceCopy);
		}
		
		protected void PassThrough(RenderTexture source, RenderTexture destination){
			Graphics.Blit(source, destination);
		}
		
		protected void OnDestroy(){
			DestroyRenderTexture(m_sourceCopy);
			DestroyImmediate(m_depthCopyMaterial);
		}
		
		protected void OnDisable(){
			DestroyRenderTexture(m_sourceCopy);
		}
		
		protected void DestroyRenderTexture(RenderTexture target){
			if (target != null){
				target.Release();
				DestroyImmediate(target);
			}
		}
		
	}
}