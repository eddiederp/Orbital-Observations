Shader "Custom/SphereShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [SphereRadius] _SphereRadius("Sphere Radius", float) = 2.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 rayDir : TEXCOORD1;
                float3 rayOrigin : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _SphereRadius;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                float3 worldPos = mul(unity_ObjectToWorld, float4(IN.positionOS.xyz, 1.0));

                // calculate and world space ray direction and origin for interpolation
                OUT.rayDir = worldPos - _WorldSpaceCameraPos.xyz;
                OUT.rayOrigin = _WorldSpaceCameraPos.xyz;

                return OUT;
            }

            float sphIntersect( float3 ro, float3 rd, float4 sph )
            {
                float3 oc = ro - sph.xyz;
                float b = dot( oc, rd );
                float c = dot( oc, oc ) - sph.w*sph.w;
                float h = b*b - c;
                if( h<0.0 ) return -1.0;
                h = sqrt( h );
                return -b - h;
            }

            half4 frag(Varyings IN) : SV_Target
            {   
                // ray origin
                float3 rayOrigin = IN.rayOrigin;

                // normalize ray vector
                float3 rayDir = normalize(IN.rayDir);

                // sphere position
                float3 spherePos = unity_ObjectToWorld._m03_m13_m23;

                // ray box intersection
                float rayHit = sphIntersect(rayOrigin, rayDir, float4(spherePos, _SphereRadius));

                // above function returns -1 if there's no intersection
                clip(rayHit);

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return color;
            }
            ENDHLSL
        }
    }
}
