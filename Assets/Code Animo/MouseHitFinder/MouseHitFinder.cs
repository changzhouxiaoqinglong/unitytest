// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo{
	
	public class ScreenRayCastData{
		public RaycastHit hitData;
		public Ray usedRay;
		public bool hit;
		public float range;
		public LayerMask activeLayers;
	}
	
	/// <summary>
	/// Mouse hit finder.
	/// </summary>
	public class MouseHitFinder : MonoBehaviour {
		
		public Camera UserCamera;
		public RaycastHit targetData;
		[Range (0,10000)]
		public float range = 10000;
		public LayerMask activeLayers = -1;
		
		private Ray lastHitRay;
		private bool m_didLastCallHit = false;
		
		public bool MouseHitSomething(){
			if (UserCamera != null){
				Ray decalRay = UserCamera.ScreenPointToRay(Input.mousePosition);// Ray from camera forward
				m_didLastCallHit = Physics.Raycast(decalRay, out targetData, range,activeLayers.value);
				if (m_didLastCallHit) lastHitRay = decalRay;
				
				return m_didLastCallHit;
			}
			else{
				// Default exception isn't logged with reference to the correct object, so log from here:
				Debug.LogException(new MissingReferenceException("No Camera Selected by MouseHitFinder."), this);
				return false;
			}
		}
		
		public ScreenRayCastData CastScreenRay(Vector3 screenPoint, Camera viewport){
			ScreenRayCastData rayData = new ScreenRayCastData();
			rayData.usedRay = viewport.ScreenPointToRay(screenPoint);// Ray from camera forward
			rayData.activeLayers = activeLayers;
			rayData.range = range;
			rayData.hit = Physics.Raycast(rayData.usedRay, out rayData.hitData, range, activeLayers.value);
			return rayData;
		}
		
		public void OnDrawGizmosSelected(){
			if (m_didLastCallHit) Gizmos.DrawLine(lastHitRay.origin, targetData.point);
		}
		
	}
	
	
}