// Copyright © 2017 Laurens Mathot
// Code Animo™ http://codeanimo.com
using UnityEngine;
using UnityEditor;

namespace CodeAnimo.SurfaceWaves
{
	[CustomEditor(typeof(SimulationLoad))]
	public class SimulationLoadEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			SimulationLoad targetComponent = (SimulationLoad)target;

			if (GUILayout.Button("Load"))
			{
				targetComponent.LoadAll();
			}

		}

	}
}