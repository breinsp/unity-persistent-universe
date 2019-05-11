// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/PlanetRings" {
	Properties{
		_Color1("Ring Color 1", Color) = (1,1,1,1)
		_Color2("Ring Color 2", Color) = (1,1,1,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_PlanetRadius("Planet Radius", float) = 0
		_Frequency("Frequency", float) = 10
	}
		SubShader{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			LOD 100

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass{

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "noiseSimplex.cginc"	

				uniform float3 _SunPos;

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
					float4 worldPos : COLOR0;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Color1;
				float4 _Color2;
				float _PlanetRadius;
				float _Frequency;

				inline float sphereIntersectAmount(float3 raydir, float3 rayorig, float3 center, float radius) {
					float sphereDistance = dot(raydir, center) - dot(raydir, rayorig);
					if (sphereDistance > 0.0) return 0.0;
					float3 closestPoint = sphereDistance * raydir + rayorig;
					float distanceToSphere = length(closestPoint - center);
					return distanceToSphere - radius;
				}

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.worldPos = float4(mul(unity_ObjectToWorld, v.vertex).xyz,0);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float3 lightPos = _SunPos;
					float3 center = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

					fixed v = tex2D(_MainTex, i.texcoord).r;
					float smoothingAmount = .5;
					float3 vertexPos = i.worldPos.xyz;

					float3 moved_light = (lightPos - center) * 1000 + center;

					float shadow = clamp(-sphereIntersectAmount(normalize(vertexPos - moved_light), vertexPos, center, _PlanetRadius) / smoothingAmount, 0.0, 1.0);

					float n = (snoise(float4(i.texcoord.y * _Frequency,0,0,0)) + 1) / 2;

					fixed4 col = lerp(_Color1, _Color2, n);
					col = lerp(col, float4(0, 0, 0, 1), shadow);
					col.a = v * v;

					return col;
				}
				ENDCG
			}
						}
}