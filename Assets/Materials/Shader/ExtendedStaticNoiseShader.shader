Shader "Custom/ExtendedStaticNoiseShader" {
    Properties {
        _MainTex ("Grass Base Color (RGB)", 2D) = "white" {}
        _NormalMap ("Grass Normal Map", 2D) = "bump" {}
        _AOMap ("Grass AO Map", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseColor ("Primary Noise Color", Color) = (1,1,1,1)
        _SecondaryNoiseColor ("Secondary Noise Color", Color) = (1,1,1,1)
        _TertiaryNoiseColor ("Tertiary Noise Color", Color) = (1,1,1,1)
        _Scale ("Primary Noise Scale", Float) = 1.0
        _SecondaryScale ("Secondary Noise Scale", Float) = 5.0
        _TertiaryScale ("Tertiary Noise Scale", Float) = 10.0
        _PrimaryNoiseImpact ("Primary Noise Impact", Range(0, 1)) = 0.5
        _SecondaryNoiseImpact ("Secondary Noise Impact", Range(0, 1)) = 0.5
        _TertiaryNoiseImpact ("Tertiary Noise Impact", Range(0, 1)) = 0.5
        _PrimaryNoiseContrast ("Primary Noise Contrast", Range(0.1, 2.0)) = 1.0
        _SecondaryNoiseContrast ("Secondary Noise Contrast", Range(0.1, 2.0)) = 1.0
        _TertiaryNoiseContrast ("Tertiary Noise Contrast", Range(0.1, 2.0)) = 1.0
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        
        _ColorThreshold ("Color Threshold", Range(0.0, 1.0)) = 0.5
        _AltMainTex ("Forest Base Color (RGB)", 2D) = "white" {}
        _AltNormalMap ("Forest Normal Map", 2D) = "bump" {}
        _AltAOMap ("Forest AO Map", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input {
            float2 uv_MainTex;
            float2 uv_AltMainTex;
            float3 worldPos;
            float2 uv_NormalMap;
            float2 uv_AOMap;
            fixed4 color : COLOR; // Vertex color input
        };

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _AOMap;
        sampler2D _NoiseTex;
        fixed4 _NoiseColor;
        fixed4 _SecondaryNoiseColor;
        fixed4 _TertiaryNoiseColor;
        float _Scale;
        float _SecondaryScale;
        float _TertiaryScale;
        float _PrimaryNoiseImpact;
        float _SecondaryNoiseImpact;
        float _TertiaryNoiseImpact;
        float _PrimaryNoiseContrast;
        float _SecondaryNoiseContrast;
        float _TertiaryNoiseContrast;
        float _Metallic;
        float _Smoothness;
        
        float _ColorThreshold;
        sampler2D _AltMainTex;
        sampler2D _AltNormalMap;
        sampler2D _AltAOMap;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Anwendung der Noise Texturen mit separaten Impact-Werten für jede Schicht
            fixed2 primaryCoord = IN.worldPos.xz / _Scale;
            fixed2 secondaryCoord = IN.worldPos.xz / _SecondaryScale;
            fixed2 tertiaryCoord = IN.worldPos.xz / _TertiaryScale;
            fixed4 primaryNoise = pow(tex2D(_NoiseTex, primaryCoord), _PrimaryNoiseContrast) * _PrimaryNoiseImpact;
            fixed4 secondaryNoise = pow(tex2D(_NoiseTex, secondaryCoord), _SecondaryNoiseContrast) * _SecondaryNoiseImpact;
            fixed4 tertiaryNoise = pow(tex2D(_NoiseTex, tertiaryCoord), _TertiaryNoiseContrast) * _TertiaryNoiseImpact;
            
            // Auswahl der Texturen basierend auf Vertex-Farben und Schwelle
            float vertexColorInfluence = saturate((IN.color.g - _ColorThreshold) * tertiaryNoise * tertiaryNoise * _TertiaryScale / 2);
            fixed4 texColor = lerp(tex2D(_MainTex, IN.uv_MainTex), tex2D(_AltMainTex, IN.uv_AltMainTex), vertexColorInfluence);
            fixed3 normal = lerp(UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap)), UnpackNormal(tex2D(_AltNormalMap, IN.uv_AltMainTex)), vertexColorInfluence);
            fixed ao = lerp(tex2D(_AOMap, IN.uv_AOMap).r, tex2D(_AltAOMap, IN.uv_AltMainTex).r, vertexColorInfluence);

            fixed4 combinedNoise = primaryNoise * secondaryNoise * tertiaryNoise;

            // Setup der finalen Material-Eigenschaften
            o.Albedo = texColor.rgb * combinedNoise.rgb * ao * 20;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Normal = normal;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
