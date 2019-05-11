Shader "Custom/StarShader"
{
	Properties
	{
		_Temp("Temperature", int) = 0
		_StarSpectrum("Color Spectrum", 2D) = "white" {}
		_FresnelStrength("Fresnel strength", float) = 0.8
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "noiseSimplex.cginc"		

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float3 normal: NORMAL;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float3 viewDir : TEXCOORD1;
					half3 normal : TEXCOORD2;
				};

				int _Temp;
				sampler2D _StarSpectrum;
				float _FresnelStrength;

				inline float4 get_star_color() {
					float u = (_Temp - 4000.0) / (30000.0 - 4000.0);
					return tex2D(_StarSpectrum, float2(u,0));
				}

				inline float3 uv_to_3d(float2 m) {
					float PI = 3.141592653589;
					float theta = 2.0 * PI * m.x;
					float phi = PI * m.y;

					float x = cos(theta) * sin(phi);
					float y = -cos(phi);
					float z = sin(theta) * sin(phi);
					return float3(x, y, z);
				}

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.viewDir = ObjSpaceViewDir(v.vertex);
					o.normal = v.normal;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{

					float distance = length(i.viewDir);
					float dist_factor = clamp(distance / 1000, 0, 1);
					float4 close, far;

					if (dist_factor < 1) {
						float3 p = normalize(uv_to_3d(i.uv));

						float fresnel = 1 - saturate(dot(i.normal, normalize(i.viewDir)));
						fresnel = smoothstep(_FresnelStrength, 1, fresnel);

						float time = _Time.y * 0.08;
						float n1 = 1 - abs(snoise(float4(p * 3, time)));
						float n2 = (snoise(float4(p * -5, time)) + 1.0) / 2;
						float n3 = (snoise(float4(p * 10, time)) + 1.0) / 2;
						float n4 = (snoise(float4(p * 20, time * 2)) + 1.0) / 2 + 0.5;

						float4 color = get_star_color();
						color = lerp(color, float4(1, 1, 1, 1), n1*n2*n3*n4);

						close = (1 + 30 * fresnel) * color;
					}
					else {
						close = float4(1, 1, 1, 1);
					}
					far = float4(1, 1, 1, 1) * 3;

					return lerp(close, far, dist_factor);
				}
				ENDCG
			}
		}
}
