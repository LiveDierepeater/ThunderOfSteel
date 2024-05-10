Shader "Custom/ExtendedStaticNoiseShader" {
    Properties {
        _MainTex ("Base Color (RGB)", 2D) = "white" {}
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
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0, 1)) = 0.5 // Metallic-Eigenschaft hinzugefügt
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5 // Smoothness-Eigenschaft hinzugefügt
        _AOMap ("AO Map", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
            float2 uv_NormalMap;
            float2 uv_AOMap;
        };

        sampler2D _MainTex;
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
        sampler2D _NormalMap;
        sampler2D _AOMap;
        float _Metallic;
        float _Smoothness;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

            // Anwendung der Noise Texturen
            fixed2 primaryCoord = IN.worldPos.xz / _Scale;
            fixed2 secondaryCoord = IN.worldPos.xz / _SecondaryScale;
            fixed2 tertiaryCoord = IN.worldPos.xz / _TertiaryScale;
            fixed4 primaryNoise = pow(tex2D(_NoiseTex, primaryCoord), _PrimaryNoiseContrast);
            fixed4 secondaryNoise = pow(tex2D(_NoiseTex, secondaryCoord), _SecondaryNoiseContrast);
            fixed4 tertiaryNoise = pow(tex2D(_NoiseTex, tertiaryCoord), _TertiaryNoiseContrast);

            fixed4 modulatedPrimaryNoise = lerp(fixed4(1, 1, 1, 1), primaryNoise * _NoiseColor, _PrimaryNoiseImpact);
            fixed4 modulatedSecondaryNoise = lerp(fixed4(1, 1, 1, 1), secondaryNoise * _SecondaryNoiseColor, _SecondaryNoiseImpact);
            fixed4 modulatedTertiaryNoise = lerp(fixed4(1, 1, 1, 1), tertiaryNoise * _TertiaryNoiseColor, _TertiaryNoiseImpact);

            fixed4 combinedNoise = modulatedPrimaryNoise * modulatedSecondaryNoise * modulatedTertiaryNoise;

            // Normal Map und AO Map
            fixed3 normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            fixed ao = tex2D(_AOMap, IN.uv_AOMap).r;

            // Setup der finalen Material-Eigenschaften
            o.Albedo = c.rgb * combinedNoise.rgb * ao;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Normal = normal;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
