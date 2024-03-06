// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System;

namespace CodeAnimo {

	[System.AttributeUsage( System.AttributeTargets.Property | System.AttributeTargets.Field, Inherited=true)]
	public class ReorderableListAttribute : PropertyAttribute {
	

		public ReorderableListAttribute(){

		}
		
	}
}