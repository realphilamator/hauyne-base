// This shader is licensed under a Creative Commons Attribution 4.0 International License.
// (https://creativecommons.org/licenses/by/4.0/)
// This means you MUST give me (YuraSuper2048) credit if you are using this.
// If you redistribute this shader (modified or not) you should save this message as is.

Shader "Custom/Masked and Layered Diffuse"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _SecondTex("Secondary (RGB) Trans (A)", 2D) = "black" {}
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _Mask("Mask (RGB) Trans (A)", 2D) = "white" {}
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
    }

        SubShader
        {
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "Queue" = "AlphaTest"
                "IgnoreProjector" = "True"
                "RenderType" = "TransparentCutout"
            }
            LOD 200

            Pass
            {
                Name "ForwardLit"
                Tags { "LightMode" = "UniversalForward" }

                AlphaToMask On

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
                #pragma multi_compile _ _ADDITIONAL_LIGHTS
                #pragma multi_compile_fog

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
                TEXTURE2D(_SecondTex); SAMPLER(sampler_SecondTex);
                TEXTURE2D(_Mask);      SAMPLER(sampler_Mask);

                CBUFFER_START(UnityPerMaterial)
                    float4 _MainTex_ST;
                    float4 _Color;
                    float  _Cutoff;
                CBUFFER_END

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float3 normalOS   : NORMAL;
                    float2 uv         : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct Varyings
                {
                    float4 positionHCS : SV_POSITION;
                    float2 uv          : TEXCOORD0;
                    float3 normalWS    : TEXCOORD1;
                    float3 positionWS  : TEXCOORD2;
                    float  fogFactor : TEXCOORD3;
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
                    OUT.positionWS = posInputs.positionWS;
                    OUT.normalWS = nrmInputs.normalWS;
                    OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                    OUT.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                    return OUT;
                }

                half4 frag(Varyings IN) : SV_Target
                {
                    half4 c = SAMPLE_TEXTURE2D(_MainTex,   sampler_MainTex,   IN.uv) * _Color;
                    half4 m = SAMPLE_TEXTURE2D(_Mask,      sampler_Mask,      IN.uv);
                    half4 s = SAMPLE_TEXTURE2D(_SecondTex, sampler_SecondTex, IN.uv) * _Color;

                    half3 albedo = lerp(c.rgb, s.rgb, s.a);
                    half  alpha = c.a * m.a * m.r + s.a * (1.0h - m.a);

                    clip(alpha - _Cutoff);

                    // Lambert lighting
                    half3 normalWS = normalize(IN.normalWS);
                    Light mainLight = GetMainLight();
                    half  NdotL = saturate(dot(normalWS, mainLight.direction));
                    half3 lighting = mainLight.color * NdotL + SampleSH(normalWS);

                    half3 color = albedo * lighting;
                    color = MixFog(color, IN.fogFactor);

                    return half4(color, alpha);
                }
                ENDHLSL
            }

            Pass
            {
                Name "ShadowCaster"
                Tags { "LightMode" = "ShadowCaster" }

                ZWrite On
                ZTest LEqual
                ColorMask 0

                HLSLPROGRAM
                #pragma vertex shadowVert
                #pragma fragment shadowFrag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
                TEXTURE2D(_Mask);      SAMPLER(sampler_Mask);
                TEXTURE2D(_SecondTex); SAMPLER(sampler_SecondTex);

                CBUFFER_START(UnityPerMaterial)
                    float4 _MainTex_ST;
                    float4 _Color;
                    float  _Cutoff;
                CBUFFER_END

                struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
                struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

                Varyings shadowVert(Attributes IN)
                {
                    Varyings OUT;
                    float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                    float3 nrmWS = TransformObjectToWorldNormal(IN.normalOS);
                    OUT.positionHCS = TransformWorldToHClip(ApplyShadowBias(posWS, nrmWS, _MainLightPosition.xyz));
                    OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                    return OUT;
                }

                half4 shadowFrag(Varyings IN) : SV_Target
                {
                    half4 c = SAMPLE_TEXTURE2D(_MainTex,   sampler_MainTex,   IN.uv) * _Color;
                    half4 m = SAMPLE_TEXTURE2D(_Mask,      sampler_Mask,      IN.uv);
                    half4 s = SAMPLE_TEXTURE2D(_SecondTex, sampler_SecondTex, IN.uv) * _Color;
                    half  alpha = c.a * m.a * m.r + s.a * (1.0h - m.a);
                    clip(alpha - _Cutoff);
                    return 0;
                }
                ENDHLSL
            }
        }

            Fallback Off
}
