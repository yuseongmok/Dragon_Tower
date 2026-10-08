Shader "DragonTower/Mir Wind Volume" {
 Properties { _MainTex("Texture",2D)="white"{} _Age("Age",Float)=0 _Mode("Mode",Float)=0 _Opacity("Opacity",Float)=1 _Gold("Gold",Float)=0 _StencilComp("Stencil Comparison",Float)=8 _Stencil("Stencil ID",Float)=0 _StencilOp("Stencil Op",Float)=0 _StencilWriteMask("Stencil Write Mask",Float)=255 _StencilReadMask("Stencil Read Mask",Float)=255 _ColorMask("Color Mask",Float)=15 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]} Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata{float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};struct v2f{float4 vertex:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};float _Age,_Mode,_Opacity,_Gold;
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 float fbm(float2 p){return noise(p)*.55+noise(p*2.03)*.28+noise(p*4.1)*.17;}
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target {float2 uv=floor(i.uv*float2(170,240))/float2(170,240);float density,light,edge;
 if(_Mode<.5){float x=(uv.x-.5)*2;float cylinder=sqrt(saturate(1-x*x));float angle=asin(clamp(x,-.999,.999));float2 flow=float2(angle*2.1-_Age*3,uv.y*6-_Age*2.4);float n=fbm(flow+float2(sin(uv.y*11-_Age*3)*.5,0));float wisps=fbm(flow*float2(2,3)+n*2);edge=saturate(cylinder*2.5)*smoothstep(0,.10,uv.y)*smoothstep(1,.82,uv.y);density=smoothstep(.32,.74,n*.7+wisps*.3)*edge;light=saturate(n*.7+pow(wisps,3)*1.5+cylinder*.13);}
 else {float2 q=(uv-.5)*2;float r=length(q);float a=atan2(q.y,q.x);float n=fbm(float2(a*2.5,r*7-_Age*6)+float2(sin(r*8-_Age*4),0));float spokes=pow(saturate(.5+.5*sin(a*13+r*9-_Age*9+n*5)),3);edge=saturate((1-r)*4);density=edge*smoothstep(.28,.72,n+spokes*.23);light=saturate((1-r)*.55+n*.5+spokes*.4);}
 float3 col=lerp(float3(.015,.15,.17),float3(.05,.70,.46),saturate(light*1.4));col=lerp(col,float3(.45,.98,.86),pow(light,3));col=lerp(col,float3(1,.79,.32),_Gold*pow(light,2)*.8);return fixed4(col,density*_Opacity*i.color.a);
 }
 ENDCG }
 }
}
