Shader "Custom/Area"
{
	Properties
	{

	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		const static int maxColorAmount = 8;

		float minimalHeight;
		float maximumHeight;
		int colorsAmount;
		float3 regionColors[maxColorAmount];
		float regionHeights[maxColorAmount];

        struct Input
        {
            float2 worldPos;
        };

		float InverseLerp(float start, float end, float value)
		{
			return saturate((value - start) / (end - start));
		}

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float heightPercentagePosition = InverseLerp(minimalHeight, maximumHeight, IN.worldPos.y);

			for (int index = 0; index < colorsAmount; index++)
			{
				float hasToPainted = saturate(sign(heightPercentagePosition - regionHeights[index]));
				o.Albedo = o.Albedo * (1 - hasToPainted) + regionColors[index] * hasToPainted;
			}
        }
        ENDCG
    }
    FallBack "Diffuse"
}
