Shader "Custom/LoadingBarCirc"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _R("Strength", Range(0, 1)) = 0
        _SPEED("Speed", Range(0, 2)) = 0
        _C1("Gradient 1", Color) = (1, 0, 0, 1)
        _C2("Gradient 2", Color) = (0, 0, 0, 1)
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
            float _R;
            float _SPEED;
            float4 _C1;
            float4 _C2;

            fixed4 frag(v2f i) : SV_Target
            {
                const float PI = UNITY_PI;

                float2 uv = i.uv * 2 - 1;
                float4 col = _C1;
                float d = length(uv);
                float circ = 1 - step(1, d);
                circ *= step(_R, d);

                float delta = _Time.y * _SPEED;
                float angle01 = atan2(uv.x, uv.y); // -pi;pi
                angle01 += PI;
                angle01 /= 2 * PI;
                angle01 = frac(delta + angle01);

                float angle = atan2(uv.x, uv.y); // -pi;pi
                angle += PI; //0;2pi
                angle /= 2 * PI; //0;1
                ;
                angle = frac(angle);

                float rot_v = 1;//step(angle, 0.8);

                col.a *= circ * rot_v * angle01;


                return col;
            }
            ENDCG
        }
    }
}