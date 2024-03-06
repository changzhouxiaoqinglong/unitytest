// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/DepthTextureVisualizer" {
	Properties {
		_MainTex ("Depth Texture", 2D) = "white" {}
		_Scale ("Depth Scale", Float) = 1
		_Bias ("Depth Bias", Float) = 0
		_CameraNearPlane ("Camera Near Clip Plane", Float) = .3
		_CameraFarPlane ("Camera Far Clip Plane", Float) = 1000
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Fog { Mode Off }
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float _Scale;
			float _Bias;
			float _CameraFarPlane;
			float _CameraNearPlane;
			
			struct v2f{
				float4 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};
			
			v2f vert(appdata_base v){
				v2f o;
				o.uv = v.texcoord;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			float getCustomDepth(float zValue){
				// All this stuff could probably be simplified and moved to the CPU to be calculated once.
				float zBufferParamY = _CameraFarPlane / _CameraNearPlane;
				float zBufferParamX =  1.0 - zBufferParamY;
				
				float zBufferParamZ = zBufferParamX / _CameraFarPlane;
				float zBufferParamW = zBufferParamY / _CameraFarPlane;
				
//				return 1.0 / (zBufferParamZ * zValue + zBufferParamW);// Linear Eye Depth
				return 1.0 / (zBufferParamX * zValue + zBufferParamY);// Linear 01 Depth
			}
			
			float getOrthographicDepth(float zValue){
				return zValue * (_CameraFarPlane - _CameraNearPlane) + _CameraNearPlane;
			}
			
			float4 frag(v2f i) : COLOR{
				float4 cameraDepth = tex2D(_MainTex, i.uv.xy);
				
				float linearDepth = getCustomDepth(cameraDepth.r);
				return float4(linearDepth.rrr * _Scale + _Bias, 1) ;
			}
			
			
			
			ENDCG
		}
	} 
}
