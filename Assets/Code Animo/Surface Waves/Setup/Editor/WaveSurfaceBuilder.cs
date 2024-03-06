// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using CodeAnimo.SurfaceWaves;

namespace CodeAnimo.SurfaceWaves.Setup{
	
	public class WaveSurfaceBuilder : ScriptableObject {
		
		public Transform rootTransform;
		public WaveMeshGroup waveSurface;
		
		public void Build(Dimensions simSize){
			GameObject waveSurfaceGameObject = new GameObject("Wave Surface");
			WaveMeshGroup waveSurface =  waveSurfaceGameObject.AddComponent<WaveMeshGroup>();
			waveSurface.simulationSize = simSize;
			
			waveSurfaceGameObject.AddComponent<CameraDepthChecker>();
			
			// Make Wave Mesh Group use editor updates to create its mesh:
			InteractiveLoader waveMeshLoader = waveSurface.GetComponent<InteractiveLoader>();
			EditorApplication.update -= waveMeshLoader.EditorUpdate;
			EditorApplication.update += waveMeshLoader.EditorUpdate;
			
			this.waveSurface = waveSurface;
			this.rootTransform = waveSurfaceGameObject.transform;
		}
		
	}
}