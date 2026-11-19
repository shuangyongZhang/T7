Shader "Custom/BlurBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Opaque" }
        GrabPass { "_GrabTexture" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _GrabTexture;
            float _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 grabUV = i.grabPos.xy / i.grabPos.w;
                
                // Simple box blur
                fixed4 col = tex2D(_GrabTexture, grabUV);
                col += tex2D(_GrabTexture, grabUV + float2(_BlurSize / _ScreenParams.x, 0));
                col += tex2D(_GrabTexture, grabUV + float2(-_BlurSize / _ScreenParams.x, 0));
                col += tex2D(_GrabTexture, grabUV + float2(0, _BlurSize / _ScreenParams.y));
                col += tex2D(_GrabTexture, grabUV + float2(0, -_BlurSize / _ScreenParams.y));
                col /= 5.0;
                
                return col;
            }
            ENDCG
        }
    }
}