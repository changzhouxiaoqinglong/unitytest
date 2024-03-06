// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;
using System.IO;

namespace CodeAnimo{
	
	public class AssetCreation {
		
		/// <summary>
		/// Creates a path string, based on the currently selected object.
		/// Defaults to "Assets" folder if no asset path can be retrieved from selection.
		/// </summary>
		/// <returns>
		/// A string containing the path based on the selected object or folder.
		/// </returns>
		public static string CreateFolderPathNearSelected(){
			string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (assetPath == "") assetPath = "Assets";
			else if (Path.GetExtension(assetPath) != ""){
				assetPath = Path.GetDirectoryName(assetPath);	
			}
			return assetPath;
		}
		
		/// <summary>
		/// Creates a path string to the given asset, based on the selected object or folder.
		/// </summary>
		/// <returns>
		/// A string containing the path based on the selected object or folder.
		/// </returns>
		/// <param name='assetName'>
		/// Name of the asset the path is for.
		/// </param>
		/// <param name='fileExtension'>
		/// File name extension, without leading period.
		/// </param>
		public static string CreateAssetPathNearSelected(string assetName, string fileExtension){
			string directory = CreateFolderPathNearSelected();
			string assetPath = directory + "/" + assetName + "." + fileExtension;
			string fullPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
			
			return fullPath;
		}
		/// <summary>
		/// Creates a path string to the given asset, based on the selected object or folder.
		/// </summary>
		/// <returns>
		/// A string containing the path based on the selected object or folder.
		/// </returns>
		/// <param name='assetName'>
		/// Name of the asset the path is for.
		/// </param>
		public static string CreateAssetPathNearSelected(string assetName){
			return CreateAssetPathNearSelected(assetName, "asset");
		}
		
		public static T CreateAsset<T>(string assetName) where T : ScriptableObject{
			string assetPath = AssetCreation.CreateAssetPathNearSelected(assetName);
			if (assetPath == "") throw new System.IO.DirectoryNotFoundException("The Created path is invalid. Try selecting a different object before asset creation.");
			
			T asset = ScriptableObject.CreateInstance<T>();
			
			AssetDatabase.CreateAsset(asset, assetPath);
			
			EditorUtility.FocusProjectWindow();
			EditorGUIUtility.PingObject(asset);
			Selection.activeObject = asset;
			return asset;
		}
		
	}
}