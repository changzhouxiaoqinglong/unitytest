// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Code Animo/Surface Waves/WaveMeshDisplacement" {
	Properties {
		WaveMapIn ("WaveMap", 2D) = "black" {}
		TerrainMapIn ("Terrain Map", 2D) = "black" {}
	}
	SubShader {
		Tags { }
		pass {
			ZTest Always Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D WaveMapIn;
			sampler2D TerrainMapIn;
			
			float4 WaveMapIn_TexelSize;// Automatically filled with texture dimensionInfo

			float _MinWaveHeight;
			
			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// Sharing data with 'calculateWaveVelocity' and syntax sugar
			struct neighbors{
				float Left;
				float Top;
				float Right;
				float Bottom;
			};

			
			v2f vert(appdata_base v){
				v2f output;
				output.position = UnityObjectToClipPos(v.vertex);
				output.uv = v.texcoord;
				return output;
			}
			
			float4 frag(float4 position : SV_POSITION, float2 uv :TEXCOORD0) : COLOR{
				float waveHeight = tex2D(WaveMapIn, uv).b;
				float4 terrain = tex2D(TerrainMapIn, uv);
				
				// Calculate Neighbour coordinates:
				float2 left = uv + float2(-WaveMapIn_TexelSize.x, 0);
				float2 top = uv + float2(0, WaveMapIn_TexelSize.y);
				float2 right = uv + float2(WaveMapIn_TexelSize.x, 0);
				float2 bottom = uv + float2(0, -WaveMapIn_TexelSize.y);

				// Load data from neighbouring cells
				neighbors waveHeights;
				waveHeights.Left	= tex2D( WaveMapIn, left ).b;
				waveHeights.Top		= tex2D( WaveMapIn, top ).b;
				waveHeights.Right	= tex2D( WaveMapIn, right ).b;
				waveHeights.Bottom	= tex2D( WaveMapIn, bottom ).b;

				neighbors terrainHeights;
				waveHeights.Left	+= tex2D( TerrainMapIn, left ).b;
				waveHeights.Top		+= tex2D( TerrainMapIn, top ).b;
				waveHeights.Right	+= tex2D( TerrainMapIn, right ).b;
				waveHeights.Bottom	+= tex2D( TerrainMapIn, bottom ).b;

				// Picking the lowest local value gives smoother terrain transitions in most cases:
				float height = min( waveHeights.Left, min( waveHeights.Top, min( waveHeights.Right, min( waveHeights.Bottom, terrain + waveHeight ) ) ) );
				
				return height;
			}
			
			ENDCG
		}
	} 
}
