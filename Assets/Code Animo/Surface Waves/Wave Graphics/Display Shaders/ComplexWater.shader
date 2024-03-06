// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com
Shader "Code Animo/Surface Waves/Complex Water" {
	Properties {
		_Refraction  ("Distorted Refraction Scale", Float) = 35.0
		
		_FogFilterColor ("Fog Filter Color", Color) = (0.7,0.96, 0.9, 1)
		_FogDiffuseColor ("Fog Diffuse Color", Color) = (0.05,0.14,0.2,1)
		
		_FogViewDist ("Water Visibility Distance", Float) = 25
				
		_BlendDepth ("Soft Shore Blend Depth", float) = 0.1
		
		_FresnelPower ("Fresnel Power", Range(1,5)) = 5
		
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Gloss ("Gloss", Range( 0, 1)) = 0.3
		_Shininess ("Shininess", Range (0.03, 1)) = 1
		
		_LightingWrap ("Lighting Wrap", Range(0,1)) = 1
		
		_FoamPower ("Foam Falloff", Range(1,8)) = 1
		_FoamThreshold("Foam Threshold", Range(0, 1)) = 0
		_FoamColor ("Foam Color", Color) = (0.6, 0.6, 0.56, 1)
		_OverUnderFoamMix ("Over/Under-water foam mix", Range(0,1)) = 0.8
		_MaxFoamDepth ("Maximum Foam Depth", Float) = 20
		
		_ClipHeight ("Clip Height", Float) = 0.001
		_Scale ("Mesh Displacement Scale", Float) = 10.0
		_EdgeULength ("Grid Edge U length (1 / VertexCount)", Float) = 0.0009765625
		_EdgeVLength ("Grid Edge V length (1 / VertexCount)", Float) = 0.0009765625
		/*[HideInInspector]*/_gridSizeU ("GridMesh Edge Size U worldUnits", Float) = 1
		/*[HideInInspector]*/_gridSizeV ("GridSize Edge Size V worldUnits", Float) = 1
		
		_HeightTex ("Heightmap", 2D) = "black" {}
		_WaterData ("WaterData", 2D) = "blue" {}
	}
	
	CGINCLUDE
	sampler2D _HeightTex;
	sampler2D _WaterData;
	sampler2D _CameraDepthTexture;
	
	sampler2D _UnderWater;// Refraction
	
	float4 _HeightTex_ST;
	
	float4 _HeightTex_TexelSize;
	float4 _UnderWater_TexelSize;
	
	float _FoamPower;
	float _FoamThreshold;
	float4 _FoamColor;
	float _OverUnderFoamMix;
	float _MaxFoamDepth;
	
	float4 _FogFilterColor;
	float4 _FogDiffuseColor;
	
	float _FogViewDist;
	float _FogFalloff;
	
	float _FresnelPower;
	
	float _Scale;
	float _EdgeULength;
	float _EdgeVLength;
	float _gridSizeU;
	float _gridSizeV;
	
	float _Refraction;
	
	float _Gloss;
	float _Shininess;
	
	float _LightingWrap;
	
	float _BlendDepth;
	
	float _ClipHeight;
	
	ENDCG
	
	SubShader {// Shader Model 5:
		LOD 800
		
		Tags { "Queue"="Transparent" "RenderType"="TransparentHeightMesh" }
		GrabPass { "_UnderWater" }
		
		
		Pass{
			Name "FORWARD"
			Tags {  "LightMode" = "ForwardBase"}
			Blend SrcAlpha OneMinusSrcAlpha // Alpha Blend for soft shorelines
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma target 5.0
			#pragma exclude_renderers opengl
			
			#pragma multi_compile_fwdbase nolightmap nodirlightmap
			#pragma multi_compile_fog

#ifndef UNITY_PASS_FORWARDBASE
			#define UNITY_PASS_FORWARDBASE
#endif

			#include "ComplexWater.cginc"
			
			ENDCG
		}
		Pass{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardAdd" }
			ZWrite Off Blend One One
			Blend SrcAlpha One
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma target 5.0
			#pragma exclude_renderers opengl
			
			#pragma multi_compile_fwdadd nolightmap nodirlightmap
			#pragma multi_compile_fog

#ifndef UNITY_PASS_FORWARDADD
			#define UNITY_PASS_FORWARDADD
#endif			
			#include "ComplexWater.cginc"
			
			ENDCG
		}
		
	}
	SubShader{// Shader Model 3:
		LOD 800
		GrabPass { "_UnderWater" }
		Tags { "Queue"="Transparent" "RenderType"="TransparentHeightMesh" }
		Pass{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase"  }
			Blend SrcAlpha OneMinusSrcAlpha // Alpha Blend for soft shorelines
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma target 3.0
			#pragma glsl
			
			#pragma multi_compile_fwdbase nolightmap nodirlightmap
			#pragma multi_compile_fog

#ifndef UNITY_PASS_FORWARDADD
			#define UNITY_PASS_FORWARDBASE
#endif
			
			#include "ComplexWater.cginc"
			
			ENDCG
		}
		Pass{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardAdd" }
			ZWrite Off Blend One One
			Blend SrcAlpha One
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma target 3.0
			#pragma glsl
			
			#pragma multi_compile_fwdadd nolightmap nodirlightmap
			#pragma multi_compile_fog

#ifndef UNITY_PASS_FORWARDADD
			#define UNITY_PASS_FORWARDADD
#endif
			
			#include "ComplexWater.cginc"
			
			ENDCG
		}
	}

}