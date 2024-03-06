// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.UnityExtensionMethods;

namespace CodeAnimo{
	
	public class CameraFreeFlight : MonoBehaviour {
		
		public float movementSpeedOrAcceleration = 100.0f;
		public Vector2 mouseSensitivity = Vector2.one;
		public float rollCorrectionSpeed = 0.3f;
		
		protected Vector3 yawAxis = Vector3.up;
		
		public MouseButton rotationMouseButton = MouseButton.Right;
		public string sidewaysAxis = "Horizontal";
		public string forwardsAxis = "Vertical";
		public string verticalAxis = "Jump";
		public string mouseXAxis = "Mouse X";
		public string mouseYAxis = "Mouse Y";
		
		public Vector3 upVector = Vector3.up;
		
		
		
		[SerializeField] protected bool m_invertMouseX = false;
		[SerializeField] protected bool m_invertMouseY = true;
		
		public bool smoothedMovement = true;
		public bool smoothedRotation = true;
		public bool rollCorrection = true;// The angular momentum that is built up during a sharp turn can create unwanted roll.
		
		[SerializeField][HideInInspector] private float m_mouseXModifier = 1;
		[SerializeField][HideInInspector] private float m_mouseYModifier = -1;
		
		private Rigidbody m_cachedRigidBody;
		
		private Vector3 m_TranslationAccelerationDirection = Vector3.zero;
		private float m_YawPerUpdate = 0;
		private float m_tiltPerUpdate = 0;
		private int m_framesSinceUpdate = 0;
		
		public bool invertMouseX{
			get { return m_invertMouseX; }
			set {
				m_invertMouseX = value;
				if (m_invertMouseX) m_mouseXModifier = -1;
				else m_mouseXModifier = 1;
			}
		}
		public bool invertMouseY{
			get { return m_invertMouseY; }
			set {
				m_invertMouseY = value;
				if (m_invertMouseY) m_mouseYModifier = -1;
				else m_mouseYModifier = 1;
			}
		}
		protected float mouseXModifier{ get { return this.mouseSensitivity.x * m_mouseXModifier; } }
		protected float mouseYModifier{ get { return this.mouseSensitivity.y * m_mouseYModifier; } }
		
		protected void OnValidate(){
			invertMouseX = m_invertMouseX;
			invertMouseY = m_invertMouseY;
		}
		
		protected void Reset(){
			Rigidbody createdRigidbody = gameObject.AddComponentIfMissing<Rigidbody>();
			if (createdRigidbody != null){
				createdRigidbody.useGravity = false;
				createdRigidbody.drag = 1;
				createdRigidbody.angularDrag = 6;
			}
			this.mouseSensitivity = new Vector2(4,4);
			this.movementSpeedOrAcceleration = 100f;
		}
		
		protected void OnEnable(){
			m_cachedRigidBody = GetComponent<Rigidbody>();
		}
		
		protected void Update(){
			m_TranslationAccelerationDirection = GetMovementDirection();
			// Only rotate the mouse if right mouse button is pressed:
			if (Input.GetMouseButton((int)rotationMouseButton)){
				ProcessRotationInput();
//				Screen.showCursor = false;
			}
		}
		
		protected void FixedUpdate(){
			MoveCamera();
			RotateCamera();
			
			if (rollCorrection) RemoveRoll(this.upVector, this.rollCorrectionSpeed);
		}
		
		protected virtual void MoveCamera(){
			Vector3 relativeVelocity = m_TranslationAccelerationDirection;
			if (this.smoothedMovement){
				m_cachedRigidBody.AddRelativeForce(this.movementSpeedOrAcceleration * relativeVelocity, ForceMode.Acceleration);
			}
			else {
				m_cachedRigidBody.velocity = this.movementSpeedOrAcceleration * this.transform.TransformDirection(relativeVelocity);
			}			
		}
		
		protected void ProcessRotationInput(){
			m_YawPerUpdate += mouseXModifier * Input.GetAxisRaw(mouseXAxis);
			m_tiltPerUpdate += mouseYModifier * Input.GetAxisRaw(mouseYAxis);
			m_framesSinceUpdate++;
		}
		
		protected virtual void RotateCamera(){			
			// calculate average input
			if (m_framesSinceUpdate > 1){
				m_YawPerUpdate /= m_framesSinceUpdate;
				m_tiltPerUpdate /= m_framesSinceUpdate;
			}
			
			float yawAmount = m_YawPerUpdate;
			float tiltAmount = m_tiltPerUpdate;
			m_YawPerUpdate = 0;
			m_tiltPerUpdate = 0;
			m_framesSinceUpdate = 0;
			
			if (smoothedRotation){
				// Fixme: can't use force because the angular moment would change the horizon.
				
				m_cachedRigidBody.AddTorque(new Vector3(0, yawAmount, 0),ForceMode.Acceleration);
				m_cachedRigidBody.AddRelativeTorque(new Vector3(tiltAmount, 0, 0), ForceMode.Acceleration);
			}
			else{
				Quaternion yaw = Quaternion.AngleAxis(yawAmount, yawAxis);
				Quaternion tilt = Quaternion.AngleAxis(tiltAmount, Vector3.right);
				m_cachedRigidBody.rotation = yaw * m_cachedRigidBody.rotation * tilt;
				m_cachedRigidBody.angularVelocity = new Vector3(0,0,0);
			}
								
		}
		
		/// <summary>
		/// Removes Roll over time.
		/// </summary>
		protected void RemoveRoll(Vector3 upVector, float correctionSpeed){
			upVector.Normalize();
			Quaternion currentRotation = m_cachedRigidBody.rotation;
			
			Vector3 roll = currentRotation * Vector3.right;// The vector that should remain horiontal.
			Vector3 correctionAxis = Vector3.Cross(roll, upVector);// The axis along which the roll vector can be leveled.
			Vector3 desiredRotation = Vector3.Cross(upVector, correctionAxis);// What the roll vector should be.
			
			float correctionAngle = Vector3.Angle(roll, desiredRotation);// How big the error is.
			correctionAngle *= Mathf.Sign(Vector3.Cross(roll, desiredRotation).z) * Mathf.Sign(desiredRotation.x);// Direction of the roll
			
			m_cachedRigidBody.AddTorque(correctionAxis * correctionAngle * correctionSpeed, ForceMode.Acceleration);
		}
		
		protected Vector3 GetMovementDirection(){
			Vector3 rawInput = new Vector3(Input.GetAxisRaw(this.sidewaysAxis), Input.GetAxisRaw(this.verticalAxis), Input.GetAxisRaw(this.forwardsAxis));
			rawInput.Normalize();
			return rawInput;
		}
		
		protected void OnDrawGizmosSelected(){
			if (m_cachedRigidBody == null) return;
			Vector3 objectPosition = m_cachedRigidBody.position;
			
			Gizmos.DrawLine(objectPosition, objectPosition + this.upVector);
			
			
		}
		
	}
}