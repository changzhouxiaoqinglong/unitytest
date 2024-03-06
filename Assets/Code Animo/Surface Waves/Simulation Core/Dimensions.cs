// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;

namespace CodeAnimo.SurfaceWaves {
	
	/// <summary>
	/// Used to indicate Wave Simulation boundaries, based on the position of the GameObject it's attached to.
	/// Unlike bounds, this is not axis aligned, and should be able to rotate.
	/// </summary>
	[AddComponentMenu("Surface Waves/Dimensions")]
	public class Dimensions : MonoBehaviour {
		
#if UNITY_EDITOR
		[HideInInspector] public CodeAnimo.Support.Article componentDescription;
#endif
		
		[SerializeField] private Vector3 m_localExtends = new Vector3(256, 64, 256);
		
		[SerializeField] protected int m_resolutionX = 512;
		public int resolutionX{
			get { return m_resolutionX; }
			set {
				if (value > 0){
					m_resolutionX = value;
					UpdateTriggerDimensions();
				}
				else{
					m_resolutionX = 1;
					throw new System.ArgumentOutOfRangeException(this.resolutionTooLowMessage);
				}
			}
		}
		[SerializeField] private int m_resolutionZ = 512;
		public int resolutionZ{
			get { return m_resolutionZ; }
			set { 
				if (value > 0){
					m_resolutionZ = value;
					UpdateTriggerDimensions();
				}
				else{
					m_resolutionZ = 1;
					throw new System.ArgumentOutOfRangeException(this.resolutionTooLowMessage);
				}
			}
		}
			
		private BoxCollider m_influenceTrigger;
		private Transform m_cachedTransformReference;
		
		private string resolutionTooLowMessage{
			get { return "Resolution must be higher than 0"; }
		}
		
		protected void OnEnable(){
			CacheTransformReference();
			FindTriggerOnGameObject();
			
		}
		
		protected void OnValidate(){
			// Throw values through property for validation:
			localExtends = m_localExtends;
			resolutionX = m_resolutionX;
			resolutionZ = m_resolutionZ;
		}
		
		protected void FindTriggerOnGameObject(){
			m_influenceTrigger = GetComponent<BoxCollider>();
			if (m_influenceTrigger != null) m_influenceTrigger.isTrigger = true;
			// This Component doesn't add BoxCollider itself,
			// because it keeps the most important components of the main simulation object at the top.
			// It does need that box collider, so warn the developer if it is missing:
			else throw new MissingComponentException("Box collider trigger required");
			
		}
		
		protected void UpdateTriggerDimensions(){
			if (m_influenceTrigger == null) FindTriggerOnGameObject();
			m_influenceTrigger.size = localSize;
			m_influenceTrigger.center = new Vector3(0, localExtends.y, 0);
		}
		
		public Vector3 localExtends{
			get { return m_localExtends; }
			set {
				if (value != m_localExtends){
					m_localExtends = value;
					UpdateTriggerDimensions();
				}
			}
		}
		
		/// <summary>
		/// Returns a reference to the transform component on the same GameObject.
		/// This reference is cached OnEnable, or if the reference isn't cached yet when you call this method.
		/// </summary>
		protected Transform cachedTransformReference{
			get {
				if (m_cachedTransformReference == null) CacheTransformReference();
				return m_cachedTransformReference;				
			}
		}
		
		public Vector3 localSize{
			get { return 2 * localExtends; }
			set{
				localExtends = 0.5f * value;
			}
		}
		
		public Vector3 localFirstCorner{
			get { return new Vector3(-localExtends.x, 0, -localExtends.z); }
		}
		
		/// <summary>
		/// The position of the corner where simulation coordinates are lowest.
		/// The Bottom Left corner in the front.
		/// </summary>
		public Vector3 firstCorner{
			get {
				return cachedTransformReference.TransformPoint(localFirstCorner);				
			}
		}
		
		/// <summary>
		/// The center of the simulation in local space.
		/// Which is above and around the transform of this object.
		/// (Unlike regular bounding boxes, not below)
		/// </summary>
		public Vector3 localCenter{
			get {
				return new Vector3(0, localExtends.y, 0);
			}
		}
		/// <summary>
		/// The center of the simulation in world space.
		/// Which is above and around the transform of this object.
		/// (Unlike regular bounding boxes, not below)
		/// </summary>
		public Vector3 center{
			get { 
				return this.cachedTransformReference.TransformPoint(localCenter);
			}
		}
		
		
		/*public void OnDrawGizmos(){
			// Temporarily move to local space:
			Matrix4x4 previousMatrix = Gizmos.matrix;
			Gizmos.matrix = cachedTransformReference.localToWorldMatrix;
			
			Gizmos.DrawWireCube(this.localCenter, this.localSize);
			
			Gizmos.matrix = previousMatrix;// Restore state
		}*/
		
		/// <summary>
		/// Saves a reference to the Transform component attached to the same GameObject.
		/// This needs to be called whenever this component is added to a different GameObject.
		/// </summary>
		protected void CacheTransformReference(){
			m_cachedTransformReference = transform;
		}
		
	}
}