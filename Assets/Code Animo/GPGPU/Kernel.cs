// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.GPGPU {
	
	public abstract class Kernel : MonoBehaviour {
		
		public abstract void Dispatch();
		public abstract void SetTexture(string textureName, Texture simTexture);
		public abstract void SetFloat(string floatName, float floatValue);
		public abstract void SetInt(string intName, int intValue);
		
		/// <summary>
		/// Overridden by subclasses, to indicate if the kernel is supported by hardware and software.
		/// </summary>
		/// <returns>
		/// True if the current Hardware and software support the kernel.
		/// </returns>
		public virtual bool SupportedBySystem(){
			return true;	
		}
		
		/// <summary>
		/// Finds a Kernel, attached to target Gameobject, that can run on this system.
		/// </summary>
		/// <returns>
		/// The first compatible Kernel
		/// </returns>
		/// <param name='target'>
		/// The GameObject on which the kernel should be found.
		/// </param>
		/// <exception cref='MissingComponentException'>
		/// Is thrown when no supported kernel was attached to the targeted GameObject.
		/// </exception>
		public static Kernel FindCompatibleKernelOnGameObject(GameObject target){
			Kernel selectedKernel = target.GetComponent<Kernel>();
			if (selectedKernel != null && selectedKernel.SupportedBySystem()) return selectedKernel;
			else{
				Kernel[] allKernels = target.GetComponents<Kernel>();
				for (int i = 0; i < allKernels.Length; i++){
					selectedKernel = allKernels[i];
					if (selectedKernel.SupportedBySystem()) return selectedKernel;
				}
			}
			
			throw new MissingComponentException("No supported kernel found on this GameObject.");
		}
		
	}
}