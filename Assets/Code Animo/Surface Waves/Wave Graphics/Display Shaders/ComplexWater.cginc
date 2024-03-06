// Copyright © 2017 Laurens Mathot
// Code Animo™ http://codeanimo.com

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "HeightmapSurface.cginc"

struct vertexInput{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
};

struct vertexOutput{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 depth : TEXCOORD1;// Water Surface Depth
	float4 projection : TEXCOORD2;// Refraction projection
	fixed3 worldNormal : TEXCOORD3;
	float4 worldPos : TEXCOORD4;// world pos and fog (w)
	float3 viewDirection : TEXCOORD5;
	#ifdef UNITY_PASS_FORWARDBASE
	fixed3 vlight : TEXCOORD6;
	#endif
	#ifdef UNITY_PASS_FORWARDADD
	half3 lightDir : TEXCOORD6;
	#endif
	LIGHTING_COORDS(7,8)
};

struct fragmentInput{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 depth : TEXCOORD1;// Water Surface Depth
	float4 projection : TEXCOORD2;// Refraction projection
	fixed3 worldNormal : TEXCOORD3;
	float4 worldPos : TEXCOORD4;// world pos and fog (w)
	float3 viewDirection : TEXCOORD5;
	#ifdef UNITY_PASS_FORWARDBASE
	fixed3 vlight : TEXCOORD6;
	#endif
	#ifdef UNITY_PASS_FORWARDADD
	half3 lightDir : TEXCOORD6;
	#endif
	LIGHTING_COORDS(7,8)
};

//====Vertex Shader====


// Calculate Projection for Refraction Grabpass Texture Lookup:
float4 calcRefrProj(in float4 screenPos){	
	// On D3D when AA is used, the main texture & scene depth texture
	// will come out in different vertical orientations.
	// So flip sampling of the texture when that is the case (main texture
	// texel size will have negative Y).
	#if UNITY_UV_STARTS_AT_TOP
		if (_UnderWater_TexelSize.y < 0) screenPos.y = - screenPos.y;
	#endif
	
//	screenPos.xyw += screenPos.w;// w is apparently a required offset. Will have to dig into projection math more to understand and update this comment.
	
	return screenPos;
}

void vert(vertexInput v, out vertexOutput o){		
	float2 uv = TRANSFORM_TEX (v.texcoord, _HeightTex);// Apply Scale and Bias settings
	o.uv = uv;
	// Apply offset:
	float surfaceHeight = vertexTextureLookup(_HeightTex, uv).r;
	float vertexOffset = (surfaceHeight) * _Scale;
    v.vertex.y += vertexOffset;
    
    o.pos = UnityObjectToClipPos (v.vertex);
    
    // Surface Depth:
    COMPUTE_EYEDEPTH(o.depth);// Store depth in texcoord for use in surface shader.

    // Reflection projection:
    o.projection = calcRefrProj(o.pos);// Output Refraction Projection, based on displaced vertex, into TEXCOORD[n] semantic
    
    // Recalculate normals:
    float2 uvDistance = float2(_EdgeULength, _EdgeVLength);
    float3 gridNormal = calcGridNormal(_HeightTex, uv, uvDistance, float3(_gridSizeU, _Scale, _gridSizeV), surfaceHeight);// Store normal for lighting calculations
    o.worldNormal = mul ((float3x3)unity_ObjectToWorld, gridNormal);

	// normal for box projection:
	o.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex).xyz;
    
   	// View Direction:
    o.viewDirection = WorldSpaceViewDir( v.vertex );
    
    #ifdef UNITY_PASS_FORWARDBASE
    // Vertex Lights:
	o.vlight = Shade4PointLights (
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		unity_4LightAtten0, o.worldPos.xyz, o.worldNormal );
    #endif
    #ifdef UNITY_PASS_FORWARDADD
    o.lightDir = WorldSpaceLightDir( v.vertex );
    #endif
    
	o.worldPos.w = o.pos.z;// fog

    // Output Lighting and shadow coords (Used by LIGHT_ATTENUATION):
    TRANSFER_VERTEX_TO_FRAGMENT(o);
}


