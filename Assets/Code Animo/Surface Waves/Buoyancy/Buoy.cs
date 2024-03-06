// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo.SurfaceWaves{
	
	[AddComponentMenu("Surface Waves/Buoyancy/Buoy")]
	public class Buoy : MonoBehaviour {
		
		public delegate void BuoyEventHandler(Buoy victim);
		public event BuoyEventHandler willBeDestroyed;
		
		public float radius{
			get { return _radius; }
			set {
				this._radius = value;
				if (this.colliderCache != null) this.colliderCache.radius = value;
			}
		}
		
		// Buoyancy manager uses this to calculate the relative position.
		public Vector3 position{
			get { return transformCache.position; } 	
		}
		
		public Vector4 velocityData{
			get {
				Vector3 velocity = affectedObject.velocity;
				return new Vector4(velocity.x, velocity.y, velocity.z, 0);	
			}
		}
		
		public Rigidbody affectedObject{
			get { return m_rigidBodyCache; }
		}
		
		[SerializeField]
		[HideInInspector]
		private float _radius = 1.0f;
		private Transform transformCache;
		[SerializeField] private SphereCollider colliderCache;
		private Rigidbody m_rigidBodyCache;
		
#if UNITY_EDITOR
		public Vector3 lastAppliedForce;
#endif
		
		private string m_noTriggerExceptionMessage{
			get { return "This buoy needs a sphere trigger. If the current GameObject needs a regular collider, try attaching this buoy to a child object instead."; }
		}
		
		protected void Reset(){
			AddMissingComponents();
			
		}
		
		protected void Awake(){
			// If not executeInEditMode:
			if (Application.isPlaying) AddMissingComponents();
		}
		
		protected void OnEnable(){
			this.transformCache = transform;
			if (this.colliderCache == null) throw new MissingComponentException(m_noTriggerExceptionMessage);
			
			this.colliderCache.radius = this.radius;
			
			m_rigidBodyCache = this.colliderCache.attachedRigidbody;
			
			if (this.affectedObject == null) throw new MissingComponentException("Buoy's trigger needs to be attached to a rigidbody");
			else this.affectedObject.WakeUp();
		}
		
		protected void OnDestroy(){
			if (willBeDestroyed != null){
				willBeDestroyed(this);	
			}
		}
		
		public void addWillBeDestroyedHandler(BuoyEventHandler handler){
			willBeDestroyed -= handler;
			willBeDestroyed += handler;		
		}
		public void removeWillBeDestroyedHandler(BuoyEventHandler handler){
			willBeDestroyed -= handler;	
		}
		
		
		public void applyBuoyancy(Vector3 force){	
			this.affectedObject.AddForceAtPosition(force, position,ForceMode.Force);
#if UNITY_EDITOR
			this.lastAppliedForce = force;		
#endif
			
			if ( float.IsNaN(force.x)){
				Debug.Log("Invalid force position: " + transform.position.x + ", " + transform.position.y + ", " + transform.position.z, this);
			}
		}
		
		protected void AddMissingComponents(){
			SphereCollider existingCollider = GetComponent<SphereCollider>();
			if (existingCollider != null){
				if (existingCollider.isTrigger){
					// Copy settings from existing collider:
					this.colliderCache = existingCollider;
					this._radius = this.colliderCache.radius;
				}
			}
			else{
				// Not present yet, create new collider:
				this.colliderCache = gameObject.AddComponent<SphereCollider>();
			}
		
			
			if (this.colliderCache != null){
				this.colliderCache.isTrigger = true;
			}
			else {
				// Incorrect Collider Type:
				throw new System.InvalidOperationException(m_noTriggerExceptionMessage);
			}
			
		}
		
	}
}//Namespace