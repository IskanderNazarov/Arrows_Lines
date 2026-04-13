Shader "Custom/GrayscaleToColorWithHighlights"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _C0("Color 0 (Dark)", Color) = (1, 1, 1, 1)
        _C1("Color 1 (Light)", Color) = (1, 1, 1, 1)
        _HighlightStrength("Highlight Strength", Range(0, 1)) = 0.5
        _A("A", Range(0, 1)) = 1
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Off
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 col : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 col : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.col = v.col;
                return o;
            }

            sampler2D _MainTex;
            float4 _C0;
            float4 _C1;
            float _HighlightStrength;
            float _A;

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Calculate grayscale value
                float grayscale = dot(texColor.rgb, float3(0.299, 0.587, 0.114) * _A);
                //grayscale *= _A;

                // Interpolate between _C0 (dark) and _C1 (light) based on grayscale value
                fixed4 colorized = lerp(_C0, _C1, grayscale);

                // Additional highlight blending towards white to bring back highlights
                fixed4 finalColor = lerp(colorized, fixed4(1, 1, 1, texColor.a), pow(grayscale, 2.0) * _HighlightStrength);

                // Set alpha to match the original texture
                finalColor.a = texColor.a * i.col.a;

                return finalColor;
            }
            ENDCG
        }
    }
}

/*float4 col = tex2D(_MainTex, i.uv);
//float gsv = (col.r + col.g + col.b) / 3; //grayscale value
//float gsv = col.r * 0.299 + col.g * 0.587 + col.b * 0.114;//grayscale value
float gsv = dot(col.rgb, float3(0.299, 0.587, 0.114));

col.rgb = lerp(_C0.rgb, _C1.rgb, gsv);
col.a *= i.col.a;

return col;*/