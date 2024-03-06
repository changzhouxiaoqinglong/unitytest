// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Code Animo/Surface Waves/DisplacementDepth" {
	
	
	SubShader {
		Tags { "RenderType"="Opaque" }
		Pass {
			Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct v2f {
			    float4 pos : SV_POSITION;
			    float2 depth : TEXCOORD0;
			};
			v2f vert( appdata_base v ) {
			    v2f o;
			    o.pos = UnityObjectToClipPos(v.vertex);
			    o.depth = o.pos.zw;
//				    UNITY_TRANSFER_DEPTH(o.depth);
			    return o;
			}
			fixed4 frag(v2f i) : COLOR {
//				    UNITY_OUTPUT_DEPTH(i.depth);
			    return float4(i.depth.x / i.depth.y, 0,0,0);
			}
			ENDCG
		}
	}
	
}