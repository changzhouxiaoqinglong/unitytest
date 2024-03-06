// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	public abstract class ComputeKernel : Kernel {
		
		public int warpWidth = 8;
		public int warpHeight = 8;
		public int warpDepth = 1;
		
		[SerializeField] private ComputeShader m_simulationShader;
		public ComputeShader simulationShader{
			get { return m_simulationShader; }
			set {
				m_simulationShader = value;
				UpdateKernelIndex(kernelName);
			}
		}
		public string kernelName{
			get { return m_kernelName; }
			set {
				m_kernelName = value;
				UpdateKernelIndex(value);
			}
			
		}
		
		public bool kernelFound{
			get { return _kernelFound; }
		}
		
		[SerializeField]
		private string m_kernelName;
		[SerializeField]
		[HideInInspector]
		private bool _kernelFound;
		[SerializeField]
		[HideInInspector]
		protected int kernelIndex = -1;// updated OnEnable
		
		protected virtual void OnValidate(){
			// pass through property for validation:
			kernelName = m_kernelName;
			simulationShader = m_simulationShader;
		}
		
		
		public abstract override void Dispatch();
		
		public void InitializeKernel(){
			UpdateKernelIndex(kernelName);
		}
		
		/// <summary>
		/// Searches for a kernel with the given name in simulationShader.
		/// Its index is needed to set data and to run (dispatch) it.
		/// </summary>
		/// <param name='kernelName'>
		/// Kernel name.
		/// </param>
		protected void UpdateKernelIndex(string kernelName){
            if (!SupportedBySystem()){
                return;
            }
            
            if (simulationShader == null){
				_kernelFound = false;
				return;
			}
			if (kernelName == null || kernelName.Length == 0){
				_kernelFound = false;
				return;
			}
			this.kernelIndex = simulationShader.FindKernel(kernelName);
			if (kernelIndex >= 0){
				_kernelFound = true;
			}
			else{
				_kernelFound = false;
			}
		}
		
		public override void SetFloat (string floatName, float floatValue) {
			simulationShader.SetFloat(floatName, floatValue);
		}
		
		public override void SetTexture (string textureName, Texture simTexture) {
			simulationShader.SetTexture(kernelIndex, textureName, simTexture);
		}
		
		/// <summary>
		/// Returns true if the user's system supports using this ComputeKernel
		/// </summary>
		/// <returns>
		/// Returns true if the user's system supports using this ComputeKernel
		/// </returns>
		public override bool SupportedBySystem () {
			return (SystemInfo.supportsComputeShaders && base.SupportedBySystem());
		}
		
		public override void SetInt (string intName, int intValue){
			simulationShader.SetInt(intName, intValue);	
		}
		
		public void SetBuffer(string bufferName, ComputeBuffer buffer){
			simulationShader.SetBuffer(kernelIndex, bufferName, buffer);	
		}
		
		protected void LogKernelNotFoundWarning(){
			if (!_kernelFound) {
				string warningMessage = "Compute kernel name: ";
				warningMessage += kernelName;
				warningMessage += " given, but the correct index was not -or could not be- found";
				Debug.LogWarning(warningMessage, this);
			}
		}
		
#if UNITY_EDITOR
		/// <summary>
		/// Tries to find and return the compute shader at the given path.
		/// Syntactic sugar around Resources.LoadAssetAtPath<ComputeShader>(path)
		/// Only works in the editor.
		/// </summary>
		/// <returns>
		/// In Editor mode, it returns the ComputeShader at the given location.
		/// In Standalone or Webplayer, it always returns null.
		/// </returns>
		/// <param name='path'>
		/// A full path to the asset like "Assets/ExampleFolder/exampleShaderFile.compute"
		/// </param>
		public static ComputeShader GetShaderAtPath(string path){
			return UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
		}
#endif
		
	}
}