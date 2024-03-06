// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves {
	
	public class BufferSaveDepthMap : DepthmapSaver {
		
		public ComputeKernel textureToBuffer;
		public ComputeKernel bufferToTexture;
		
		private Vector4[] pixelArray;
		
		public override bool dataStored {
			get {
				if (pixelArray == null) return false;
				else return true;
			}
		}
		
		public override void ReadDepthMap(RenderTexture depthMap) {		
			int elementCount = depthMap.width * depthMap.height;
			ComputeBuffer outputBuffer = new ComputeBuffer(elementCount, 16);
			
			this.textureToBuffer.SetTexture("DepthTextureIn", depthMap);
			this.textureToBuffer.SetBuffer("DepthBufferOut", outputBuffer);
			
			this.textureToBuffer.Dispatch();
			
			
			Vector4[] pixelArray = new Vector4[elementCount];
			
			outputBuffer.GetData(pixelArray);
			outputBuffer.Dispose();
			
			this.pixelArray = pixelArray;
		}
		
		public override void WriteDepthMap(RenderTexture depthMap){
			if (!this.dataStored) throw new System.NullReferenceException("Trying to set a depth map, but no pixel data is stored.");
			
			ComputeBuffer inputBuffer = new ComputeBuffer(pixelArray.Length, 16);
			inputBuffer.SetData(pixelArray);
			
			this.bufferToTexture.SetBuffer("DepthBufferIn", inputBuffer);
			this.bufferToTexture.SetTexture("DepthTextureOut", depthMap);
			
			this.bufferToTexture.Dispatch();
			
			
			inputBuffer.Dispose();
			
		}
		
	}
}