using UnityEngine;
namespace DragonTower {
 // Presentation only. Two pooled meshes, deterministic model-clock animation, no hit callbacks.
 public sealed class VoidBossSkillVfx {
   static readonly Vector2[] shape={new Vector2(-.92f,-.22f),new Vector2(-.78f,.48f),new Vector2(-.25f,.91f),new Vector2(.52f,.73f),new Vector2(.95f,.12f),new Vector2(.63f,-.68f),new Vector2(-.25f,-.82f)};
  BattleView view;LightningMeshLayer floor,body;
  readonly Color ink=new Color(.09f,.07f,.16f),stone=new Color(.24f,.24f,.34f),face=new Color(.40f,.39f,.53f),violet=new Color(.55f,.34f,.78f),pale=new Color(.83f,.76f,.97f);
  public int QuadCount=>(floor==null?0:floor.Count)+(body==null?0:body.Count);
  public int DroppedQuads=>(floor==null?0:floor.Overflow)+(body==null?0:body.Overflow);
  public void Initialize(BattleView owner){view=owner;floor=Layer("Void boundary floor (pooled)");body=Layer("Void boundary fragments (pooled)");}
  LightningMeshLayer Layer(string name){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);var m=go.GetComponent<LightningMeshLayer>();m.raycastTarget=false;return m;}
  public void Clear(){if(floor!=null){floor.Begin();floor.End();}if(body!=null){body.Begin();body.End();}}
  public void Destroy(){if(floor!=null)Object.Destroy(floor.gameObject);if(body!=null)Object.Destroy(body.gameObject);}
  static Color A(Color c,float a){c.a=a;return c;}
  void Depth(LightningMeshLayer m,bool front){int n=view.playerArt.transform.GetSiblingIndex()+(front?1:0);if(m.transform.GetSiblingIndex()<view.playerArt.transform.GetSiblingIndex())n--;m.transform.SetSiblingIndex(n);}
  void Rift(Vector2 p,float length,float width,float alpha){for(int j=0;j<12;j++){float u=j/12f,v=(j+1)/12f;var a=p+new Vector2((u-.5f)*length,(u-.5f)*length*.28f+(j%2)*4);var z=p+new Vector2((v-.5f)*length,(v-.5f)*length*.28f+((j+1)%2)*4);floor.Line(a,z,width+6,A(ink,alpha));floor.Line(a,z,width+2,A(violet,alpha));floor.Line(a,z,Mathf.Max(1,width*.35f),A(pale,alpha));}}
  void Rock(Vector2 p,float size,float crack){
   for(int i=0;i<7;i++){var a=p+shape[i]*size;var z=p+shape[(i+1)%7]*size;body.Quad(p,a,z,p,i<3?face:stone);body.Line(a,z,4,ink);}
   body.Line(p+new Vector2(-.7f,.42f)*size,p+new Vector2(-.23f,.76f)*size,3,pale);
   body.Quad(p+new Vector2(-.62f,-.12f)*size,p+new Vector2(-.2f,.32f)*size,p+new Vector2(.27f,.15f)*size,p+new Vector2(.05f,-.48f)*size,stone);
   if(crack>0){var a=p+new Vector2(-.2f,.85f)*size;var q=p+new Vector2(.08f,.12f)*size;var z=p+new Vector2(-.18f,-.7f)*size;body.Line(a,q,3+crack*3,ink);body.Line(q,z,3+crack*3,ink);body.Line(a,q,1+crack*2,violet);body.Line(q,z,1+crack*2,pale);body.Line(q,p+new Vector2(.8f,.12f)*size,2,violet);}
  }
  void Ring(LightningMeshLayer mesh,Vector2 p,float radius,float squash,float width,Color color,float rotation=0){
   for(int i=0;i<24;i++){float a=i*Mathf.PI/12+rotation,z=(i+1)*Mathf.PI/12+rotation;mesh.Line(p+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*squash),p+new Vector2(Mathf.Cos(z)*radius,Mathf.Sin(z)*radius*squash),width,color);}
  }
  void Core(Vector2 p,float size,float pulse){body.Diamond(p,size+5,size*1.3f+5,ink);body.Diamond(p,size,size*1.3f,violet);body.Diamond(p+new Vector2(-size*.2f,size*.2f),size*.55f,size*.75f,pale);body.Diamond(p,size*.2f,size*.35f,A(Color.white,pulse));}
  void Orb(Vector2 p,float size){
   for(int i=0;i<16;i++){float a=i*Mathf.PI/8,z=(i+1)*Mathf.PI/8;var u=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size;var v=p+new Vector2(Mathf.Cos(z),Mathf.Sin(z))*size;body.Quad(p,u,v,p,ink);body.Line(u,v,4,i<8?violet:pale);}
   Ring(body,p,size*.65f,1,3,violet);body.Diamond(p+new Vector2(-size*.24f,size*.3f),size*.2f,size*.24f,pale);
  }
  void Collapsed(BattleModel b,Vector2 source){
   var center=source+Vector2.up*195;
   float pulse=.6f+.15f*Mathf.Sin((float)b.Time*3);
   for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var p=center+new Vector2(Mathf.Cos(a)*85,Mathf.Sin(a)*85);body.Line(p,p+new Vector2(i%2==0?12:-10,16),6,ink);body.Line(p,p+new Vector2(i%2==0?12:-10,16),2,A(violet,pulse));}
   for(int i=0;i<4;i++)Rift(new Vector2(i%2==0?-203:203,-280-i*85),44,2,pulse);
  }
  public void Render(BattleModel b){floor.Begin();body.Begin();Depth(floor,false);Depth(body,true);
   var motion=view.GetComponent<BattleAnimation>();var target=motion.BossSkillPlayerGround;var source=motion.BossSkillEnemyGround;
   if(b.VoidBoundaryCollapsed)Collapsed(b,source);
   if(!b.BossSkillBusy){floor.End();body.End();return;}float t=b.BossPhaseProgress;var s=b.ActiveBossSkill;bool cut=s.attackVfx=="void_cut";int shot=b.BossShotNumber;
   bool interval=b.BossState==BossSkillState.Interval,travel=b.BossState==BossSkillState.Travel,prep=b.BossPreparation;
   if(cut){
    if(prep){Rift(target,195,1+3*t,.4f+.5f*t);for(int i=0;i<7;i++){var p=source+new Vector2(-66+i*22,55+(i%2)*6);body.Diamond(p,4+6*t,2+3*t,A(violet,.5f+.4f*t));}}
    if(interval||prep&&shot>1)Rift(target,220,3+Mathf.Sin((float)b.Time*14),.95f);
    // Convex leading edge faces down; its center reaches the unchanged impact anchor at t=1.
    if(travel){var p=Vector2.Lerp(source+Vector2.up*45,target,t);float w=90+40*t;for(int i=0;i<18;i++){float u=i/18f,v=(i+1)/18f;var a=p+new Vector2((u-.5f)*w*2,(1-Mathf.Sin(u*Mathf.PI))*38);var z=p+new Vector2((v-.5f)*w*2,(1-Mathf.Sin(v*Mathf.PI))*38);float aw=2+17*Mathf.Sin(u*Mathf.PI),zw=2+17*Mathf.Sin(v*Mathf.PI);body.Quad(a+Vector2.up*aw,a-Vector2.up*3,z-Vector2.up*3,z+Vector2.up*zw,ink);body.Quad(a+Vector2.up*aw*.65f,a,z,z+Vector2.up*zw*.65f,violet);body.Line(a,z,3,pale);}Rift(target,220,2,.7f);}
   }else if(s.attackVfx=="void_sealing"){
    var center=source+Vector2.up*175;
    if(b.BossBarrierActive||prep&&b.BossBarrierOutcome==BossGimmickOutcome.None){
     float f=b.BossBarrierActive?(float)b.EnemyShieldHP/Mathf.Max(1,b.EnemyShieldMaxHP):1;
     Ring(body,center,83,1.05f,7,ink);Ring(body,center,79,1.05f,3,A(violet,.9f));
     for(int i=0;i<6;i++){float a=i*Mathf.PI/3+(float)b.Time*.3f;var p=center+new Vector2(Mathf.Cos(a)*75,Mathf.Sin(a)*79);body.Diamond(p,7,12,pale);body.Line(p,center,2,A(violet,.45f));}
     Core(center,22,.65f);
     for(int i=0;i<(1-f)*8;i++)body.Line(center+new Vector2(-65+i*18,40-i%3*30),center+new Vector2(-54+i*18,14-i%3*30),5,ink);
    }
    if(b.BossBarrierOutcome==BossGimmickOutcome.Broken){float u=t;for(int i=0;i<12;i++){float a=i*Mathf.PI/6;body.Diamond(center+new Vector2(Mathf.Cos(a)*(36+u*85),Mathf.Sin(a)*(36+u*85)),7*(1-u),12*(1-u),A(pale,1-u));}}
    if(b.BossBarrierOutcome==BossGimmickOutcome.Failed&&prep){Core(center,25+9*t,.9f);Ring(floor,target,Mathf.Lerp(95,38,t),.3f,3,pale);}
    if(b.BossBarrierOutcome==BossGimmickOutcome.Failed&&travel){var p=Vector2.Lerp(source,target,t);Ring(body,p,105+35*t,.35f,10,ink);Ring(body,p,103+35*t,.35f,6,violet);Ring(body,p,103+35*t,.35f,2,pale);}
   }else if(s.attackVfx=="void_tracker"){
    // Fixed-lane combat: the orb tracks the landing anchor, then visibly locks before impact.
    if(prep){float a=(float)b.Time*2.8f;var p=target+new Vector2(Mathf.Sin(a)*45,120+Mathf.Cos(a)*12);Orb(p,19+4*t);Ring(floor,target,65,.3f,2,A(violet,.6f));}
    if(travel){float locked=(float)b.Time-t*s.damageTime;var start=target+new Vector2(Mathf.Sin(locked*2.8f)*45,120+Mathf.Cos(locked*2.8f)*12);var p=Vector2.Lerp(start,target,t);Orb(p,23+5*t);Ring(floor,target,38,.3f,4,pale);floor.Line(target+new Vector2(-48,0),target+new Vector2(48,0),2,violet);floor.Line(target+new Vector2(0,-18),target+new Vector2(0,18),2,violet);for(int i=1;i<5;i++)body.Diamond(p+Vector2.up*i*14,5-i,8-i,A(violet,1-i*.18f));}
   }else if(s.attackVfx=="void_collapse"){
    // The count of intact rocks shows exactly how many impacts remain in this cast.
    if(prep||interval||travel)for(int i=shot-1;i<3;i++){bool active=i==shot-1;var home=target+new Vector2((i-1)*100,235+(i%2)*28);float crack=active?(interval?.15f:prep?t:1):0;var p=active&&travel?Vector2.Lerp(home,target,t*t):home;if(active&&prep)p.x+=Mathf.Round(Mathf.Sin((float)b.Time*47)*5*t/2)*2;Rock(p,active?30:23,crack);if(active&&travel)for(int j=1;j<6;j++)body.Diamond(Vector2.Lerp(p,home,j*.09f),6-j,11-j,A(violet,1-j*.15f));}
    if(prep||travel){float radius=prep?Mathf.Lerp(90,38,t):38;for(int i=0;i<24;i++){float a=i*Mathf.PI/12,z=(i+1)*Mathf.PI/12;floor.Line(target+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.3f),target+new Vector2(Mathf.Cos(z)*radius,Mathf.Sin(z)*radius*.3f),3,pale);}}
   }
   float age=(float)(b.Time-b.BossImpactAt);if(b.BossSkillHasImpacted&&age>=0&&age<.5f){float u=age/.5f;Rift(target,210+110*u,10*(1-u),1-u);for(int i=0;i<16;i++){float a=i*2.39996f;var p=target+new Vector2(Mathf.Cos(a)*(28+100*u),Mathf.Sin(a)*(15+25*u)+Mathf.Sin(u*Mathf.PI)*45);body.Diamond(p,(5+i%3)*(1-u),(7+i%4)*(1-u),A(i%3==0?pale:violet,1-u));}}
   floor.End();body.End();
  }
 }
}
