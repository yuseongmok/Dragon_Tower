using UnityEngine;
namespace DragonTower {
 public sealed partial class PrimordialBossSkillVfx {
  void RenderContractEnd(BattleModel b,Vector2 center,Vector2 target,float clock){
   float u=b.BossPhaseProgress;int shot=Mathf.Max(1,b.BossShotNumber);
   bool prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel;
   float fade=b.BossState==BossSkillState.Recovery?1-u:1;
   // Behind actors: the arena darkens, but health and the player remain readable.
   rear.Quad(new Vector2(-240,0),new Vector2(240,0),new Vector2(240,-850),new Vector2(-240,-850),A(ink,.87f*fade));
   Color c=shot==2?purple:gold;
   Circle(center,190,clock*.6f,c,.85f*fade);
   for(int i=0;i<3;i++){
    var p=new Vector2((i-1)*65,-155);
    Color rune=i==1?purple:gold;
    float size=i==shot-1?22:13;
    Glyph(front,p,i,size,A(rune,fade));
    if(i<shot-1)Ring(front,p,size+5,1,2,A(ivory,.4f*fade),12);
   }
   Ring(front,center,35+5*Mathf.Sin(clock*6),1,5,A(c,fade),24);
   front.Diamond(center,14,20,A(ivory,fade));
   if(prep||travel){
    floor.transform.SetAsLastSibling();
    Telegraph(target,travel?1:u,c,shot==2);
    Ring(front,new Vector2((shot-2)*65,-155),45*(1-(travel?1:u))+24,1,3,c,32);
   }
   if(travel){
    if(shot==1){
     var top=new Vector2(0,40);var end=Vector2.Lerp(top,target,u);
     front.Line(top,end,100,A(gold,.75f));front.Line(top,end,44,ivory);front.Line(top,end,12,Color.white);
    }else if(shot==2){
     for(int sign=-1;sign<=1;sign+=2){
      float x=sign*Mathf.Lerp(280,65,u);
      for(int k=0;k<7;k++){var p=new Vector2(x+sign*Mathf.Sin(k+clock*7)*12,-220-k*65);front.Line(p,p+new Vector2(sign*30,-60),19,A(purple,.85f));front.Line(p,p+new Vector2(sign*30,-60),4,ivory);}
     }
    }else{
     for(int k=0;k<6;k++)Ring(front,center,60+u*650-k*25,.9f,12,A(k%2==0?gold:purple,.8f),64);
    }
   }
   float age=(float)(b.Time-b.BossImpactAt);
   if(b.BossSkillHasImpacted&&age>=0&&age<.7f){
    float t=age/.7f;
    Burst(target,t,c,true);
    for(int i=0;i<32;i++){
     float a=i*2.39996f;var p=target+Polar(a,60+400*t,.85f);
     front.Line(p,p+Polar(a,40*(1-t)),5,A(i%2==0?gold:purple,1-t));
    }
   }
   if(b.BossState==BossSkillState.Recovery&&b.FirstContractEndSuccesses==3){
    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;front.Line(center+Polar(a,12),center+Polar(a,40+u*60),3,A(ivory,fade));}
   }
  }
 }
}
