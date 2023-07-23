#ifndef UNIVERSAL_NPRLIT_GBUFFER_PASS_INCLUDED
#define UNIVERSAL_NPRLIT_GBUFFER_PASS_INCLUDED

#include "../ShaderLibrary/NPRGBuffer.hlsl"

// TODO: Currently we support viewDirTS caclulated in vertex shader and in fragments shader.
// As both solutions have their advantages and disadvantages (etc. shader target 2.0 has only 8 interpolators).
// We need to find out if we can stick to one solution, which we needs testing.
// So keeping this until I get manaul QA pass.
#if defined(_PARALLAXMAP) && (SHADER_TARGET >= 30)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

// keep this file in sync with LitForwardPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD1;
#endif

    half3 normalWS                  : TEXCOORD2;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    half4 tangentWS                 : TEXCOORD3;    // xyz: tangent, w: sign
#endif
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half3 vertexLighting            : TEXCOORD4;    // xyz: vertex lighting
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD5;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS                 : TEXCOORD6;
#endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV       : TEXCOORD8; // Dynamic lightmap UVs
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void PreInitializeInputData(Varyings input, out InputData inputData, out NPRAddInputData addInputData)
{
    inputData = (InputData)0;
    addInputData = (NPRAddInputData)0;
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
    #endif

    #if defined(_NORMALMAP) || defined(_DETAIL)
        float sgn = input.tangentWS.w;      // should be either +1 or -1
        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        inputData.tangentToWorld = tangentToWorld;
    #endif
    
    inputData.normalWS = input.normalWS;

    inputData.viewDirectionWS = viewDirWS;
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
    inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif
    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    #else
    inputData.fogCoord = 0.0;
    #endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #endif

    addInputData.linearEyeDepth = DepthSamplerToLinearDepth(input.positionCS.z);
}

void InitializeInputData(Varyings input, half3 normalTS, inout NPRAddInputData addInputData, inout InputData inputData)
{
    #if EYE && (defined(_NORMALMAP) || defined(_DETAIL))
        half3 corneaNormalTS = normalTS;
        half3 irisNormalTS = half3(-corneaNormalTS.x, -corneaNormalTS.y, corneaNormalTS.z);
        half3 tempNormal = corneaNormalTS;
        corneaNormalTS = lerp(corneaNormalTS, irisNormalTS, _BumpIrisInvert);
        irisNormalTS = lerp(irisNormalTS, tempNormal, _BumpIrisInvert);
        addInputData.corneaNormalWS = NormalizeNormalPerPixel(TransformTangentToWorld(corneaNormalTS, inputData.tangentToWorld));
        addInputData.irisNormalWS = NormalizeNormalPerPixel(TransformTangentToWorld(irisNormalTS, inputData.tangentToWorld));
        inputData.normalWS = addInputData.corneaNormalWS;
    #elif (defined(_NORMALMAP) || defined(_DETAIL))
        inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
        inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    #endif

    #if defined(DYNAMICLIGHTMAP_ON)
        inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
    #else
        inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
    #endif
}

half3 NPRIndirectLighting(BRDFData brdfData, InputData inputData, Varyings input, half occlusion)
{
    half3 indirectDiffuse = inputData.bakedGI * occlusion;
    half3 reflectVector = reflect(-inputData.viewDirectionWS, inputData.normalWS);
    half NoV = saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);
    #if _RENDERENVSETTING || _CUSTOMENVCUBE
    half3 indirectSpecular = NPRGlossyEnvironmentReflection(reflectVector, brdfData.perceptualRoughness, occlusion);
    #else
    half3 indirectSpecular = 0;
    #endif
    half3 indirectColor = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

    #if _MATCAP
    half3 matCap = SamplerMatCap(_MatCapColor, input.uv.zw, inputData.normalWS, inputData.normalizedScreenSpaceUV, TEXTURE2D_ARGS(_MatCapTex, sampler_MatCapTex));
    indirectColor += matCap;
    #endif
    
    return indirectColor;
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitGBufferPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;

    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        real sign = input.tangentOS.w * GetOddNegativeScale();
        half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
    #endif

    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
        output.tangentWS = tangentWS;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
        half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
        output.viewDirTS = viewDirTS;
    #endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
        output.vertexLighting = vertexLight;
    #endif

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        output.positionWS = vertexInput.positionWS;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    output.positionCS = vertexInput.positionCS;

    return output;
}

