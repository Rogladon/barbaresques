Shader "Unlit/Map Shader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_provinces ("Provinces Map", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _provinces;
			float4 _provinces_TexelSize;

			bool isclreq(fixed3 a, fixed3 b) {
				return length(a - b) < 1.0 / 255.0;
			}

			bool IsBorder(float borderWidth, fixed2 coord, fixed3 currentColor) {
			#define IS_SAME_PROV(x, y) isclreq(currentColor, tex2D(_provinces, coord + fixed2(x, y)))
				return
					/*u */ !IS_SAME_PROV(_provinces_TexelSize.x * borderWidth, 0)
					|| /*b */ !IS_SAME_PROV(-_provinces_TexelSize.x * borderWidth, 0)
					|| /* l*/ !IS_SAME_PROV(0, -_provinces_TexelSize.y * borderWidth)
					|| /* r*/ !IS_SAME_PROV(0, +_provinces_TexelSize.y * borderWidth)
				;
			#undef IS_SAME_PROV
			}

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				// sample the texture
				fixed3 col = tex2D(_MainTex, i.uv).rgb;
				if (IsBorder(1, i.uv, col.rgb)) {
					col *= 0.1;
				}
				return fixed4(col, 1);
			}
			ENDCG
		}
	}
}
