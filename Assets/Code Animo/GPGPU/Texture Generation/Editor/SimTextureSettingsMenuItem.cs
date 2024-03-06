// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.GPGPU {
	
	public class SimTextureSettingsMenuItem : ScriptableObject {
#if CODEANIMO_DEV
		[MenuItem("Assets/Create/Code Animo/Surface Waves/Texture Settings")]
		public static void CreateSimTextureSettingsAsset(){
			AssetCreation.CreateAsset<SimulationTextureSettings>("Texture Settings");
		}
#endif


	}
}