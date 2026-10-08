Shader "DragonTower/Hydra Living Toxin" {
 Properties { [PerRendererData] _MainTex("Atlas",2D)="white"{} _FlowTime("Flow",Float)=0 _Phase("Phase",Float)=0 _RiftCenter("Rift center",Vector)=(0,0,0,0) _StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil ID",Float)=0 _StencilOp("Stencil Op",Float)=0 _StencilWriteMask("Stencil Write Mask",Float)=255 _StencilReadMask("Stencil Read Mask",Float)=255 _ColorMask("Color Mask",Float)=15 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] } Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float2 local:TEXCOORD1;};sampler2D _MainTex;float _FlowTime,_Phase;float4 _RiftCenter;
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 v2f vert(appdata v){v2f o;o.local=v.vertex.xy;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target {
  float2 cell=floor(i.local/2)*2;float n=noise(cell*.023+float2(-_FlowTime*.72,_FlowTime*.34));float current=noise(cell*.035+float2(_FlowTime*.5,-_FlowTime*.8));
  float2 wobble=float2(n-.5,current-.5)*.007;fixed4 tex=tex2D(_MainTex,i.uv+wobble);float originalToxin=saturate((tex.g-max(tex.r,tex.b))*3);
  float edge=saturate(tex.a-min(tex2D(_MainTex,i.uv+float2(.004,0)).a,tex2D(_MainTex,i.uv-float2(.004,0)).a));
  float ridge=pow(saturate(1-abs(current-.52)*10),3);float face=saturate(dot(tex.rgb,float3(.22,.5,.28)));
  // Readable broad violet planes; fine poison veins remain an accent.
  float plane=floor(saturate(face*2.1+n*.24)*4)/3;
  float3 col=lerp(float3(.12,.045,.20),float3(.57,.29,.72),saturate(plane));
  col+=float3(.24,.70,.045)*ridge*(.16+.24*n);col+=float3(.46,.95,.10)*originalToxin*(.55+.25*n);col+=float3(.48,.32,.62)*edge*.85;
  float vapor=smoothstep(.12,.52,n);float rear=smoothstep(.025,.23,i.uv.x+n*.10);float alpha=tex.a*(.88+.10*vapor)*rear*i.color.a;
  // A physical closure plane occludes the summon. It never shrinks the head texture.
  float2 rel=i.local-_RiftCenter.xy;float u=saturate(dot(rel,float2(.82,.57236))/525+.5);float close=smoothstep(0,1,saturate(((_Phase-.86)/.14)*1.5-(1-u)*.5));float width=pow(max(.001,sin(u*3.14159265)),.75)*136*(1-close);float across=abs(dot(rel,float2(-.57236,.82)));float mask=1-smoothstep(width-10,width+3,across);
  alpha*=lerp(1,mask,saturate((_Phase-.84)*50));float emerge=smoothstep(.10,.85,i.color.a);alpha*=smoothstep((1-emerge)*.8,(1-emerge)*.8+.18,n+.30);
  return fixed4(col,alpha);
 }
 ENDCG }
 }
}
