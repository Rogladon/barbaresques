Shader "Unlit/Map Shader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_provinces ("Provinces Map", 2D) = "white" {}
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

			bool isclreq(fixed3 a, fixed3 b) {
				return length(a - b) < 1.0 / 255.0;
			}

			#define PROVS_MAX 256
			#define NULL_PROV (0)
			uniform fixed4 _provs[PROVS_MAX];

			int GetProvinceId(fixed3 color) {
				for (int i = 1; i < PROVS_MAX; i++) {
					if (isclreq(_provs[i].rgb, color)) {
						return i;
					}
				}
				return NULL_PROV;
			}

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _provinces;
			float4 _provinces_TexelSize;


			bool IsBorder(float borderWidth, fixed2 uv, fixed3 currentColor) {
			#define IS_SAME_PROV(x, y) (isclreq(currentColor, tex2D(_provinces, uv + fixed2(x, y)).rgb) && length(tex2D(_provinces, uv + fixed2(x, y)).rgb) > 0.003)
				return /* u   */ !IS_SAME_PROV(_provinces_TexelSize.x * borderWidth, 0)
					|| /* b   */ !IS_SAME_PROV(-_provinces_TexelSize.x * borderWidth, 0)
					|| /*   l */ !IS_SAME_PROV(0, -_provinces_TexelSize.y * borderWidth)
					|| /*   r */ !IS_SAME_PROV(0, +_provinces_TexelSize.y * borderWidth)
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
				fixed3 provinceColor = tex2D(_provinces, i.uv).rgb;
				int provinceId = GetProvinceId(provinceColor);

				fixed3 textureColor = tex2D(_MainTex, i.uv).rgb;

				fixed3 result = textureColor;

				if (provinceId != NULL_PROV) {
					for (int b = 5; b > 0; b--) {
						if (IsBorder(b, i.uv, provinceColor.rgb)) {
							result *= 1 - 64 * (1.0/255.0);
						}
					}
				}
				return fixed4(result, 1);
			}
			ENDCG
		}
	}
}
