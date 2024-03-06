// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Code Animo/Surface Waves/WaveHeightOffsetKernel" {
	Properties {
		GroundDepth ("Ground Depth", 2D) = "black" {}
		DisplaceDepth( "Displace Depth", 2D ) = "black" {}
		WaveMapIn( "Wave Map Input", 2D ) = "black" {}
		
		customNearClip ("Custom Near Clip Plane Distance", Float) = 0
		customFarClip ("Custom Far Clip Plane Distance", Float) = 0
		
		groundDepthOffset ("Ground Depth Offset", Float) = -1
		groundDepthScale ("Ground Depth Scale", Float) = 10
	}
	SubShader {
		Tags { }
		
		Pass{
			ZTest Always Fog { Mode Off }
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D GroundDepth;
			sampler2D DisplaceDepth;
			sampler2D WaveMapIn;
			
			float customNearClip;
			float customFarClip;
			
			float groundDepthOffset;
			float groundDepthScale;
			
			float4 GroundDepth_ST;// Required for TRANSFORM_TEX. Automatically filled with scale/bias data
			
			void vert( appdata_base v, out float2 uv : TEXCOORD0, out float4 pos : SV_POSITION){
				uv = v.texcoord;
				pos = UnityObjectToClipPos(v.vertex);			
			}
			
			float getOrthographicDepth(float zValue){
				#if(defined(UNITY_REVERSED_Z))
					zValue = 1.0 - zValue;
				#endif
				return zValue * (customFarClip - customNearClip) + customNearClip;
			}
			
			float4 frag(float2 uv : TEXCOORD0, float4 pos:SV_POSITION) : Color{
				float2 mirroredUv = uv;
				mirroredUv.y = 1 - uv.y;// Mirror UV, because camera switched back and front.
				
				float groundHeight = (getOrthographicDepth( tex2D( GroundDepth, mirroredUv ).r ) + groundDepthOffset) / groundDepthScale;// Bottom Up
				float displaceHeight = (getOrthographicDepth( tex2D( DisplaceDepth, mirroredUv ).r ) + groundDepthOffset) / groundDepthScale;
				float waterDepth = tex2D( WaveMapIn, uv ).b;// Top Down
				
				return float4(groundHeight, max(0, groundDepthScale * (groundHeight + waterDepth > displaceHeight)),0,1);
			}
			
			
			ENDCG
		}
	} 
}
