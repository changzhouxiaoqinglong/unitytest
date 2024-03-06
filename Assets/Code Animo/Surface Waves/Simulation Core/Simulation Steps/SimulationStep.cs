// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;

namespace CodeAnimo.SurfaceWaves{
	
	public abstract class SimulationStep : MonoBehaviour {
		
		#if UNITY_EDITOR
		[HideInInspector] public CodeAnimo.Support.Article componentHelp;
		#endif
		
		/// <summary>
		/// Should be used by subclasses to prepare for the simulation.
		/// They might want to call FindKernel, FindTextureManager or LoadState
		/// </summary>
		public abstract void LoadData();
		public abstract void RunStep();
		
	}
}