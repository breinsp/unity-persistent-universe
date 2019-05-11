Shader "Custom/IcePlanetShader" {
	Properties{
		_SnowColor("Snow Color", Color) = (1,1,1,1)
		_DirtColor("Dirt Color", Color) = (1,1,1,1)
		_RockColor("Rock Color", Color) = (1,1,1,1)
		_SnowTex("Snow (RGB)", 2D) = "white" {}
		_RockTex("Rock (RGB)", 2D) = "white" {}
		_DirtTex("Dirt (RGB)", 2D) = "white" {}
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#pragma surface surf Lambert fullforwardshadows
#pragma target 3.0

		sampler2D _SnowTex;
		sampler2D _RockTex;
		sampler2D _DirtTex;

		struct Input {
			float2 uv_SnowTex;
			float4 color : COLOR;
			float3 worldNormal;
			float3 worldPos;
		};
		fixed4 _SnowColor;
		fixed4 _DirtColor;
		fixed4 _RockColor;

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

			fixed4 snow1 = tex2D(_SnowTex, IN.uv_SnowTex * 1000);
			fixed4 snow2 = tex2D(_SnowTex, IN.uv_SnowTex * 5000);
			fixed4 snow3 = tex2D(_SnowTex, IN.uv_SnowTex * 10000);
			fixed4 snow = (snow1 + snow2 + snow3) / 3 * _SnowColor;

			fixed4 rock1 = tex2D(_RockTex, IN.uv_SnowTex * 2000);
			fixed4 rock2 = tex2D(_RockTex, IN.uv_SnowTex * 10000);
			fixed4 rock = (rock1 + rock2) / 2 * _RockColor;

			fixed4 dirt1 = tex2D(_DirtTex, IN.uv_SnowTex * 2000);
			fixed4 dirt2 = tex2D(_DirtTex, IN.uv_SnowTex * 10000);
			fixed4 dirt = (dirt1 + dirt2) / 2 * _DirtColor;

			float angle = abs(rad_to_deg(angle_between(IN.worldNormal, IN.worldPos - center)));
			float rock_blend = lerp_slope(angle, 20, 30);
			float dirt_blend = lerp_slope(angle, 15, 20);
			float slope_factor = 1 - clamp(angle / 50, 0, 1);

			fixed4 snow_dirt = lerp(snow, dirt, dirt_blend *0.85);
			fixed4 tex = lerp(snow_dirt, rock, rock_blend);

			o.Albedo = tex.rgb *	0.8;
			o.Alpha = 1;
		}
		ENDCG
	}
		FallBack "Diffuse"
}
