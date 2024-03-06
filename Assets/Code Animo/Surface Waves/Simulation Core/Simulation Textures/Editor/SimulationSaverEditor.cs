// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.Collections;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves {
	
	[CustomEditor(typeof(SimulationSaver))]
	public class SimulationSaverEditor : Editor {
		
		override public void OnInspectorGUI(){
			SimulationSaver targetComponent = target as SimulationSaver;
			DrawDefaultInspector();
			
			if (GUILayout.Button("Save WaveLevel")) targetComponent.saveWaveLevel();
			if (GUILayout.Button("Restore waveLevel")) targetComponent.restoreWaveLevel();
			
			
		}
		
	}
}