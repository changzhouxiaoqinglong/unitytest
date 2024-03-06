// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.SurfaceWaves {
	
	[AddComponentMenu("Mesh/Wave Mesh")]
	public class WaveMesh : GridMesh {
		
		public float verticalBoundIncrease = 50;
		
		public override void GenerateGrid () {
			base.GenerateGrid ();
			increaseBoundsSize();
		}
		
		/// <summary>
		/// Workaround for the fact that the waveMesh height isn't known on the CPU and will culled too quickly.
		/// </summary>
		private void increaseBoundsSize(){
			Bounds customBounds = this.generatedMesh.bounds;
			Vector3 offsetBounds = customBounds.center;
			offsetBounds.y += this.verticalBoundIncrease;
			customBounds.Encapsulate(offsetBounds);
			this.generatedMesh.bounds = customBounds;
		}
			
	}
}