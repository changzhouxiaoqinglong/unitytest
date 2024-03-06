// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves{
	
	[AddComponentMenu("Surface Waves/Wave Sources/Mouse Wave Source")]
	public class MouseSource : WaveSource {
		
		public MouseHitFinder playerMouseTracker;
		public MouseButton mouseButtonId = MouseButton.Left;
		
		public override void RunStep (){
			if (Input.GetMouseButton((int)mouseButtonId)  && playerMouseTracker.MouseHitSomething()){
				forceUnchangedOutput = false;
				this.transform.position = playerMouseTracker.targetData.point;
			}
			else {
				forceUnchangedOutput = true;
			}

			base.RunStep();
		}
		
		protected override void Reset () {
			gameObject.layer = defaultWaveInputLayer;
			base.Reset ();
		}
		
		protected override void AddMissingComponents () {
			MouseHitFinder mouseTracker = AddComponentIfMissingAndSetup<MouseHitFinder>();
			if (mouseTracker != null){
				mouseTracker.UserCamera = Camera.main;
				mouseTracker.activeLayers -= 1 << defaultWaveInputLayer;
				this.playerMouseTracker = mouseTracker;
			}
			
			base.AddMissingComponents ();
			AddComponentIfMissingAndSetup<SM3Kernel>();
			AddComponentIfMissingAndSetup<Rigidbody>();
			AddComponentIfMissingAndSetup<SphereCollider>();
		}
	
	}
}