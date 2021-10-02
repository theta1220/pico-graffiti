Shader "Hidden/ChromaticAberration"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _RedX ("Red", Range(-0.5, 0.5)) = 0.0
        _GreenX ("Green", Range(-0.5, 0.5)) = 0.0
        _BlueX ("Blue", Range(-0.5, 0.5)) = 0.0
        
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
            float _RedX;
            float _GreenX;
            float _BlueX;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 r = i.uv + float2(_RedX, 0);
                float2 g = i.uv + float2(_GreenX, 0);
                float2 b = i.uv + float2(_BlueX, 0);
                col.r = tex2D(_MainTex, r).r;
                col.g = tex2D(_MainTex, g).g;
                col.b = tex2D(_MainTex, b).b;
                return col;
            }
            ENDCG
        }
    }
}
