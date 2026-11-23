Shader "ImmersiveSubtitles/UI/Speech Bubble"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BubbleTex ("Bubble", 2D) = "white" {}
        _BubbleOutlineTex ("Bubble Outline", 2D) = "white" {}
        _TailTex ("Tail", 2D) = "white" {}
        _TailOutlineTex ("Tail Outline", 2D) = "white" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 fillUV : TEXCOORD0;
                float4 outlineUV : TEXCOORD1;
                fixed4 outlineColor : TEXCOORD2;
                float4 worldPosition : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _BubbleTex;
            sampler2D _BubbleOutlineTex;
            sampler2D _TailTex;
            sampler2D _TailOutlineTex;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.color = v.color;
                OUT.fillUV = v.texcoord0;
                OUT.outlineUV = v.texcoord1;
                OUT.outlineColor = v.texcoord2;
                return OUT;
            }

            half ShapeAlpha(sampler2D tex, float2 uv)
            {
                float2 inside = step(0, uv) * step(uv, 1);
                return tex2D(tex, uv).a * inside.x * inside.y;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half fillA = max(ShapeAlpha(_BubbleTex, IN.fillUV.xy), ShapeAlpha(_TailTex, IN.fillUV.zw));
                half outlineA = max(ShapeAlpha(_BubbleOutlineTex, IN.outlineUV.xy), ShapeAlpha(_TailOutlineTex, IN.outlineUV.zw)) * IN.outlineColor.a;

                half a = fillA + outlineA * (1 - fillA);
                half3 rgb = (IN.color.rgb * fillA + IN.outlineColor.rgb * outlineA * (1 - fillA)) / max(a, 0.0001);
                half4 color = half4(rgb, a * IN.color.a);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
