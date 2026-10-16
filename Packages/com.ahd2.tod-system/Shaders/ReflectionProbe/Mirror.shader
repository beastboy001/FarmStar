Shader "Hidden/ReflectorProbe/Mirror"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }

		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Blend Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_MaskTex);
			SAMPLER(sampler_MaskTex);

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float2 uv : TEXCOORD0;
				float4 positionCS : SV_POSITION;
			};

			Varyings vert(Attributes v)
			{
				Varyings o;
				o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
				o.uv = v.uv;
				o.uv.x = 1 - o.uv.x;
				return o;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				half mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, IN.uv).x;
				half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
				col.a = mask;
				return col;
			}
			ENDHLSL
		}
	}
}
