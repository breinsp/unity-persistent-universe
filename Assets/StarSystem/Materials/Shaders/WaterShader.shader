Shader "Custom/WaterShader" {
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "noiseSimplex.cginc"		

			sampler2D_float _CameraDepthTexture;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
				float2 depth : TEXCOORD1;
			};

			float4 _Color;

			v2f vert(appdata_full v)
			{
				v2f o;

				float3 p = v.normal;
				float n = snoise(float4(p * 300, _Time.y * 0.1));

				float4 vert = v.vertex * (1 + n * 0.0001);

				o.vertex = UnityObjectToClipPos(vert);
				o.screenPos = ComputeScreenPos(o.vertex);
				UNITY_TRANSFER_DEPTH(o.depth);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 foamParameters = float4(0.5 ,0.5, 50, 1.0);

				half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);

				half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
				depth = LinearEyeDepth(depth);
				edgeBlendFactors = saturate(foamParameters * (depth - i.screenPos.w));
				edgeBlendFactors.y = 1.0 - edgeBlendFactors.y;

				fixed3 color = _Color.rgb + half3(1, 1, 1) * edgeBlendFactors.y;

				return fixed4(_Color.rgb,1);// fixed4(float3(1, 1, 1) * i.screenPos.w, 1);// fixed4(color, 1);
			}
			ENDCG
		}
	}
}