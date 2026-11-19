Shader "Toon/ToonShader URP"
{
    Properties
    {
        [Header(Base)]
        _TintColor ("Tint Color (A = Strength)", Color) = (1, 1, 1, 0)

        [Header(Toon Shading)]
        _ShadowColor ("Shadow Color", Color) = (0.72, 0.65, 0.78, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.4
        _ShadowSoftness ("Shadow Softness", Range(0.01, 0.5)) = 0.15
        _MinBrightness ("Min Brightness", Range(0, 1)) = 0.45

        [Header(Specular)]
        [Toggle(_USE_SPEC)] _UseSpec ("Enable Specular", Float) = 0
        _SpecColor2 ("Specular Color", Color) = (1, 1, 1, 0.3)
        _SpecSize ("Specular Size", Range(0.9, 0.999)) = 0.96
        _SpecSoftness ("Specular Softness", Range(0.001, 0.05)) = 0.02

        [Header(Rim Light)]
        [Toggle(_USE_RIM)] _UseRim ("Enable Rim Light", Float) = 0
        _RimColor ("Rim Color", Color) = (1, 0.95, 0.9, 1)
        _RimPower ("Rim Power", Range(1, 8)) = 4.0
        _RimIntensity ("Rim Intensity", Range(0, 0.5)) = 0.15

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0.2, 0.15, 0.12, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.03)) = 0.004

        [Header(Normal Map)]
        [Toggle(_USE_NORMALMAP)] _UseNormalMap ("Enable Normal Map", Float) = 0
        [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0, 2)) = 1.0
        _BumpTiling ("Normal Tiling", Float) = 1

        [Header(Additional Lights)]
        _AdditionalLightStrength ("Additional Light Strength", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _TintColor;
            float4 _ShadowColor;
            float4 _SpecColor2;
            float4 _RimColor;
            float4 _OutlineColor;
            float _ShadowThreshold;
            float _ShadowSoftness;
            float _MinBrightness;
            float _SpecSize;
            float _SpecSoftness;
            float _RimPower;
            float _RimIntensity;
            float _OutlineWidth;
            float _BumpScale;
            float _BumpTiling;
            float _AdditionalLightStrength;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex outlineVert
            #pragma fragment outlineFrag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings outlineVert(Attributes input)
            {
                Varyings output;
                float3 worldScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                float avgScale = (worldScale.x + worldScale.y + worldScale.z) / 3.0;
                float3 posOS = input.positionOS.xyz + input.normalOS * (_OutlineWidth / max(avgScale, 0.001));
                output.positionCS = TransformObjectToHClip(posOS);
                return output;
            }

            half4 outlineFrag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ToonForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog

            #pragma shader_feature_local _USE_SPEC
            #pragma shader_feature_local _USE_RIM
            #pragma shader_feature_local _USE_NORMALMAP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #ifdef _USE_NORMALMAP
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float  fogFactor   : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                float4 vertColor   : TEXCOORD5;
                #ifdef _USE_NORMALMAP
                float3 tangentWS   : TEXCOORD6;
                float3 bitangentWS : TEXCOORD7;
                #endif
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS  = posInputs.positionCS;
                output.positionWS  = posInputs.positionWS;
                output.uv          = input.uv;
                output.normalWS    = TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor   = ComputeFogFactor(posInputs.positionCS.z);
                output.shadowCoord = GetShadowCoord(posInputs);
                output.vertColor   = input.color;

                #ifdef _USE_NORMALMAP
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.tangentWS   = normInputs.tangentWS;
                output.bitangentWS = normInputs.bitangentWS;
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normal  = normalize(input.normalWS);
                float3 posWS   = input.positionWS;
                float3 viewDir = GetWorldSpaceNormalizeViewDir(posWS);

                #ifdef _USE_NORMALMAP
                float3 bumpNormal = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv * _BumpTiling),
                    _BumpScale
                );
                float3x3 TBN = float3x3(
                    normalize(input.tangentWS),
                    normalize(input.bitangentWS),
                    normal
                );
                normal = normalize(mul(bumpNormal, TBN));
                #endif

                float3 vertCol = input.vertColor.rgb;
                float3 albedo  = lerp(vertCol, vertCol * _TintColor.rgb, _TintColor.a);

                Light mainLight      = GetMainLight(input.shadowCoord);
                float NdotL          = dot(normal, mainLight.direction);
                float wrappedNdotL   = NdotL * 0.5 + 0.5;

                float toon = smoothstep(
                    _ShadowThreshold - _ShadowSoftness,
                    _ShadowThreshold + _ShadowSoftness,
                    wrappedNdotL
                );
                toon *= mainLight.shadowAttenuation;
                toon  = max(toon, _MinBrightness);

                float3 finalColor = lerp(albedo * _ShadowColor.rgb, albedo * mainLight.color, toon);
                finalColor += albedo * SampleSH(normal) * (1.0 - toon * 0.5);

                #ifdef _USE_SPEC
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float NdotH    = saturate(dot(normal, halfDir));
                float spec     = smoothstep(_SpecSize - _SpecSoftness, _SpecSize + _SpecSoftness, NdotH);
                spec *= step(0.5, wrappedNdotL) * mainLight.shadowAttenuation;
                finalColor += _SpecColor2.rgb * spec * _SpecColor2.a;
                #endif

                #ifdef _USE_RIM
                float rim        = pow(1.0 - saturate(dot(normal, viewDir)), _RimPower);
                float rimLitBias = saturate(NdotL * 0.5 + 0.5);
                finalColor += _RimColor.rgb * rim * rimLitBias * _RimIntensity;
                #endif

                InputData inputData = (InputData)0;
                inputData.positionWS              = posWS;
                inputData.normalWS                = normal;
                inputData.viewDirectionWS         = viewDir;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                uint additionalLightCount = GetAdditionalLightsCount();

                #if USE_CLUSTER_LIGHT_LOOP
                LIGHT_LOOP_BEGIN(additionalLightCount)
                    Light addLight = GetAdditionalLight(lightIndex, posWS, half4(1, 1, 1, 1));
                    float addNdotL = dot(normal, addLight.direction) * 0.5 + 0.5;
                    float addToon  = smoothstep(
                        _ShadowThreshold - _ShadowSoftness * 2.0,
                        _ShadowThreshold + _ShadowSoftness * 2.0,
                        addNdotL
                    );
                    float addAtten = addLight.distanceAttenuation * addLight.shadowAttenuation;
                    finalColor += albedo * addLight.color * addToon * addAtten * _AdditionalLightStrength;
                LIGHT_LOOP_END
                #else
                for (uint i = 0u; i < additionalLightCount; i++)
                {
                    Light addLight = GetAdditionalLight(i, posWS, half4(1, 1, 1, 1));
                    float addNdotL = dot(normal, addLight.direction) * 0.5 + 0.5;
                    float addToon  = smoothstep(
                        _ShadowThreshold - _ShadowSoftness * 2.0,
                        _ShadowThreshold + _ShadowSoftness * 2.0,
                        addNdotL
                    );
                    float addAtten = addLight.distanceAttenuation * addLight.shadowAttenuation;
                    finalColor += albedo * addLight.color * addToon * addAtten * _AdditionalLightStrength;
                }
                #endif

                finalColor = MixFog(finalColor, input.fogFactor);
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                float3 posWS  = TransformObjectToWorld(input.positionOS.xyz);
                float3 normWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(posWS, normWS, _LightDirection));
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half DepthFrag(Varyings input) : SV_Target { return input.positionCS.z; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex DNVert
            #pragma fragment DNFrag

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings   { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; };

            Varyings DNVert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS   = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 DNFrag(Varyings input) : SV_Target
            {
                return half4(normalize(input.normalWS) * 0.5 + 0.5, 0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
