Shader "UI/UITexBlur"
{
	Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Parameter("Parameter", Vector) = (0.002, 0.002, 0, 0)
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Parameter;

            fixed4 frag(v2f_img i) : SV_Target
            {
                half4 finnalColor = tex2D (_MainTex, i.uv - _Parameter.xy * 3.0) * 0.0205000006f;
	            finnalColor += tex2D (_MainTex, i.uv - _Parameter.xy * 2.0) * 0.0855000019f;
	            finnalColor += tex2D (_MainTex, i.uv - _Parameter.xy) * 0.231999993f;
	            finnalColor += tex2D (_MainTex, i.uv) * 0.324000001f;
	            finnalColor += tex2D (_MainTex, i.uv + _Parameter.xy) *  0.231999993f;
	            finnalColor += tex2D (_MainTex, i.uv + _Parameter.xy * 2.0) * 0.0855000019f;
	            finnalColor += tex2D (_MainTex, i.uv + _Parameter.xy * 3.0) * 0.0205000006f;
                return finnalColor;
            }
        ENDCG
        }
    }
}