Shader "GPUAnimationSkinning/InstancedVertFragShader" {
	Properties {
		mainTex ("Albedo (RGB)", 2D) = "red" {}
		hueShiftMask ("Hue shift mask", 2D) = "black" {}
	}
	SubShader {
		Tags {
			"RenderType" = "Opaque"
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass {
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#pragma multi_compile_instancing
			#pragma target 3.0

			#include "UnityStandardCore.cginc"
	#if SHADER_TARGET >= 30
			#include "AnimationCore.cginc"
	#endif

			sampler2D mainTex;
			sampler2D hueShiftMask;

			struct v2f {
				float4 pos        : SV_POSITION;
				float2 uv_MainTex : TEXCOORD0;
				// float3 ambient    : TEXCOORD1;
				// float3 diffuse    : TEXCOORD2;
				float3 color      : TEXCOORD3;
				uint instanceID : SV_INSTANCEID;
				// SHADOW_COORDS(4)
			};

			v2f vert(appdata_full v, uint instanceID : SV_InstanceID) {  
			#if SHADER_TARGET >= 30
				float4x4 transformMatrix = TransformMatrix(instanceID);
				float4x4 animationMatrix = AnimationMatrix(v.texcoord1, v.texcoord2, instanceID);
				 
				float4 posWorld = mul(transformMatrix, mul(animationMatrix, v.vertex));
			#else
				float4 posWorld = v.vertex;    
			#endif

				float3 color         = v.color;
				float3 worldPosition = posWorld.xyz;
				// float3 worldNormal   = v.normal;

				// float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
				// float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
				// float3 diffuse = (ndotl * _LightColor0.rgb);

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
				o.uv_MainTex = v.texcoord;
				// o.ambient = ambient;
				// o.diffuse = diffuse;
				o.color = color;
				// TRANSFER_SHADOW(o)
				o.instanceID = instanceID;
				return o;
			}

			StructuredBuffer<float4> objectTintsBuffer;
			fixed3 hsl2rgb(fixed3 HSL) {
				float R = abs(HSL.x * 6.0 - 3.0) - 1.0;
				float G = 2.0 - abs(HSL.x * 6.0 - 2.0);
				float B = 2.0 - abs(HSL.x * 6.0 - 4.0);
				fixed3 RGB = clamp(fixed3(R,G,B), 0.0, 1.0);
				float C = (1.0 - abs(2.0 * HSL.z - 1.0)) * HSL.y;
				return (RGB - 0.5) * C + HSL.z;
			}
			fixed3 rgb2hsl(in fixed3 c) {
				float h = 0.0;
				float s = 0.0;
				float l = 0.0;
				float r = c.r;
				float g = c.g;
				float b = c.b;
				float cMin = min( r, min( g, b ) );
				float cMax = max( r, max( g, b ) );

				l = ( cMax + cMin ) / 2.0;
				if ( cMax > cMin ) {
					float cDelta = cMax - cMin;
					
					//s = l < .05 ? cDelta / ( cMax + cMin ) : cDelta / ( 2.0 - ( cMax + cMin ) ); Original
					s = l < .0 ? cDelta / ( cMax + cMin ) : cDelta / ( 2.0 - ( cMax + cMin ) );
					
					if ( r == cMax ) {
						h = ( g - b ) / cDelta;
					} else if ( g == cMax ) {
						h = 2.0 + ( b - r ) / cDelta;
					} else {
						h = 4.0 + ( r - g ) / cDelta;
					}

					if ( h < 0.0) {
						h += 6.0;
					}
					h = h / 6.0;
				}
				return fixed3( h, s, l );
			}

			fixed4 frag(v2f i) : SV_Target {
				// fixed shadow = SHADOW_ATTENUATION(i);
				fixed4 albedo = tex2D(mainTex, i.uv_MainTex);
				fixed4 hueShiftValue = tex2D(hueShiftMask, i.uv_MainTex);

				float hueFactor = length(hueShiftValue.rgb) / length(fixed3(1,1,1));
				fixed3 albedoHsl = rgb2hsl(albedo.rgb);
				fixed3 tintHsl = rgb2hsl(objectTintsBuffer[i.instanceID].rgb);
				fixed3 tintedColor = hsl2rgb(fixed3(tintHsl.x, albedoHsl.y, albedoHsl.z));
				fixed3 color = hueFactor * tintedColor + (1 - hueFactor) * albedo.rgb;
				// float3 lighting = i.diffuse * shadow + i.ambient;
				fixed4 output = fixed4(color * i.color, albedo.a);
				return output;
			}

			ENDCG
		}
	}
}