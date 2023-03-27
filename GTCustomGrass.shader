Shader "Unlit/GTCustomGrass" {
    Properties {
        [Header(Surface options)]
        // [MainTexture] _ColorMap("Color", 2D) = "white" {}
        // [MainColor]_ColorTint("Tint", Color) = (1, 1, 1, 1)

        _TopTint("Top Tint", Color) = (1, 1, 1, 1)
        _BottomTint("Bottom Tint", Color) = (1, 1, 1, 1)
        _Fade("Top Fade Offset", Range(-1,10)) = 0
        _AmbientAdjustment("Ambient Adjustment", Range(-1,10)) = 0
        _Smoothness("Smoothness", Float) = 0
    }

    SubShader {
        Tags{"RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry"}

        Pass {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            // Blend One Zero
            // ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            
            struct appdata_t {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 diffuseColor : COLOR;
            };

            struct MeshProperties {
                float4x4 mat;
                float4 color;
            };

            // TEXTURE2D(_ColorMap); SAMPLER(sampler_ColorMap);
            // float4 _ColorMap_ST;
            // float4 _ColorTint;
            float _Smoothness;

            float4 _TopTint;
            float4 _BottomTint;
            float _Fade;
            float _AmbientAdjustment;

            StructuredBuffer<MeshProperties> _Properties;

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                v2f o;

                float4 posOS = mul(_Properties[instanceID].mat, i.positionOS);
                VertexPositionInputs posnInputs = GetVertexPositionInputs(posOS);

                // VertexPositionInputs posnInputs = GetVertexPositionInputs(i.positionOS);
                VertexNormalInputs normInputs = GetVertexNormalInputs(i.normalOS);

                // float4 pos = mul(_Properties[instanceID].mat, i.positionOS);
                // o.positionCS = mul(UNITY_MATRIX_MVP, pos);
                // o.positionWS = mul(unity_ObjectToWorld, pos);
                // o.normalWS = normalize(mul((float3x3)unity_WorldToObject, i.normalOS));

                o.positionCS = posnInputs.positionCS;
                o.positionWS = posnInputs.positionWS;
                o.normalWS = normInputs.normalWS;
                // o.uv = TRANSFORM_TEX(i.uv, _ColorMap);

                o.uv = i.uv;
                o.diffuseColor = _Properties[instanceID].color; // o.diffuseColor = i.color;

                return o;
            }

            float4 frag(v2f i) : SV_Target {

                // get ambient color from environment lighting
                float4 ambient = float4(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w, 1); //float4(ShadeSH9(float4(0,0,1,1)),0);
                return ambient;

                // float shadow = 0;
                
                // half4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                
                // #if _MAIN_LIGHT_SHADOWS_CASCADE || _MAIN_LIGHT_SHADOWS
                //     Light mainLight = GetMainLight(shadowCoord);
                //     shadow = mainLight.shadowAttenuation;
                // #else
                //     Light mainLight = GetMainLight();
                // #endif
                
                // // extra point lights support
                // float3 extraLights;
                // int pixelLightCount = GetAdditionalLightsCount();
                // for (int j = 0; j < pixelLightCount; ++j) {
                //     Light light = GetAdditionalLight(j, i.positionWS, half4(1, 1, 1, 1));
                //     float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
                //     extraLights += attenuatedLightColor;
                // }

                // // fade over the length of the grass
                // float verticalFade = saturate(i.uv.y + _Fade);
                // // extraLights *= verticalFade;

                // // colors from the tool with tinting from the grass script
                // float4 baseColor = lerp(_BottomTint, _TopTint, verticalFade) * float4(i.diffuseColor);
                
                // float4 final = float4(0, 0, 0, 0);
                // final = baseColor;

                // //add in shadows
                // final *= shadow;

                // // if theres a main light, multiply with its color and intensity           
                // final *= float4(mainLight.color, 1);         
                
                // // add in ambient
                // final += (ambient * baseColor) ;

                // final += float4(extraLights, 1);
                // return final;
            }
            ENDHLSL
        }
    }
}
    //     Pass {
    //         // The shadow caster pass, which draws to shadow maps
    //         Name "ShadowCaster"
    //         Tags{"LightMode" = "ShadowCaster"}
    //         ZWrite On
    //         ZTest LEqual
    //         Cull Off

    //         // ColorMask 0 // No color output, only depth

    //         HLSLPROGRAM
    //         // #pragma multi_compile_shadowcaster

    //         #pragma vertex vert
    //         #pragma fragment frag

    //         // #pragma shader_feature_local _ DISTANCE_DETAIL

    //         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

    //         struct appdata_t{
    //             float3 positionOS : POSITION;
    //             float3 normalOS : NORMAL;
    //         };

    //         struct v2f {
    //             float4 positionCS : SV_POSITION;
    //         };

    //         // These are set by Unity for the light currently "rendering" this shadow caster pass
    //         float3 _LightDirection;

    //         // This function offsets the clip space position by the depth and normal shadow biases
    //         float4 GetShadowCasterPositionCS(float3 positionWS, float3 normalWS) {
    //             float3 lightDirectionWS = _LightDirection;
    //             // From URP's ShadowCasterPass.hlsl
    //             float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    //             // We have to make sure that the shadow bias didn't push the shadow out of
    //             // the camera's view area. This is slightly different depending on the graphics API
    //             #if UNITY_REVERSED_Z
    //                 positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    //             #else
    //                 positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    //             #endif
    //                 return positionCS;
    //         }

    //         v2f vert(appdata_t i) {
    //             v2f o;

    //             VertexPositionInputs posnInputs = GetVertexPositionInputs(i.positionOS); // Found in URP/ShaderLib/ShaderVariablesFunctions.hlsl
    //             VertexNormalInputs normInputs = GetVertexNormalInputs(i.normalOS); // Found in URP/ShaderLib/ShaderVariablesFunctions.hlsl

    //             o.positionCS = GetShadowCasterPositionCS(posnInputs.positionWS, normInputs.normalWS);
    //             return o;
    //         }

    //         float4 frag(v2f i) : SV_TARGET {
    //             return 0;
    //         }

    //         ENDHLSL
    //     }
