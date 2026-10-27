Shader "DragonTower/Contract Dragon Apparition" {
 Properties {[PerRendererData] _MainTex("Dragon",2D)="white"{} _StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil ID",Float)=0 _StencilOp("Stencil Op",Float)=0 _StencilWriteMask("Stencil Write Mask",Float)=255 _StencilReadMask("Stencil Read Mask",Float)=255 _ColorMask("Color Mask",Float)=15}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]} Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct A{float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};struct V{float4 vertex:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};sampler2D _MainTex;
 V vert(A i){V o;o.vertex=UnityObjectToClipPos(i.vertex);o.color=i.color;o.uv=i.uv;return o;}
 fixed4 frag(V i):SV_Target{fixed4 t=tex2D(_MainTex,i.uv);float l=dot(t.rgb,float3(.25,.55,.2));l=saturate(floor(l*5)/5*1.35);float3 lo=lerp(float3(.12,.08,.23),float3(.16,.20,.25),i.color.r);float3 hi=lerp(float3(.84,.60,1),float3(1,.92,.61),i.color.r);return fixed4(lerp(lo,hi,l),t.a*i.color.a);}
 ENDCG}
 }
}
