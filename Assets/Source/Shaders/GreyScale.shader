Shader "Custom/UI/GreyScale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Pass
        {
            Name "Default"
            Tags { "RenderType"="Transparent" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            v2f vert (appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                return OUT;
            }
            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, IN.uv);
                float luminance = 0.2126 * texColor.r + 0.7152 * texColor.g + 0.0722 * texColor.b;
                return fixed4(luminance, luminance, luminance, 1);
            }
            ENDCG
        }
    }
}