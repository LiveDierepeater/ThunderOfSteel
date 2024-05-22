Shader "Unlit/ChipShader"
{
    Properties
    {
        [PerRendererData] _MaskTex ("Mask Texture", 2D) = "white" {}
        _PlayerColor ("Player Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
             #pragma multi_compile_instancing
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _PlayerColor)
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MaskTex;
            //float4 _PlayerColor;

            v2f vert (appdata v)
            {
                v2f o;

                //setup instance id
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //setup instance id
                UNITY_SETUP_INSTANCE_ID(i);

                //get _Color Property from buffer
                fixed4 pColor = UNITY_ACCESS_INSTANCED_PROP(Props, _PlayerColor);
                
                fixed4 mask = tex2D(_MaskTex, i.uv);
                fixed3 color = mask.r * pColor + mask.g * fixed3(1.0, 1.0, 1.0);
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
