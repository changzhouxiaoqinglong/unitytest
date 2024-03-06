// Copyright © 2017 Laurens Mathot
// Code Animo™ http://codeanimo.com

Shader "Hidden/WriteDepthNear" {
	Properties { 
		_MainTex ("Color", any) = "black" {}
	}
	SubShader { 
		Pass {
 			ZTest Always Cull Off ZWrite On Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			
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
				o.vertex.z = UNITY_NEAR_CLIP_VALUE;
				o.texcoord = v.texcoord.xy;
				return o;
			}

			fixed4 frag (v2f i) : COLOR{
				return tex2D(_MainTex, i.texcoord);
			}
			ENDCG 

		}
	}
	Fallback Off 
}