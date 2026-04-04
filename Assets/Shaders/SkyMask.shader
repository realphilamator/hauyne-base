Shader "Masked/SkyMask-URP"
{
    Properties
    {
        // Assign your skybox cubemap here, or leave blank to use the ambient probe
        [NoScaleOffset] _Skybox("Skybox Cubemap (optional)", Cube) = "" {}
    }

        SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+10" }

        Pass
        {
            Name "SkyMask"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_Skybox); SAMPLER(sampler_Skybox);

            CBUFFER_START(UnityPerMaterial)
                // (empty - cubemap has no ST)
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 viewDir     : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // World-space direction from camera to vertex = sky lookup direction
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDir = posWS - GetCameraPositionWS();
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 sky = SAMPLE_TEXTURECUBE(_Skybox, sampler_Skybox, normalize(IN.viewDir));
                return half4(sky.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}