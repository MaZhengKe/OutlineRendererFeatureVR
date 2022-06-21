Shader "MK/OutlineMask"
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
        //Blend One One

        Pass
        {
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
    }
}