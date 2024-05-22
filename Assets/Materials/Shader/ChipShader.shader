Shader "Unlit/ChipShader"
{
    Properties
    {
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _PlayerColor ("Player Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _MaskTex;
            float4 _PlayerColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mask = tex2D(_MaskTex, i.uv);
                fixed3 color = mask.r * _PlayerColor.rgb + mask.g * fixed3(1.0, 1.0, 1.0);
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
