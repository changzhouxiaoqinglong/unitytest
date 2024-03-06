// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/Code Animo/Surface Waves/GroundHeightDepth" {
	Properties{
	}
	Category{
		Fog { Mode Off  }
		Cull Off
		Zwrite On
		ZTest GEqual
		
		SubShader {
		Tags { "RenderType"="Opaque" }
		Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				struct v2f {
				    float4 pos : SV_POSITION;
				};
				v2f vert( appdata_base v ) {
				    v2f o;
				    o.pos = UnityObjectToClipPos(v.vertex);				    
					return o;
				}
				fixed4 frag(v2f i) : COLOR {
					return fixed4( 0,0,0,0 );
				}
				ENDCG
			}
		}
		
	}
}