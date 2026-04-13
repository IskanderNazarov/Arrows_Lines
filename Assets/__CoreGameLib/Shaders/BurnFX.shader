Shader "Custom/BurnFX"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _P("Strength", Range(0, 1)) = 0
        _A("A", Range(0, 1)) = 0
        _B("B", Range(0, 1)) = 0
    }

    SubShader
    {
        // No culling or depth


        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        Pass
        {
            ZTest off
            Blend SrcAlpha OneMinusSrcAlpha
            cull off

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

            //[a,b] -> [0,1]
            float inverseLerp(float a, float b, float v)
            {
                return (v - a) / (b - a);
            }

            float remap(float aFrom, float bFrom, float v, float aTo, float bTo)
            {
                return aTo + (bTo - aTo) * (v - aFrom) / (bFrom - aFrom);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.col = v.col;
                return o;
            }

            sampler2D _MainTex;
            float _P;
            float _A;
            float _B;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 texCol = tex2D(_MainTex, uv);
                texCol.rgb *= (_P * (i.col.rgb - 1) + 1);
                


                //col.rgb *= inverseLerp(0, _D, col.rgb);
                float r = _P * inverseLerp(_A, _B, texCol.r) + (1 - _P) * texCol.r;
                float g = _P * inverseLerp(_A, _B, texCol.g) + (1 - _P) * texCol.g;
                float b = _P * inverseLerp(_A, _B, texCol.b) + (1 - _P) * texCol.b;
                texCol = float4(r, g, b, texCol.a);


                return texCol;
            }
            ENDCG
        }
    }
}