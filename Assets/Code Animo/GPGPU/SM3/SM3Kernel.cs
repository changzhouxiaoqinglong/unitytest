// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	[AddComponentMenu("GPGPU/Shader Model 3 Kernel")]
	public class SM3Kernel : Kernel {
		
		#if UNITY_EDITOR
		[HideInInspector] public CodeAnimo.Support.Article componentHelp;
		#endif

		public Shader simulationShader;
		
		public string outputTextureName;
		public int pass = -1;// All passes by default
		
		protected RenderTexture targetTexture;
		[HideInInspector][SerializeField] protected Material m_simulationMaterial;

		protected void OnEnable(){
			if (m_simulationMaterial == null) CreateSimulationMaterial();
		}


		public override void Dispatch (){
			if (this.targetTexture == null){
				string message = "No output texture has been declared matching the name " + outputTextureName + ".";
				message += " On the Object called: " + gameObject.name;
				throw new System.NullReferenceException(message);
			}
			RenderTexture.active = this.targetTexture;
			Graphics.Blit(null, this.m_simulationMaterial, pass);
		}
		
		public override void SetFloat (string floatName, float floatValue){
			if (this.m_simulationMaterial.HasProperty(floatName)){
				this.m_simulationMaterial.SetFloat(floatName, floatValue);
			}
			else{
				throw new System.InvalidProgramException("Material with the name '" + this.m_simulationMaterial.name + "', does not contain a property called " + floatName);
			}
		}
		
		public override void SetInt (string intName, int intValue){
			if (this.m_simulationMaterial.HasProperty(intName)){
				this.m_simulationMaterial.SetInt(intName, intValue);
			}
			else{
				throw new System.InvalidProgramException("Material with the name '" + this.m_simulationMaterial.name + "', does not contain a property called " + intName);
			}
		}
		
		public override void SetTexture (string textureName, Texture simTexture){
			if (textureName.Equals(this.outputTextureName)){
				this.targetTexture = simTexture as RenderTexture;
			}
			else{
				if (this.m_simulationMaterial.HasProperty(textureName)){
					this.m_simulationMaterial.SetTexture(textureName, simTexture);
				}
				else{
					throw new System.InvalidOperationException("Material with the name '" + this.m_simulationMaterial.name + "', does not contain a property called " + textureName);
				}
			}
		}
		
		/// <summary>
		/// If this Kernel is supported by the user's system.
		/// </summary>
		/// <returns>
		/// Returns true if shader model 3 is supported, and the base class is supported too.
		/// </returns>
		public override bool SupportedBySystem () {
			bool baseSupport = base.SupportedBySystem();
			int shaderModel3 = 30;
			bool shaderModelSupported = SystemInfo.graphicsShaderLevel >= shaderModel3;
			return shaderModelSupported && baseSupport;
		}

		protected void CreateSimulationMaterial(){
			m_simulationMaterial = new Material(this.simulationShader);
		}
	}
}