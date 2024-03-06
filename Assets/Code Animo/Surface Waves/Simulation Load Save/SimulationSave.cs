// Copyright © 2017 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.GPGPU;

namespace CodeAnimo.SurfaceWaves
{
	public class SimulationSave : MonoBehaviour
	{
		public SurfaceWaves.WaveFlowCompute flowStep;
		public SurfaceWaves.WaveHeightCompute heightStep;

		private ComputeKernel2D simKernel;

		public SimulationTextureData flowData;
		public SimulationTextureData heightData;

		private void Reset()
		{
			this.simKernel = GetComponent<ComputeKernel2D>();
			if (this.simKernel == null) { this.simKernel = gameObject.AddComponent<ComputeKernel2D>(); }

			this.simKernel.kernelName = "readTexture";
		}

		private void Awake()
		{
			this.simKernel = GetComponent<ComputeKernel2D>();
		}

		public void SaveAll()
		{
			if (this.simKernel.SupportedBySystem())
			{
				flowData.pixels = moveTextureToCPU(flowStep.outputData);
				heightData.pixels = moveTextureToCPU(heightStep.outputData);

				if (flowData.pixels != null && heightData.pixels != null)
				{
					Debug.Log("Simulation successfully saved to assets.");
				}
			}
			else { Debug.Log("Compute shaders not supported", this); }
		}

		/// <summary>
		/// Move the floating point pixel data from a RenderTexture on the GPU, to the CPU where it can be manipulated in C#
		/// </summary>
		public Vector4[] moveTextureToCPU(RenderTexture textureToSave)
		{
			if (textureToSave.dimension != UnityEngine.Rendering.TextureDimension.Tex2D)
			{
				Debug.Log("Only 2D texture are supported", this);
				return null;
			}

			int elementCount = textureToSave.width * textureToSave.height;

			Vector4[] pixels = new Vector4[elementCount];
			ComputeBuffer pixelBuffer = new ComputeBuffer(elementCount, 16);

			this.simKernel.SetBuffer("PixelOut", pixelBuffer);
			this.simKernel.SetTexture("TextureToSave", textureToSave);

			this.simKernel.Dispatch();
			pixelBuffer.GetData(pixels);
			pixelBuffer.Release();

			return pixels;
		}

	}

}