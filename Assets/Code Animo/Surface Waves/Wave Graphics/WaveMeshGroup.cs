// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.SurfaceWaves {
	
	[AddComponentMenu("Surface Waves/Graphics/Wave Mesh Group")]
	[System.Serializable]
	public class WaveMeshGroup : GridMeshGroup {
		
		#if UNITY_EDITOR
		[HideInInspector] public CodeAnimo.Support.Article componentHelp;
		#endif
		
		public float maximumHeight = 256;
		public Dimensions simulationSize;
		
		/// <summary>
		/// Starts the creating group.
		/// Actual mesh creation will happen through EditorUpdate.
		/// </summary>
		public override void StartCreatingGroup () {
			if (isCreatingGroup) return;
			this.totalMeshWidth = simulationSize.localSize.x;
			this.totalMeshDepth = simulationSize.localSize.z;
			
			
			
			base.StartCreatingGroup ();
			
			// override start position:
			this.sessionStartPosition = simulationSize.firstCorner;
		}
		
		protected override void Reset () {
			base.Reset ();
			meshNamePrefix = "Grouped Wave Mesh";
		}
		
		protected override GridMesh CreateSegment(){
			GameObject segmentNode = new GameObject("incomplete grouped mesh");
			WaveMesh segment = segmentNode.AddComponent<WaveMesh>();
			
			return segment;	
		}

		protected override void SetupSegment (GridMesh segment) {
			base.SetupSegment (segment);
			WaveMesh waveSegment = segment as WaveMesh;
			if (waveSegment == null) throw new System.NullReferenceException("The segment can not be processed because it isn't a WaveMesh segment");
			
			waveSegment.verticalBoundIncrease = this.maximumHeight;
		}
		
	}
}