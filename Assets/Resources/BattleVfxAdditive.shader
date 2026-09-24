Shader "DragonTower/Battle VFX Additive"
{
    Properties
    {
        [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        [HDR] _TintColor("Tint Color", Color) = (1,1,1,1)
        _FlowSpeed("Flow Speed", Vector) = (0,0,0,0)
        _Brightness("Brightness", Range(0.25,2)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off Lighting Off ZWrite Off ZTest LEqual
        // Both color and coverage accumulate additively into the transparent render texture.
        // Coverage is calculated from visible RGB below, so opaque black texture backgrounds
        // contribute zero alpha and cannot appear as rectangles during UI compositing.
        Blend One One, One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST,_TintColor,_FlowSpeed;
            float _Brightness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(v.vertex);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex)+_FlowSpeed.xy*_Time.y;
                o.color=v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 sample=tex2D(_MainTex,i.uv)*_TintColor*i.color;
                fixed3 rgb=sample.rgb*(sample.a*_Brightness);
                fixed coverage=saturate(max(rgb.r,max(rgb.g,rgb.b)));
                return fixed4(rgb,coverage);
            }
            ENDCG
        }
    }
}
