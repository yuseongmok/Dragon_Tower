Shader "DragonTower/Haesin Split Water" {
 Properties { _MainTex("Silhouette only",2D)="white"{} _Cell("Cell",Float)=0 _Half("Half",Float)=1 _Age("Age",Float)=0 _Floor("Water surface",Float)=-9999 _Form("Formation",Float)=1 _Break("Water breakup",Float)=0 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct a {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct v {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;float worldY:TEXCOORD1;};
 sampler2D _MainTex;float _Cell,_Half,_Age,_Floor,_Form,_Break;
 v vert(a i){v o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;o.worldY=mul(unity_ObjectToWorld,i.vertex).y;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 fixed4 frag(v i):SV_Target {
 clip(i.worldY-_Floor);float2 u=float2(i.uv.x*2-_Cell,i.uv.y);clip(_Half*(u.y-.55*u.x-.21));
 // Reuse silhouette and coarse volume only; discard source color and fine anatomy.
 float2 q=floor(u*150)/150;float clock=floor(_Age*24)/24;
 float2 wobble=float2(sin(q.y*42+clock*8),cos(q.x*31-clock*7))*.004;
 float2 uv=float2((q.x+wobble.x+_Cell)*.5,q.y+wobble.y);
 float mask=tex2D(_MainTex,uv).a;
 float inner=min(min(tex2D(_MainTex,uv+float2(.004,0)).a,tex2D(_MainTex,uv-float2(.004,0)).a),min(tex2D(_MainTex,uv+float2(0,.008)).a,tex2D(_MainTex,uv-float2(0,.008)).a));
 float edge=saturate((mask-inner)*4);
 float drift=q.y*23+q.x*7+sin(q.x*18-clock*3)*1.3-clock*7;
 float current=pow(saturate(sin(drift)),7);
 float reverse=pow(saturate(cos(q.x*18-q.y*11+sin(q.y*16+clock*2)+clock*5)),15);
 float volume=saturate(1-length((q-float2(.60,.52))*float2(1.5,1.1)));
 float hull=(tex2D(_MainTex,uv+float2(.012,.02)).b+tex2D(_MainTex,uv+float2(-.012,.02)).b+tex2D(_MainTex,uv+float2(.012,-.02)).b+tex2D(_MainTex,uv+float2(-.012,-.02)).b)*.25;
 hull=floor(hull*4)/4;float3 col=lerp(float3(.008,.045,.13),float3(.025,.46,.67),saturate(volume*.45+hull*.9));
 current=floor(current*4)/4;reverse=floor(reverse*3)/3;
 col=lerp(col,float3(.06,.70,.84),current*.95);col=lerp(col,float3(.43,.91,.93),reverse*.65);
 float foam=edge*step(.3,hash(floor(q*65)+floor(clock*8)));col=lerp(col,float3(.88,1,1),foam);
 float2 jaw=(q-float2(.69,.60))/float2(.19,.15);float mouthRadius=length(jaw);float mouth=1-smoothstep(.86,1,mouthRadius);
 float rim=(1-smoothstep(.06,.15,abs(mouthRadius-1)))*step(.53,q.x);
 if(_Cell<.5){col=lerp(col,float3(.003,.025,.07),mouth*.95);col=lerp(col,float3(.52,.93,.96),rim*.9);float tooth=step(.7,jaw.y)*step(jaw.y,.95)*step(abs(frac(q.x*27)-.5),(.95-jaw.y)*1.7);col=lerp(col,float3(.83,1,1),tooth*mouth);}
 else {float seam=1-smoothstep(.006,.016,abs(q.y-(.56+.16*(q.x-.55)*(q.x-.55)*8)));seam*=step(.48,q.x)*step(q.x,.89);col=lerp(col,float3(.65,.97,.98),seam*.85);}
 col=floor(col*20)/20;
 float cut=1-smoothstep(.001,.006,abs(q.y-.55*q.x-.21));float cutLight=saturate((_Age-1.68)/.27);col=lerp(col,lerp(float3(.03,.7,.86),float3(.94,1,1),cutLight),cut*cutLight);
 float cells=hash(floor(q*40));float breakup=step(_Break,cells);float formation=saturate((_Form-q.y*.45)*2.4);
 float alpha=mask*(.65+volume*.12+current*.12+reverse*.10+foam*.25)*formation*breakup;
 return float4(col,alpha)*i.color;
 }
 ENDCG }
 }
}
