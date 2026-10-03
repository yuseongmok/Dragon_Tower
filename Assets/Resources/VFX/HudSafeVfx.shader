Shader "DragonTower/HUD Safe VFX"
{
 Properties
 {
  [PerRendererData] _MainTex ("Sprite",2D)="white" {}
  _DstBlend ("Destination Blend", Float)=10
  _HudFadeRange ("HUD world Y fade", Vector)=(300,245,0.24,0)
  _StencilComp ("Stencil Comparison", Float)=8
  _Stencil ("Stencil ID", Float)=0
  _StencilOp ("Stencil Operation", Float)=0
  _StencilWriteMask ("Stencil Write Mask", Float)=255
  _StencilReadMask ("Stencil Read Mask", Float)=255
  _ColorMask ("Color Mask", Float)=15
 }
 SubShader
 {
  Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
  Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha [_DstBlend] ColorMask [_ColorMask]
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
   #include "UnityCG.cginc"
   #include "UnityUI.cginc"
   struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   struct v2f {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float4 world:TEXCOORD1;float hudY:TEXCOORD2;};
   sampler2D _MainTex;float4 _ClipRect;float4 _HudFadeRange;
   v2f vert(appdata v){v2f o;o.world=v.vertex;o.hudY=mul(unity_ObjectToWorld,v.vertex).y;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target
   {
    fixed4 c=tex2D(_MainTex,i.uv)*i.color;
    #ifdef UNITY_UI_CLIP_RECT
    c.a*=UnityGet2DClipping(i.world.xy,_ClipRect);
    #endif
    float fade=saturate((_HudFadeRange.x-i.hudY)/max(0.001,_HudFadeRange.x-_HudFadeRange.y));
    c.a*=lerp(_HudFadeRange.z,1,fade*fade*(3-2*fade));
    return c;
   }
   ENDCG
  }
 }
}

