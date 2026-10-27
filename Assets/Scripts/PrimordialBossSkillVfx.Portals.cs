using UnityEngine;
namespace DragonTower {
 public sealed partial class PrimordialBossSkillVfx {
  DarkVfxStyle portalStyle;
  // Adapted from the player's Apocalypse Rift and Column, using the same style asset.
  void ContractRift(Vector2 at,float radius,float time,float alpha,Color cue,bool active){
   var s=portalStyle;
   for(int layer=0;layer<4;layer++)for(int j=0;j<64;j++){
    float a=j*Mathf.PI/32,z=(j+1)*Mathf.PI/32,r=radius*(1-layer*.065f);
    float ra=r*(1+.12f*Mathf.Sin(j*1.7f)+.04f*Mathf.Sin(time*8+j)),rz=r*(1+.12f*Mathf.Sin((j+1)*1.7f)+.04f*Mathf.Sin(time*8+j+1));
    rear.Quad(at,at+Polar(a,ra,.26f),at+Polar(z,rz,.26f),at,A(layer==3?s.core:layer==2?s.purple:layer==1?s.bright:s.blackPurple,alpha));
   }
   // Broken orbit segments, rune anchors, and inward embers give the portal depth.
   for(int k=0;k<20;k++){
    float a=k*Mathf.PI/10+time*.15f;var p=at+Polar(a,radius*1.14f,.3f);
    Glyph(rear,p,k,5,A(cue,alpha*(active?.85f:.35f)));
    var inward=at+Polar(a,radius*(.35f+.55f*Mathf.Repeat(time*.5f+k*.13f,1)),.23f);
    rear.Line(inward,inward+Polar(a,12,.3f),2,A(cue,alpha*.5f));
   }
   Ring(rear,at,radius*1.07f,.28f,active?5:2,A(cue,alpha*(active?1:.25f)),64);
   for(int i=0;i<16;i++){float a=i*2.4f;var p=at+Polar(a,radius,.28f);rear.Line(p,p+Polar(a,14+i%4*8),3,A(s.purple,alpha));}
  }
  void ContractColumn(Vector2 from,Vector2 to,float radius,float time,float alpha,bool light){
   var dir=(to-from).normalized;var n=new Vector2(-dir.y,dir.x);
   float length=Vector2.Distance(from,to);if(length<2)return;
   Color deep=light?new Color(.28f,.10f,.015f):new Color(.035f,.008f,.07f);
   Color body=light?new Color(.94f,.56f,.10f):new Color(.52f,.17f,.76f);
   Color mist=light?new Color(1,.88f,.46f):new Color(.77f,.46f,.93f);
   Color core=light?new Color(1,.98f,.80f):new Color(.89f,.82f,1);
   // A continuous cylindrical curtain, with coarse animated density rather than separate strands.
   for(int x=0;x<32;x++)for(int y=0;y<44;y++){
    float u=x/32f,v=y/44f,xx=(u*2-1),yy=v*length;
    float flow=time*5-v*10;
    float noise=Mathf.Sin(xx*9+flow)+.6f*Mathf.Sin(xx*21-flow*1.3f)+.4f*Mathf.Sin(v*27+xx*13+time*8);
    float shade=noise*.25f+.55f-Mathf.Abs(xx)*.2f;
    Color c=shade<.22f?deep:shade<.48f?body:shade<.78f?mist:core;
    var q=from+dir*yy+n*(xx*radius);
    front.Quad(q,q+n*(radius*2/32),q+n*(radius*2/32)+dir*(length/44+1),q+dir*(length/44+1),A(c,alpha*.74f));
   }
   // Broad black flames rise inside the luminous curtain; pointed and torn silhouettes.
   for(int k=0;k<15;k++){
    float along=Mathf.Repeat(time*.3f+k*.173f,1),offset=Mathf.Sin(k*7.13f)*radius*.88f;
    var root=Vector2.Lerp(from,to,along)+n*offset;
    float h=Mathf.Min(length*.55f,90+(k%4)*33),w=16+k%3*9;
    for(int j=0;j<12;j++){
     float u=j/12f,v=(j+1)/12f;
     Vector2 a=root+dir*(u*h)+n*Mathf.Sin(time*7+k+u*9)*w*u,b=root+dir*(v*h)+n*Mathf.Sin(time*7+k+v*9)*w*v;
     float width=w*(1-u)*(1+.3f*Mathf.Sin(k+u*18));
     if(Vector2.Dot(b-from,dir)>length)break;
     front.Line(a,b,width*2,A(deep,alpha*.95f));
     front.Line(a+n*width*.7f,b+n*width*.6f,3,A(body,alpha*.9f));
    }
   }
   // Sparse white-violet fracture flashes, changing shape in discrete animation frames.
   int frame=Mathf.FloorToInt(time*12);
   for(int k=0;k<5;k++){
    float x=(k-2)*radius*.4f;Vector2 previous=from+n*x;
    for(int j=1;j<=14;j++){
     float t=j/14f;var q=Vector2.Lerp(from,to,t)+n*(x+Mathf.Sin(j*12.7f+k*9+frame)*22);
     if((frame+k)%4!=0){front.Line(previous,q,8,A(mist,alpha*.3f));front.Line(previous,q,2,A(core,alpha*.9f));}
     if(j%4==0)front.Line(q,q+n*23-dir*25,2,A(core,alpha*.6f));previous=q;
    }
   }
   // Short axial streaks sell rapid energy flow without adding another damage flash.
   for(int k=0;k<18;k++){
    float v=Mathf.Repeat(time*3+k*.173f,1);var q=Vector2.Lerp(from,to,v)+n*Mathf.Sin(k*8.3f)*radius*.86f;
    float trail=Mathf.Min(28+k%3*9,length*v);
    front.Line(q-dir*trail,q,2,A(core,alpha*.55f));
   }
   // Bright sidewalls and flattened rims reinforce a single enormous energy volume.
   for(int sign=-1;sign<=1;sign+=2){front.Line(from+n*radius*sign,to+n*radius*sign,8,A(body,alpha*.8f));front.Line(from+n*(radius-5)*sign,to+n*(radius-5)*sign,2,A(core,alpha*.7f));}
   Ring(front,to,radius,.14f,7,A(mist,alpha*.8f),64);
  }
  void ContractSeal(Vector2 at,float r,float clock,float alpha){
   Color lavender=new Color(.82f,.66f,.98f),white=new Color(1,.94f,1);
   for(int j=0;j<64;j++){float a=j*Mathf.PI/32,b=(j+1)*Mathf.PI/32;front.Quad(at,at+Polar(a,r),at+Polar(b,r),at,A(new Color(.07f,.025f,.12f),alpha*.88f));}
   for(int j=0;j<4;j++)Ring(front,at,r*(.68f+j*.105f),1,j==3?5:2,A(lavender,alpha*.8f),96,clock*.1f);
   for(int k=0;k<24;k++){float a=k*Mathf.PI/12-clock*.15f;Glyph(front,at+Polar(a,r*.94f),k,5,A(lavender,alpha));}
   for(int k=0;k<3;k++){
    float a=k*Mathf.PI*2/3+clock*.3f;var p=at+Polar(a,r*.57f);
    Ring(front,p,r*.43f,1,3,A(white,alpha*.8f),64,-clock);
    Ring(front,p,r*.36f,1,2,A(lavender,alpha),6,clock*.25f);
    for(int j=0;j<5;j++){float b=j*Mathf.PI*.4f+clock*.25f;front.Line(p+Polar(b,r*.35f),p+Polar(b+Mathf.PI*.8f,r*.35f),2,A(lavender,alpha*.7f));}
   }
   for(int k=0;k<12;k++){
    float a=k*Mathf.PI/6,noise=Mathf.Sin(k*7+Mathf.Floor(clock*15));
    var end=at+Polar(a,r*.65f);var mid=at+Polar(a+.17f*noise,r*.36f);
    front.Line(at,mid,6,A(purple,alpha*.55f));front.Line(mid,end,4,A(lavender,alpha));front.Line(at,mid,2,A(white,alpha));
   }
   front.Diamond(at,r*.12f,r*.25f,A(white,alpha));front.Diamond(at,r*.24f,r*.09f,A(white,alpha));
  }
  void ContractSealBurst(Vector2 at,float t,float clock){
   float spread=1-Mathf.Exp(-t*12),fade=Mathf.Clamp01(1-t/1.2f);
   ContractSeal(at,145+spread*95,clock,fade);
   Color white=new Color(1,.95f,1),lavender=new Color(.82f,.65f,1);
   float flash=Mathf.Clamp01(1-t/.22f);
   for(int k=0;k<40;k++){
    float a=k*2.39996f;var d=Polar(a,1);var n=Polar(a+Mathf.PI*.5f,1);
    float inner=15+spread*70,outer=inner+(80+k%7*20)*Mathf.Clamp01(1-t/.7f);
    front.Quad(at+d*inner,at+d*(inner+25)-n*(5+k%4),at+d*outer,at+d*(inner+25)+n*(5+k%4),A(k%3==0?white:lavender,fade));
   }
   for(int k=0;k<3;k++)Ring(front,at,40+spread*(170+k*65),1,5*fade,A(lavender,fade*.7f),96);
   front.Diamond(at,95*flash,170*flash,A(white,flash));front.Diamond(at,170*flash,65*flash,A(white,flash));
   for(int k=0;k<45;k++){var p=at+Polar(k*2.39996f,60+t*(180+k%5*45));front.Diamond(p,4*fade,10*fade,A(k%2==0?white:purple,fade));}
  }
  void RenderContractPortals(BattleModel b,Vector2 target,float clock){
   if(portalStyle==null)portalStyle=Resources.Load<DarkSkillLibrary>("DarkSkills/Library").style;
   float u=b.BossPhaseProgress,fade=b.BossState==BossSkillState.Recovery?1-u:1;
   bool prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel,wait=b.BossDirection!=BossBladeDirection.Front;
   int shot=b.BossShotNumber;Color cue=wait?purple:gold;front.transform.SetAsLastSibling();
   rear.Quad(new Vector2(-240,0),new Vector2(240,0),new Vector2(240,-850),new Vector2(-240,-850),A(ink,.85f*fade));
   var top=new Vector2(0,-95);var bottom=new Vector2(0,-760);
   float open=b.BossState==BossSkillState.Warning?Mathf.SmoothStep(.15f,1,u):1;
   bool upper=shot!=2,lower=shot>=2;
   float age=(float)(b.Time-b.BossImpactAt);
   bool after=b.BossSkillHasImpacted&&age<.45f&&!prep&&!travel;
   bool compress=shot==3&&b.BossState==BossSkillState.Interval||shot==4&&b.BossState!=BossSkillState.Recovery;
   float pressure=shot==3?Mathf.Clamp01(age/.65f)*.25f:shot==4?(prep?.25f+.65f*u:travel?.9f+.1f*u:.25f):0;
   float quiet=shot==4&&travel?Mathf.Clamp01((u-.65f)/.35f):0;
   var meeting=new Vector2(0,-425);
   if(compress)meeting+=new Vector2(Mathf.Sin(clock*31)*4*pressure,Mathf.Sin(clock*12)*12*(1-quiet));
   if(compress){
    float width=174-35*pressure,feeding=.85f*(1-quiet);
    ContractColumn(top,meeting,width,clock,feeding,false);
    ContractColumn(bottom,meeting,width,-clock,feeding,true);
    // Opposed jets remain connected; surplus energy spits sideways from the contact plane.
    for(int k=0;k<24;k++){
     float life=Mathf.Repeat(clock*(1.6f+pressure)+k*.137f,1),sign=k%2==0?-1:1;
     var q=meeting+new Vector2(sign*(20+life*230),Mathf.Sin(k*4.3f)*life*65);
     float f=Mathf.Sin(life*Mathf.PI)*(1-quiet);
     front.Line(q-new Vector2(sign*(14+pressure*22),0),q,3+pressure*3,A(k%3==0?ivory:k%2==0?purple:gold,f));
    }
   }
   if(shot<4&&(travel||after)&&!compress){
    float reach=travel?(1-Mathf.Pow(1-u,3)):1,alpha=travel?.9f:(1-age/.45f)*.9f;
    if(upper)ContractColumn(top,Vector2.Lerp(top,shot==3?meeting:bottom,reach),shot==3?174:153,clock,alpha,shot==3?false:!wait);
    if(lower)ContractColumn(bottom,Vector2.Lerp(bottom,shot==3?meeting:top,reach),shot==3?174:153,-clock,alpha,shot==3?true:!wait);
   }
   if(travel&&shot<4){
    float pulse=Mathf.Clamp01(1-u/.22f);
    if(upper){Ring(front,top,178+26*(1-pulse),.28f,8*pulse,A(shot==3?portalStyle.bright:cue,pulse),64);}
    if(lower){Ring(front,bottom,178+26*(1-pulse),.28f,8*pulse,A(shot==3?ivory:cue,pulse),64);}
   }
   ContractRift(top,178*open,clock,fade,shot>=3?purple:cue,upper&&(shot<4||compress));
   ContractRift(bottom,178*open,-clock,fade,shot>=3?gold:cue,lower&&(shot<4||compress));
   if(shot==3&&after){
    float impact=1-Mathf.Clamp01(age/.22f);
    Ring(front,meeting,35+age*900,.18f,14*impact,A(ivory,impact),64);
    front.Diamond(meeting,130*impact,28*impact,A(ivory,impact));
    float t=Mathf.Clamp01(age/.45f),f=1-t;
    for(int k=0;k<36;k++){float a=k*2.39996f;var q=meeting+Polar(a,35+t*(130+k%5*25));front.Line(q-Polar(a,24*f),q,5*f,A(k%2==0?purple:gold,f));front.Diamond(q,4*f,12*f,A(ivory,f));}
   }
   if(shot==3&&(after||compress)||shot==4){
    if(shot==4&&b.BossState==BossSkillState.Recovery)ContractSealBurst(meeting,u*1.2f,clock);
    else{
     float r=shot==3?35+Mathf.Clamp01(age/.45f)*55:prep?90+u*65:travel?Mathf.Lerp(155,65,Mathf.SmoothStep(0,1,u)):90;
     if(quiet>0)front.Quad(new Vector2(-240,0),new Vector2(240,0),new Vector2(240,-850),new Vector2(-240,-850),A(ink,quiet*.6f));
     ContractSeal(meeting,r,clock*(1+pressure),1);
     for(int k=0;k<10;k++){
      float a=k*Mathf.PI/5+clock*.4f;var start=meeting+Polar(a,r*.45f);
      var bend=meeting+Polar(a+.12f*Mathf.Sin(clock*25+k),r*.8f);
      var end=meeting+Polar(a,r*1.1f);
      front.Line(start,bend,3,A(ivory,pressure*(1-quiet)));front.Line(bend,end,2,A(purple,pressure*(1-quiet)));
     }
     for(int j=0;j<24;j++){float angle=j*2.39996f;var q=meeting+Polar(angle,r+35+15*Mathf.Sin(clock*9+j));front.Line(q,q+Polar(angle,15),3,j%2==0?purple:ivory);}
    }
   }
   if(prep||travel){floor.transform.SetAsLastSibling();Telegraph(target,travel?1:u,cue,wait);
    for(int i=0;i<3;i++)Ring(floor,target+new Vector2((i-1)*40,90),9,1,3,cue,wait?4:3);
   }
   if(b.BossSkillHasImpacted&&age<.55f){
    float f=1-age/.55f;for(int i=0;i<3;i++)Ring(front,target,30+age*(260+i*150),.4f+i*.15f,6*f,A(portalStyle.bright,f*.8f));
    for(int i=0;i<30;i++){var p=target+Polar(i*2.39996f,35+age*(210+i%5*45));front.Diamond(p,5*f,13*f,A(portalStyle.purple,f));}
   }
  }
 }
}

