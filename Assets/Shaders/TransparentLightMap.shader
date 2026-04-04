Shader "Legacy Shaders/Simplified/Transparent LightMap"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _LightMap ("Lightmap (Greyscale)", 2D) = "white" {}
        [Toggle(_CULLING_OFF)] _CullingOff ("Disable Culling", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"  = "UniversalPipeline"
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_CullingOff]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _CULLING_OFF
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);  SAMPLER(sampler_MainTex);
            TEXTURE2D(_LightMap); SAMPLER(sampler_LightMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _LightMap_ST;
                half4  _Color;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float2 uv2        : TEXCOORD1;
                half4  color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float2 uvLightMap  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                half4  color       : TEXCOORD3;
                float  fogFactor   : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.normalWS    = nrmInputs.normalWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv,  _MainTex);
                OUT.uvLightMap  = TRANSFORM_TEX(IN.uv2, _LightMap);
                OUT.color       = IN.color;
                OUT.fogFactor   = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 mainTex      = SAMPLE_TEXTURE2D(_MainTex,  sampler_MainTex,  IN.uv) * _Color;
                half  lightIntensity = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, IN.uvLightMap).r;

                half3 albedo   = mainTex.rgb * IN.color.rgb;
                half  alpha    = mainTex.a   * IN.color.a;
                half3 emission = mainTex.rgb * lightIntensity;

                // Lambert lighting
                half3 normalWS = normalize(IN.normalWS);
                Light mainLight = GetMainLight();
                half  NdotL    = saturate(dot(normalWS, mainLight.direction));
                half3 lighting  = mainLight.color * NdotL + SampleSH(normalWS);

                half3 color = albedo * lighting + emission;
                color = MixFog(color, IN.fogFactor);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
