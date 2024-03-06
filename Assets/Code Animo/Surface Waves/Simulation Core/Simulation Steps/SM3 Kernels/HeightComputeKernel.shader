// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Code Animo/Surface Waves/HeightComputeKernel" {
	Properties {
		WaveMapIn ("Wave Heightmap", 2D) ="black" {}
		AddedWavesMap ("Wave Input/Output", 2D) = "black" {}
		FlowMapIn ("Flow map", 2D) = "black" {}
		FoamDecay ("Foam Decay", Range(0, 0.99)) = 0.5
		FoamMultiplier ("Foam Multiplier", Float) = 100
	}
	SubShader {
		Tags { }
		Pass{
			ZTest Always Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
	
			sampler2D WaveMapIn;
			sampler2D AddedWavesMap;
			sampler2D FlowMapIn;
			
			float FoamDecay;
			float FoamMultiplier;
			
			float4 FlowMapIn_ST;// Automatically filled with scale/bias data
			float4 WaveMapIn_TexelSize;// Automatically filled with texture dimensionInfo
			
			void vert (appdata_base v, out float4 pos:SV_POSITION, out float2 uv:TEXCOORD0){
				pos = UnityObjectToClipPos(v.vertex);
				uv = TRANSFORM_TEX (v.texcoord, FlowMapIn);// Can't we use the raw input uv coordinates? These textures are set by script, without changing bias and scale.
			}
			
			
			// Sharing data with 'calculateWaveVelocity' and syntax sugar
			struct waveFlow{
				float Left;
				float Top;
				float Right;
				float Bottom;
			};
			
			float2 calculateWaveVelocity(float currentHeight, float previousHeight, waveFlow flowFrom, waveFlow flowTo){
				float2 velocity = float2(0,0);
				float avgHeight = 0.5 * (previousHeight + currentHeight);// Average height of this tile, from two frames.
			    
			    if (avgHeight > 0.0){// We'll dividing by avHeight soon
			        // Calculate the amount of waves flowing through this column:
			        float xAmount = ( (flowFrom.Left - flowTo.Left) + (flowTo.Right - flowFrom.Right) ) * 0.5;
			        float yAmount = ( (flowFrom.Bottom - flowTo.Bottom) + (flowTo.Top - flowFrom.Top) ) * 0.5;// In buoyancy sim, it seemed as if this was reversed.
			        velocity.x = clamp(xAmount / avgHeight, -10, 10);// x component of velocity, prevent overflow
			        velocity.y = clamp(yAmount / avgHeight, -10, 10);// y component of velocity, prevent overflow
			    }
				return velocity;
			}
			
			float4 frag(float4 pos:SV_POSITION, float2 uv:TEXCOORD0):COLOR{
				float4 newWave = float4(0,0,0,0);
				
				// Calculate Neighbour coordinates:
				float2 left		= uv + float2(-WaveMapIn_TexelSize.x,0);
				float2 top		= uv + float2(0,WaveMapIn_TexelSize.y);
				float2 right	= uv + float2(WaveMapIn_TexelSize.x, 0);
				float2 bottom	= uv + float2(0,-WaveMapIn_TexelSize.y);			
			
				// Load data for local cell:
				float4 sampledWaveHeight	= tex2D(WaveMapIn, uv);
				float4 outFlow 				= tex2D(FlowMapIn, uv);
				float2 addedWave			= tex2D(AddedWavesMap, uv).rg;// Wave Sources and sinks
				
				// Load data from neighbouring cells:
		    	waveFlow flowFrom;
				flowFrom.Left	= tex2D(FlowMapIn,	left	).b;// B Channel = Right
				flowFrom.Top	= tex2D(FlowMapIn,	top		).a;// A Channel = Bottom
			    flowFrom.Right	= tex2D(FlowMapIn,	right	).r;// R channel = Left
			    flowFrom.Bottom	= tex2D(FlowMapIn,	bottom	).g;// G Channel = Top
				
				// It's easier for humans to read and correct flowToLeft than outFlow.r
				waveFlow flowTo;
				flowTo.Left		= outFlow.r;
				flowTo.Top		= outFlow.g;
				flowTo.Right	= outFlow.b;
				flowTo.Bottom	= outFlow.a;
				
				float previousWaveHeight = sampledWaveHeight.b;
				
				// Don't try to get flux from outside the grid:
				if (left.x		< 0.0)	flowFrom.Left = 0.0;
				if (top.y		> 1.0)	flowFrom.Top = 0.0;
				if (right.x		> 1.0)	flowFrom.Right = 0.0;
				if (bottom.y	< 0.0)	flowFrom.Bottom = 0.0;
				
				// Calculate total amount of in and out flux:
			    float totInFlow		= flowFrom.Left + flowFrom.Top + flowFrom.Right + flowFrom.Bottom + addedWave.g;
			    float totOutFlow	= flowTo.Left + flowTo.Top + flowTo.Right + flowTo.Bottom + addedWave.r;
			    totOutFlow			= min(totOutFlow, previousWaveHeight + totInFlow);// Avoid removing more wave height than available.
			    
			    // Update wave Height:
			    newWave.b = previousWaveHeight + (totInFlow - totOutFlow);
				
				// Velocity: 
				newWave.rg = calculateWaveVelocity(newWave.b, previousWaveHeight, flowFrom, flowTo);
				
				// Foam:
				float shockFoam		= abs(totInFlow - totOutFlow);
				float previousFoam	= sampledWaveHeight.a * FoamDecay;// Left over from last frame
				
				shockFoam *= abs(newWave.r) + abs(newWave.g);
				shockFoam *= FoamMultiplier;
				
				newWave.a = saturate(previousFoam + shockFoam);// Foam in Alpha channel
				
				// Write output:
				return newWave;
			}
			
			ENDCG
		}
	} 
}
