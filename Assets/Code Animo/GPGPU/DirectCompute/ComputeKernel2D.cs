// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	/// <summary>
	/// 2D Compute Kernel
	/// Expects to be on the same object as the TextureFactory creating the output texture, for resolution data.
	/// </summary>
	[AddComponentMenu("GPGPU/Compute Kernel 2D")]
	public class ComputeKernel2D : ComputeKernel {
		
		#if UNITY_EDITOR
		[HideInInspector] public CodeAnimo.Support.Article componentHelp;
		#endif
	
		public bool forceCustomResolution = false;
		
		[SerializeField] private int m_customResolutionU = 512;
		/// <summary>
		/// Used to determine the number of warp groups that should be dispatched.
		/// Recommended to keep this value a multiple of the warp size of your target hardware.
		/// </summary>
		/// <exception cref='System.ArgumentOutOfRangeException'>
		/// Is thrown when the argument isn't larger than 0
		/// </exception>
		public int resolutionU{
			get {
				if (willUseCustomResolution) return m_customResolutionU;
				else return m_outputCreator.resolutionU;
			}
			set {
				if (value > 0 ) m_customResolutionU = value;
				else{
					m_customResolutionU = 1;
					throw new System.ArgumentOutOfRangeException("Kernel can't have a negative resolution");
				}
			}
		}
		
		[SerializeField] private int m_customResolutionV = 512;
		/// <summary>
		/// Used to determine the number of warp groups that should be dispatched.
		/// Recommended to keep this value a multiple of the warp size of your target hardware. 
		/// </summary>
		/// <exception cref='System.ArgumentOutOfRangeException'>
		/// Is thrown when the argument isn't larger than 0
		/// </exception>
		public int resolutionV{
			get {
				if (willUseCustomResolution) return m_customResolutionV;
				else return m_outputCreator.resolutionV;
			}
			set {
				if (value > 0) m_customResolutionV = value;
				else{
					m_customResolutionV = 1;
					throw new System.ArgumentOutOfRangeException("Kernel can't have a negative resolution");;
				}
			}
		}
		
		public bool willUseCustomResolution{
			get { return forceCustomResolution || m_outputCreator == null; }
		}
		
		override protected void OnValidate(){
			base.OnValidate();
			// Validate through properties:
			resolutionU = m_customResolutionU;
			resolutionV = m_customResolutionV;
		}
		
		protected void Reset(){
			m_outputCreator = GetComponent<TextureFactory>();
		}
		
		protected void OnEnable(){
			m_outputCreator = GetComponent<TextureFactory>();
		}
		
		/// <summary>
		/// The texture factory on the same GameObject as this, is most likely to provide the output texture for this component.
		/// </summary>
		private TextureFactory m_outputCreator;
		
		/// <summary>
		/// Calculates the number of threadGroups required and dispatches the kernel.
		/// </summary>
		public override void Dispatch () {
			if (kernelIndex < 0) LogKernelNotFoundWarning();
			else{
				int uGroups = resolutionU / this.warpWidth;
				int vGroups = resolutionV / this.warpHeight;
						
				simulationShader.Dispatch(kernelIndex, uGroups, vGroups, 1);
			}
		}
		
	}
}