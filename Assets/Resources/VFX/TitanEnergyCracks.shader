Shader "DragonTower/Titan Energy Cracks"
{
 Properties
 {
  [PerRendererData] _MainTex ("Sprite",2D)="white" {}
  _Charge ("Charge",Range(0,1))=0
  _ActorRect ("Actor bounds",Vector)=(0,0,1,1)
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
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
   #include "UnityCG.cginc"
   #include "UnityUI.cginc"
   struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   struct v2f {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float4 world:TEXCOORD1;float2 absolute:TEXCOORD2;};
   sampler2D _MainTex;float4 _ClipRect;float4 _ActorRect;float _Charge;
   v2f vert(appdata v){v2f o;o.world=v.vertex;o.absolute=mul(unity_ObjectToWorld,v.vertex).xy;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target
   {
    fixed4 tex=tex2D(_MainTex,i.uv);fixed4 c=i.color;
    float orange=smoothstep(.12,.30,tex.r-tex.b)*smoothstep(.35,.68,tex.r)*smoothstep(.02,.14,tex.g-tex.b);
    float along=saturate((i.absolute.x-_ActorRect.x)/max(.001,_ActorRect.z-_ActorRect.x));
    float flow=saturate(_Charge*1.8-along*.8);
    c.a*=tex.a*orange*flow;
    #ifdef UNITY_UI_CLIP_RECT
    c.a*=UnityGet2DClipping(i.world.xy,_ClipRect);
    #endif
    return c;
   }
   ENDCG
  }
 }
}
