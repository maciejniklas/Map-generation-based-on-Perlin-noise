Shader "Custom/Area"
{
	Properties
	{
		mainTexture("Texture", 2D) = "white"{}
		scale("Scale", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows

		#pragma target 3.0

		const static int maxRegionsAmount = 8;

		float minimalHeight;
		float maximumHeight;
		int regionsAmount;
		float3 colors[maxRegionsAmount];
		float heights[maxRegionsAmount];
		float mixture[maxRegionsAmount];
		float textureScales[maxRegionsAmount];
		float impacts[maxRegionsAmount];
		UNITY_DECLARE_TEX2DARRAY(regionsTexture);

		sampler2D mainTexture;
		float scale;

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		float InverseLerp(float start, float end, float value)
		{
			return saturate((value - start) / (end - start));
		}

		float3 TriplanarMapping(float3 worldPos, float scale, float3 normal, int index)
		{
			float3 scaledWorldPos = worldPos / scale;

			float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(regionsTexture, float3(scaledWorldPos.y, scaledWorldPos.z, index)) * normal.x;
			float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(regionsTexture, float3(scaledWorldPos.x, scaledWorldPos.z, index)) * normal.y;
			float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(regionsTexture, float3(scaledWorldPos.x, scaledWorldPos.y, index)) * normal.z;

			return xProjection + yProjection + zProjection;
		}

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float heightUV = InverseLerp(minimalHeight, maximumHeight, IN.worldPos.y);
			float3 absoluteNormal = abs(IN.worldNormal);
			absoluteNormal /= absoluteNormal.x + absoluteNormal.y + absoluteNormal.z;

			for (int index = 0; index < regionsAmount; index++)
			{
				float hasToPainted = InverseLerp(-mixture[index] / 2 - 1E-4, mixture[index] / 2, heightUV - heights[index]);

				float3 color = colors[index] * impacts[index];
				float3 textureColor = TriplanarMapping(IN.worldPos, textureScales[index], absoluteNormal, index) * (1 - impacts[index]);

				o.Albedo = o.Albedo * (1 - hasToPainted) + (color + textureColor) * hasToPainted;
			}
        }
        ENDCG
    }
    FallBack "Diffuse"
}
