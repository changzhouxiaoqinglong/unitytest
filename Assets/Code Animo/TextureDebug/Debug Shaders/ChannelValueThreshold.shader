// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Custom/ChannelValueThreshold" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Threshold ("Threshold", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Pass{
		
			CGPROGRAM
			#pragma fragment frag
			#pragma vertex vert
			#include "UnityCG.cginc"
			
			float _Threshold;
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			void vert(appdata_base vertexInput, out float4 pos : SV_POSITION, out float2 uv : TEXCOORD0){	
				pos = UnityObjectToClipPos(vertexInput.vertex);// Transform to clip space
				uv = TRANSFORM_TEX(vertexInput.texcoord, _MainTex);// Apply texture offsets and scaling.
			}
						
			float4 frag(float4 pos : SV_POSITION, float2 uv : TEXCOORD0) : COLOR{
				float4 mainTex = tex2D(_MainTex, uv);
				float4 outputColor = float4(0,0,0,1);
				if (mainTex.r > _Threshold) outputColor.r = 1;
				if (mainTex.r > _Threshold) outputColor.g = 1;
				if (mainTex.b > _Threshold) outputColor.b = 1;
				
				return outputColor;
			}
			
			
			ENDCG
		}
	} 
}
