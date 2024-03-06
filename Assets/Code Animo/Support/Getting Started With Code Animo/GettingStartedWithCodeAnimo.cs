// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com
#if UNITY_EDITOR

using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace CodeAnimo.Support{
	
	/// <summary>
	/// Used as Asset in combination with its editor and the GettingStarted attributes, to easily show which products are installed, and how to start using them
	/// </summary>
	public class GettingStartedWithCodeAnimo : ScriptableObject {
		
		public string aboutThisClass = "This file shows you where to find installed tools by Code Animo.\n\n" +
				"Feel free to delete this file when you know where to find things.";
		[NonSerializedAttribute] public List<ProductData> foundProducts = null;
		
		public bool automaticUpdateOnEnable = true;
		
		public void OnEnable(){
			if (automaticUpdateOnEnable) FindProductAttributes();
		}
		
		/// <summary>
		/// Triggers a search for all ProductInfoAttributes in all loaded assemblies.
		/// </summary>
		public void FindProductAttributes(){
			List<ProductData> foundProducts = new List<ProductData>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++) {
				FindAttributesInAssembly(assemblies[i], foundProducts);
			}
			this.foundProducts = foundProducts;
		}
		
		/// <summary>
		/// Iterates over types in the given assembly to find all ProductInfoAttributes
		/// </summary>
		/// <param name='attributeType'>
		/// Attribute type.
		/// </param>
		/// <param name='selectedAssembly'>
		/// The assembly in which the attributes should be found.
		/// </param>
		/// <param name='outputList'>
		/// The list to which all the found attributes should be added.
		/// </param>
		protected void FindAttributesInAssembly(Assembly selectedAssembly, List<ProductData> outputList){
			Type[] typeList = selectedAssembly.GetTypes();
			for (int i = 0; i < typeList.Length; i++) {
				object[] rawAttributes = typeList[i].GetCustomAttributes(typeof(ProductInfoAttribute), false);
				for (int j = 0; j < rawAttributes.Length; j++) {
					ProductData info = new ProductData((ProductInfoAttribute) rawAttributes[j], typeList[i]); 
					outputList.Add( info);
				}
			}
		}
		
	}
}
#endif