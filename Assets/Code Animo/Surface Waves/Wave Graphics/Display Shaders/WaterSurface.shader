// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Code Animo/Surface Waves/WaterSurface" {
	Properties {
		_Refraction  ("Distorted Refraction Scale", Float) = 35.0
		
		_FogFilterColor ("Fog Filter Color", Color) = (0.7,0.96, 0.9, 1)
		_FogDiffuseColor ("Fog Diffuse Color", Color) = (0.05,0.14,0.2,1)
		
		_FogFalloff ("Fog Falloff", Range(0,1)) = 0.8
		_FogViewDist ("Water Visibility Distance", Float) = 25
				
		_BlendDepth ("Soft Shore Blend Depth", float) = 1.0
		
		_ReflectionCube ("Reflection Cube", Cube) = "_Skybox" {}
		_FresnelPower ("Fresnel Power", Range(1,5)) = 5
		
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Gloss ("Gloss", Range( 0, 1)) = 0.3
		_Shininess ("Shininess", Range (0.03, 1)) = 1
		
		_LightingWrap ("Lighting Wrap", Range(0,1)) = 1
		
		_FoamStrength ("Foam Strength", Range(0, 100)) = 100
		_FoamPower ("Foam Falloff", Range(4,0)) = 1
		_FoamBias ("Foam Offset", Range(-1, 0)) = -0.5
		_FoamColor ("Foam Color", Color) = (0.6, 0.6, 0.56, 1)
		
		_Scale ("Mesh Displacement Scale", Float) = 10.0
		_gridSizeU ("GridSize U (normal calculation)", Float) = 1
		_gridSizeV ("GridSize V (normal calculation)", Float) = 1
		
		_HeightTex ("Heightmap", 2D) = "black" {}
		_WaterData ("WaterData", 2D) = "blue" {}
	}
	
	// Shared between Shader Model 3 and Shader Model 5:
	CGINCLUDE
		#include "HeightmapSurface.cginc"
		
		sampler2D _HeightTex;
		sampler2D _WaterData;
		sampler2D _CameraDepthTexture;
		
		sampler2D _UnderWater;// Refraction
		samplerCUBE _ReflectionCube;
		
		float4 _HeightTex_TexelSize;
		float4 _UnderWater_TexelSize;
		
		float _FoamStrength;
		float _FoamPower;
		float _FoamBias;
		float4 _FoamColor;
		
		float4 _FogFilterColor;
		float4 _FogDiffuseColor;
		
		float _FogViewDist;
		float _FogFalloff;
		
		float _FresnelPower;
		
		float _Scale;
		float _gridSizeU;
		float _gridSizeV;
		
		float _Refraction;
		
		float _Gloss;
		float _Shininess;
		
		float _LightingWrap;
		
		float _BlendDepth;
		
		struct Input {
			float4 projection : TEXCOORD;// Refraction projection
			float2 depth : TEXCOORD1;// Water Surface Depth
			float2 uv_WaterData;
			float3 worldNormal;
			float3 viewDir;
		};
		
		struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
            float2 depth : TEXCOORD1;// Water Surface Depth
        };
        
        
//=================Vertex Shader=============================
		
		// Calculate Projection for Refraction Grabpass Texture Lookup:
		float4 calcRefrProj(in float4 vertexPos){		
			
			// calculate the position on screen:
			float4 vertScreenPos = UnityObjectToClipPos(vertexPos);
			
			// On D3D when AA is used, the main texture & scene depth texture
			// will come out in different vertical orientations.
			// So flip sampling of the texture when that is the case (main texture
			// texel size will have negative Y).
			#if UNITY_UV_STARTS_AT_TOP
				if (_UnderWater_TexelSize.y < 0) vertScreenPos.y *= -1;
			#endif
			
			return (vertScreenPos * 0.5) + (0.5 * vertScreenPos.w);// Apply Bias and Scale Matrix to move to [0,1]
		}
		
		
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
		    v.normal = calcGridNormal(_HeightTex, uv, _HeightTex_TexelSize.xy, float3(_gridSizeU, _Scale, _gridSizeV), surfaceHeight);// Store normal for lighting calculations
		    
		    o.projection = calcRefrProj(v.vertex);// Output Refraction Projection, based on displaced vertex, into TEXCOORD0 semantic		    
		    
		    COMPUTE_EYEDEPTH(o.depth);// Store depth in texcoord for use in surface shader.
		}
		
//==============Custom Lighting

	fixed4 LightingSubSurface (SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten) {
		half3 h = normalize (lightDir + viewDir);
		
		fixed nDotL = dot (s.Normal, lightDir);
		fixed wrap = (nDotL + _LightingWrap) / (1.0f + _LightingWrap);
		fixed diff = max (0, wrap);
		
		float nh = max (0, dot (s.Normal, h));
		float spec = pow (nh, s.Specular*128.0) * s.Gloss;
		
		fixed4 c;
		c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * _SpecColor.rgb * spec) * (atten * 2);
		c.a = s.Alpha + _LightColor0.a * _SpecColor.a * spec * atten;
		return c;
	}

	fixed4 LightingSubSurface_PrePass (SurfaceOutput s, half4 light) {
		fixed spec = light.a * s.Gloss;
		
		fixed4 c;
		c.rgb = (s.Albedo * light.rgb + light.rgb * _SpecColor.rgb * spec);
		c.a = s.Alpha + spec * _SpecColor.a;
		return c;
	}	
				
