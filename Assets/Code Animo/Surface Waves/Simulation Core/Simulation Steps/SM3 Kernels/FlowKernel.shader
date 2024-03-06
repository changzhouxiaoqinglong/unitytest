// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Code Animo/Surface Waves/FlowKernel" {
	Properties {
		WaveMapIn ("Wave Heightmap", 2D) ="black" {}
		HeightOffsetIn ("Height Offset", 2D) = "black" {}
		FlowMapIn ("Flow map", 2D) = "black" {}
		TimeStep ("Time per Step", Range(0.001,1)) = 0.13
		FlowDamping ("Flow Damping", Float) = 0.92
	}
	SubShader {
		Tags { }
		Pass{
			ZTest Always Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
	
			sampler2D WaveMapIn;
			sampler2D HeightOffsetIn;
			sampler2D FlowMapIn;
			
			float TimeStep;
			float FlowDamping;
			
			float4 FlowMapIn_ST;// Automatically filled with scale/bias data
			float4 WaveMapIn_TexelSize;// Automatically filled with texture dimensionInfo
	
			void vert (appdata_base v, out float4 pos:SV_POSITION, out float2 uv:TEXCOORD0){
				pos = UnityObjectToClipPos(v.vertex);
				uv = TRANSFORM_TEX (v.texcoord, FlowMapIn);// Can't we use the raw input uv coordinates? These textures are set by script, without changing bias and scale.
			}
			
			float4 frag(float4 pos:SV_POSITION, float2 uv:TEXCOORD0):COLOR{
				float4 newFlow;
			
				// Calculate Neighbour coordinates:
				float2 left		= uv + float2(-WaveMapIn_TexelSize.x,0);
				float2 top		= uv + float2(0,WaveMapIn_TexelSize.y);
				float2 right	= uv + float2(WaveMapIn_TexelSize.x, 0);
				float2 bottom	= uv + float2(0,-WaveMapIn_TexelSize.y);
				
				// Load data for local cell
				float4 waveHeight		= tex2D(WaveMapIn, uv);
				float heightOffset		= tex2D(HeightOffsetIn, uv).r;
				float4 oldFlow			= tex2D(FlowMapIn, uv);
				
				// Look up terrain Heights:
				float leftHeightOffset		= tex2D(HeightOffsetIn, left).r;
				float topHeightOffset		= tex2D(HeightOffsetIn, top).r;
				float rightHeightOffset		= tex2D(HeightOffsetIn, right).r;
				float bottomHeightOffset	= tex2D(HeightOffsetIn, bottom).r;
				
				// Look up wave heights:
				float waveLeft		= tex2D(WaveMapIn, left).b;
				float waveTop		= tex2D(WaveMapIn, top).b;
				float waveRight		= tex2D(WaveMapIn, right).b;
				float waveBottom	= tex2D(WaveMapIn, bottom).b;
				
				float localPressure	= waveHeight.b + heightOffset;
				
				//======
				// Calculate the intended acceleration towards neighbour pixels
				// Notes:
			    // pressure is actually (height * viscosity * gravity), gravity is assumed to be 1
			    // acceleration is actually (delta pressure / distance * viscosity), in this case, distance is assumed to be 1
			    // deltaT specifies how big the acceleration steps are. Lower numbers mean more precision, slower movement. Higher means more artefacts, faster movement.
			    //=====
				float leftPressure			= waveLeft		+ leftHeightOffset;
				float topPressure			= waveTop		+ topHeightOffset;
				float rightPressure			= waveRight	+ rightHeightOffset;
				float bottomPressure 		= waveBottom	+ bottomHeightOffset;
			     
			    float leftAcceleration		= TimeStep * (localPressure - leftPressure);
			    float topAcceleration		= TimeStep * (localPressure - topPressure);
			    float rightAcceleration		= TimeStep * (localPressure - rightPressure);
			    float bottomAcceleration	= TimeStep * (localPressure - bottomPressure);
			    
			    
				// Don't let waves flow out of the grid:
				if (left.x		< 0.0)	leftAcceleration = 0.0;
				if (top.y		> 1.0)	topAcceleration = 0.0;
				if (right.x		> 1.0)	rightAcceleration = 0.0;
				if (bottom.y	< 0.0)	bottomAcceleration = 0.0;
				
				// Only calculate flow away from here:
			    newFlow.r = max(0, leftAcceleration);
			    newFlow.g = max(0, topAcceleration);
			    newFlow.b = max(0, rightAcceleration);
			    newFlow.a = max(0, bottomAcceleration);
			    
			    // Continue existing motion:
				newFlow += oldFlow * FlowDamping;
				
				// Prevent wave level from dropping below 0:     
	    		float totalOutFlow = newFlow.r + newFlow.g + newFlow.b + newFlow.a;
				
				float maxOutFlow = waveHeight.b;
				
				if (totalOutFlow > maxOutFlow){
			        if (totalOutFlow != 0.0){// Don't divide by zero.
			            newFlow *= maxOutFlow / totalOutFlow;
			        }
		        	else newFlow = float4(0,0,0,0);
		        }
		        
		        return newFlow;
			}
			
			ENDCG
		}
	} 
}
