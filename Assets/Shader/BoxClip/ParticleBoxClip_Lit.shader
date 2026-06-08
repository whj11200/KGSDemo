Shader "Custom/URP/ParticleBoxClip_Lit_Advanced"
{
    Properties
    {
        // =========================
        // Surface Options
        // =========================
        [Enum(Metallic,0,Specular,1)] _WorkflowMode ("Workflow Mode", Float) = 0
        [Enum(Opaque,0,Transparent,1)] _Surface ("Surface Type", Float) = 1
        [Enum(Alpha,0,Premultiply,1,Additive,2,Multiply,3)] _BlendMode ("Blending Mode", Float) = 0
        [Toggle] _PreserveSpecular ("Preserve Specular Lighting", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Render Face", Float) = 2
        [Toggle(_ALPHATEST_ON)] _AlphaClip ("Alpha Clipping", Float) = 0
        _Cutoff ("Threshold", Range(0,1)) = 0.5
        [Toggle] _ReceiveShadows ("Receive Shadows", Float) = 1

        // =========================
        // Surface Inputs
        // =========================
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)

        _SectionColor ("Clip Edge Color", Color) = (1,0.2,0,1)
        _SectionThickness ("Section Thickness", Float) = 0.02
        _UseSectionColor ("Use Section Color", Float) = 0

        // Metallic workflow
        _Metallic ("Metallic", Range(0,1)) = 0
        _MetallicGlossMap ("Metallic Map", 2D) = "white" {}

        // Specular workflow
        _SpecColor ("Specular Color", Color) = (0.2,0.2,0.2,1)

        _Smoothness ("Smoothness", Range(0,1)) = 0.3

        // Emission
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("Emission Map", 2D) = "white" {}
        _UseEmissionMap ("Use Emission Map", Float) = 0
        _EmissionIntensity ("Emission Intensity", Float) = 1

        // Particle alpha threshold
        _AlphaThreshold ("Soft Alpha Threshold", Range(0,1)) = 0.001

        // Hidden render states (CustomEditor가 제어)
        [HideInInspector] _SrcBlend ("__src", Float) = 5
        [HideInInspector] _DstBlend ("__dst", Float) = 10
        [HideInInspector] _ZWrite ("__zw", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);              SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MetallicGlossMap);     SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_EmissionMap);          SAMPLER(sampler_EmissionMap);

            float4 _BaseMap_ST;
            float4 _MetallicGlossMap_ST;
            float4 _EmissionMap_ST;

            float _WorkflowMode;
            float _Surface;
            float _BlendMode;
            float _PreserveSpecular;
            float _AlphaClip;
            float _Cutoff;
            float _ReceiveShadows;

            float4 _BaseColor;

            float4 _SectionColor;
            float _SectionThickness;
            float _UseSectionColor;

            float _Metallic;
            float4 _SpecColor;
            float _Smoothness;

            float4 _EmissionColor;
            float _UseEmissionMap;
            float _EmissionIntensity;

            float _AlphaThreshold;

            float3 _BoxMin;
            float3 _BoxMax;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float2 uvMetallic  : TEXCOORD3;
                float2 uvEmission  : TEXCOORD4;
                float4 color       : COLOR;
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

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.worldPos    = TransformObjectToWorld(v.positionOS.xyz);
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.positionHCS = TransformWorldToHClip(o.worldPos);

                o.uv         = TRANSFORM_TEX(v.uv, _BaseMap);
                o.uvMetallic = TRANSFORM_TEX(v.uv, _MetallicGlossMap);
                o.uvEmission = TRANSFORM_TEX(v.uv, _EmissionMap);
                o.color      = v.color;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 p = i.worldPos;

                BoxClipDiscard(p);

                float4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float4 particleColor = baseTex * _BaseColor * i.color;

                float alpha = particleColor.a;

                #if defined(_ALPHATEST_ON)
                    clip(alpha - _Cutoff);
                #else
                    if (_Surface > 0.5 && alpha <= _AlphaThreshold)
                        discard;
                #endif

                bool isSection = IsSectionSurface(p);

                float3 albedo = particleColor.rgb;
                if (isSection && _UseSectionColor > 0.5)
                {
                    albedo = lerp(albedo, _SectionColor.rgb, _SectionColor.a);
                }

                float metallicTex = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, i.uvMetallic).r;
                float metallic = saturate(metallicTex * _Metallic);

                float3 diffuseColor;
                float3 specColor;

                // Workflow Mode
                if (_WorkflowMode < 0.5)
                {
                    // Metallic
                    float3 dielectricSpec = float3(0.04, 0.04, 0.04);
                    specColor = lerp(dielectricSpec, albedo, metallic);
                    diffuseColor = albedo * (1.0 - metallic);
                }
                else
                {
                    // Specular
                    specColor = _SpecColor.rgb;
                    diffuseColor = albedo;
                }

                float3 N = normalize(i.worldNormal);
                if (length(N) < 0.001)
                {
                    N = normalize(-GetWorldSpaceViewDir(p));
                }

                float3 V = normalize(GetWorldSpaceViewDir(p));
                float3 ambient = SampleSH(N);

                float specPow = lerp(8.0, 128.0, _Smoothness);

                float3 color = diffuseColor * ambient;

                // Main light
                float4 shadowCoord = TransformWorldToShadowCoord(p);
                Light mainLight = GetMainLight(shadowCoord);

                float3 Lm = normalize(-mainLight.direction);
                float NdotLm = saturate(dot(N, Lm));

                float mainShadow = lerp(1.0, mainLight.shadowAttenuation, _ReceiveShadows);
                float mainAtten = mainLight.distanceAttenuation * mainShadow;

                color += diffuseColor * mainLight.color.rgb * NdotLm * mainAtten;

                float3 Hm = normalize(Lm + V);
                float NdotHm = saturate(dot(N, Hm));
                float3 specMain = specColor * pow(NdotHm, specPow) * NdotLm;

                if (_Surface > 0.5 && _PreserveSpecular < 0.5)
                    specMain *= alpha;

                color += specMain * mainLight.color.rgb * mainAtten;

                // Additional lights
                #if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
                {
                    uint lightCount = GetAdditionalLightsCount();
                    for (uint li = 0u; li < lightCount; li++)
                    {
                        Light l = GetAdditionalLight(li, p);

                        float3 L = normalize(l.direction);
                        float NdotL = saturate(dot(N, L));
                        float shadowAtten = lerp(1.0, l.shadowAttenuation, _ReceiveShadows);
                        float atten = l.distanceAttenuation * shadowAtten;

                        color += diffuseColor * l.color.rgb * NdotL * atten;

                        float3 H = normalize(L + V);
                        float NdotH = saturate(dot(N, H));
                        float3 specAdd = specColor * pow(NdotH, specPow) * NdotL;

                        if (_Surface > 0.5 && _PreserveSpecular < 0.5)
                            specAdd *= alpha;

                        color += specAdd * l.color.rgb * atten;
                    }
                }
                #endif

                // Emission
                float3 emissionTex = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uvEmission).rgb;
                float useEmissionTex = step(0.5, _UseEmissionMap);
                float3 emissionMask = lerp(float3(1,1,1), emissionTex, useEmissionTex);
                float3 emission = emissionMask * _EmissionColor.rgb * _EmissionIntensity;

                color += emission;

                // Blend mode 보정
                if (_Surface > 0.5)
                {
                    // Premultiply
                    if (_BlendMode > 0.5 && _BlendMode < 1.5)
                    {
                        float3 specKeep = 0;
                        if (_PreserveSpecular > 0.5)
                            specKeep = color * 0; // 자리만 유지

                        color *= alpha;
                    }
                    // Additive
                    else if (_BlendMode > 1.5 && _BlendMode < 2.5)
                    {
                        color *= alpha;
                    }
                    // Multiply
                    else if (_BlendMode > 2.5)
                    {
                        color *= alpha;
                    }
                }
                else
                {
                    alpha = 1.0;
                }

                return half4(color, alpha);
            }
            ENDHLSL
        }

        // Opaque/AlphaClip에서만 의미 있음. 투명 파티클이면 보통 꺼도 됨.
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float _Cutoff;

            float3 _BoxMin;
            float3 _BoxMax;

            float3 _LightDirection;
            float3 _LightPosition;
            float4 _ShadowBias;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv         : TEXCOORD1;
                float4 color      : COLOR;
            };

            inline float3 ApplySimpleShadowBias(float3 positionWS, float3 normalWS, float3 lightDirWS)
            {
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
                    lightDirWS = normalize(_LightPosition - positionWS);
                #else
                    lightDirWS = normalize(_LightDirection);
                #endif

                positionWS = ApplySimpleShadowBias(positionWS, normalWS, lightDirWS);

                o.positionWS = positionWS;
                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.color = v.color;
                return o;
            }

            half4 fragShadow(Varyings i) : SV_Target
            {
                float3 p = i.positionWS;

                if (p.x < _BoxMin.x || p.x > _BoxMax.x ||
                    p.y < _BoxMin.y || p.y > _BoxMax.y ||
                    p.z < _BoxMin.z || p.z > _BoxMax.z)
                {
                    discard;
                }

                #if defined(_ALPHATEST_ON)
                    float4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                    float alpha = (baseTex * _BaseColor * i.color).a;
                    clip(alpha - _Cutoff);
                #endif

                return 0;
            }
            ENDHLSL
        }
    }

    CustomEditor "ParticleBoxClipLitGUI"
}