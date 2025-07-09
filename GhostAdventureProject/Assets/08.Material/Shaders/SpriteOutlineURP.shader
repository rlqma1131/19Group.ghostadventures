Shader "URP/SimpleSpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _Thickness ("Thickness", Float) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 100

        Pass
        {
            Name "OutlinePass"
            Tags { "LightMode"="UniversalForward" }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float alpha = tex2D(_MainTex, i.uv).a;

                if (alpha > 0.1)
                {
                    discard;
                }

                float2 texel = float2(_Thickness / _ScreenParams.x, _Thickness / _ScreenParams.y);
                float outline = 0.0;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x, y) * texel;
                        outline += tex2D(_MainTex, i.uv + offset).a;
                    }
                }

                if (outline > 0)
                {
                    return _OutlineColor;
                }

                discard;
            }
            ENDHLSL
        }
    }
}
