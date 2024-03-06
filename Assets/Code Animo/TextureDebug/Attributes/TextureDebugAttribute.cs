// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;
namespace CodeAnimo{
	[System.AttributeUsage( System.AttributeTargets.Property | System.AttributeTargets.Field, Inherited=true)]
	public class TextureDebugAttribute : PropertyAttribute {
		
		public bool inputBox = true;
		public bool materialSelector = false;
		public bool openInViewerButton = false;
		public float previewWidth = 200f;
		
		public TextureDebugAttribute(){
			
		}
		
	}
}