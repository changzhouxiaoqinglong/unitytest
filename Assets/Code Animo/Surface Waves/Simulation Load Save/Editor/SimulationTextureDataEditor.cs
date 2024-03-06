// Copyright (c) 2017 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.SurfaceWaves
{

	[CustomEditor(typeof(SimulationTextureData))]
	public class SimulationTextureDataEditor : Editor
	{

		[MenuItem("Assets/Create/Code Animo/Surface Waves/Simulation Texture Data")]
		public static void CreateAssetExportCollection()
		{
			AssetCreation.CreateAsset<SimulationTextureData>("Simulation Texture Data");
		}
		/*
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var targetComponent = (SimulationTextureData)target;

		}*/

	

	}
}