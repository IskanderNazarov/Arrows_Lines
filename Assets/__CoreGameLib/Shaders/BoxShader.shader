Shader "Curstom/BoxShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SM("Smooth area", Range(0, 0.25)) = 0
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
            float _SM;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= i.col;
                float2 uv = i.uv * 2 - 1;

                float x = abs(uv.x);
                float y = abs(uv.y);
                float tx = 1 - ((x - 1 + _SM) / _SM);
                float ty = 1 - ((y - 1 + _SM) / _SM);
                col.a = tx;
                col.a *= ty;

                 /*if (x > 1 - _SM)
                {
                    if (y < _SM || y > 1 - _SM)
                    {
                        col.a = (tx + ty) / 2;
                    }
                }
                else if (x < _SM)
                {
                    if (y < _SM || y > 1 - _SM)
                    {
                        col.a = (tx + ty) / 2;
                    }
                }*/


                return col;
            }
            ENDCG
        }
    }
}