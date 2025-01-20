Shader "Billboard/ProgressBar"
{
    Properties
    {
    	_Color ("Main Color", Color) = (1,1,1,1)  
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        Cull Front
        Pass
        {
            HLSLINCLUDE
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            CBUFFER_END
            
            struct appdata
            {
                float3 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertexColor : TEXCOORD0;
                float3 vertex : SV_POSITION;
            };

            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToWorld(v.vertex);
                o.vertexColor = v.color;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
            	return i.vertexColor* _Color;
            }
            ENDHLSL
        }
    }
}

