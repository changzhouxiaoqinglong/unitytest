// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System;

namespace CodeAnimo.Support{
	
	[AttributeUsage(AttributeTargets.Class)]
	public class ProductInfoAttribute : System.Attribute {
		
		public string productName;
		public string productVersion;
		public string startupInfo;
		public string folderName;
		
		public ProductInfoAttribute(string productName, string startupInfo){
			this.productName = productName;
			this.startupInfo = startupInfo;
			
		}
		
	}
}