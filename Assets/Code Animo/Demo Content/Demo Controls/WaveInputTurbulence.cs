// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;

namespace CodeAnimo.SurfaceWaves {

	[RequireComponent(typeof(WaveSource))]
	public class WaveInputTurbulence : MonoBehaviour {
		
		[SerializeField] private float m_minimumIntensity = 0f;
		public float minimumIntensity {
			get { return m_minimumIntensity; }
			set { m_minimumIntensity = Mathf.Clamp(value, -1f, m_maximumIntensity); }
		}
		
		[SerializeField] private float m_maximumIntensity = 0.2f;
		public float maximumIntensity{
			get { return m_maximumIntensity; }
			set { m_maximumIntensity = Mathf.Clamp(value, m_minimumIntensity, 1f); }
		}
		
		[TooltipAttribute("The amount changed per Update")]
		public float changeSpeed = 0.01f;
		
		[TooltipAttribute("The distance from the target at which a new target intensity is chosen")]
		[RangeAttribute(0,1)]public float m_touchDistance = .001f;

		private WaveSource m_selectedWaveSource;
		private float m_originalIntensity;
		private float m_targetValue = 0f;
		

		protected void OnEnable(){
			m_selectedWaveSource = GetComponent<WaveSource>();
			m_originalIntensity = m_selectedWaveSource.inputIntensity;
			PickTargetValue();
		}
		protected void OnDisable(){
			RestoreIntensity();
		}

		protected void Update(){
			float currentValue = GetCurrentValue();
			float newValue = CalculateMovedValue(currentValue);
			ApplyNewValue(newValue);
			if (Mathf.Abs(m_targetValue - newValue) < m_touchDistance) PickTargetValue();
				
		}
		
		protected float GetCurrentValue(){
			return m_selectedWaveSource.inputIntensity;
		}
		protected void ApplyNewValue(float newValue){
			m_selectedWaveSource.inputIntensity = newValue;
		}
				
		protected void PickTargetValue(){
			m_targetValue = Random.Range(m_minimumIntensity, m_maximumIntensity);
		}
		
		protected float CalculateMovedValue(float currentValue){
			return Mathf.MoveTowards(m_selectedWaveSource.inputIntensity, m_targetValue, changeSpeed);
		}

		protected void OnValidate(){
			// Feed fields back through the property to apply constraints:
			minimumIntensity = m_minimumIntensity;
			maximumIntensity = m_maximumIntensity;
		}
		
		protected void RestoreIntensity(){
			m_selectedWaveSource.inputIntensity = m_originalIntensity;
		}
		
	}
}