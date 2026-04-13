Shader "Custom/LineShader"
{
    Properties
    {
        // [PerRendererData] говорит Unity, что текстура будет браться из SpriteRenderer
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _W ("Gradient width", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True" // Важно для оптимизации спрайтов
        }

        Cull Off 
        ZWrite Off 
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
                float2 uv : TEXCOORD0;
                // 1. Запрашиваем цвет у SpriteRenderer (семантика COLOR)
                float4 color : COLOR; 
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                // 2. Подготавливаем переменную для передачи цвета во фрагментный шейдер
                fixed4 color : COLOR; 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _W;
            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // Передаем цвет, заодно перемножая его с Tint-цветом материала
                o.color = v.color * _Color; 
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 3. Умножаем пиксель текстуры на цвет из SpriteRenderer
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                float yIdx = i.uv.y * 2 - 1;
                float absy = abs(yIdx);
                
                float a = saturate(1.0 - (absy - _W) / (1.0 - _W));

                col.a *= a; 
                
                return col;
            }
            ENDCG
        }
    }
}