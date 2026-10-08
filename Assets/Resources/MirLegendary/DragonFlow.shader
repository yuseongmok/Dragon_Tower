Shader "DragonTower/Mir Divine Wind" {
 Properties { [PerRendererData] _MainTex("Dragon",2D)="white"{} _FlowTime("Flow",Float)=0 _Gold("Gold",Float)=0 _StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil ID",Float)=0 _StencilOp("Stencil Op",Float)=0 _StencilWriteMask("Stencil Write Mask",Float)=255 _StencilReadMask("Stencil Read Mask",Float)=255 _ColorMask("Color Mask",Float)=15 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]} Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};sampler2D _MainTex;float _FlowTime,_Gold;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
 fixed4 frag(v2f i):SV_Target {float2 uv=i.uv;fixed4 t=tex2D(_MainTex,uv);float pulse=pow(saturate(.5+.5*sin(uv.x*65-uv.y*18+_FlowTime*17)),12);float edge=saturate(t.a-min(tex2D(_MainTex,uv+float2(0,.008)).a,tex2D(_MainTex,uv-float2(0,.008)).a));float3 col=t.rgb*.86+float3(.03,.2,.17);col+=float3(.18,.65,.55)*pulse*.6;float highlights=saturate((t.r+t.g-t.b)*.7);col+=lerp(float3(.18,.65,.65),float3(1,.68,.14),_Gold)*(edge*.6+pulse*.4+highlights*_Gold*.25);float dissolve=saturate(i.color.a*1.8-.8+(.5+.5*sin(uv.x*21+uv.y*32-_FlowTime*9))*.4);return fixed4(col,t.a*i.color.a*lerp(.75,1,dissolve));}
 ENDCG }
 }
}
