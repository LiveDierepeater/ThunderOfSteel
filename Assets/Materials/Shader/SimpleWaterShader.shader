Shader "Custom/SimpleWaterShader" {
    Properties {
        _TintColor ("Tint Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _MainMapSpeed ("Main Map Speed", Vector) = (0.1, 0.1, 0, 0)
        _BumpMapSpeed ("Normal Map Speed", Vector) = (0.1, 0.1, 0, 0)
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.5
        _RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.1
        _FresnelPower ("Fresnel Power", Range(0.1, 10.0)) = 2.0
        _EdgeGradientTex ("Linear Gradient texture", 2D) = "white" {}
        _EdgeColor ("Water Foam", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 98
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:blend
        #include "UnityCG.cginc"

        fixed4 _TintColor;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        float4 _MainMapSpeed;
        float4 _BumpMapSpeed;
        float _ReflectionStrength;
        float _RefractionStrength;
        float _FresnelPower;
        fixed4 _EdgeColor;
        sampler2D _EdgeGradientTex;

        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_EdgeTex;
            float3 viewDir;
            //fixed4 color : COLOR; // Zusätzlicher Input für Vertex-Farben
        };
        
        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Scrollende Normal Map
            float2 scrolledUV = IN.uv_BumpMap + _Time.y * _BumpMapSpeed.xy;
            fixed3 normal = UnpackNormal(tex2D(_BumpMap, scrolledUV));
            
            // Grundtextur mit Scrollen und Vertex-Farben modifiziert
            float2 mainScrolledUV = IN.uv_MainTex + _Time.y * _MainMapSpeed.xy;
            fixed4 c = tex2D(_MainTex, mainScrolledUV);
            
            //float vertexColorInfluence = IN.color.b * tex2D(_EdgeGradientTex, IN.uv_EdgeTex) * c * c;
            //fixed4 texColor = lerp(c, _EdgeColor, vertexColorInfluence);
            
            // Reflektion und Refraktion
            fixed fresnel = pow(1.0 - dot(normalize(IN.viewDir), normal), _FresnelPower);
            o.Albedo = c.rgb * (1.0 - fresnel * _ReflectionStrength) * _TintColor.rgb;
            o.Normal = normal;
            o.Alpha = c.a;
            o.Smoothness = 0.5;
            o.Metallic = 0.0;
            
            // Simulierte Refraktion
            o.Emission = fresnel * _ReflectionStrength * UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflect(-IN.viewDir, normal)) * _RefractionStrength;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
