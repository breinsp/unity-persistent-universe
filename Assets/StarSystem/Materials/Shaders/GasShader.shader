Shader "Custom/GasShader" {
	Properties {
		_Color1 ("Color 1", Color) = (1,1,1,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Color3 ("Color 3", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Frequency1("Frequency 1", float) = 1
		_Frequency2("Frequency 2", float) = 5
		_Strength1("Strength 1", float) = 1
		_Strength2("Strength 2", float) = .5
		_Speed("Speed", float) = 1
		_Thickness("Thickness", float) = 0.3
		_FresnelWidth("Fresnel Width", float) = 0.7
		_FresnelStrength("Fresnel Strength", float) = 1.3
		_ScatteringOffset("Scattering Offset", float) = 0.3
	}
		SubShader{
			Tags { "RenderType" = "Opaque"}
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Lambert vertex:vert

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			#include "noiseSimplex.cginc"		
			#include "UnityCG.cginc"

		uniform float3 _SunPos;

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float3 viewDir;
			float3 worldNormal;
			float3 localPos;
		};

		fixed4 _Color1;
		fixed4 _Color2;
		fixed4 _Color3;
		float _Frequency1;
		float _Frequency2;
		float _Strength1;
		float _Strength2;
		float _Speed;
		float _Thickness;
		float _FresnelWidth;
		float _FresnelStrength;
		float _ScatteringOffset;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = v.vertex.xyz;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float3 lightPos = _SunPos;
			float3 center = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

			float fresnel = 1 - saturate(dot(IN.worldNormal, normalize(IN.viewDir)));
			fresnel = smoothstep(_FresnelWidth, 1, fresnel);

			float dotp = dot(normalize(lightPos - center), normalize(IN.worldNormal)) + _ScatteringOffset;
			float factor = clamp(dotp / (1.0 + _ScatteringOffset),0,1); //normalize to 0 - 1, cut off negative values

			float4 atmo = lerp(_Color1, _Color2, factor);
			atmo *= factor;
			atmo *= (fresnel * _FresnelStrength + _Thickness);

			float3 pos = normalize(IN.localPos);
			float2 uv = float2(IN.uv_MainTex.y, IN.uv_MainTex.x);

			float time = _Time.y * 0.05 * _Speed;
			float frq1 = _Frequency1 * 6;
			float frq2 = _Frequency2 * 6;
			float n1 = snoise(float4(pos*frq1, time*2)) * _Strength1;
			float n2 = snoise(float4(pos*-frq2, time)) * _Strength2;
			float n3 = snoise(float4(pos*frq2*2, time)) * _Strength2 * 0.5;

			float n = (n1 + n2 + n3) / 100;
			uv.x += n;

			float blend1 = tex2D(_MainTex, uv).r;
			float blend2 = tex2D(_MainTex, uv).g;

			fixed4 c1 = lerp(_Color1, _Color2, blend1);
			fixed4 c2 = lerp(_Color1, _Color3, blend2);
			fixed4 c = (c1 + c2) / 2;
			c = c * factor * factor;

			o.Emission = (c + atmo)/2;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
