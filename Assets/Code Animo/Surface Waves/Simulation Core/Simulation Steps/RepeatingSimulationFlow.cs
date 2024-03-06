// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;

namespace CodeAnimo.SurfaceWaves{
	
	public class RepeatingSimulationFlow : SimulationFlow {
		public bool fixedFrameRate = true;
		[SerializeField] private float m_targetFrameRate = 60;
		[SerializeField] private float m_maxUpdateTime = 0.1f;
		[HideInInspector] [SerializeField] private float m_timeStepSize = 0.016f;

		private float m_nextUpdateTime = 0;
		private float m_firstUpdateTimeOfRun = 0;

		private int m_maximumLoopCount = 480;// Infinite Loop prevention, double this if your computer can hit this without crashing.
		private int m_loopCount = 0;

		public float maxUpdateTime{
			get { return m_maxUpdateTime; }
			set {
				if (value > .1f) m_maxUpdateTime = 0.1f;
				else if (value <= 0) m_maxUpdateTime = 0.008f;
				else m_maxUpdateTime = value;
				SetFirstFixedFrameData();
			}
		}

		public float targetFrameRate{
			get { return m_targetFrameRate; }
			set {
				if (value <= 0) m_targetFrameRate = 1f / m_maxUpdateTime;
				else { 
					m_targetFrameRate = value;
				}
				m_timeStepSize = 1f / m_targetFrameRate;
				CalculateNextStepTime();
			}
		}

		protected void OnValidate(){
			targetFrameRate = m_targetFrameRate;
			maxUpdateTime = m_maxUpdateTime;
		}

		protected void OnEnable(){
			CalculateNextStepTime ();
		}
		
		public override void RunStep () {
			if (!enabled) return;

			if (fixedFrameRate){
				m_firstUpdateTimeOfRun = m_nextUpdateTime;// Simulation time limiting.
				m_loopCount = 0;// Infinite loop prevention

				while (m_nextUpdateTime <= Time.time) {
					base.RunStep ();
					CalculateNextStepTime();// Update loop conditions.

					if (spentTooMuchTime()){
						// Could be trying to simulate too much,
						// or execution may have been paused because of alt-tab.
						SetFirstFixedFrameData();
						break;
					}

					// Infinite Loop prevention:
					m_loopCount++;
					if (m_loopCount > m_maximumLoopCount){
						this.enabled = false;
						throw new System.Exception("Infinite loop prevention. Number of loops exceeds " + m_maximumLoopCount + " which probably can't be handled within one frame.");
					}
				}
			}
			else {
				base.RunStep();
			}
		}

		protected void CalculateNextStepTime(){
			m_nextUpdateTime += m_timeStepSize;
		}

		protected void SetFirstFixedFrameData(){
			m_nextUpdateTime = Time.time + m_timeStepSize;
		}

		protected bool isProbablyFirstUpdate(){
			return m_nextUpdateTime == 0;
		}

		protected bool spentTooMuchTime(){
			return m_nextUpdateTime - m_firstUpdateTimeOfRun > m_maxUpdateTime;
		}
		
	}
}