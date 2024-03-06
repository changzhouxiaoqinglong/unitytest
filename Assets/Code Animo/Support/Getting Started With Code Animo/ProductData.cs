// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


namespace CodeAnimo.Support{
	
	public class ProductData {
	
		public Type registeredType;
		public string productName;
		public string productVersion;
		public string startupInfo;
		
//		private string m_folderName;
		
		public ProductData(ProductInfoAttribute attribute, Type attachedType){
			this.registeredType = attachedType;
			this.productName = attribute.productName;
			this.productVersion = attribute.productVersion;
			this.startupInfo = attribute.startupInfo;
//			m_folderName = attribute.folderName;
		}
		
		public MonoScript GetMonoScript(){
			if (typeof(MonoBehaviour).IsAssignableFrom(registeredType)){
				GameObject temporaryInstance = new GameObject("Temporary Instance");
				MonoBehaviour temporaryComponent = (MonoBehaviour) temporaryInstance.AddComponent(registeredType);
				MonoScript asset = MonoScript.FromMonoBehaviour(temporaryComponent);
				
				ScriptableObject.DestroyImmediate(temporaryInstance);
				return asset;
			}
			else if (typeof(ScriptableObject).IsAssignableFrom(registeredType) ){
				ScriptableObject temporaryInstance = ScriptableObject.CreateInstance(registeredType);
				MonoScript asset = MonoScript.FromScriptableObject(temporaryInstance);
				
				ScriptableObject.DestroyImmediate(temporaryInstance);
				return asset;
			}
			else{
				throw new InvalidOperationException("Registered product type must be either MonoBehaviour or ScriptableObject");
			}
		}
		
		public string GetDirectory(){
			MonoScript script = GetMonoScript();
			string assetPath = AssetDatabase.GetAssetPath(script);
			ScriptableObject.DestroyImmediate(script);
			return assetPath;
		}
		
//		public string FindDirectoryAbovePath(string path, string directoryName){
//			if (directoryName == "") return path;
			
//			string parentDirectory = path;
			
//			parentDirectory = System.IO.Path.GetDirectoryName(parentDirectory);
//			if (parentDirectory.EndsWith(directoryName));

//		}
		
	}
}
#endif