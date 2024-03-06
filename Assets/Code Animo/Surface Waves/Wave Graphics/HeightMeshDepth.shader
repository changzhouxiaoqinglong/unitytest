// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Code Animo/Surface Waves/HeightMeshDepth" {
	Properties{
		_HeightTex ("Heightmap", 2D) = "black" {}
		_Scale ("Maximum Displacement", Float) = 10.0
	}
	Category{
		Fog { Mode Off }
		
		SubShader {
			Tags { "RenderType"="Opaque" }
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				struct v2f {
				    float4 pos : SV_POSITION;
				    #ifdef UNITY_MIGHT_NOT_HAVE_DEPTH_TEXTURE
				    float2 depth : TEXCOORD0;
					#endif
				};
				v2f vert( appdata_base v ) {
				    v2f o;
				    o.pos = UnityObjectToClipPos(v.vertex);
//				    o.depth = o.pos.zw;
				    UNITY_TRANSFER_DEPTH(o.depth);
				    return o;
				}
				fixed4 frag(v2f i) : COLOR {
				    UNITY_OUTPUT_DEPTH(i.depth);
//				    return float4(i.depth.x / i.depth.y, 0,0,0); // Used when Unity can't use depth buffer data.
				}
				ENDCG
			}
		}
		
		SubShader {
		    Tags { "RenderType"="HeightMesh" }
		    Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 5.0
				#include "UnityCG.cginc"
				#include "Display Shaders/HeightmapSurface.cginc"
				
				sampler2D _HeightTex;
				float4 _HeightTex_ST;
				float _Scale;
				
				struct v2f {
				    float4 pos : SV_POSITION;
				    #ifdef UNITY_MIGHT_NOT_HAVE_DEPTH_TEXTURE
				    float2 depth : TEXCOORD0;
					#endif
				};
				
				v2f vert (appdata_base v) {
				  	v2f o;
				    // HeightMap Stuff
				    float2 uv = TRANSFORM_TEX(v.texcoord, _HeightTex);
				    float4 localTex = vertexTextureLookup(_HeightTex, uv);
				    float vertexOffset = (localTex.r) * _Scale;
				    v.vertex.y += vertexOffset;
				    
				    o.pos = UnityObjectToClipPos (v.vertex);
				    UNITY_TRANSFER_DEPTH(o.depth);
//				    o.depth = o.pos.zw;// TODO: handle platform differences.
				    return o;
				}
				
				fixed4 frag(v2f i) : COLOR {
//					return float4(i.depth.x / i.depth.y, 0,0,0);
				    UNITY_OUTPUT_DEPTH(i.depth);// Write out to simulated depth buffer if native depth buffer is not supported.
				}
				ENDCG
		    }
		}
	}
}