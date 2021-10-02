Shader "Hidden/PixelOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Width("Tex Width", Float) = 320.0
        _Height("Tex Height", Float) = 180.0
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _Width;
            float _Height;
            float4 _OutlineColor;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 texcol = tex2D(_MainTex, i.uv);

                // このピクセルの周囲の透明度の最大値を調べる
                float sum = 
                  tex2D(_MainTex, i.uv + float2(0 / _Width, 1 / _Height)).a
                + tex2D(_MainTex, i.uv + float2(0 / _Width, -1 / _Height)).a
                + tex2D(_MainTex, i.uv + float2(1 / _Width, 0 / _Height)).a
                + tex2D(_MainTex, i.uv + float2(-1 / _Width, 0 / _Height)).a;
                
                // + tex2D(_MainTex, i.uv + float2(0 / _Width, 2 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(0 / _Width, -2 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(2 / _Width, 0 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(-2 / _Width, 0 / _Height)).a
                
                // + tex2D(_MainTex, i.uv + float2(1 / _Width, 1 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(1 / _Width, -1 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(-1 / _Width, 1 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(-1 / _Width, -1 / _Height)).a

                // + tex2D(_MainTex, i.uv + float2(1 / _Width, 2 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(1 / _Width, -2 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(-1 / _Width, 2 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(-1 / _Width, -2 / _Height)).a

                // + tex2D(_MainTex, i.uv + float2(2 / _Width, 1 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(2 / _Width, -1 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(-2 / _Width, 1 / _Height)).a
                // + tex2D(_MainTex, i.uv + float2(-2 / _Width, -1 / _Height)).a;

                // このピクセルが透明なら、周囲の透明度の最大値で塗る。
                if (texcol.a < 0.5 && sum > 0.3f)
                    return _OutlineColor;
                else
                    return texcol;
            }
            ENDCG
        }
    }
}
