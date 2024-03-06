// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections.Generic;

namespace CodeAnimo.SurfaceWaves {
	
	/// <summary>
	/// Responsible for calling simulation steps,
	/// in the appropriate order,
	/// at the appropriate time.
	/// Central authority over the flow a set of connected steps, to simply synchronization. 
	/// </summary>
	[AddComponentMenu("Surface Waves/Simulation Flow")]
	public class SimulationFlow : SimulationStep {
		[ReorderableList][SerializeField] protected List<SimulationStep> m_steps = new List<SimulationStep>();
		
		public bool loadStepsOnStart = false;
		public bool runStepsOnUpdate = false;
		
		public override void LoadData () {
			for (int i = 0; i < m_steps.Count; i++) {
				m_steps[i].LoadData();
			}
		}
		
		public override void RunStep () {
			for (int i = 0; i < m_steps.Count; i++) {
				m_steps[i].RunStep();
			}
		}
		
		public void AddStep(SimulationStep step){
			m_steps.Add(step);
		}
		
		/// <summary>
		/// Removes the first reference to the given step, from the list of steps.
		/// </summary>
		/// <param name='step'>
		/// The step that should be removed.
		/// </param>
		public void RemoveStep(SimulationStep step){
			m_steps.Remove(step);
		}
		
		protected void Start(){
			if (loadStepsOnStart) LoadData();
		}
		
		protected void Update(){
			if (runStepsOnUpdate) RunStep();
		}
	
	}
}