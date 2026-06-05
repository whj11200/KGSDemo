Shader "Custom/UndergroundSmokeCylinder_URP"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.5,0.5,0.5,1)
        _TopColor ("Top Color", Color) = (0.6,0.6,0.6,1)

        _Opacity ("Opacity (0-1 Smoke, 1-2 Flame)", Range(0,3)) = 1

        _HeightMin("Height Min (World Y)", Float) = 0
        _HeightMax("Height Max (World Y)", Float) = 1
        _HeightFadeEnd("Height Fade End (0-1)", Range(0,1)) = 0.85
        _HeightFadeSoft("Height Fade Softness", Range(0.001,0.5)) = 0.12

        _NoiseScale("Noise Scale", Float) = 2
        _NoiseSpeed("Noise Speed", Float) = 0.35
        _NoiseStrength("Noise Strength", Range(0,2)) = 1
        _NoiseContrast("Noise Contrast", Range(0.2,6)) = 2.2

        _EdgePower("Edge Power (Fresnel)", Range(0.2,8)) = 3
        _EdgeStrength("Edge Strength", Range(0,2)) = 1

        _DepthFadeDistance("Depth Fade Distance", Range(0.001,2)) = 0.25

        // -------- Spread from center (normalized) --------
        _HalfLength("Half Length (Object Y units)", Float) = 5.0
        _Spread01("Spread 0-1", Range(0,1)) = 0
        _SpreadSoft("Spread Softness", Range(0.001,0.5)) = 0.08

        // -------- Flame --------
        _FlameColor("Flame Color", Color) = (1,0.45,0.1,1)
        _FlameCoreColor("Flame Core Color", Color) = (1,0.9,0.4,1)
        _FlameSpeed("Flame Speed", Range(0,10)) = 6.0
        _FlameWidth("Flame Front Width", Range(0.001,0.3)) = 0.06
        _FlameIntensity("Flame Intensity", Range(0,5)) = 2.0
        _FlameNoise("Flame Noise", Range(0,3)) = 1.2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _TopColor;

                float _Opacity;
                float _HeightMin;
                float _HeightMax;
                float _HeightFadeEnd;
                float _HeightFadeSoft;

                float _NoiseScale;
                float _NoiseSpeed;
                float _NoiseStrength;
                float _NoiseContrast;

                float _EdgePower;
                float _EdgeStrength;

                float _DepthFadeDistance;

                float _HalfLength;
                float _Spread01;
                float _SpreadSoft;

                float4 _FlameColor;
                float4 _FlameCoreColor;
                float _FlameSpeed;
                float _FlameWidth;
                float _FlameIntensity;
                float _FlameNoise;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionOS  : TEXCOORD2;   // scaled
                float3 positionOS0 : TEXCOORD4;   // original
                float4 screenPos   : TEXCOORD3;
            };

            // ---------- small 2D value noise (cheap) ----------
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float valueNoise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash21(i);
                float b = hash21(i + float2(1,0));
                float c = hash21(i + float2(0,1));
                float d = hash21(i + float2(1,1));

                float2 u = f*f*(3.0 - 2.0*f);
                return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
            }

            float fbm2(float2 p)
            {
                float n = 0;
                float amp = 0.6;
                float freq = 1.0;
                n += amp * valueNoise2D(p * freq);
                freq *= 2.03;
                amp *= 0.5;
                n += amp * valueNoise2D(p * freq);
                return n;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float r01 = saturate(_Spread01);

                float3 posOS0 = IN.positionOS.xyz;
                float3 posOS  = posOS0;

                // Y축 중앙(0) 기준으로 길이 스케일 (0이면 0, 1이면 원래 길이)
                posOS.y = posOS0.y * r01;

                VertexPositionInputs pos = GetVertexPositionInputs(posOS);
                VertexNormalInputs   nor = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = pos.positionCS;
                OUT.positionWS  = pos.positionWS;
                OUT.normalWS    = nor.normalWS;

                OUT.positionOS  = posOS;
                OUT.positionOS0 = posOS0;

                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // -------- Height mask (World Y -> 0..1) --------
                float h01 = saturate((IN.positionWS.y - _HeightMin) / max(1e-5, (_HeightMax - _HeightMin)));

                float fadeStart = saturate(_HeightFadeEnd - _HeightFadeSoft);
                float heightFade = 1.0 - smoothstep(fadeStart, _HeightFadeEnd, h01);

                // -------- Noise (World XZ + time scroll) --------
                float t = _Time.y * _NoiseSpeed;
                float2 p = IN.positionWS.xz * _NoiseScale + float2(t, t * 0.7);

                float n = fbm2(p);
                n = pow(saturate(n), _NoiseContrast);
                float noiseMask = saturate(lerp(1.0, n, _NoiseStrength));

                // -------- Edge soft (Fresnel) --------
                float3 V = SafeNormalize(GetWorldSpaceViewDir(IN.positionWS));
                float3 N = SafeNormalize(IN.normalWS);
                float fres = pow(1.0 - saturate(dot(N, V)), _EdgePower);
                float edgeSoft = saturate(1.0 - fres * _EdgeStrength);

                // -------- Depth fade --------
                float2 uv = IN.screenPos.xy / max(1e-5, IN.screenPos.w);

                float sceneRaw = SampleSceneDepth(uv);
                float sceneEye = LinearEyeDepth(sceneRaw, _ZBufferParams);

                float raw = IN.screenPos.z / max(1e-5, IN.screenPos.w);
                float fragEye = LinearEyeDepth(raw, _ZBufferParams);

                float depthDiff = sceneEye - fragEye;
                float depthFade = saturate(depthDiff / max(1e-5, _DepthFadeDistance));

                // -------- Spread reveal (Y axis, original local) --------
                float axisPos0 = IN.positionOS0.x;
                float u01 = saturate(abs(axisPos0) / max(1e-5, _HalfLength));

                float r01 = saturate(_Spread01);
                float reveal = 1.0 - smoothstep(r01 - _SpreadSoft, r01, u01);
                reveal = saturate(reveal);

                // -------- Opacity split: 0~1 smoke, 1~2 flame --------
                float smoke01 = saturate(_Opacity);
                float flame01 = saturate(_Opacity - 1.0);

                // -------- Smoke alpha --------
                float smokeAlpha = smoke01 * heightFade * noiseMask * edgeSoft * depthFade * reveal;

                // -------- Flame spread front (from start/bottom to top) --------
                // axisPos0 기준: (-HalfLength) ~ (+HalfLength) 라고 가정
                float startToTop01 = saturate((axisPos0 + _HalfLength) / max(1e-5, (_HalfLength * 2.0)));

                // 빠르게 전진하는 front
                float front = saturate(_Time.y * _FlameSpeed);
                // _Opacity가 더 커지면 이미 더 번진 상태로 보이게 보정
                float flameProgress = saturate(front + flame01 * 0.65);

                // 얇은 "불 띠" (front 주변만)
                float bandA = smoothstep(flameProgress - _FlameWidth, flameProgress, startToTop01);
                float bandB = 1.0 - smoothstep(flameProgress, flameProgress + _FlameWidth, startToTop01);
                float band2 = saturate(bandA * bandB);

                // 불 노이즈 (연기 노이즈보다 좀 더 거칠게)
                float flameN = fbm2(p * 1.35 + float2(31.2, 9.7));
                flameN = pow(saturate(flameN), 2.0);
                float flameNoise = lerp(1.0, flameN, _FlameNoise);

                float flameMask = flame01 * band2 * flameNoise;

                float flameAlpha = flameMask * edgeSoft * depthFade * reveal * _FlameIntensity;

                // -------- Final alpha --------
                float alpha = saturate(smokeAlpha + flameAlpha);

                // -------- Colors --------
                float3 smokeCol = lerp(_BaseColor.rgb, _TopColor.rgb, h01);

                float core = pow(saturate(band2), 1.8);
                float3 flameCol = lerp(_FlameColor.rgb, _FlameCoreColor.rgb, core);

                // 불은 발광처럼 연기 위에 더하기
                float3 col = smokeCol + flameCol * flameAlpha;

                return float4(col, alpha);
            }
            ENDHLSL
        }
    }
}