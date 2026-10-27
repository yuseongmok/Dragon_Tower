using UnityEngine;
namespace DragonTower {
 public sealed partial class PrimordialBossSkillVfx {
  void RenderVoidClaw(BattleModel b,Vector2 ground,float clock){
   bool second=b.BossShotNumber==2,prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel;
   bool wait=b.BossDirection!=BossBladeDirection.Front;Color cue=wait?purple:gold;
   float u=b.BossPhaseProgress,age=(float)(b.Time-b.BossImpactAt);
   bool scar=b.BossSkillHasImpacted||second;
   float fade=b.BossState==BossSkillState.Recovery?1-u:1;
   Color abyss=new Color(.045f,.025f,.085f),violet=new Color(.38f,.10f,.61f),edge=new Color(.76f,.43f,.94f);
   rear.transform.SetAsLastSibling();front.transform.SetAsLastSibling();floor.transform.SetAsLastSibling();
   for(int claw=0;claw<3;claw++){
    var from=new Vector2(-295,-110-claw*105);var to=new Vector2(295,-580-claw*105);
    if(b.ActiveBossSkill.randomClawJudgment&&b.VoidClawMirrored){from.x=-from.x;to.x=-to.x;}
    var dir=(to-from).normalized;var n=new Vector2(-dir.y,dir.x);
    float reveal=scar?1:travel?Mathf.Clamp01(u/.75f):0;
    if(prep){rear.Line(from,to,7,A(abyss,.8f));rear.Line(from,to,3,A(cue,.5f+.4f*u));}
    if(reveal>0){
     for(int k=0;k<40;k++){
      float t=k/40f,z=(k+1)/40f;if(t>reveal)break;z=Mathf.Min(z,reveal);
      float width=(5+13*Mathf.Sin(t*Mathf.PI));
      var a=Vector2.Lerp(from,to,t)+n*Mathf.Sin(k*2.7f+claw)*3;
      var c=Vector2.Lerp(from,to,z)+n*Mathf.Sin((k+1)*2.7f+claw)*3;
      rear.Line(a,c,width+8,A(violet,fade*.6f));rear.Line(a,c,width,A(abyss,fade));
      rear.Line(a+n*width*.45f,c+n*width*.45f,3,A(edge,fade*.85f));
      rear.Line(a-n*width*.4f,c-n*width*.4f,2,A(violet,fade));
     }
     if(travel&&!second){var tip=Vector2.Lerp(from,to,reveal);front.Line(tip-dir*160,tip,22,edge);front.Line(tip-dir*95,tip,7,ivory);front.Diamond(tip,14,24,ivory);}
    }
    bool fire=second&&(travel||b.BossState==BossSkillState.Recovery);
    if(scar){
     float power=fire?(travel?u:Mathf.Clamp01(1-u)):(second&&prep?.15f+.3f*u:.1f);
     for(int k=0;k<27;k++){
      float t=(k+.5f)/27f;var q=Vector2.Lerp(from,to,t);
      float flick=.65f+.35f*Mathf.Sin(clock*23+k*4.7f+claw);
      float height=(25+65*power)*flick,baseWidth=5+9*power;
      var up=new Vector2(Mathf.Sin(clock*7+k)*.25f,1).normalized;
      var side=new Vector2(-up.y,up.x);
      // Curled segmented tongues, not triangular crystal spikes.
      for(int seg=0;seg<7;seg++){
       float v=seg/7f,w=(seg+1)/7f;
       float curl=Mathf.Sin(clock*10+k*2.3f+v*5)*height*.22f*v;
       float curl2=Mathf.Sin(clock*10+k*2.3f+w*5)*height*.22f*w;
       var a=q+up*height*v+side*curl;var z=q+up*height*w+side*curl2;
       float width=baseWidth*2*Mathf.Pow(1-v,1.3f);
       front.Line(a,z,width+4,A(abyss,fade*(.3f+power*.6f)));
       front.Line(a,z,width,A(violet,fade*(.3f+power*.7f)));
       if(v<.72f)front.Line(a,z,width*.48f,A(edge,fade*power));
       if(fire&&v<.3f)front.Line(a,z,width*.2f,A(new Color(.89f,.7f,1),fade*power));
      }
      var spark=q+up*(height+Mathf.Repeat(clock*.65f+k*.137f,1)*55);
      front.Diamond(spark,2,4,A(edge,fade*(.2f+power*.6f)));
     }
    }
   }
   if(prep){Telegraph(ground,u,cue,wait);
    // Bright response markers stay above the dark-violet artwork for both stages.
    for(int i=0;i<3;i++){var q=ground+new Vector2((i-1)*45,75);Ring(floor,q,8,1,3,cue,wait?4:3);}}
   if(b.BossSkillHasImpacted&&age<.3f){float f=1-age/.3f;Ring(front,ground,30+age*300,.35f,6*f,A(edge,f));}
  }
 }
}
