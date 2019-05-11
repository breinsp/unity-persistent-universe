Shader "Custom/WaterPlanetShader" {
	Properties{
		_Color("Overall Color", Color) = (1,1,1,1)
		_SandColor("Sand Color", Color) = (1,1,1,1)
		_SandTexture("Sand Texture", 2D) = "white" {}
		_GrassColor("Grass Color", Color) = (1,1,1,1)
		_GrassTexture("Grass Texture", 2D) = "white" {}
		_DirtColor("Dirt Color", Color) = (1,1,1,1)
		_DirtTexture("Dirt Texture", 2D) = "white" {}
		_RockColor("Rock Color", Color) = (1,1,1,1)
		_RockTexture("Rock Texture", 2D) = "white" {}
		_Alpha("Alpha",float) = 1.0
		_SurfaceTexture("Surface Texture", 2D) = "white" {}
		_Lod("LOD", int) = 0
		_SandLevel("Sand Level",float) = 1.0
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		ZWrite On
		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Lambert vertex:vert

			// Use shader model 3.0 target, to get nicer looking lighting
	#pragma target 3.0

			sampler2D _SandTexture;
			sampler2D _GrassTexture;
			sampler2D _RockTexture;
			sampler2D _DirtTexture;
			sampler2D _SurfaceTexture;

			struct Input {
				float2 uv_SandTexture;
				float3 worldNormal;
				float3 worldPos;
				float4 color : COLOR;
			};

			float _Alpha;
			float _SandLevel;
			fixed4 _Color;
			fixed4 _SandColor;
			fixed4 _DirtColor;
			fixed4 _GrassColor;
			fixed4 _RockColor;

			void vert(inout appdata_full v) {
				if (length(v.normal) < .9) {
					v.normal = normalize(v.vertex);
				}
			}

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
				return clamp(blend,0,1);
			}

			inline float get_sand_level(float3 position, float noise) {

				float height = length(position);

				float lower_sand = _SandLevel * 1.0002;
				float upper_sand = _SandLevel * 1.0005;

				return clamp((height - lower_sand) / (upper_sand - lower_sand),0,1);
			}

			void surf(Input IN, inout SurfaceOutput o) {
				float3 center = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

				float noise = IN.color.r;
				float3 localPos = IN.worldPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

				float sand_level = get_sand_level(localPos, noise);

				fixed4 t1a = tex2D(_SandTexture, IN.uv_SandTexture * 2000.0) * _SandColor;
				fixed4 t1b = tex2D(_SandTexture, IN.uv_SandTexture * 500.0) * _SandColor;
				fixed4 t1 = (t1a + t1b) / 2;
				fixed4 t2a = tex2D(_GrassTexture, IN.uv_SandTexture * 2000.0) * _GrassColor;
				fixed4 t2b = tex2D(_GrassTexture, IN.uv_SandTexture * 500.0) * _GrassColor;
				fixed4 t2c = tex2D(_GrassTexture, IN.uv_SandTexture * 200.0) * _GrassColor;
				fixed4 t2 = (t2a + t2b + t2c) / 3;
				fixed4 t3a = tex2D(_DirtTexture, IN.uv_SandTexture * 2000.0) * _DirtColor;
				fixed4 t3b = tex2D(_DirtTexture, IN.uv_SandTexture * 500.0) * _DirtColor;
				fixed4 t3 = (t3a + t3b) / 2;
				fixed4 t4a = tex2D(_RockTexture, IN.uv_SandTexture * 2000.0) * _RockColor;
				fixed4 t4b = tex2D(_RockTexture, IN.uv_SandTexture * 500.0) * _RockColor;
				fixed4 t4 = (t4a + t4b) / 2;
				fixed4 t5 = tex2D(_SurfaceTexture, IN.uv_SandTexture);

				fixed4 grass_dirt_blend = lerp(t2, t3, noise);

				float angle = abs(rad_to_deg(angle_between(IN.worldNormal,IN.worldPos - center)));
				float rock_blend = lerp_slope(angle, 15, 30);
				float slope_factor = 1;// -clamp(angle / 50, 0, 1);

				fixed4 sand_blend = lerp(t1, grass_dirt_blend, sand_level);
				fixed3 final = lerp(sand_blend, t4, rock_blend).rgb;
				final = clamp(final * t5 * _Color, fixed3(0, 0, 0), fixed3(1, 1, 1)) * slope_factor;

				o.Albedo = final;	
				o.Alpha = _Alpha;
			}
			ENDCG
		}
		FallBack "Diffuse"
}
