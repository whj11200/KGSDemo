Shader "Custom/RopeTubeFill_Lit"
{
    Properties
    {
        [Header(Color)]
        _HoseColor ("Empty Hose Color", Color) = (0.01, 0.01, 0.01, 1)
        _LiquidColor ("Liquid Color", Color) = (0.0, 1.0, 0.0, 1)

        [Header(Fill)]
        _Fill ("Fill", Range(0, 1)) = 0
        _FillSoftness ("Fill Softness", Range(0.0001, 0.1)) = 0.005
        _Reverse ("Reverse", Range(0, 1)) = 0

        [Header(Fill Mode)]
        _UsePositionFill ("Use XYZ Position Fill", Range(0, 1)) = 0

        [Header(XYZ Fill Direction)]
        _FillDirection ("Fill Direction XYZ", Vector) = (1, 0, 0, 0)
        _FillMin ("Fill Min", Float) = -1
        _FillMax ("Fill Max", Float) = 1

        [Header(Empty Hose PBR)]
        _HoseMetallic ("Hose Metallic", Range(0, 1)) = 0.05
        _HoseSmoothness ("Hose Smoothness", Range(0, 1)) = 0.55

        [Header(Filled Area PBR)]
        _LiquidMetallic ("Liquid Metallic", Range(0, 1)) = 0
        _LiquidSmoothness ("Liquid Smoothness", Range(0, 1)) = 0.85

        [Header(Environment)]
        _Occlusion ("Occlusion", Range(0, 1)) = 1
        [HDR] _EmissionColor ("Emission", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;

                // 기존 텍스처 UV
                float2 uv : TEXCOORD0;

                // Fill 진행률 UV
                float2 uv2 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;

                float3 positionWS : TEXCOORD0;
                half3 normalWS    : TEXCOORD1;

                float2 uv  : TEXCOORD2;
                float2 uv2 : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)

                half4 _HoseColor;
                half4 _LiquidColor;

                float _Fill;
                float _FillSoftness;
                float _Reverse;

                float _UsePositionFill;

                float4 _FillDirection;
                float _FillMin;
                float _FillMax;

                float _HoseMetallic;
                float _HoseSmoothness;

                float _LiquidMetallic;
                float _LiquidSmoothness;

                float _Occlusion;

                half4 _EmissionColor;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(input.normalOS);

                output.positionHCS =
                    positionInputs.positionCS;

                output.positionWS =
                    positionInputs.positionWS;

                output.normalWS =
                    normalInputs.normalWS;

                output.uv =
                    input.uv;

                output.uv2 =
                    input.uv2;

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                // ==================================================
                // A. UV 기반 진행률
                //
                // RopeTubeRenderer에서
                //
                // uv2.x = 0 ~ 1 호스 전체 길이
                // ==================================================

                float uvProgress =
                    saturate(input.uv2.x);


                // ==================================================
                // B. XYZ Position 기반 진행률
                // ==================================================

                float3 objectPositionWS =
                    TransformObjectToWorld(
                        float3(0, 0, 0)
                    );


                float3 relativePosition =
                    input.positionWS -
                    objectPositionWS;


                float3 direction =
                    _FillDirection.xyz;


                float directionLength =
                    max(
                        length(direction),
                        0.0001
                    );


                direction /= directionLength;


                // 위치를 설정한 방향으로 투영
                float positionOnAxis =
                    dot(
                        relativePosition,
                        direction
                    );


                // FillMin ~ FillMax
                //          ↓
                //         0~1
                float positionProgress =
                    saturate(
                        (positionOnAxis - _FillMin)
                        /
                        max(
                            _FillMax - _FillMin,
                            0.0001
                        )
                    );


                // ==================================================
                // Fill Mode
                //
                // 0 = UV
                // 1 = XYZ
                // ==================================================

                float progress =
                    lerp(
                        uvProgress,
                        positionProgress,
                        step(
                            0.5,
                            _UsePositionFill
                        )
                    );


                // ==================================================
                // Reverse
                // ==================================================

                if (_Reverse > 0.5)
                {
                    progress =
                        1.0 - progress;
                }


                // ==================================================
                // Fill Mask
                // ==================================================

                float fillMask = 0;


                if (_Fill > 0.0001)
                {
                    fillMask =
                        1.0 -
                        smoothstep(
                            _Fill - _FillSoftness,
                            _Fill + _FillSoftness,
                            progress
                        );
                }


                if (_Fill >= 0.9999)
                {
                    fillMask = 1;
                }


                // ==================================================
                // Material
                // ==================================================

                half3 albedo =
                    lerp(
                        _HoseColor.rgb,
                        _LiquidColor.rgb,
                        fillMask
                    );


                half metallic =
                    lerp(
                        _HoseMetallic,
                        _LiquidMetallic,
                        fillMask
                    );


                half smoothness =
                    lerp(
                        _HoseSmoothness,
                        _LiquidSmoothness,
                        fillMask
                    );


                half3 normalWS =
                    normalize(
                        input.normalWS
                    );


                // ==================================================
                // URP InputData
                // ==================================================

                InputData inputData =
                    (InputData)0;


                inputData.positionWS =
                    input.positionWS;


                inputData.positionCS =
                    input.positionHCS;


                inputData.normalWS =
                    normalWS;


                inputData.viewDirectionWS =
                    GetWorldSpaceNormalizeViewDir(
                        input.positionWS
                    );


                inputData.shadowCoord =
                    TransformWorldToShadowCoord(
                        input.positionWS
                    );


                inputData.fogCoord = 0;


                inputData.vertexLighting =
                    VertexLighting(
                        input.positionWS,
                        normalWS
                    );


                inputData.bakedGI =
                    SampleSH(
                        normalWS
                    );


                inputData.normalizedScreenSpaceUV =
                    GetNormalizedScreenSpaceUV(
                        input.positionHCS
                    );


                inputData.shadowMask =
                    half4(1, 1, 1, 1);


                // ==================================================
                // Surface Data
                // ==================================================

                SurfaceData surfaceData =
                    (SurfaceData)0;


                surfaceData.albedo =
                    albedo;


                surfaceData.metallic =
                    metallic;


                surfaceData.specular =
                    half3(
                        0.5,
                        0.5,
                        0.5
                    );


                surfaceData.smoothness =
                    smoothness;


                surfaceData.normalTS =
                    half3(
                        0,
                        0,
                        1
                    );


                surfaceData.occlusion =
                    _Occlusion;


                surfaceData.emission =
                    _EmissionColor.rgb;


                surfaceData.alpha =
                    1;


                surfaceData.clearCoatMask =
                    0;


                surfaceData.clearCoatSmoothness =
                    0;


                // ==================================================
                // PBR Lighting
                // ==================================================

                half4 color =
                    UniversalFragmentPBR(
                        inputData,
                        surfaceData
                    );


                color.a = 1;

                return color;
            }

            ENDHLSL
        }
    }
}