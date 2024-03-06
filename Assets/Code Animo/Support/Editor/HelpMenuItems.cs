// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.Support{
	
	public class HelpMenuItems {
		
		[MenuItem("Help/Code Animo/Context Sensitive Help")]
		public static void OpenHelpViewer(){
			EditorWindow.GetWindow<HelpViewer>();
		}
		
		
	}
}