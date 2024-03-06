// Copyright © 2017 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using CodeAnimo.GPGPU;
using System;

namespace CodeAnimo.SurfaceWaves
{
	public class SimulationLoad : SimulationStep
	{
		public SimulationSave dataSource;
		private ComputeKernel2D simKernel;

		private void Reset()
		{
			this.simKernel = GetComponent<ComputeKernel2D>();
			if (this.simKernel == null) { this.simKernel = gameObject.AddComponent<ComputeKernel2D>(); }

			this.simKernel.kernelName = "loadTexture";
		}

		private void Awake()
		{
			this.simKernel = GetComponent<ComputeKernel2D>();
		}

		public override void LoadData()
		{
			LoadAll();
		}
		public override void RunStep()
		{
			// Nothing happens during regular runs
		}

		public void LoadAll()
		{
			if (this.simKernel.SupportedBySystem())
			{
				moveToGPU(dataSource.flowData.pixels, dataSource.flowStep.outputData);
				moveToGPU(dataSource.heightData.pixels, dataSource.heightStep.outputData);
			}
			else { Debug.Log("Compute shaders not supported", this); }
		}

		/// <summary>
		/// Move the data from the CPU to the GPU, where it can be used by shaders.
		/// </summary>
		public void moveToGPU(Vector4[] pixels, RenderTexture target)
		{
			if (target.dimension != UnityEngine.Rendering.TextureDimension.Tex2D)
			{
				Debug.Log("Only 2D texture are supported", this);
				return;
			}
			int elementCount = target.width * target.height;
			ComputeBuffer pixelBuffer = new ComputeBuffer(elementCount, 16);
			pixelBuffer.SetData(pixels);

			this.simKernel.SetBuffer("PixelIn", pixelBuffer);
			this.simKernel.SetTexture("TargetTexture", target);

			this.simKernel.Dispatch();
			pixelBuffer.SetData(pixels);
			pixelBuffer.Release();
		}

	}

}