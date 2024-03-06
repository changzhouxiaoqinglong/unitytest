// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Code Animo/Surface Waves/OpaqueFluid" {
	Properties {
		_HeightTex ("Heightmap", 2D) = "black" {}
		_WaterData ("WaterData", 2D) = "black" {}
		
		_FogColor ("Fog Diffuse Color", Color) = (0.05,0.14,0.2,1)
		
		_DiffuseStrength ("Diffuse Strength", Range(0,1)) = 0
		_EmissionStrength ("Emission Strength", Float) = 1
		
		_ReflectionCube ("Reflection Cube", Cube) = "" {}
		_FresnelPower ("Fresnel Power", Range(1,5)) = 5
		
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Gloss ("Gloss", Range( 0, 1)) = 0.3
		_Shininess ("Shininess", Range (0.03, 1)) = 1
		
		_Scale ("Mesh Displacement Scale", Float) = 10.0
	}
	
	CGINCLUDE
		#include "HeightmapSurface.cginc"

		sampler2D _HeightTex;
		sampler2D _WaterData;
		samplerCUBE _ReflectionCube;
		
		float4 _HeightTex_TexelSize;
		
		float4 _FogColor;
		
		float _DiffuseStrength;
		float _EmissionStrength;
		
		float _FresnelPower;
		
		float _Scale;
		
		float _Gloss;
		float _Shininess;
		
		struct Input {
			float2 uv_WaterData;
			float3 worldNormal;
			float3 viewDir;
		};
		
		struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
        };
		
		
		//=================Vertex Shader=============================
		
		
		float2 surfVertTransformTex(float2 texcoord){
			// Calculate mesh coordinates, 
			// Can't use TRANSFORM_TEX with vertex shaders used with surface shaders
			// since _HeightTex_ST does not seem to be available yet at this point in the shader
			// And setting  _HeightTex_ST gives problems when it is used later.
			// Does not actually change the texture coordinates.
			float4 customUVTransform = float4(1,1,0,0);
			return texcoord.xy * customUVTransform.xy + customUVTransform.zw;
			
//			float2 uv = TRANSFORM_UV(0);// apply the appropriate uv offset and tiling. (Only works if the uv for this thing is the first. GrabPass might use first uv, for example.)		
		}
										
		void WaterVert (inout appdata v, out Input o){			
			UNITY_INITIALIZE_OUTPUT(Input,o);// Input Initialization required for DirectX11
			
			float2 uv = surfVertTransformTex(v.texcoord.xy);
			
			float surfaceHeight = vertexTextureLookup(_HeightTex, uv).r;
			
			
			float vertexOffset = (surfaceHeight) * _Scale;
		    v.vertex.y += vertexOffset;
		    v.normal = calcGridNormal(_HeightTex, uv, _HeightTex_TexelSize.xy, float3(1, _Scale, 1), surfaceHeight.r);// Store normal for lighting calculations
		}
		
		
//==============Surface Shader====================

		void surf (Input IN, inout SurfaceOutput o) {
			float3 normViewDir = normalize(IN.viewDir);
			float3 normWorldNormal = normalize(IN.worldNormal);
			
			float fresnel = saturate( dot(normWorldNormal, normViewDir) );// Control mix between effects from above and below water, based on viewing angle
			fresnel = pow(1 - fresnel, _FresnelPower);// Adjust falloff
			
			float4 waterData = tex2D(_WaterData, IN.uv_WaterData);
			clip (waterData.b - 0.001);// Prevent Z fighting with terrain at low water height.
			
			float3 reflectionDir = reflect(-normViewDir, normWorldNormal);// Custom reflection vector calculation.
			float3 reflection = texCUBE(_ReflectionCube, reflectionDir).rgb;
			
			// Mix over and underwater effects:
			// Refracted light relies on lighting calculations done on underlying objects, so it uses Emission.
			o.Emission = lerp(_FogColor * _EmissionStrength, reflection, fresnel);
			o.Albedo = lerp(_FogColor * _DiffuseStrength, float3(0,0,0), fresnel).rgb;
			
			// Emphasize sunlight and other important lights, over other reflections:
			o.Specular = _Shininess;
			o.Gloss = _Gloss;
			o.Alpha = 1;
		}
		
	ENDCG
		
		
		
	SubShader { // Shader Model 5:
		
		Tags { "Queue"="Transparent" "RenderType"="TransparentHeightMesh"}
		LOD 500
		
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:WaterVert nolightmap
		#pragma target 5.0
		#pragma debug
		#pragma exclude_renderers opengl
		
		ENDCG
	}
	
	SubShader { // Shader Model 3:
		
		Tags { "Queue"="Transparent" "RenderType"="TransparentHeightMesh"}
		LOD 500
		
		
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:WaterVert nolightmap
		#pragma target 3.0
		
		// When compiling for Desktop OpenGL, use GLSL instead of ARB to support tex2dlod
		#pragma glsl
		
		ENDCG
	}




}
