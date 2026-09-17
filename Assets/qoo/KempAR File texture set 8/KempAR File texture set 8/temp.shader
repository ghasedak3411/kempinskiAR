Shader "Custom/URP_LitWithAO_UV2_Metallic"
{
    Properties
    {
        _BaseMap ("Base Map (UV1)", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _MetallicGlossMap ("Metallic (R) Smoothness (A) - UV1", 2D) = "white" {}
        _Metallic ("Metallic Multiplier", Range(0,1)) = 1
        _Smoothness ("Smoothness Multiplier", Range(0,1)) = 0.5

        _BumpMap ("Normal Map (UV1)", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1

        _AOMap ("AO Map (UV2)", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            // فقط هدرهای core/لایتینگ/سرفیس‌دیتا - بدون SurfaceInput.hlsl
            // (SurfaceInput.hlsl تکسچرهای _BaseMap/_BumpMap/_MetallicGlossMap رو
            // خودش از قبل تعریف می‌کنه و با تعریف دستی ما تداخل ایجاد می‌کند)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv0        : TEXCOORD0; // UV1 - آلبیدو/متال/نرمال
                float2 uv1        : TEXCOORD1; // UV2 - فقط AO
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv0         : TEXCOORD0;
                float2 uv1         : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float3 normalWS    : TEXCOORD3;
                float4 tangentWS   : TEXCOORD4;
                float3 viewDirWS   : TEXCOORD5;
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MetallicGlossMap); SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_AOMap); SAMPLER(sampler_AOMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Metallic;
                half _Smoothness;
                half _BumpScale;
                half _AOStrength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = normInputs.normalWS;
                OUT.tangentWS   = float4(normInputs.tangentWS, IN.tangentOS.w);
                OUT.viewDirWS   = GetWorldSpaceViewDir(posInputs.positionWS);

                OUT.uv0 = TRANSFORM_TEX(IN.uv0, _BaseMap);
                OUT.uv1 = IN.uv1; // UV2 خام برای AO

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- نمونه‌برداری تکسچرها ---
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv0);
                half4 albedo = baseSample * _BaseColor;

                half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, IN.uv0);
                half metallic = metallicGloss.r * _Metallic;
                half smoothness = metallicGloss.a * _Smoothness;

                half4 normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv0);
                half3 normalTS = UnpackNormalScale(normalSample, _BumpScale);

                half ao = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, IN.uv1).r; // UV2
                ao = lerp(1.0h, ao, _AOStrength);

                // --- ساخت فضای مماسی ---
                float3 bitangentWS = cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w;
                half3x3 tangentToWorld = half3x3(IN.tangentWS.xyz, bitangentWS, IN.normalWS);
                half3 normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
                normalWS = NormalizeNormalPerPixel(normalWS);

                // --- ساخت اینپوت‌های PBR ---
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = SafeNormalize(IN.viewDirWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.fogCoord = 0;
                inputData.vertexLighting = half3(0, 0, 0);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo.rgb;
                surfaceData.metallic = metallic;
                surfaceData.specular = half3(0, 0, 0);
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = half3(0, 0, 0);
                surfaceData.occlusion = ao; // AO از UV2
                surfaceData.alpha = albedo.a;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 1;

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }

        // برای اینکه این مدل خودش هم سایه بندازه
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // برای اینکه این مدل بتونه در Depth Prepass و بعضی افکت‌ها (مثل SSAO خودش) درست دیده بشه
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
