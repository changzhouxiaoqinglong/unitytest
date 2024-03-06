// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com
Shader "Hidden/CameraDepthCopy" {
	Properties { 
		_MainTex ("", any) = "" {}
	}
	SubShader { 
		Pass {
 			ZTest Always Cull Off ZWrite On Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			sampler2D _CameraDepthTexture;
			
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
				o.texcoord = v.texcoord.xy;
				
				return o;
			}

			float4 frag (v2f i) : COLOR{
				return float4(tex2D(_CameraDepthTexture, i.texcoord).r, 0,0,0);
			}
			ENDCG 

		}
	}
	Fallback Off 
}