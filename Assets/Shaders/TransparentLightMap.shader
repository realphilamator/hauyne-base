Shader "Legacy Shaders/Simplified/Transparent LightMap"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _LightMap ("Lightmap (Greyscale)", 2D) = "white" {}
        [Toggle(_CullingOff)] _CullingOff ("Disable Culling", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 200
        Cull [_CullingOff]

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        sampler2D _LightMap;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_LightMap;
            fixed4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed lightIntensity = tex2D(_LightMap, IN.uv_LightMap).r;

            o.Albedo = mainTex.rgb * IN.color.rgb;
            o.Alpha = mainTex.a * IN.color.a;
            o.Emission = mainTex.rgb * lightIntensity;
        }
        ENDCG
    }
    Fallback "Legacy Shaders/Transparent/VertexLit"
}
