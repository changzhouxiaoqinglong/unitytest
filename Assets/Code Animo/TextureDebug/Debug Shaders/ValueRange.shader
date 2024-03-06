// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Debug/ValueRange" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MinValue ("Minimum Value", Float) = 0
		_ValueRange ("Range", Float) = 1
	}
	SubShader {
		Pass{
			Lighting Off Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _MinValue;
			float _ValueRange;
			
			void vert(appdata_base vertexInput, out float4 pos : SV_POSITION, out float2 uv : TEXCOORD0){	
				pos = UnityObjectToClipPos(vertexInput.vertex);// Transform to clip space
				uv = TRANSFORM_TEX(vertexInput.texcoord, _MainTex);// Apply texture offsets and scaling.
			}
						
			float4 frag(float4 pos : SV_POSITION, float2 uv : TEXCOORD0) : COLOR{
				float4 mainTex = tex2D(_MainTex, uv);
				float4 outputColor = (mainTex - _MinValue) / _ValueRange;
				
				outputColor.a = 1.0f;
				
				return outputColor;
			}
		
		ENDCG
		}
	} 
}
