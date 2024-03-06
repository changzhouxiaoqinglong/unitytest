// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System;
using System.Collections;

namespace CodeAnimo {
	
	public class GridHeightData : MonoBehaviour {
		
		virtual public bool hasData { get { return false; } }
		
		virtual public int maximumU{
			get{ return int.MaxValue; }
		}
		virtual public int maximumV{
			get { return int.MaxValue; }
		}
		
		public event EventHandler HeightDataUpdated;
	
		virtual public float getGridHeight(int u, int v){
			return 0.0f;
		}
		
		public void subscribeToHeightDataUpdated(EventHandler listener){
			HeightDataUpdated -= listener;// Prevent double subscriptions.
			HeightDataUpdated += listener;
		}
		
		public void unsubscribeFromHeightDataUpdated(EventHandler listener){
			HeightDataUpdated -= listener;	
		}
		
		protected void onHeightDataUpdated(EventArgs e){
			if (this.HeightDataUpdated == null) return;
			
			HeightDataUpdated(this, e);
		}
	}
}