Shader "Custom/RockShader" {
	Properties{
		_Color1("Color1", Color) = (1,1,1,1)
		_Color2("Color2", Color) = (1,1,1,1)
		_DustTex("Dust (RGB)", 2D) = "white" {}
		_RockTex("Rock (RGB)", 2D) = "white" {}
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma surface surf Lambert fullforwardshadows
			#pragma target 3.0

			sampler2D _DustTex;
			sampler2D _RockTex;

			struct Input {
				float2 uv_DustTex;
				float4 color : COLOR;
				float3 worldNormal;
				float3 worldPos;
			};

			fixed4 _Color1;
			fixed4 _Color2;

			inline float angle_between(float3 v1, float3 v2) {
				return acos(dot(v1, v2) / (length(v1)*length(v2)));
			}

			inline float rad_to_deg(float rad) {
				float PI = 3.14159;
				return rad / (2 * PI) * 360;
			}

			inline float lerp_slope(float angle, float min, float max) {
				float blend = 0;
				if (angle > min) {
					if (angle > max) {
						blend = 1;
					}
					else {
						blend = (angle - min) / (max - min);
					}
				}
				return clamp(blend, 0, 1);
			}

			void surf(Input IN, inout SurfaceOutput o) {
				float3 center = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				float noise = IN.color.r;

				fixed4 dust1 = tex2D(_DustTex, IN.uv_DustTex * 2000);
				fixed4 dust2 = tex2D(_DustTex, IN.uv_DustTex * 10000);
				fixed4 dust = (dust1 + dust2) / 2;
				
				fixed4 rock1 = tex2D(_RockTex, IN.uv_DustTex * 2000);
				fixed4 rock2 = tex2D(_RockTex, IN.uv_DustTex * 10000);
				fixed4 rock = (rock1 + rock2) / 2;

				float angle = abs(rad_to_deg(angle_between(IN.worldNormal, IN.worldPos - center)));
				float rock_blend = lerp_slope(angle, 15, 30);

				fixed4 color = lerp(_Color1, _Color2, noise);
				fixed4 rockColor = lerp(_Color1, _Color2, 1-noise);
				
				fixed4 tex = lerp(dust * color, rock*rockColor, rock_blend);

				o.Albedo = tex.rgb;
				o.Alpha = 1;
			}
			ENDCG
	}
		FallBack "Diffuse"
}
