// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Code Animo/Surface Waves/WaveInput" {
	Properties {
		xLoc ("X", Range(0, 1)) = 0.5
		yLoc ("Y", Range(0, 1)) = 0.5
		
		InputShape ("Input Shape", 2D) = "white" {}
		WaveMapIn ("Previous Input", 2D) = "black" {}
		Intensity ("Intensity", Range(-1, 1)) = 0.5
		SizeRatio ("SizeRatio", Range(0, 1)) = 0.1
	}
	SubShader {
		Tags { }
		
		Pass{
			ZTest Always Fog { Mode Off }
			CGPROGRAM
			#pragma target 3.0
			#pragma fragment frag
			#pragma vertex vert
			#include "UnityCG.cginc"
			
			float xLoc;
			float yLoc;
			
			sampler2D InputShape;
			sampler2D WaveMapIn;
			float Intensity;
			float SizeRatio;
			
			
			void vert(appdata_base v, out float2 uv : TEXCOORD0, out float4 pos : SV_POSITION){
				uv = v.texcoord;
				pos = UnityObjectToClipPos( v.vertex);
			}
			
			float4 frag(float2 uv : TEXCOORD0) : COLOR{
				// transform the coordinates to scale and translate the input shape:
				float2 inputUv = (float2(xLoc, yLoc) - uv) / SizeRatio + float2(0.5, 0.5);
				
				float height = tex2D(InputShape, inputUv).r * Intensity;
				height *= inputUv.x >= 0 && inputUv.x <=1 && inputUv.y >= 0 && inputUv.y <=1;// No texture wrapping.
				
				float4 previousInput = tex2D(WaveMapIn, uv);
				
				return previousInput + float4(-min(0, height), max(0, height), 0,0);
			}
					
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
