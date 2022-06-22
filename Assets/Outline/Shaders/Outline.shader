Shader "KuanMi/Outline"
{

    Properties
    {
        _ZTest("ZTest", Float) = 3.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPeipeline" = "UniversalPepeline"
        }
        LOD 100

        Pass
        {
            name "mask"
            ZWrite off
            ZTest [_ZTest]
            HLSLPROGRAM
            #pragma  vertex vert
            #pragma  fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            int EyeIndex;

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

                return output;
            }


            half frag(Varyings input):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 1;
            }
            ENDHLSL
        }
        Pass
        {
            Name "FullScreenPass"

            ZWrite Off
            Ztest Off
            Cull Off
            //Blend SrcAlpha OneMinusSrcAlpha
            Blend SrcAlpha OneMinusSrcAlpha
            //Blend SrcAlpha zero
            //Blend  one  one

            HLSLPROGRAM
            #pragma  vertex vert
            #pragma  fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionHCS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Note: The pass is setup with a mesh already in clip
                // space, that's why, it's enough to just output vertex
                // positions
                output.positionCS = float4(input.positionHCS.xyz, 1.0);

                #if UNITY_UV_STARTS_AT_TOP
                output.positionCS.y *= -1;
                #endif

                output.uv = input.uv;
                return output;
            }

            #define C45 0.707107
            #define C225 0.9238795
            #define S225 0.3826834
            #define MAX_SAMPLES 16

            static float2 offsets[MAX_SAMPLES] = {
                float2(1, 0),
                float2(-1, 0),
                float2(0, 1),
                float2(0, -1),

                float2(C45, C45),
                float2(C45, -C45),
                float2(-C45, C45),
                float2(-C45, -C45),

                float2(C225, S225),
                float2(C225, -S225),
                float2(-C225, S225),
                float2(-C225, -S225),
                float2(S225, C225),
                float2(S225, -C225),
                float2(-S225, C225),
                float2(-S225, -C225)
            };

            float _SamplePrecision;
            float _OutlineWidth;
            float4 _OutlineColor;


            TEXTURE2D_X(_MaskTex);
            SAMPLER(sampler_MaskTex);

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);


            half sampleMask(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_MaskTex, sampler_MaskTex, uv).r;
            }

            half4 frag(Varyings input):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                const half mask = sampleMask(input.uv);

                //return float4(mask,mask,mask,1);
                //clip(0.5 - mask);

                const int sample_count = min(2 * pow(2, _SamplePrecision), MAX_SAMPLES);

                const float2 uv_offset_per_pixel = 1.0 / _ScreenSize.xy;

                float outlineMask = 0;
                for (int i = 0; i < sample_count; ++ i)
                {
                    outlineMask = max(sampleMask(input.uv + uv_offset_per_pixel * _OutlineWidth * offsets[i]),outlineMask);
                }
                //clip(outlineMask-0.5);
                outlineMask *= 1 - mask;

                return _OutlineColor*outlineMask;
            }
            ENDHLSL
        }
    }
}