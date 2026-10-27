
Shader "Custom/DrunkVisionBlurURP"
{
    Properties
    {
        _BlurStrength ("Blur Strength", Float) = 4
        _ClearRadius ("Clear Radius", Float) = 0.15
        _Feather ("Feather", Float) = 0.25
        _PlayerScreenPos ("Player Screen Position", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "DrunkVision"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _BlurStrength;
            float _ClearRadius;
            float _Feather;
            float4 _PlayerScreenPos;

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                float2 difference = uv - _PlayerScreenPos.xy;
                difference.x *= _ScreenParams.x / _ScreenParams.y;

                float distanceFromPlayer = length(difference);

                float blurMask = smoothstep(
                    _ClearRadius,
                    _ClearRadius + max(_Feather, 0.001),
                    distanceFromPlayer
                );

                float2 texelSize = 1.0 / _ScreenParams.xy;
                float2 offset = texelSize * _BlurStrength;

                half4 original = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    uv
                );

                half4 blurred = 0;

                blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset * float2(-1, -1));
                blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset * float2( 0, -1));
                blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset * float2( 1, -1));

                blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset * float2(-1,  0));
                blurred += original;
                blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset * float2( 1,  0));

                blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset * float2(-1,  1));
                blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset * float2( 0,  1));
                blurred += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset * float2( 1,  1));

                blurred /= 9.0;

                return lerp(original, blurred, blurMask);
            }

            ENDHLSL
        }
    }
}
