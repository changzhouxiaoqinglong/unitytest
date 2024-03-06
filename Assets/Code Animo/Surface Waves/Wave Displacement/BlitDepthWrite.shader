// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/BlitDepthWrite" {
	Properties { 
		_MainTex ("", any) = "" {}
		_Depth ("Depth", Range(0,1)) = 0
	}
	SubShader { 
		Pass {
 			ZTest Always Cull Off ZWrite On Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Depth;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v){
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				// Apply custom depth

			#if(defined(UNITY_REVERSED_Z))
				o.vertex.z = (1 - _Depth) * o.vertex.w;
			#else
				o.vertex.z = _Depth * o.vertex.w;
			#endif
				
				o.texcoord = v.texcoord.xy;
				return o;
			}

			fixed4 frag (v2f i) : COLOR{
//				return tex2D(_MainTex, i.texcoord);
				return float4(0,0,0,0);
			}
			ENDCG 

		}
	}
	Fallback Off 
}