//====Fragment Shader====
struct LightingData{
	fixed3 Albedo;
	fixed3 WrappedAlbedo;
	fixed3 Normal;
	half Specular;
	fixed Gloss;
};



fixed3 LightingWrappedWater (LightingData s, fixed3 lightDir, half3 viewDirection, fixed atten) {
	half3 h = normalize (lightDir + viewDirection);
	
	fixed nDotL = dot (s.Normal, lightDir);
	
	fixed diffuse = max (0, nDotL);
	
	fixed wrap = (nDotL + _LightingWrap) / (1.0f + _LightingWrap);
	fixed wrappedDiff = max (0, wrap);
	
	float nh = max (0, dot (s.Normal, h));
	float spec = pow (nh, s.Specular*128.0) * s.Gloss;
	
	fixed3 c;

	c =		_LightColor0.rgb * wrappedDiff * s.WrappedAlbedo;
	c +=	_LightColor0.rgb * diffuse * s.Albedo;
	c +=	_LightColor0.rgb * _SpecColor.rgb * spec;
	return c;
}

float4 correctProjection(in float4 projection){
	projection.xy = 0.5 * (projection.xy + 1 * projection.w);
	return projection;
}

float getCustomDepth(float zValue){
	// All this stuff could probably be simplified and moved to the CPU to be calculated once.
	float zBufferParamY = 3333;
	float zBufferParamX =  -3332;
	
	float zBufferParamZ = zBufferParamX / 1000;
	float zBufferParamW = zBufferParamY / 1000;
	
//	return 1.0 / (zBufferParamZ * zValue + zBufferParamW);// Linear Eye Depth
	return 1 / (-3332 * zValue + 3333);// Linear 01 Depth
}

