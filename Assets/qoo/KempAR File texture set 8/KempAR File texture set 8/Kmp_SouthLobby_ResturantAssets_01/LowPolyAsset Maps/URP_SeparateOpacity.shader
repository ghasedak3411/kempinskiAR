Shader "Custom/URP_SeparateOpacity"
{
    Properties
    {
        _BaseMap("Base Map (RGB)", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _OpacityMap("Opacity Map (Grayscale/Alpha)", 2D) = "white" {}
        _Opacity("Opacity Multiplier", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_OpacityMap);
            SAMPLER(sampler_OpacityMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _OpacityMap_ST;
                half4 _BaseColor;
                half _Opacity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // نمونه‌برداری از تکسچر اصلی و اعمال رنگ
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                // نمونه‌برداری از تکسچر Opacity (از کانال r به عنوان ماسک سیاه و سفید استفاده می‌شود)
                half opacityMask = SAMPLE_TEXTURE2D(_OpacityMap, sampler_OpacityMap, input.uv).r;

                // محاسبه آلفای نهایی
                half finalAlpha = baseColor.a * opacityMask * _Opacity;

                return half4(baseColor.rgb, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}