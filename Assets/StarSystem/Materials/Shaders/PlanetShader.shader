Shader "Custom/PlanetShader" {
	Properties{
		_Color("Overall Color", Color) = (1,1,1,1)
		_Color1("Color 1", Color) = (1,1,1,1)
		_Texture1("Texture 1", 2D) = "white" {}
		_Color2("Color 2", Color) = (1,1,1,1)
		_Texture2("Texture 2", 2D) = "white" {}
		_Color3("Color 3", Color) = (1,1,1,1)
		_Texture3("Texture 3", 2D) = "white" {}
		_Alpha("Alpha",float) = 1.0
		_Texture4("Surface Texture", 2D) = "white" {}
		_Lod("LOD", int) = 0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _Texture1;
			sampler2D _Texture2;
			sampler2D _Texture3;
			sampler2D _Texture4;

			struct Input {
				float2 uv_Texture1;
				float3 worldNormal;
				float3 worldPos;
				float4 color : COLOR;
			};

			float _Alpha;
			fixed4 _Color;
			fixed4 _Color1;
			fixed4 _Color2;
			fixed4 _Color3;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
				UNITY_INSTANCING_BUFFER_END(Props)

				inline float angle_between(float3 v1, float3 v2) {
					return acos(dot(v1, v2) / (length(v1)*length(v2)));
				}

				inline float rad_to_deg(float rad) {
					float PI = 3.14159;
					return rad / (2 * PI) * 360;
				}

				inline float lerp_rock(float angle) {
					float blend = 0;
					float lower_end = 13.0;
					float upper_end = 20.0;
					if (angle > lower_end) {
						if (angle > upper_end) {
							blend = 1;
						}
						else {
							blend = (angle - lower_end) / (upper_end - lower_end);
						}
					}
					return blend;
				}

				void surf(Input IN, inout SurfaceOutputStandard o) {
					float3 center = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
					
					fixed4 t1 = tex2D(_Texture1, IN.uv_Texture1 * 2000.0) * _Color1;
					fixed4 t2 = tex2D(_Texture2, IN.uv_Texture1 * 2000.0) * _Color2;
					fixed4 t3 = tex2D(_Texture3, IN.uv_Texture1 * 2000.0) * _Color3;
					fixed4 t4 = tex2D(_Texture4, IN.uv_Texture1);

					float noise = IN.color.r;
					fixed4 t1_t2_blend = lerp(t1, t2, noise);

					float angle = rad_to_deg(angle_between(IN.worldNormal,IN.worldPos- center));
					float blend = lerp_rock(angle);
					fixed4 color = lerp(t1_t2_blend, t3, blend);

					color = color * t4 * _Color;

					o.Albedo = color;
					o.Metallic = 0;
					o.Smoothness = 0;
					o.Alpha = _Alpha;
				}
				ENDCG
		}
			FallBack "Diffuse"
}
