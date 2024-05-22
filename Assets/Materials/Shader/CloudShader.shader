Shader "Custom/CloudShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _DetailNormalMap ("Detail Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Map Strength", Range(0.0, 2.0)) = 1.0
        _DetailNormalStrength ("Detail Normal Map Strength", Range(0.0, 2.0)) = 1.0
        _Power ("Contrast Power", Range(0.1, 5.0)) = 1.0
        _Transparency ("Base Transparency", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert alpha

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _DetailNormalMap;
        float _NormalStrength;
        float _DetailNormalStrength;
        float _Power;
        float _Transparency;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float2 uv_DetailNormalMap;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 n = tex2D(_NormalMap, IN.uv_NormalMap);
            fixed4 dn = tex2D(_DetailNormalMap, IN.uv_DetailNormalMap);

            // Combine normal maps with their respective strengths
            float3 normal = UnpackNormal(n) * _NormalStrength;
            float3 detailNormal = UnpackNormal(dn) * _DetailNormalStrength;
            float3 combinedNormal = normalize(normal + detailNormal);

            o.Albedo = c.rgb;
            o.Normal = combinedNormal;

            float alpha = pow(c.r, _Power) * _Transparency;
            o.Alpha = alpha;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