half3 NPRGDiffuseLightingBuffer(BRDFData brdfData, half2 uv, LightingData lightingData, half radiance)
{
    half3 diffuse = 0;

    #if _CELLSHADING
    diffuse = CellShadingDiffuse(radiance, _CELLThreshold, _CELLSmoothing, _HighColor.rgb, _DarkColor.rgb);
    #elif _LAMBERTIAN
    diffuse = lerp(_DarkColor.rgb, _HighColor.rgb, radiance);
    #elif _RAMPSHADING
    diffuse = RampShadingDiffuse(radiance, _RampMapVOffset, _RampMapUOffset, TEXTURE2D_ARGS(_DiffuseRampMap, sampler_DiffuseRampMap));
    #elif _CELLBANDSHADING
    diffuse = CellBandsShadingDiffuse(radiance, _CELLThreshold, _CellBandSoftness, _CellBands,  _HighColor.rgb, _DarkColor.rgb);
    #elif _SDFFACE
    diffuse = SDFFaceDiffuse(uv, lightingData, _SDFShadingSoftness, _HighColor.rgb, _DarkColor.rgb, TEXTURECUBE_ARGS(_SDFFaceTex, sampler_SDFFaceTex));
    #endif
    diffuse *= brdfData.diffuse;
    return diffuse;
}

LightingData InitializeLightingDataGBuffer(Light mainLight, Varyings input, half3 normalWS, half3 viewDirectionWS, NPRAddInputData addInputData)
{
    LightingData lightData;
    lightData.lightColor = mainLight.color;
    #if EYE
    lightData.NdotL = dot(addInputData.irisNormalWS, mainLight.direction.xyz);
    #else
    lightData.NdotL = dot(normalWS, mainLight.direction.xyz);
    #endif
    lightData.NdotLClamp = saturate(lightData.NdotL);
    lightData.HalfLambert = lightData.NdotL * 0.5 + 0.5;
    half3 halfDir = SafeNormalize(mainLight.direction + viewDirectionWS);
    lightData.LdotHClamp = saturate(dot(mainLight.direction.xyz, halfDir.xyz));
    lightData.NdotHClamp = saturate(dot(normalWS.xyz, halfDir.xyz));
    lightData.NdotVClamp = saturate(dot(normalWS.xyz, viewDirectionWS.xyz));
    lightData.HalfDir = halfDir;
    lightData.lightDir = mainLight.direction;
    #if defined(_RECEIVE_SHADOWS_OFF)
    lightData.ShadowAttenuation = 1;
    #elif _DEPTHSHADOW
    lightData.ShadowAttenuation = DepthShadow(_DepthShadowOffset, _DepthOffsetShadowReverseX, _DepthShadowThresoldOffset, _DepthShadowSoftness, input.positionCS.xy, mainLight.direction, addInputData);
    #else
    lightData.ShadowAttenuation = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
    #endif

    return lightData;
}

// Used in Standard (Physically Based) shader
FragmentOutput LitGBufferPassFragment(Varyings input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    InputData inputData;
    NPRAddInputData addInputData;
    PreInitializeInputData(input, inputData, addInputData);

    NPRSurfaceData surfaceData;
    InitializeNPRStandardSurfaceData(input.uv, inputData, surfaceData);

    InitializeInputData(input, surfaceData.normalTS, addInputData, inputData);
    
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    // Stripped down version of UniversalFragmentPBR().

    // in LitForwardPass GlobalIllumination (and temporarily LightingPhysicallyBased) are called inside UniversalFragmentPBR
    // in Deferred rendering we store the sum of these values (and of emission as well) in the GBuffer
    BRDFData brdfData, clearCoatbrdfData;
    InitializeNPRBRDFData(surfaceData, brdfData, clearCoatbrdfData);

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
    NPRMainLightCorrect(_LightDirectionObliqueWeight, mainLight);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
    
    half3 indirectLighting = NPRIndirectLighting(brdfData, inputData, input, surfaceData.occlusion);
    LightingData lightingData = InitializeLightingDataGBuffer(mainLight, input, inputData.normalWS, inputData.viewDirectionWS, addInputData);
    half radiance = LightingRadiance(lightingData, _UseHalfLambert, surfaceData.occlusion, _UseRadianceOcclusion);

    half3 diffuse = NPRGDiffuseLightingBuffer(brdfData, input.uv, lightingData, radiance);

    return BRDFDataToGbuffer(brdfData, diffuse, inputData, surfaceData.smoothness, surfaceData.emission + indirectLighting, surfaceData.occlusion);
}

#endif
