using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace CodeAnimo.GPGPU{

	[System.Serializable]
	[AddComponentMenu("GPGPU/Texture Factory")]
	public class TextureFactory : MonoBehaviour {
		
		#if UNITY_EDITOR
			[HideInInspector] public Support.Article componentHelp;
		#endif
		
		
		public SimulationTextureSettings textureSettings;
		
		public int resolutionU = 512;
		public int resolutionV = 512;
		
		public int registeredTextureCount{
			get { 
				if (createdTextures != null) return createdTextures.Count;
				else return 0;
			}
		}

		/// <summary>
		/// Returns the number of textures, excluding the latest one.
		/// Convenience property, to make off-by-one errors, and their prevention slightly more obvious.
		/// </summary>
		/// <value>The previous textures count.</value>
		public int previousTexturesCount{
			get {
				if (this.createdTextures != null) return System.Math.Max(0, this.createdTextures.Count - 1);
				else return 0;
			}
		}
		
		public RenderTexture LatestTexture{
			get { 
				int count = this.registeredTextureCount;
				if (count < 1) return null;
				else return createdTextures[count -1];
			}	
		}
		
		public List<RenderTexture> registeredTextures{
			get { return createdTextures; }	
		}
		
		
		/// <summary>
		/// The number of textures that is allowed before starting to destroy old textures.
		/// </summary>
		public int allowedRecentTextureCount{
			get { return m_allowedRecentTextureCount; }
			set {
				if (m_allowedRecentTextureCount != value){
					m_allowedRecentTextureCount = value;
					DestroyRegisteredTextures(value);// If there are now too many textures, destroy them.
				}
			}
		}
		[SerializeField][HideInInspector]
		private int m_allowedRecentTextureCount = 1;
		
		private bool _initialized = false;
		
		[SerializeField]
		[HideInInspector]
		private List<RenderTexture> createdTextures;
		
		private Texture2D m_clearTexture;
		
		protected void Awake(){
			Initialize();
		}
		
		protected virtual void OnDestroy(){
			DestroyRegisteredTextures(0);// On Scene change, or component deletion, destroy all contained textures.
			DestroyClearTexture();
		}
		
		protected void Initialize(){
			if (_initialized) Debug.LogWarning("Trying to initialize Texture Factory while it is already initialized. Existing data will be used.", this);
			else {
				// Any old Textures created during edit mode are invalid, so use a new list:
				this.createdTextures = new List<RenderTexture>(allowedRecentTextureCount + 1);// In a typical scenario, two entries in the list are used.
				
				_initialized = true;
			}
		}
		
		/// <summary>
		/// Creates RenderTexture based on texture settings
		/// Destroys the oldest renderTexture created by this component.
		/// By default, the most recent old texture is kept, because it might still be needed as input.
		/// If you disable automaticDestruction, make sure to destroy the created texture manually.
		/// </summary>
		/// <returns>
		/// The created RenderTexture.
		/// </returns>
		/// <param name='name'>
		/// Name given to the created texture.
		/// </param>
		/// <param name='automaticDestruction'>
		/// If this is set to true(default), the created texture is registered to be automatically destroyed when a maximum number of Textures has been created.
		/// </param>
		/// <exception cref='System.InvalidOperationException'>
		/// Is thrown trying to call this method before this thing has been initialized.
		/// </exception>
		public RenderTexture CreateOutputTexture(string name, bool automaticDestruction){
			if (!_initialized){
				Initialize();
	//			throw new System.InvalidOperationException("Tried to create Output Texture before initialization was complete. Is this textureBuilder enabled and active?");
			}
			
			var simRT = new RenderTexture(
				this.resolutionU,
				this.resolutionV,
				textureSettings.textureDepth,
				textureSettings.dataPrecision,
				textureSettings.readWriteMode
			);
			setSimulationTextureSettings(simRT, name);
			simRT.Create();// Compute RenderTextures are not set to active, so we need to manually create them. Unless a temporary RT is reused.
			if (automaticDestruction) RegisterTexture(simRT);
			return simRT;
		}
		
		public RenderTexture CreateOutputTexture(string name){
			return CreateOutputTexture(name, true);
		}
		
		public RenderTexture CreateOutputTexture(){
			return CreateOutputTexture("Unnamed Output Texture", true);
		}
		
		
		/// <summary>
		/// Apply remaining texture settings
		/// This method sets those up.
		/// </summary>
		/// <param name='simulationTexture'>
		/// The texture for which the settings should be set.
		/// </param>
		/// <param name='name'>
		/// The name that the texture should use.
		/// </param>
		private void setSimulationTextureSettings(RenderTexture simulationTexture, string name){
			simulationTexture.name = name;
			simulationTexture.enableRandomWrite = textureSettings.enableRandomWrite;
			simulationTexture.anisoLevel = textureSettings.anisoLevel;
			simulationTexture.filterMode = textureSettings.filterMode;
			simulationTexture.hideFlags = HideFlags.HideAndDontSave;
			simulationTexture.wrapMode = textureSettings.wrapMode;
		}
		
		/// <summary>
		/// Destroys all but one of the RenderTextures created by this component.
		/// Useful when you want to clear memory when you know an old texture is no longer needed as input.
		/// </summary>
		public void DestroyOldTextures(){
			DestroyRegisteredTextures(1);
		}
		
		/// <summary>
		/// Destroys all registered textures.
		/// </summary>
		public void DestroyAllTextures(){
			DestroyRegisteredTextures(0);
		}
		
		/// <summary>
		/// Keep track of the created RenderTexture.
		/// If there is more than one RenderTexture created by this component,
		/// the oldest one is destroyed first.
		/// </summary>
		/// <param name='addedTexture'>
		/// The texture that should be added.
		/// </param>
		private void RegisterTexture(RenderTexture addedTexture){
			createdTextures.Add(addedTexture);

			if (this.registeredTextureCount > allowedRecentTextureCount){
				DestroyRegisteredTextures(allowedRecentTextureCount);
			}
		}
		
		/// <summary>
		/// Destroy all registered textures
		/// Except for the given amount of recent textures
		/// </summary>
		/// <param name='recentKeptCount'>
		/// The number of textures that should be kept.
		/// </param>
		private void DestroyRegisteredTextures(int recentKeptCount){
			while(this.previousTexturesCount > recentKeptCount){
				RenderTexture victim = RemoveOldest();
				DestroyRenderTexture(victim);
			}
		}
		
		/// <summary>
		/// Removes oldest Texture from the list and returns it.
		/// </summary>
		/// <returns>
		/// The removed texture.
		/// </returns>
		private RenderTexture RemoveOldest(){
			RenderTexture oldestTexture = createdTextures[0];
			createdTextures.RemoveAt(0);
			return oldestTexture;
		}
		
		/// <summary>
		/// Releases and destroys the given RenderTexture.
		/// </summary>
		/// <param name='victim'>
		/// The RenderTexture that will be destroyed
		/// </param>
		/// <exception cref='MissingReferenceException'>
		/// Is thrown when the RenderTexture is already null.
		/// </exception>
		private void DestroyRenderTexture(RenderTexture victim){
			if (victim == null) throw new MissingReferenceException("A RenderTexture seems to have disappeared. Perhaps the renderer threw it away?");
			victim.Release();
			DestroyImmediate(victim);
		}
		
		private void DestroyClearTexture(){
			Destroy(m_clearTexture);	
		}
		
		public Texture2D GetClearTexture(){
			if (m_clearTexture == null){
				m_clearTexture = new Texture2D(1, 1,TextureFormat.ARGB32, false);
				
				m_clearTexture.name = "clear texture";
				Color transparentBlack = new Color(0,0,0,0);
				
				m_clearTexture.SetPixel(0,0, transparentBlack);
				
				m_clearTexture.filterMode = FilterMode.Point;
				
				m_clearTexture.Apply(false, true);
			}
			return m_clearTexture;
		}
		
	}
}// Namespace