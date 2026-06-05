Shader "Custom/URP/BoxClipWithSection_Tex_LitFull_TransparentAlpha"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Alpha ("Global Alpha", Range(0,1)) = 0.5

        _SectionMap ("Section Map", 2D) = "white" {}
        _SectionColor ("Section Color", Color) = (1,0,0,1)
        _UseSectionTexture ("Use Section Texture (0/1)", Float) = 0
        _SectionThickness ("Section Thickness", Float) = 0.02

        _SpecColor ("Spec Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.6

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Render Face", Float) = 2

        // URP/ShaderGraph style material state values.
        // These help keep the material conceptually aligned with:
        // Surface Type = Transparent, Blending Mode = Alpha.
        [HideInInspector] _Surface ("__surface", Float) = 1
        [HideInInspector] _Blend ("__blend", Float) = 0
        [HideInInspector] _SrcBlend ("__src", Float) = 5      // SrcAlpha
        [HideInInspector] _DstBlend ("__dst", Float) = 10     // OneMinusSrcAlpha
        [HideInInspector] _ZWrite ("__zw", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull [_Cull]

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

            // Lightmap / mixed lighting safety
            #pragma multi_compile_fragment _ _SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);    SAMPLER(sampler_BaseMap);
            TEXTURE2D(_SectionMap); SAMPLER(sampler_SectionMap);

            float4 _BaseMap_ST;
            float4 _BaseColor;
            float  _Alpha;

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
                o.uv          = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 p = i.worldPos;

                // Box 밖 제거
                BoxClipDiscard(p);

                // 단면 판정
                bool isSection = IsSectionSurface(p);

                float4 baseTex    = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float4 sectionTex = SAMPLE_TEXTURE2D(_SectionMap, sampler_SectionMap, i.uv);

                float3 albedo;
                float  alpha;

                if (isSection)
                {
                    float useTex = step(0.5, _UseSectionTexture);
                    albedo = lerp(_SectionColor.rgb, sectionTex.rgb * _SectionColor.rgb, useTex);

                    // Section은 SectionColor.a를 기본 알파로 사용.
                    // SectionMap을 쓸 경우 SectionMap.a도 같이 반영.
                    alpha = lerp(_SectionColor.a, sectionTex.a * _SectionColor.a, useTex);
                }
                else
                {
                    albedo = baseTex.rgb * _BaseColor.rgb;

                    // Transparent Alpha Blend용 알파.
                    // JPG처럼 alpha가 없는 텍스처면 baseTex.a는 1로 들어옴.
                    alpha = baseTex.a * _BaseColor.a;
                }

                alpha *= _Alpha;

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

                // Specular, Blinn-Phong
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

                        color += (albedo * l.color.rgb * NdotL) * atten;

                        float3 H = normalize(L + V);
                        float  NdotH = saturate(dot(N, H));
                        float3 specAdd = _SpecColor.rgb * pow(NdotH, specPow) * NdotL;
                        color += (specAdd * l.color.rgb) * atten;
                    }
                }
                #endif

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
