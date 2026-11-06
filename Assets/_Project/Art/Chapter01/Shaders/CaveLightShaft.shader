Shader "SonDoong/CaveLightShaft"
{
    Properties { _BaseColor("Light colour",Color)=(1,.8,.5,.055) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes i) { Varyings o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o; }
            half4 Frag(Varyings i):SV_Target
            {
                half across=pow(saturate(1-abs(i.uv.x*2-1)),2);
                half lengthFade=smoothstep(0,.12,i.uv.y)*(1-smoothstep(.78,1,i.uv.y));
                return half4(_BaseColor.rgb,_BaseColor.a*across*lengthFade);
            }
            ENDHLSL
        }
    }
}
