Shader "Curstom/GradientMirrored"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _C1("Color start", Color) = (0, 0, 0, 1)
        _C2("Color end", Color) = (0, 0, 0, 1)
        _W("GradientWidth", Range(0.001, 1)) = 0
        _H("GradientHeight", Range(0.001, 1)) = 0
        [MaterialToggle]IsVertical("Is vertical", float) = 0
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
            float _W;
            float _H;
            float4 _C1;
            float4 _C2;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = abs(i.uv * 2 - 1);
                fixed4 col = lerp(_C1, _C2, IsVertical ? uv.y : uv.x);

                float vx = uv.x;
                float vy = uv.y;

                float w = _W;
                float h = _H;
                //col = float3(0, 0, 0);
                float aw = 1 - step(1 - w, vx) * (vx - 1 + w) / w; // + step(v, w);
                float ah = 1 - step(1 - h, vy) * (vy - 1 + h) / h; // + step(vy, h);
                return float4(col.rgb, col.a * aw * ah);
            }
            ENDCG
        }
    }
}