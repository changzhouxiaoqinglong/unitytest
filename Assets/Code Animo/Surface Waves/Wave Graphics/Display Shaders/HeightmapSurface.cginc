// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

#ifndef CODEANIMO_HEIGHTMAP_SURFACE_INCLUDED
#define CODEANIMO_HEIGHTMAP_SURFACE_INCLUDED

// The following snipped is shamelessly ripped from Aras
// (http://forum.unity3d.com/threads/63231-How-to-use-vertex-texture-fetch?p=405266&viewfull=1#post405266), 
// then modified
// Texture lookup in vertex shader is 'special', 
// here is a solution that uses a directX oriented tex2Dlod, 
// which apparently works on openGL if compiled on an openGL system:
float4 vertexTextureLookup(sampler2D tex, float2 uv){
	#if !defined(SHADER_API_D3D11_9X)
		return tex2Dlod (tex, float4(uv.xy, 0, 0));// requires at least shader model 3
	#else
		return float4(0,0,0,0);
	#endif
}

// Left hand normal
float2 get2DNormal(float firstY, float secondY, float deltaX){
	float deltaY = secondY - firstY;
	return normalize(float2(-deltaY, deltaX));
}


// Assumes a grid with the diagonal lines going from the topleft to bottomRight
float3 calcGridNormal(sampler2D heightMap, float2 uv, float2 neighbourDistance, float3 meshDimensions, float localValue){
	float2 topLeft		= uv - neighbourDistance.xy;
	float2 top			= uv - float2(0,neighbourDistance.y);
	
	float2 left			= uv - float2(neighbourDistance.x,0);
	float2 right		= uv + float2(neighbourDistance.x, 0);
	
	float2 bottom		= uv + float2(0, neighbourDistance.y);
	float2 bottomRight	= uv + float2(neighbourDistance.x, neighbourDistance.y);
	
	// Get Texture Colors:
	float4 topLeftTex = vertexTextureLookup(heightMap, topLeft);
	float4 topTex = vertexTextureLookup(heightMap, top);
	
	float4 leftTex = vertexTextureLookup(heightMap, left);
	float4 rightTex = vertexTextureLookup(heightMap, right);
	
	float4 bottomTex = vertexTextureLookup(heightMap, bottom);
	float4 bottomRightTex = vertexTextureLookup(heightMap, bottomRight);
	
	float topLeftValue = topLeftTex.r;
	float topValue = topTex.r;
	
	float leftValue = leftTex.r;			
	float rightValue = rightTex.r;
	
	float bottomValue = bottomTex.r;
	float bottomRightValue = bottomRightTex.r;
	
	// Calculate Edges:
	// All calculated from the topLeft to bottomRight
	float3 topLeftHorzEdge = float3(1, topValue - topLeftValue, 0) * meshDimensions;
	
	float3 topLeftVertEdge = float3(0, leftValue - topLeftValue, 1) * meshDimensions;
	float3 topVertEdge = float3(0, localValue - topValue, 1) * meshDimensions;			
	
	float3 leftHorzEdge = float3(1, localValue - leftValue, 0) * meshDimensions;
	float3 rightHorzEdge = float3(1, rightValue - localValue, 0) * meshDimensions;
	
	float3 bottomVertEdge = float3( 0, bottomValue - localValue, 1) * meshDimensions;
	float3 bottomRightVertEdge = float3 (0, bottomRightValue - rightValue, 1) * meshDimensions;
	
	float3 bottomRightHorzEdge = float3(1, bottomRightValue - bottomValue, 0) * meshDimensions;
	
	// Calculate Face Normals:
	float3 topLeftNormal = cross(topVertEdge, topLeftHorzEdge);
	float3 leftTopNormal = cross(topLeftVertEdge, leftHorzEdge);
	float3 rightTopNormal = cross(topVertEdge, rightHorzEdge);
	
	float3 leftBottomNormal = cross(bottomVertEdge, leftHorzEdge);
	float3 bottomRightNormal = cross(bottomVertEdge, bottomRightHorzEdge);
	float3 rightBottomNormal = cross(bottomRightVertEdge, bottomRightHorzEdge);
	
	// Combine Normals.
	return normalize(topLeftNormal + leftTopNormal + rightTopNormal + leftBottomNormal + bottomRightNormal + rightBottomNormal);
}



#endif