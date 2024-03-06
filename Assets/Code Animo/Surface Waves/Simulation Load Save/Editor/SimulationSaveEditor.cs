// Copyright © 2017 Laurens Mathot
// Code Animo™ http://codeanimo.com
using UnityEngine;
using UnityEditor;

namespace CodeAnimo.SurfaceWaves
{
	[CustomEditor(typeof(SimulationSave))]
	public class SimulationSaveEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			SimulationSave targetComponent = (SimulationSave)target;

			if (GUILayout.Button("Save"))
			{
				targetComponent.SaveAll();
			}

		}

	}
}