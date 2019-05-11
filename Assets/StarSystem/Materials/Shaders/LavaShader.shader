Shader "Custom/LavaShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Intensity("Intensity", float) = 1
		_Speed("Speed", float) = 0
		_Frequency("Frequency", float) = 0
		_OffsetStrength("Offset Strength", float) = 0.5

	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#include "noiseSimplex.cginc"		

			#pragma surface surf Lambert fullforwardshadows
			#pragma target 3.0
			#pragma vertex vert

			struct Input {
				float3 worldPos;
				float3 worldNormal;
				float4 color : COLOR;
				float3 viewDir;
			};

			fixed4 _Color;
			float _Intensity;
			float _Speed;
			float _Frequency;
			float _OffsetStrength;

			void vert(inout appdata_full v)
			{
				float3 p = v.normal;
				float n = snoise(float4(p * 300, _Time.y * 0.1));
				float4 vert = v.vertex * (1 + n * 0.0001);
				v.vertex = vert;
				v.color = fixed4(0,0,0,0);
			}

			void surf(Input IN, inout SurfaceOutput o) {
				float fresnel = pow(saturate(dot(IN.worldNormal, normalize(IN.viewDir))),2) * 0.8 + 0.2;
				
				float noisex = snoise(float4(float3(IN.worldNormal * _Frequency) + float3(0, 0, 10), _Time.y * _Speed*1));
				float noisey = snoise(float4(float3(IN.worldNormal * _Frequency) + float3(0, 0, 50), _Time.y * _Speed*2));
				float noisez = snoise(float4(float3(IN.worldNormal * _Frequency) + float3(0, 0, 100), _Time.y * _Speed*0.5));

				float3 offset = float3(noisex, noisey, noisez) * _OffsetStrength;

				float noise1 = snoise(float4(float3(IN.worldNormal * _Frequency + offset), _Time.y * _Speed));
				float noise2 = snoise(float4(float3(IN.worldNormal * _Frequency / 10 + offset), _Time.y * _Speed));

				float noise = (noise1 + noise2) / 2;

				fixed3 color = lerp(_Color.rgb, _Color.rgb / 2, noise);

				o.Emission = color * _Intensity * fresnel;
				o.Alpha = 1;
			}
			ENDCG
	}
		FallBack "Diffuse"
}
