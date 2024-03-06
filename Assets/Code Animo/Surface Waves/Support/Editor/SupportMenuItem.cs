// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using UnityEditor;

namespace CodeAnimo.Support{
	
	public class SupportMenuItem {
		
		public static string urlForReportingSurfaceWavesIssues = "https://bitbucket.org/codeanimo/surface-waves/issues";
		
		[MenuItem("Help/Code Animo/Surface Waves/Report Issue (web)")]
		public static void ReportSurfaceWavesIssue(){
			Help.BrowseURL(urlForReportingSurfaceWavesIssues);
		}
		
		
	}
}