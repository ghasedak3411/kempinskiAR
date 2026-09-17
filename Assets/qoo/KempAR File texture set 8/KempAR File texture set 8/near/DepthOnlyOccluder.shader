Shader "Custom/DepthOnlyOccluder"
{
    // این شیدر هیچ رنگی رندر نمی‌کنه (کاملاً نامرئیه)
    // ولی همچنان توی Depth Buffer می‌نویسه، پس هر ابجکت دیگه‌ای
    // (مثل برج مجازی ما) که پشتش قرار بگیره، به‌درستی پنهان/کلیپ میشه.
    Properties { }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            // هیچ رنگی نوشته نمیشه - کاملاً نامرئی
            ColorMask 0

            // ولی عمق نوشته میشه - همون چیزی که برای Occlusion لازمه
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                // این خط عملاً هیچ‌وقت روی رنگ تأثیر نمی‌ذاره چون ColorMask 0 است،
                // ولی برای معتبر بودن شیدر یه خروجی لازم داریم.
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
