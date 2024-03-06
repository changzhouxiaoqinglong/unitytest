// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;
using System.IO;

namespace CodeAnimo.SurfaceWaves {
	
	public class SimulationSaver : MonoBehaviour {
		
		public string saveName = "";
		private string nameExtension = ".png";
		public bool attach_date = false;
		
		public ComputeShader simCompute;
		
		public Texture2D savedWaves;
		
		// The number of threads per threadgroup:
		private int warpWidth = 32;
		private int warpHeight = 32;
		
		
		public void saveWaveLevel(){
	//		RenderTexture argb32Water = convertToWritableTexture("Water Level", simulation.lastWaterTexture);
	//		Texture2D writableWater = renderTextureToTexture2D(argb32Water);
	////		writeTex2D(writableWater);// Write to file
	//		if (this.savedWater != null) DestroyImmediate(savedWater);// Clean up old texture, before removing the reference.
	//		savedWater = writableWater;
	//		DestroyImmediate(argb32Water);
			
		}
		
		public void restoreWaveLevel(){
	//		simulation.waterStart = savedWater;	
	//		simulation.initializeSimulation();
		}
		
		/**
		 * Initialize Simulation textures with input textures.
		 **/
		private RenderTexture convertToWritableTexture(string textureName, Texture input){
			int kernelId = simCompute.FindKernel("basicBlit");
			
			RenderTexture resultTexture = createStorageTexture(input.width, input.height, textureName);// Must write to different texture than used for reading.
			
			simCompute.SetTexture(kernelId,"BlitIn", input);
			simCompute.SetTexture(kernelId,"BlitOut", resultTexture);
			
			simCompute.Dispatch(kernelId, input.width / warpWidth, input.height / warpHeight,1);
			
			return resultTexture;
		}
		
		
		private RenderTexture createStorageTexture(int width, int height, string name){
			var storageRT = new RenderTexture(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
			storageRT.name = name;
			storageRT.enableRandomWrite = true;
			storageRT.anisoLevel = 0;
			storageRT.filterMode = FilterMode.Point;
			storageRT.Create();
			
			return storageRT;
		}
		
		
		/**
		 * Converts a 2D RenderTexture to a Texture2D, so that it can be read by readpixels.
		 * Usually results in a grey texture if the rendertexture format is not supported.
		 **/
		public Texture2D renderTextureToTexture2D(RenderTexture original){
			Texture2D resultingTexture = new Texture2D(original.width, original.height, TextureFormat.ARGB32, false, true);
			resultingTexture.name = original.name;
			Rect copyArea = new Rect(0,0, original.width, original.height);
			
			RenderTexture previousActive = RenderTexture.active;
			
			if (original.format == RenderTextureFormat.ARGB32) RenderTexture.active = original;
			else{ 
				throw new System.FormatException("The RenderTexture needs to have the ARGB32 RenderTextureFormat. " + original.format + " found instead.");
			}
			resultingTexture.ReadPixels(copyArea,0,0, true);
			resultingTexture.Apply();
			
			RenderTexture.active = previousActive;
			
			return resultingTexture;
		}
		
		
		
		/**
		 * Actually writes a 2D texture to file.
		 * FIXME: Savename can tamper with save location...
		 */
		public void writeTex2D(Texture2D texture){
			if (!texture){
				Debug.LogWarning("While trying to write a texture to disk, it turned out the texture wasn't there", this);	
				return;
			}
			byte[] pngData = texture.EncodeToPNG();
			
			// Save data to Disk:
			string dataName = "textureDumps/" + saveName;
			if (attach_date) dataName += System.DateTime.Now.ToFileTime();
			dataName += nameExtension;
			
			FileStream fileStore = new FileStream(dataName,FileMode.Create);
			BinaryWriter fileWrite = new BinaryWriter(fileStore);
			fileWrite.Write(pngData);
			fileWrite.Close();
			fileStore.Close();
			Debug.Log("I successfully wrote \"" + saveName + nameExtension +"\" to disk. Location: " + dataName, this);
		}
		
		
		void OnGUI(){
			GUILayout.BeginArea(new Rect(400,200,200,200));
			
			if (GUILayout.Button("Save Sim")) saveWaveLevel();
			if (GUILayout.Button("Restore Sim")) restoreWaveLevel();
				
			GUILayout.EndArea();
		}
		
		
	}
}