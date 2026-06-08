Shader "Custom/URP/BoxClip_Lit_KeepClamp"
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

        // Alpha threshold
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
            "UniversalMaterialType"="Lit"
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
            #pragma shader_feature_local_fragment _SPECULAR_SETUP

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #pragma multi_compile_fog
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ SHADOWS_SHADOWMASK

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
                float2 uv         : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float2 uvMetallic  : TEXCOORD3;
                float2 uvEmission  : TEXCOORD4;
                float  fogCoord    : TEXCOORD5;

                #if defined(LIGHTMAP_ON)
                    float2 lightmapUV : TEXCOORD6;
                #endif
            };

            // =========================
            // Clamp logic - 기존과 동일
            // =========================
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
                o.fogCoord   = ComputeFogFactor(o.positionHCS.z);

                #if defined(LIGHTMAP_ON)
                    o.lightmapUV = v.lightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
                #endif

                return o;
            }

            inline half4 GetShadowMaskCompat()
            {
                #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
                    return SAMPLE_SHADOWMASK(0);
                #else
                    return half4(1, 1, 1, 1);
                #endif
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 p = i.worldPos;

                BoxClipDiscard(p);

                float4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float4 baseColor = baseTex * _BaseColor;

                float alpha = baseColor.a;

                #if defined(_ALPHATEST_ON)
                    clip(alpha - _Cutoff);
                #else
                    if (_Surface > 0.5 && alpha <= _AlphaThreshold)
                        discard;
                #endif

                bool isSection = IsSectionSurface(p);

                half3 albedo = baseColor.rgb;
                if (isSection && _UseSectionColor > 0.5)
                {
                    albedo = lerp(albedo, _SectionColor.rgb, _SectionColor.a);
                }

                half metallicTex = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, i.uvMetallic).r;
                half metallic = saturate(metallicTex * _Metallic);

                half3 emissionTex = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uvEmission).rgb;
                half useEmissionTex = step(0.5, _UseEmissionMap);
                half3 emissionMask = lerp(half3(1,1,1), emissionTex, useEmissionTex);
                half3 emission = emissionMask * _EmissionColor.rgb * _EmissionIntensity;

                half3 N = normalize(i.worldNormal);
                if (length(N) < 0.001)
                {
                    N = normalize(-GetWorldSpaceViewDir(p));
                }

                InputData inputData = (InputData)0;
                inputData.positionWS = p;
                inputData.normalWS = N;
                inputData.viewDirectionWS = SafeNormalize(GetWorldSpaceViewDir(p));

                if (_ReceiveShadows > 0.5)
                {
                    inputData.shadowCoord = TransformWorldToShadowCoord(p);
                }
                else
                {
                    inputData.shadowCoord = float4(0, 0, 0, 0);
                }

                inputData.fogCoord = i.fogCoord;
                inputData.vertexLighting = VertexLighting(p, N);

                #if defined(LIGHTMAP_ON)
                    inputData.bakedGI = SAMPLE_GI(i.lightmapUV, half3(0,0,0), N);
                #else
                    inputData.bakedGI = SampleSH(N);
                #endif

                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionHCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.alpha = (_Surface > 0.5) ? alpha : 1.0;
                surfaceData.emission = emission;
                surfaceData.occlusion = 1.0;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.clearCoatMask = 0.0;
                surfaceData.clearCoatSmoothness = 0.0;

                if (_WorkflowMode < 0.5)
                {
                    // Metallic workflow
                    surfaceData.metallic = metallic;
                    surfaceData.specular = half3(0.04, 0.04, 0.04);
                }
                else
                {
                    // Specular workflow
                    surfaceData.metallic = 0.0;
                    surfaceData.specular = _SpecColor.rgb;
                }

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);

                if (_Surface < 0.5)
                {
                    color.a = 1.0;
                }

                // 기존 파티클 셰이더의 BlendMode별 알파 보정과 동일한 성격을 유지
                if (_Surface > 0.5)
                {
                    // Premultiply / Additive / Multiply
                    if ((_BlendMode > 0.5 && _BlendMode < 1.5) ||
                        (_BlendMode > 1.5 && _BlendMode < 2.5) ||
                        (_BlendMode > 2.5))
                    {
                        color.rgb *= alpha;
                    }
                }

                return color;
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
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv         : TEXCOORD1;
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
                    float alpha = (baseTex * _BaseColor).a;
                    clip(alpha - _Cutoff);
                #endif

                return 0;
            }
            ENDHLSL
        }
    }

    CustomEditor "ParticleBoxClipLitGUI"
}