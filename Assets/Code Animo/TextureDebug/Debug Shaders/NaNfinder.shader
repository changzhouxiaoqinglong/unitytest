// Copyright (c) 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Debug/NaNFinder" {
	Properties {
		_MainTex ("AnalysedTexture (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Pass{
			Lighting Off Fog { Mode Off }
			CGPROGRAM
			#pragma fragment frag
			#pragma vertex vert
			#include "UnityCG.cginc"
			#pragma exclude_renderers gles
	
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			void vert(appdata_base vertexInput, out float4 pos : SV_POSITION, out float2 uv : TEXCOORD0){	
				pos = UnityObjectToClipPos(vertexInput.vertex);// Transform to clip space
				uv = TRANSFORM_TEX(vertexInput.texcoord, _MainTex);// Apply texture offsets and scaling.
			}
						
			float4 frag(float4 pos : SV_POSITION, float2 uv : TEXCOORD0) : COLOR{
				float4 mainTex = tex2D(_MainTex, uv);
				float4 outputColor = float4(0,0,0,1);
				#if !defined(SHADER_TARGET_GLSL)
				if (isnan(mainTex.r)) outputColor.r = 1.0f;
				if (isnan(mainTex.g)) outputColor.g = 1.0f;
				if (isnan(mainTex.b)) outputColor.b = 1.0f;
				#endif
				
				outputColor.a = 1.0f;
				
				return outputColor;
			}
			ENDCG
		}
	} 
}
