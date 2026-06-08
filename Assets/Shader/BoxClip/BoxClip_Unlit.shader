Shader "Custom/UI/BoxClip_Unlit"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color ("Tint", Color) = (1,1,1,1)

        _BoxMin ("Box Min World", Vector) = (-9999,-9999,-9999,0)
        _BoxMax ("Box Max World", Vector) = (9999,9999,9999,0)

        [Toggle] _UseBoxClip ("Use Box Clip", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UIBoxClip"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_ST;
            float4 _Color;

            float4 _BoxMin;
            float4 _BoxMax;
            float _UseBoxClip;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 color : COLOR;
                float2 uv : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);

                o.positionWS = worldPos;
                o.positionHCS = TransformWorldToHClip(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                if (_UseBoxClip > 0.5)
                {
                    if (i.positionWS.x < _BoxMin.x || i.positionWS.x > _BoxMax.x ||
                        i.positionWS.y < _BoxMin.y || i.positionWS.y > _BoxMax.y ||
                        i.positionWS.z < _BoxMin.z || i.positionWS.z > _BoxMax.z)
                    {
                        discard;
                    }
                }

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;

                clip(col.a - 0.001);

                return col;
            }
            ENDHLSL
        }
    }
}