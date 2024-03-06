// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections.Generic;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo.SurfaceWaves {
	
	[AddComponentMenu("Surface Waves/Wave Sources/Wave Source List")]
	public class WaveSourceList : SimulationStep {
		
		[HideInInspector]public GameObject waveSourceSettings;
		[HideInInspector]public GameObject waveDrainSettings;
		
		/// <summary>
		/// The combined inputs and outputs.
		/// </summary>/
		[SerializeField][TextureDebug(inputBox=false)]
		private RenderTexture m_outputData;
		public RenderTexture outputData{
			get { return m_outputData; }
		}
		public Dimensions simulationSize;
		
		[ReorderableList][SerializeField]
		protected List<WaveSource> m_sourceList = new List<WaveSource>();
		
		protected bool m_dataLoaded = false;
		
		public void AddStep(WaveSource source){
			m_sourceList.Add(source);
			if (m_dataLoaded && Application.isPlaying) source.LoadData();
		}
		
		/// <summary>
		/// Removes the first reference to the given step, from the list of steps.
		/// </summary>
		/// <param name='step'>
		/// The step that should be removed.
		/// </param>
		public void RemoveStep(WaveSource source){
			m_sourceList.Remove(source);
		}
		
		protected void Awake(){
			m_dataLoaded = false;
		}
		
		
		public override void LoadData (){
			int elementCount = m_sourceList.Count;
			for (int i = 0; i < elementCount; i++) {
				m_sourceList[i].LoadData();
			}
			m_dataLoaded = true;
		}
		public override void RunStep (){
			int elementCount = m_sourceList.Count;
			for (int i = 0; i < elementCount; i++) {
				WaveSource source = m_sourceList[i];
				
				// Remove deleted objects from list:
				if (source == null){
					m_sourceList.RemoveAt(i);
					i--;
					elementCount = m_sourceList.Count;
					continue;
				}
				else{
					if (i > 0){
						// link elements together:
						source.previousInput = m_sourceList[i -1];
					}
					else {
						source.previousInput = null;
					}
	
					source.RunStep();
				}
			}

			if (elementCount > 0)	m_outputData = m_sourceList[elementCount - 1].outputData;
		}
		
		
		
		
		
	}
}