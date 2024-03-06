// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Debug/TextureAnalysis" {
	Properties {
		_MainTex ("AnalysedTexture (RGB)", 2D) = "white" {}
		_MaskColor ("Mask Color", Color) = (1,1,1,1)
		_EdgeColor ("Texture Edge Color", Color) = (0.5,0.5,0.5,1)
		_Scale ("Scale", Float) = 1
		_Bias ("Bias", Float) = 0
	}
	SubShader {
		Pass{
			Lighting Off Fog { Mode Off }
			CGPROGRAM
			#pragma fragment frag
			#pragma vertex vert
			#include "UnityCG.cginc"
	
			float _Scale;
			float _Bias;
			
			float4 _MaskColor;
			float4 _EdgeColor;
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			void vert(appdata_base vertexInput, out float4 pos : SV_POSITION, out float2 uv : TEXCOORD0){	
				pos = UnityObjectToClipPos(vertexInput.vertex);// Transform to clip space
				uv = TRANSFORM_TEX(vertexInput.texcoord, _MainTex);// Apply texture offsets and scaling.
			}
						
			float4 frag(float4 pos : SV_POSITION, float2 uv : TEXCOORD0) : COLOR{
				float4 mainTex = tex2D(_MainTex, uv) * _MaskColor;
				
				// Visualize Texture edges:
				if (uv.x < 0) mainTex *= _EdgeColor;
				if (uv.y < 0) mainTex *= _EdgeColor;
				if (uv.x > 1) mainTex *= _EdgeColor;
				if (uv.y > 1) mainTex *= _EdgeColor;
				
				
				float4 outputColor = (mainTex + _Bias) * _Scale;// Apply bias first, so that it's absolute value isn't changed by scale.
				
				outputColor.a = 1.0f;
				
				return outputColor;
			}
			ENDCG
		}
	} 
}
