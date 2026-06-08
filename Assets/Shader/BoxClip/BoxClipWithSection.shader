Shader "Custom/URP/BoxClipWithSection_Tex_LitFull_NoIncludeShadowPass"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color", Color) = (1,1,1,1)

        _SectionMap ("Section Map", 2D) = "white" {}
        _SectionColor ("Section Color", Color) = (1,0,0,1)
        _UseSectionTexture ("Use Section Texture (0/1)", Float) = 0
        _SectionThickness ("Section Thickness", Float) = 0.02

        _SpecColor ("Spec Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        // =========================================================
        // ForwardLit : 메인/추가 라이트 + 그림자 받기 + SH(환경광)
        // =========================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Main light shadows
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // Additional lights
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            // (선택) shadowmask/lightmap 안전빵
            #pragma multi_compile_fragment _ _SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);    SAMPLER(sampler_BaseMap);
            TEXTURE2D(_SectionMap); SAMPLER(sampler_SectionMap);

            float4 _BaseColor;
            float4 _SectionColor;
            float  _UseSectionTexture;
            float  _SectionThickness;

            float4 _SpecColor;
            float  _Smoothness;

            float3 _BoxMin;
            float3 _BoxMax;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv          : TEXCOORD2;
            };

            inline void BoxClipDiscard(float3 p)
            {
                if (p.x < _BoxMin.x || p.x > _BoxMax.x ||
                    p.y < _BoxMin.y || p.y > _BoxMax.y ||
                    p.z < _BoxMin.z || p.z > _BoxMax.z)
                {
                    discard;
                }
            }

            inline bool IsSectionSurface(float3 p)
            {
                return
                    abs(p.x - _BoxMin.x) < _SectionThickness ||
                    abs(p.x - _BoxMax.x) < _SectionThickness ||
                    abs(p.y - _BoxMin.y) < _SectionThickness ||
                    abs(p.y - _BoxMax.y) < _SectionThickness ||
                    abs(p.z - _BoxMin.z) < _SectionThickness ||
                    abs(p.z - _BoxMax.z) < _SectionThickness;
            }

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.worldPos    = TransformObjectToWorld(v.positionOS.xyz);
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.positionHCS = TransformWorldToHClip(o.worldPos);
                o.uv          = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 p = i.worldPos;

                // Box 밖 제거
                BoxClipDiscard(p);

                // 단면 판정
                bool isSection = IsSectionSurface(p);

                // 알베도 선택
                float3 baseTex    = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb;
                float3 sectionTex = SAMPLE_TEXTURE2D(_SectionMap, sampler_SectionMap, i.uv).rgb;

                float3 albedo;
                if (isSection)
                {
                    float useTex = step(0.5, _UseSectionTexture);
                    albedo = lerp(_SectionColor.rgb, sectionTex * _SectionColor.rgb, useTex);
                }
                else
                {
                    albedo = baseTex * _BaseColor.rgb;
                }

                // Lighting
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(GetWorldSpaceViewDir(p));
                float3 ambient = SampleSH(N);

                float specPow = lerp(8.0, 256.0, _Smoothness);

                // Main Light + shadow receive
                float4 shadowCoord = TransformWorldToShadowCoord(p);
                Light mainLight = GetMainLight(shadowCoord);

                float3 Lm = normalize(-mainLight.direction);
                float  NdotLm = saturate(dot(N, Lm));
                float  mainAtten = mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                float3 color = 0;

                // Diffuse
                color += albedo * (ambient + mainLight.color.rgb * NdotLm * mainAtten);

                // Spec (Blinn-Phong)
                float3 Hm = normalize(Lm + V);
                float  NdotHm = saturate(dot(N, Hm));
                float3 specMain = _SpecColor.rgb * pow(NdotHm, specPow) * NdotLm;
                color += specMain * mainLight.color.rgb * mainAtten;

                // Additional Lights
                #if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
                {
                    uint lightCount = GetAdditionalLightsCount();
                    for (uint li = 0u; li < lightCount; li++)
                    {
                        Light l = GetAdditionalLight(li, p);

                        float3 L = normalize(l.direction);
                        float  NdotL = saturate(dot(N, L));
                        float  atten = l.distanceAttenuation * l.shadowAttenuation;

                        // Diffuse
                        color += (albedo * l.color.rgb * NdotL) * atten;

                        // Spec
                        float3 H = normalize(L + V);
                        float  NdotH = saturate(dot(N, H));
                        float3 specAdd = _SpecColor.rgb * pow(NdotH, specPow) * NdotL;
                        color += (specAdd * l.color.rgb) * atten;
                    }
                }
                #endif

                return half4(color, 1);
            }
            ENDHLSL
        }

        // =========================================================
        // ShadowCaster : 그림자 투영 (include 없이 자체 구현)
        // =========================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow

            // URP가 shadow caster에서 라이트 타입에 따라 이 키워드 사용
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float3 _BoxMin;
            float3 _BoxMax;

            // URP ShadowCaster에서 세팅되는 전역값들(버전 차이를 피하려고 우리가 선언만 함)
            float3 _LightDirection;   // directional
            float3 _LightPosition;    // punctual(근사)
            float4 _ShadowBias;       // (depthBias, normalBias, unused, unused)

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            inline float3 ApplySimpleShadowBias(float3 positionWS, float3 normalWS, float3 lightDirWS)
            {
                // 최소한의 바이어스(아크네 줄이기 용). 프로젝트마다 값은 URP가 세팅해줌.
                positionWS += lightDirWS * _ShadowBias.x;
                positionWS += normalWS   * _ShadowBias.y;
                return positionWS;
            }

            Varyings vertShadow(Attributes v)
            {
                Varyings o;

                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS   = normalize(TransformObjectToWorldNormal(v.normalOS));

                float3 lightDirWS;
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    // punctual: 월드에서 "광원 방향" 근사
                    lightDirWS = normalize(_LightPosition - positionWS);
                #else
                    // directional
                    lightDirWS = normalize(_LightDirection);
                #endif

                positionWS = ApplySimpleShadowBias(positionWS, normalWS, lightDirWS);

                o.positionWS = positionWS;

                // ShadowCaster 패스에서는 URP가 VP 행렬을 라이트 기준으로 세팅함
                o.positionCS = TransformWorldToHClip(positionWS);

                return o;
            }

            half4 fragShadow(Varyings i) : SV_Target
            {
                float3 p = i.positionWS;

                // Box 밖이면 그림자도 제거(클립 모양 그대로 투영)
                if (p.x < _BoxMin.x || p.x > _BoxMax.x ||
                    p.y < _BoxMin.y || p.y > _BoxMax.y ||
                    p.z < _BoxMin.z || p.z > _BoxMax.z)
                {
                    discard;
                }

                return 0;
            }
            ENDHLSL
        }
    }
}