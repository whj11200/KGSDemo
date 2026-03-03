Shader "Custom/URP/BoxClipWithSection_Tex_LitFull_FIXED_LIGHTDIR"
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

        _AmbientStrength ("Ambient Strength", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

        // =========================================================
        // ForwardLit : 메인/추가 라이트 + 그림자 받기
        // =========================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

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
            float  _AmbientStrength;

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
                    discard;
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
                BoxClipDiscard(p);

                bool isSection = IsSectionSurface(p);

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

                float3 N = normalize(i.worldNormal);
                float3 V = normalize(GetWorldSpaceViewDir(p));

                // Ambient
                float3 ambient = SampleSH(N) * _AmbientStrength;

                float specPow = lerp(8.0, 256.0, _Smoothness);

                // Main light ( 방향 부호 수정: - 제거)
                float4 shadowCoord = TransformWorldToShadowCoord(p);
                Light mainLight = GetMainLight(shadowCoord);

                float3 Lm = normalize(mainLight.direction);
                float  NdotLm = saturate(dot(N, Lm));
                float  mainAtten = mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                float3 color = 0;

                // Diffuse (main)
                color += albedo * (ambient + mainLight.color.rgb * NdotLm * mainAtten);

                // Spec (main)
                float3 Hm = normalize(Lm + V);
                float  NdotHm = saturate(dot(N, Hm));
                float3 specMain = _SpecColor.rgb * pow(NdotHm, specPow) * NdotLm;
                color += specMain * mainLight.color.rgb * mainAtten;

                // Additional lights ( l.direction도 그대로 사용)
                #if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
                {
                    uint lightCount = GetAdditionalLightsCount();
                    for (uint li = 0u; li < lightCount; li++)
                    {
                        Light l = GetAdditionalLight(li, p);

                        float3 L = normalize(l.direction);
                        float  NdotL = saturate(dot(N, L));
                        float  atten = l.distanceAttenuation * l.shadowAttenuation;

                        color += (albedo * l.color.rgb * NdotL) * atten;

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
        // ShadowCaster : 그림자 투영 (클립만 적용)
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float3 _BoxMin;
            float3 _BoxMax;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vertShadow(Attributes v)
            {
                Varyings o;
                float3 ws = TransformObjectToWorld(v.positionOS.xyz);
                o.positionWS = ws;
                o.positionCS = TransformWorldToHClip(ws); // 라이트 VP는 URP가 설정
                return o;
            }

            half4 fragShadow(Varyings i) : SV_Target
            {
                float3 p = i.positionWS;

                if (p.x < _BoxMin.x || p.x > _BoxMax.x ||
                    p.y < _BoxMin.y || p.y > _BoxMax.y ||
                    p.z < _BoxMin.z || p.z > _BoxMax.z)
                    discard;

                return 0;
            }
            ENDHLSL
        }
    }
}