//==============Surface Shader====================
		
		float4 calculateRefractionUv(in float3 worldNormal, in float4 projection){
			float2 normal = worldNormal.xz;
			float2 uvOffset = normal * _Refraction * _UnderWater_TexelSize.xy;
			uvOffset *= projection.z;// More refraction, further away from the camera?
			
			float4 refractionCoordinates = projection;
            refractionCoordinates.xy = projection.xy + uvOffset;
            
			return refractionCoordinates;
		}		

		void surf (Input IN, inout SurfaceOutput o) {
			float3 normViewDir = normalize(IN.viewDir);
			float3 normWorldNormal = normalize(IN.worldNormal);
			
			
			float fresnel = saturate( dot(normWorldNormal, normViewDir) );// Control mix between effects from above and below water, based on viewing angle
			fresnel = pow(1 - fresnel, _FresnelPower);// Adjust falloff
			
			
			float4 refractionUV = calculateRefractionUv(normWorldNormal, IN.projection);// Apply Refraction offset to Transparency projection

			float4 waterData = tex2D(_WaterData, IN.uv_WaterData);
			clip (waterData.b - 0.001);// Prevent Z fighting with terrain at low water height.
			float4 refraction = tex2Dproj( _UnderWater, UNITY_PROJ_COORD(refractionUV));
			
			float foamAmount = saturate(pow(waterData.a, _FoamPower) * _FoamStrength + _FoamBias);
			
			// Distance of terrain from eye:
			float refractedDepth = DECODE_EYEDEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(refractionUV)).r);
			float unrefractedDepth = DECODE_EYEDEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.projection)).r);// Needed when extreme refraction would leave areas without fog.
			float terrainDepth = max(refractedDepth, unrefractedDepth);// Workaround for fog working incorrectly close to objects. Switching only if refracting above water didn't really help.
			
			float fogDepth = max(0, terrainDepth - IN.depth.x);
			
			
			// Foam:
			float3 foam = _FoamColor.rgb * foamAmount;// Only show foam in areas of high accelleration.
			
			
			// Refraction:
			float remainingLight = saturate(1 - fogDepth / _FogViewDist);// [0,1] proximity to visibility edge
			// Light is reduced exponentially. Normally lights handle the exponential falloff themselves.
			// In this case light goes through the water twice.
			// This assume the path the light takes is as long as the path the eye took:
			remainingLight = pow(remainingLight, 4);
			
			float fogContribution = pow(1 - remainingLight, _FogFalloff);
			
			refraction *= remainingLight;// Reduce the amount of light
			refraction *= 1 - fresnel;
			refraction *= 1 - foamAmount;
			refraction *= lerp(float4(1,1,1,1), _FogFilterColor, fogContribution);// Filter colors based on depth

			// Fog Diffuse:
			float3 fogDiffuse = _FogDiffuseColor.rgb * fogContribution;
			fogDiffuse *= 1 - fresnel;
			fogDiffuse *= 1 - foamAmount;
			
			// Reflection:
			float3 reflectionDir = reflect(-normViewDir, normWorldNormal);// Custom reflection vector calculation.
			float4 reflection = texCUBE(_ReflectionCube, reflectionDir);
			reflection *= fresnel;
			
			
			
			
			o.Emission = reflection + refraction;
			o.Albedo = foam + fogDiffuse;

			
			// Emphasize sunlight and other important lights, over other reflections:
			o.Specular = _Shininess;
			o.Gloss = _Gloss;
		}
		
		

		
	ENDCG
	
	SubShader { // Shader Model 5:
		LOD 800
		Tags { "Queue"="Transparent" "RenderType"="TransparentHeightMesh" }
		GrabPass { "_UnderWater" }
		
		CGPROGRAM
		#pragma surface surf SubSurface vertex:WaterVert nolightmap exclude_path:prepass
		#pragma target 5.0
		#pragma debug
		#pragma exclude_renderers opengl
		
		ENDCG
	}
	
	SubShader { // Shader Model 3:
		LOD 800
		Tags { "Queue"="Transparent" "RenderType"="TransparentHeightMesh" }//"PreviewType"="Plane"
		GrabPass { "_UnderWater" }
		
		CGPROGRAM
		#pragma surface surf SubSurface vertex:WaterVert nolightmap exclude_path:prepass
		#pragma target 3.0
		#pragma debug
		
		// When compiling for Desktop OpenGL, use GLSL instead of ARB to support tex2dlod
		#pragma glsl
		
		ENDCG
	}
	
	FallBack "WaterSimulation/BasicWaterSurface"
}
