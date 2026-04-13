Shader "Custom/SnakeLineClip"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _ClipDistance ("Clip Distance", Float) = 0.0
        
        // Сюда нужно будет вписать ширину линии из твоего контроллера (например, 0.45), 
        // чтобы математика отсечения работала идеально точно в юнитах.
        _LineWidth ("Line Width", Float) = 0.45 
    }
    SubShader
    {
        // Указываем, что шейдер прозрачный и должен рендериться поверх остального
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off // Выключаем отбраковку нелицевых полигонов на случай перекручивания линии

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
                float4 color : COLOR; // LineRenderer умеет передавать цвет вершин
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _ClipDistance;
            float _LineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // TRANSFORM_TEX нужен, если ты захочешь двигать (offset) текстуру
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Вычисляем абсолютную дистанцию от хвоста (Index 0)
                float absoluteDistance = i.uv.x * _LineWidth;

                // 2. Если текущий пиксель находится ближе к хвосту, чем расстояние среза,
                // функция clip прерывает отрисовку этого пикселя (делает его невидимым).
                clip(absoluteDistance - _ClipDistance);

                // 3. Получаем цвет из текстуры и умножаем на Tint Color и Vertex Color
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                
                // 4. Дополнительно отрезаем прозрачные пиксели самой текстуры (альфа-тест)
                clip(col.a - 0.01);

                return col;
            }
            ENDCG
        }
    }
}