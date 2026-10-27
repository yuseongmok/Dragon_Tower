using UnityEngine;
namespace DragonTower {
 // Two bounded, pixel-snapped meshes. All animation reads battle time, not gameplay RNG.
 public sealed class SentryBossSkillVfx {
  BattleView view;LightningMeshLayer floor,air;
  static readonly Color Dark=new Color(.12f,.15f,.26f),Blue=new Color(.27f,.43f,.73f),Cyan=new Color(.49f,.86f,.94f),Pale=new Color(.89f,.96f,1),Copper=new Color(.65f,.53f,.35f);
  public int QuadCount=>(floor==null?0:floor.Count)+(air==null?0:air.Count);
  public int DroppedQuads=>(floor==null?0:floor.Overflow)+(air==null?0:air.Overflow);
  public void Initialize(BattleView v){view=v;floor=Layer("Sentry electrical floor (pooled)");air=Layer("Sentry lightning and core (pooled)");}
  LightningMeshLayer Layer(string name){var g=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=g.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);var m=g.GetComponent<LightningMeshLayer>();m.raycastTarget=false;return m;}
  public void Clear(){if(floor!=null){floor.Begin();floor.End();air.Begin();air.End();}}
  public void Destroy(){if(floor!=null)Object.Destroy(floor.gameObject);if(air!=null)Object.Destroy(air.gameObject);}
  Color A(Color c,float a){c.a=a;return c;}
  void Depth(LightningMeshLayer m,Transform actor,bool after){int i=actor.GetSiblingIndex()+(after?1:0);if(m.transform.GetSiblingIndex()<actor.GetSiblingIndex())i--;m.transform.SetSiblingIndex(i);}
  void Ring(LightningMeshLayer m,Vector2 p,float rx,float ry,float w,Color c){for(int i=0;i<32;i++){float a=i*Mathf.PI/16,z=(i+1)*Mathf.PI/16;m.Line(p+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),p+new Vector2(Mathf.Cos(z)*rx,Mathf.Sin(z)*ry),w,c);}}
  void Bolt(Vector2 from,Vector2 to,float width,float alpha,float seed){Vector2 last=from;for(int i=1;i<=9;i++){float t=i/9f;var p=Vector2.Lerp(from,to,t);if(i<9)p.x+=Mathf.Sin(i*7.71f+seed)*18;air.Line(last,p,width+5,A(Blue,alpha));air.Line(last,p,width,A(Cyan,alpha));air.Line(last,p,2,A(Pale,alpha));if(i==3||i==6){var branch=p+new Vector2((i==3?-1:1)*28,-21);air.Line(p,branch,3,A(Cyan,alpha*.7f));}last=p;}}
  public void Render(BattleModel b){
   floor.Begin();air.Begin();Depth(floor,view.enemyArt.transform,false);Depth(air,view.playerArt.transform,true);
   var motion=view.GetComponent<BattleAnimation>();var source=motion.BossSkillEnemyGround;var target=motion.BossSkillPlayerGround;float time=(float)b.Time;
   // A small world-space capacitor beside the boss leaves its silhouette and HUD clear.
   var core=source+new Vector2(136,80);float charge=b.BossCoreCharge/100f;
   Ring(air,core,22,28,5,Dark);Ring(air,core,20,26,3,Copper);air.Diamond(core,12,19,Blue);air.Diamond(core+Vector2.up*2,7,12,A(Cyan,.35f+.65f*charge));air.Diamond(core+new Vector2(-3,6),2,4,Pale);
   for(int i=0;i<10;i++){var p=core+new Vector2(-27+i*6,-36);air.Line(p,p+Vector2.up*6,4,i<charge*10?Cyan:Dark);}
   if(charge>.7f)for(int i=0;i<3;i++){float a=time*2+i*2.094f;air.Diamond(core+new Vector2(Mathf.Cos(a)*28,Mathf.Sin(a)*34),2,3,Cyan);}
   if(b.BossSkillBusy){var s=b.ActiveBossSkill;bool ring=s.attackVfx=="sentry_ring",judgement=s.requiresFullCore;float t=b.BossPhaseProgress;
    if(b.BossPreparation){
     if(ring){Ring(floor,source,70,18,4,Blue);Ring(floor,source,52+15*t,13,2,Cyan);for(int k=1;k<5;k++){var p=Vector2.Lerp(source,target,k/5f);floor.Line(p+new Vector2(-7,6),p,2,A(Cyan,.65f));floor.Line(p+new Vector2(7,6),p,2,A(Cyan,.65f));}}
     else {float shrink=b.BossState==BossSkillState.Warning?1:1-t;Ring(floor,target,68,20,5,Dark);Ring(floor,target,66,18,3,Blue);Ring(floor,target,66+30*shrink,18+9*shrink,2,Cyan);for(int sign=-1;sign<=1;sign+=2)floor.Line(target+new Vector2(sign*76,-7),target+new Vector2(sign*76,10),3,Cyan);
      // Fixed overhead spark signals the vertical trajectory without a screen flash.
      var top=new Vector2(target.x,-230);air.Diamond(top,judgement?17:10,judgement?29:19,Blue);air.Diamond(top,judgement?9:5,judgement?19:12,Cyan);
      if(judgement){Ring(air,core,29+3*Mathf.Sin(time*8),34,3,Cyan);for(int side=-1;side<=1;side+=2)Bolt(core+new Vector2(side*20,0),new Vector2(side*185,-315),2,.5f,side);}
     }
    }
    if(b.BossState==BossSkillState.Travel){
     if(ring){var p=Vector2.Lerp(source,target,t);Ring(floor,p,66+42*t,17+11*t,7,Dark);Ring(floor,p,66+42*t,17+11*t,4,Cyan);Ring(floor,p+Vector2.up*7,64+42*t,16+11*t,2,Blue);for(int k=0;k<8;k++){float a=k*Mathf.PI/4;air.Diamond(p+new Vector2(Mathf.Cos(a)*(66+42*t),Mathf.Sin(a)*(17+11*t)),3,5,Pale);}}
     else {var top=new Vector2(target.x,-230);var end=Vector2.Lerp(top,target,t);Bolt(top,end,judgement?12:7,1,b.BossShotNumber*4);Ring(floor,target,66,18,3,Cyan);}
    }
    float age=(float)(b.Time-b.BossImpactAt);if(b.BossSkillHasImpacted&&age>=0&&age<.32f){float a=1-age/.32f;
     if(!ring)Bolt(new Vector2(target.x,-230),target,judgement?14:8,a,b.BossShotNumber*4);
     Ring(floor,target,65+age*200,18+age*55,4,A(Cyan,a));for(int i=0;i<12;i++){float angle=i*2.39996f;var p=target+new Vector2(Mathf.Cos(angle)*(30+age*180),Mathf.Sin(angle)*18+age*80);air.Diamond(p,3,5,A(Cyan,a));}
    }
    if(s.lowHPShots>0&&b.EnemyHP*100f/b.CurrentEnemyMaxHP<=s.lowHPShotThreshold)for(int i=0;i<s.lowHPShots;i++)air.Diamond(source+new Vector2((i-.5f)*16,105),3,4,i<b.BossShotNumber?Cyan:Dark);
   }
   floor.End();air.End();
  }
 }
}
