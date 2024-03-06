// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.SurfaceWaves {
	
	public abstract class DepthmapSaver : MonoBehaviour {
		
		public abstract bool dataStored{ get; }
		
		public abstract void ReadDepthMap(RenderTexture depthMap);
		public abstract void WriteDepthMap(RenderTexture depthMap);
		
	}
}