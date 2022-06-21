Shader "MK/OutlineFullScreenSimple"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "RenderPeipeline" = "UniversalPepeline"
        }
        LOD 100
        ZWrite Off
        Ztest Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Name "FullScreenPass"

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
                float4 uv_offset:TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _OutlineWidth;
            float4 _OutlineColor;


            TEXTURE2D_X(_MaskTex);
            SAMPLER(sampler_MaskTex);
            float2 _MaskTex_TexelSize;

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);


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
                output.uv_offset.xy=output.uv+float2(-_MaskTex_TexelSize.x,-_MaskTex_TexelSize.y)* _OutlineWidth;
                output.uv_offset.zw=output.uv+float2(_MaskTex_TexelSize.x,-_MaskTex_TexelSize.y)*_OutlineWidth;
                return output;
            }
            
            half sampleMask(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_MaskTex, sampler_MaskTex, uv);
            }

            half4 frag(Varyings input):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half col1 = sampleMask(input.uv);
                half col2 = sampleMask(input.uv_offset.xy);
                half col3 = sampleMask(input.uv_offset.zw);
                half diff = abs(col1*2-col2-col3);
                
                half4 outline=_OutlineColor*diff;

                
                return outline;
            }
            ENDHLSL
        }
    }
}