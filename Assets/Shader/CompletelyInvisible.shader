Shader "Custom/CompletelyInvisible"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "Invisible"
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }

            // 뎁스 버퍼에 기록하지 않음
            ZWrite Off

            // 화면의 RGB/Alpha 채널에 아무것도 기록하지 않음
            ColorMask 0

            // 일반적인 깊이 테스트는 유지
            ZTest LEqual

            Cull Back

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(0, 0, 0, 0);
            }

            ENDHLSL
        }
    }

    Fallback Off
}