Shader "Custom/VolumetricAtmosphereShader"
{
	Properties
	{
		_Color1("Color 1", Color) = (1,1,1,1)
		_Color2("Color 2", Color) = (1,1,1,1)
		_PlanetRadius("Planet Radius", float) = 0
		_AtmosphereThickness("Atmosphere Thickness", float) = 0
		_ScatteringOffset("Scattering Offset", float) = 0.3
	}
		SubShader
		{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 100

			ZWrite Off
			Blend SrcColor OneMinusSrcColor
			Cull Front
			CGPROGRAM
			#pragma surface surf Lambert vertex:vert

			#include "UnityCG.cginc"

			uniform float3 _SunPos;

			fixed4 _Color1;
			fixed4 _Color2;
			float _PlanetRadius;
			float _AtmosphereThickness;
			float _ScatteringOffset;

			struct Input {
				float3 worldPos;
				float3 viewDir;
				float3 worldNormal;
				float3 localPos;
			};

			void vert(inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input, o);
				o.localPos = v.vertex.xyz;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				float3 lightPos = _SunPos;
				float outer_radius = _PlanetRadius + _AtmosphereThickness;

				float3 worldPos = IN.worldPos;

				float3 center = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
				float3 sample_point_n = normalize(worldPos - center);
				float3 sample_point = center + (sample_point_n * outer_radius);
				float3 light_dir = normalize(lightPos - center);
				float3 player = _WorldSpaceCameraPos;
				float3 player_to_point = normalize(sample_point - player);
				float3 player_to_center = center - player;
				float3 player_to_light = normalize(lightPos - player);

				float light_dotp = dot(light_dir, IN.worldNormal) + _ScatteringOffset;
				float light_multiplier = clamp(light_dotp / (1 + _ScatteringOffset),0,1);

				float4 color = lerp(_Color2,_Color1,pow(light_multiplier, .25));

				float atmo_density = 0;

				float3 intersect = cross(player_to_point, center - player);
				float distance = 0;
				float dotp = dot(player_to_point, normalize(player_to_center));

				if (dotp > 0) {
					distance = length(intersect);
				}
				else {
					distance = length(player - center);
				}

				atmo_density = 1 - ((distance - _PlanetRadius) / _AtmosphereThickness);
				atmo_density = pow(atmo_density, 1);
				atmo_density = clamp(atmo_density - 0.2, 0, 1);
				dotp = clamp(dotp, 0, 1);

				float flare = clamp(dot(player_to_point, player_to_light), 0, 1);
				float bg_size = atmo_density * .5;
				float flare_size = atmo_density * 0.05;
				float bg = clamp((flare - (1 - bg_size)),0,1) / bg_size;
				bg = pow(bg, 4) * .5;
				flare = clamp((flare - (1 - flare_size)), 0, 1) / flare_size;
				flare = pow(flare, 4) * 2 + bg;

				color = color *(1 + dotp * 2);

				float4 result = fixed4(color.rgb, atmo_density);
				result = result * light_multiplier + flare;
				result = clamp(result, 0, 5);

				o.Emission = result.xyz * atmo_density;
			}
			ENDCG
	}
}
