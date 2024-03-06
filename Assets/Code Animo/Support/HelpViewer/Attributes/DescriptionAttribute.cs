// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System;

namespace CodeAnimo{
	
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
	public class DescriptionAttribute : PropertyAttribute {
		
		public float height = 250f;
		
		public DescriptionAttribute(){
		
		}
		
	}
}