fixed4 frag(fragmentInput i) : COLOR {
	LightingData o;
	UNITY_INITIALIZE_OUTPUT(LightingData, o);// Input Initialization required for DirectX11
	
	float3 normViewDir = normalize(i.viewDirection);
	o.Normal = i.worldNormal;// It appears this normal is already normalized.
	
	float fresnel = saturate( dot(i.worldNormal, normViewDir) );// Control mix between effects from above and below water, based on viewing angle
	fresnel = pow(1 - fresnel, _FresnelPower);// Adjust falloff
	
	i.projection = correctProjection(i.projection);

	float4 refractionUV = i.projection;
	refractionUV.xy += i.worldNormal.xz * _Refraction;

	float4 waterData = tex2D(_WaterData, i.uv);
	
	clip (waterData.b - _ClipHeight);// Prevent Z fighting with terrain at low water height.
	
	// Refraction:
	fixed4 refraction = tex2Dproj( _UnderWater, UNITY_PROJ_COORD(refractionUV));
	
	// Distance of terrain from eye:
	float refractedDepth = DECODE_EYEDEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(refractionUV)).r);
	float unrefractedDepth = DECODE_EYEDEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projection)).r);// Needed when extreme refraction would leave areas without fog.
	float terrainDepth = max(refractedDepth, unrefractedDepth);// Workaround for fog working incorrectly close to objects. Switching only if refracting above water didn't really help.
	
	float fogDepth = max(0, terrainDepth - i.depth.x);

	//Soft Shorelines:
	float shoreLineMix = pow(saturate(fogDepth - _BlendDepth), 2);
	
	// Depth Fog:
	float remainingLight = saturate(1 - fogDepth / _FogViewDist);// [0,1] proximity to visibility edge
	// Light is reduced exponentially. Normally lights handle the exponential falloff themselves.
	// In this case light goes through the water twice.
	// This assume the path the light takes is as long as the path the eye took:
	remainingLight = pow(remainingLight, 4);
	
	float fogContribution = 1 - remainingLight;
	
	// Fog Diffuse:
	float3 fogDiffuse = _FogDiffuseColor.rgb * fogContribution;
	fogDiffuse *= 1 - fresnel;
	
	// Foam:
	float foamAmount = pow(waterData.a, _FoamPower);
	float foamPresence = saturate((foamAmount - _FoamThreshold) / (1 -_FoamThreshold));
	
	float3 foam = _FoamColor.rgb * foamPresence;
	
	float aboveWaterFoam = foamPresence * (foamPresence > _OverUnderFoamMix);
	float underWaterFoam = foamPresence * (foamPresence < _OverUnderFoamMix);
	
	// underwater foam:
	if (foamPresence < _OverUnderFoamMix && foamPresence > 0){
		float maxFoamDepth = min(_MaxFoamDepth, fogDepth);// Shouldn't be deeper than existing refractions.
		float foamDepth = lerp(maxFoamDepth, 0, underWaterFoam / _OverUnderFoamMix);
		float remainingFoamLight = saturate(1 - foamDepth / _FogViewDist);
		remainingFoamLight *= pow(remainingFoamLight, 4);
		
		foam *= remainingFoamLight;
		foam *= 1 - fresnel;
	}
	
	
	// Fog Affects Refracted light:
	refraction *= remainingLight;// Reduce the amount of light
	refraction *= 1 - fresnel;
	refraction *= 1 - foamPresence;
	refraction *= lerp(float4(1,1,1,1), _FogFilterColor, fogContribution);// Filter colors based on depth
	
	// Reflection:
	float3 reflectionDir0 = reflect(-normViewDir, normalize(o.Normal));
	float3 reflectionDir1 = BoxProjectedCubemapDirection (reflectionDir0, i.worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
	reflectionDir0 = BoxProjectedCubemapDirection (reflectionDir0, i.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
	fixed4 reflection0 = UNITY_SAMPLE_TEXCUBE_LOD( unity_SpecCube0, reflectionDir0, 0 );
	
	float probeBlend = unity_SpecCube0_BoxMin.w;
    UNITY_BRANCH
    if ( probeBlend < 0.99999)
    {
        fixed4 reflection1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD( unity_SpecCube1, unity_SpecCube0, reflectionDir1, 0 );
        reflection0 = lerp(reflection1, reflection0, probeBlend);
    }


	reflection0 *= fresnel;
	reflection0 *= 1 - aboveWaterFoam;
	
	//===Lighting===
	// Diffuse light:
	o.Albedo = foam;// Underwater foam is drawn on top of light from fog, because the light from the fog can come from elsewhere, whereas the foam light is local.
	o.WrappedAlbedo = fogDiffuse;
	
	// Emphasize sunlight and other important lights, over other reflections:
	o.Specular = _Shininess;
	o.Gloss = _Gloss;
	
	fixed lightAttenuation = LIGHT_ATTENUATION(i) + 0.0f;// Unity 2018.1.0b can have LIGHT_ATTENUATION(a) defined to nothing, and pragma require doesn't help not compile for those cases... so + 0.0f is a workaround for the error message, I guess...
	
	// Light Direction:
	#ifdef UNITY_PASS_FORWARDBASE
	fixed3 lightDirection = _WorldSpaceLightPos0.xyz;
	#endif
	#ifdef UNITY_PASS_FORWARDADD
		#ifndef USING_DIRECTIONAL_LIGHT
		fixed3 lightDirection = normalize(i.lightDir);
		#else
		fixed3 lightDirection = i.lightDir;
		#endif
	#endif
	
	// Calculate standard non-emissive Lighting:
	fixed3 litSurface = LightingWrappedWater(o, lightDirection, normalize(half3(i.viewDirection)), lightAttenuation);
	
	#ifdef UNITY_PASS_FORWARDBASE			
	litSurface += i.vlight * (o.WrappedAlbedo.rgb + o.Albedo.rgb);// Vertex Light
	litSurface += reflection0.rgb + refraction.rgb;
	#endif

	UNITY_APPLY_FOG( i.worldPos.w, litSurface );

	return fixed4(litSurface, shoreLineMix);
}