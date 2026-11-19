
Shader "Custom/ParticlesAdditive" {
  Properties {
    _MainTex ("Particle Texture", 2D) = "white" {}
  }
  SubShader { 
    Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
    BindChannels {
        Bind "vertex", Vertex
        Bind "color", Color
        Bind "texcoord", TexCoord
    }
    ZWrite Off
    Cull Off
    Fog {
        Color (0,0,0,0)
    }
    
    
    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_ST;
    
    struct VertexInput {
        float4 position:POSITION;
        fixed2 coord:TEXCOORD0;
        fixed4 color : COLOR;
    };
    struct PixelInput
    {
        float4 position:POSITION;
        fixed2 uv_MainTex:TEXCOORD;
        fixed4 color : COLOR;
    };
    
    PixelInput vert(VertexInput I)
    {
        PixelInput O;
        O.position=UnityObjectToClipPos(I.position);
        O.uv_MainTex=  TRANSFORM_TEX(I.coord,_MainTex);
        O.color = I.color;

        return O;
    }
    fixed4 frag(PixelInput I):COLOR
    {
        fixed4 c = tex2D (_MainTex, I.uv_MainTex) * I.color;
        return c;
    }
    
    fixed4 grayscale(PixelInput I): COLOR
    {
        fixed4 c = tex2D (_MainTex, I.uv_MainTex) * I.color;
        c.rgb+= 0.2;
        c.a = min(c.a, 0.3);
        return c;
    }
    ENDCG
    
    Pass 
        {
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM        
        #pragma vertex vert
        #pragma fragment grayscale
        ENDCG
    }

    
    
    Pass
    {
        Blend SrcAlpha One
        SetTexture [_MainTex] { combine texture *  previous }
        // CGPROGRAM
        // #pragma vertex vert
        // #pragma fragment frag
        // ENDCG
    }
}
}