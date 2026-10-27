using UnityEngine;
namespace DragonTower {
 // Two bounded, pooled meshes; model-clock presentation only.
 public sealed class LegacyPrimordialBossSkillVfx {
  BattleView view; LightningMeshLayer floor,body;
  readonly Color ink=new Color(.13f,.13f,.22f),gold=new Color(.82f,.65f,.32f),ivory=new Color(.96f,.89f,.71f),purple=new Color(.52f,.35f,.70f);
  public int QuadCount=>(floor==null?0:floor.Count)+(body==null?0:body.Count);
  public int DroppedQuads=>(floor==null?0:floor.Overflow)+(body==null?0:body.Overflow);
  public void Initialize(BattleView v){view=v;floor=Layer("Contract floor (pooled)");body=Layer("Contract effects (pooled)");}
  LightningMeshLayer Layer(string name){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);var m=go.GetComponent<LightningMeshLayer>();m.raycastTarget=false;return m;}
  public void Clear(){if(floor!=null){floor.Begin();floor.End();}if(body!=null){body.Begin();body.End();}}
  public void Destroy(){if(floor!=null)Object.Destroy(floor.gameObject);if(body!=null)Object.Destroy(body.gameObject);}
  static Color A(Color c,float a){c.a=a;return c;}
  void Ring(LightningMeshLayer m,Vector2 p,float r,float squash,float w,Color c){for(int i=0;i<32;i++){float a=i*Mathf.PI/16,z=(i+1)*Mathf.PI/16;m.Line(p+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*squash),p+new Vector2(Mathf.Cos(z)*r,Mathf.Sin(z)*r*squash),w,c);}}
  void Depth(LightningMeshLayer m,bool front){int n=view.playerArt.transform.GetSiblingIndex()+(front?1:0);if(m.transform.GetSiblingIndex()<view.playerArt.transform.GetSiblingIndex())n--;m.transform.SetSiblingIndex(n);}
  void Spear(Vector2 p,Color c){body.Line(p+Vector2.up*76,p,9,ink);body.Line(p+Vector2.up*72,p,4,c);body.Quad(p+new Vector2(-16,18),p+Vector2.up*36,p+new Vector2(16,18),p+Vector2.down*20,ink);body.Quad(p+new Vector2(-11,18),p+Vector2.up*29,p+new Vector2(11,18),p+Vector2.down*14,c);body.Line(p+Vector2.up*24,p+Vector2.down*12,2,ivory);}
  public void Render(BattleModel b){
   floor.Begin();body.Begin();Depth(floor,false);Depth(body,true);
   var motion=view.GetComponent<BattleAnimation>();var target=motion.BossSkillPlayerGround;var source=motion.BossSkillEnemyGround;var chest=source+Vector2.up*150;
   Color c=b.PrimordialPhase==1?gold:purple;
   if(b.PrimordialTransitioning){
    float t=b.PrimordialTransitionProgress;
    Ring(body,target+Vector2.up*48,75,1.15f,5,A(gold,Mathf.Sin(t*Mathf.PI)));
    for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var p=chest+new Vector2(Mathf.Cos(a)*105,Mathf.Sin(a)*105);body.Line(p,p+new Vector2(i%2==0?18:-18,28),4,A(purple,t));body.Diamond(p+Vector2.up*t*40,4,8,A(ivory,1-t));}
    body.Line(new Vector2(0,-180),chest,3,A(gold,1-t));floor.End();body.End();return;
   }
   if(!b.BossSkillBusy){floor.End();body.End();return;}
   float u=b.BossPhaseProgress;bool prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel;string key=b.ActiveBossSkill.attackVfx;
   if(key=="contract_seal"){
    if(b.BossBarrierActive||prep&&b.BossBarrierOutcome==BossGimmickOutcome.None){
     Ring(body,chest,100,1.05f,8,ink);Ring(body,chest,96,1.05f,3,c);
     for(int i=0;i<8;i++){float a=i*Mathf.PI/4+(float)b.Time*.15f;var p=chest+new Vector2(Mathf.Cos(a)*96,Mathf.Sin(a)*100);body.Diamond(p,8,12,ivory);body.Line(p,chest,2,A(c,.4f));}
    }
    if(b.BossBarrierOutcome==BossGimmickOutcome.Broken)for(int i=0;i<16;i++){float a=i*Mathf.PI/8;body.Diamond(chest+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(95+80*u),6*(1-u),12*(1-u),A(c,1-u));}
    if(b.BossBarrierOutcome==BossGimmickOutcome.Failed){if(prep)Ring(floor,target,95-55*u,.3f,4,c);if(travel)Ring(body,Vector2.Lerp(source,target,u),110+35*u,.35f,7,c);}
   }else{
    if(prep||travel)Ring(floor,target,prep?95-50*u:45,.3f,3,A(c,.8f));
    if(key=="contract_spears"){
     if(prep||travel||b.BossState==BossSkillState.Interval)for(int i=b.BossShotNumber-1;i<3;i++){var home=target+new Vector2((i-1)*85,245);Spear(i==b.BossShotNumber-1&&travel?Vector2.Lerp(home,target,u*u):home,c);}
    }else if(key=="contract_breath"){
     if(prep){Ring(body,chest,18+18*u,1,4,c);for(int i=0;i<8;i++)body.Diamond(chest+new Vector2(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4))*(60-34*u),3,5,ivory);}
     if(travel){var p=Vector2.Lerp(chest,target,u);for(int j=0;j<5;j++){var q=p+Vector2.up*j*15;Ring(body,q,(30+80*u)*(1-j*.1f),.3f,6-j,A(c,1-j*.17f));}for(int i=-3;i<=3;i++)body.Diamond(p+new Vector2(i*20,Mathf.Abs(i)*5),5,12,ivory);}
    }else if(key=="contract_debris"){
     if(prep||travel||b.BossState==BossSkillState.Interval)for(int i=b.BossShotNumber-1;i<3;i++){var home=target+new Vector2((i-1)*100,250);var p=i==b.BossShotNumber-1&&travel?Vector2.Lerp(home,target,u*u):home;body.Diamond(p,29,37,ink);body.Diamond(p+Vector2.up*3,24,29,new Color(.4f,.43f,.53f));body.Line(p+new Vector2(-12,22),p+new Vector2(8,-22),4,c);body.Line(p+new Vector2(-16,22),p+new Vector2(10,28),3,ivory);}
    }
   }
   float age=(float)(b.Time-b.BossImpactAt);if(b.BossSkillHasImpacted&&age>=0&&age<.5f){float t=age*2;Ring(floor,target,40+100*t,.35f,6*(1-t),A(c,1-t));for(int i=0;i<18;i++){float a=i*2.39996f;body.Diamond(target+new Vector2(Mathf.Cos(a)*(25+100*t),Mathf.Sin(a)*28+Mathf.Sin(t*Mathf.PI)*60),5*(1-t),9*(1-t),A(i%3==0?ivory:c,1-t));}}
   floor.End();body.End();
  }
 }
}
