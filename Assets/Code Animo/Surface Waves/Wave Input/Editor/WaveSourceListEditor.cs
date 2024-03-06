// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo.SurfaceWaves{
	
	[CustomEditor(typeof(WaveSourceList))]
	public class WaveSourceListEditor : Editor {
		
		public override void OnInspectorGUI () {
			DrawDefaultInspector();
			WaveSourceList targetComponent = (WaveSourceList) target;
			
			
			EditorGUILayout.BeginHorizontal();{
				if (GUILayout.Button("Create Source", EditorStyles.miniButtonLeft)){
					GameObject source = CreateSource(targetComponent);
					Selection.activeGameObject = source;
					EditorGUIUtility.PingObject(source);
				}
				if (GUILayout.Button("Create Drain", EditorStyles.miniButtonRight)){
					GameObject drain = CreateDrain(targetComponent);
					Selection.activeGameObject = drain;
					EditorGUIUtility.PingObject(drain);
				}
				
			}EditorGUILayout.EndHorizontal();
		}
		
		public GameObject CreateSource(WaveSourceList targetComponent){
			return CreateWaveSourceAsChild(targetComponent, "Wave Source", targetComponent.waveSourceSettings);
		}
		
		public GameObject CreateDrain(WaveSourceList targetComponent){
			return CreateWaveSourceAsChild(targetComponent, "Wave Drain", targetComponent.waveDrainSettings);
		}
		
		protected GameObject CreateWaveSourceAsChild(WaveSourceList targetComponent ,string name, GameObject settingsPrefab){
			if (settingsPrefab == null) throw new MissingReferenceException("Default settings for " + name + " missing, it should be set up in the default references for this script");
			GameObject sourceNode = new GameObject(name);
			GameObjectExtensions.Unity4_3_4UndoCrashWorkaroundEnabled = false;
			Undo.IncrementCurrentGroup();
			int undoGroup = Undo.GetCurrentGroup();
			Undo.RegisterCreatedObjectUndo(sourceNode, name + " creation");
//			WaveSource source = Undo.AddComponent<WaveSource>(sourceNode);
			WaveSource source = sourceNode.AddComponent<WaveSource>();// crash if subObjects use Undo.addComponent too.
			source.ApplyPrefabSettings(settingsPrefab);
						
			source.transform.parent = targetComponent.transform;
			source.transform.localPosition = new Vector3(0,0,0);
			source.simulationSize = targetComponent.simulationSize;
			
			Undo.RecordObject(targetComponent, "Step Added");
			targetComponent.AddStep(source);
			
			
			Undo.CollapseUndoOperations(undoGroup);
			GameObjectExtensions.Unity4_3_4UndoCrashWorkaroundEnabled = true;
			
			return sourceNode;
		}
		
		
	}
}