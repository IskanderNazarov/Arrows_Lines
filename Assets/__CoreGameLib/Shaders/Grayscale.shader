Shader "Curstom/Grayscale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest off
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        blend SrcAlpha OneMinusSrcAlpha

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
                float4 col: COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 col: COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.col = v.col;
                return o;
            }

            sampler2D _MainTex;
            bool IsVertical;
            float4 _C1;
            float4 _C2;

            fixed4 frag(v2f i) : SV_Target
            {
                float4 t_col = tex2D(_MainTex, i.uv);
                float4 col = t_col * i.col;

                float v = col.r * 0.299 + col.g * 0.587 + col.b * 0.114;
                col = float4(v, v, v, col.a);
                return col;
            }
            ENDCG
        }
    }
}