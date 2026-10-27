using UnityEngine;
namespace DragonTower {
 // Pixel-snapped geometry, shared ice palette, fixed two-mesh pool. No gameplay RNG.
 public sealed class FrostBossSkillVfx {
  BattleView view;LightningMeshLayer floor,air;IceVfxStyle style;
  Color Dark=>style!=null?style.deepNavy:new Color32(20,38,77,255);
  Color Blue=>style!=null?style.iceBlue:new Color32(57,123,204,255);
  Color Cyan=>style!=null?style.cyan:new Color32(89,205,238,255);
  Color Pale=>style!=null?style.pale:new Color32(183,231,250,255);
  public int QuadCount=>(floor==null?0:floor.Count)+(air==null?0:air.Count);
  public int DroppedQuads=>(floor==null?0:floor.Overflow)+(air==null?0:air.Overflow);
  public void Initialize(BattleView v){view=v;style=Resources.Load<IceSkillLibrary>("IceSkills/Library")?.style;floor=Layer("Frost boss floor (pooled)");air=Layer("Frost boss crystals (pooled)");}
  LightningMeshLayer Layer(string n){var g=new GameObject(n,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=g.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);var m=g.GetComponent<LightningMeshLayer>();m.raycastTarget=false;return m;}
  public void Clear(){if(floor!=null){floor.Begin();floor.End();air.Begin();air.End();}}
  public void Destroy(){if(floor!=null)Object.Destroy(floor.gameObject);if(air!=null)Object.Destroy(air.gameObject);}
  Color A(Color c,float a){c.a=a;return c;}
  void Depth(LightningMeshLayer m,Transform actor,bool after){int i=actor.GetSiblingIndex()+(after?1:0);if(m.transform.GetSiblingIndex()<actor.GetSiblingIndex())i--;m.transform.SetSiblingIndex(i);}
  void Ring(LightningMeshLayer m,Vector2 p,float rx,float ry,float width,Color c){for(int i=0;i<32;i++){float a=i*Mathf.PI/16,z=(i+1)*Mathf.PI/16;m.Line(p+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),p+new Vector2(Mathf.Cos(z)*rx,Mathf.Sin(z)*ry),width,c);}}
  void Crystal(Vector2 p,float w,float h,float alpha,float crack=0){
   var l=p+new Vector2(-w,h*.6f);var r=p+new Vector2(w,h*.6f);var tip=p+Vector2.up*h;var bl=p+new Vector2(-w*.75f,0);var br=p+new Vector2(w*.75f,0);
   air.Quad(bl,l,tip,p,A(Blue,alpha));air.Quad(p,tip,r,br,A(Cyan,alpha));air.Line(bl,l,3,A(Dark,alpha));air.Line(l,tip,3,A(Pale,alpha));air.Line(tip,r,3,A(Pale,alpha));air.Line(r,br,3,A(Dark,alpha));air.Line(tip,p+Vector2.up*h*.12f,2,A(Pale,alpha*.7f));air.Line(bl,br,3,A(Dark,alpha));
   if(crack>0){var c=p+Vector2.up*h*.4f;air.Line(c+new Vector2(-w*.8f,h*.16f),c,2,A(Pale,alpha*crack));air.Line(c,c+new Vector2(w*.8f,-h*.14f),3,A(Dark,alpha*crack));air.Line(c,c+new Vector2(-w*.25f,-h*.23f),2,A(Pale,alpha*crack));}
  }
  void Shards(Vector2 p,float age,float radius){float t=age/.45f;for(int i=0;i<16;i++){float a=i*2.39996f;var q=p+new Vector2(Mathf.Cos(a)*(35+radius*t),Mathf.Sin(a)*(12+radius*.32f*t)+45*Mathf.Sin(t*Mathf.PI));air.Diamond(q,3+3*(1-t),6+5*(1-t),A(i%2==0?Cyan:Pale,1-t));}Ring(floor,p,35+radius*t,10+radius*t*.25f,3,A(Cyan,1-t));}
  public void Render(BattleModel b){
   floor.Begin();air.Begin();Depth(floor,view.enemyArt.transform,false);Depth(air,view.playerArt.transform,true);var anim=view.GetComponent<BattleAnimation>();var source=anim.BossSkillEnemyGround;var target=anim.BossSkillPlayerGround;float time=(float)b.Time;
   if(view.enemyStatus!=null)view.enemyStatus.transform.SetAsLastSibling();
   for(int i=0;i<b.FrostWeaknessStacks;i++){var p=source+new Vector2(113+i*16,65);air.Line(p+new Vector2(-4,8),p,3,Pale);air.Line(p,p+new Vector2(5,-8),3,Cyan);}
   if(b.BossSkillBusy){var s=b.ActiveBossSkill;float t=b.BossPhaseProgress;float age=(float)(b.Time-b.BossImpactAt);bool barrier=s.attackVfx=="frost_recovery_barrier";
    if(barrier){
     if(b.BossPreparation||b.BossBarrierActive){float strength=b.BossPreparation?t:1;float damage=b.BossBarrierActive?1-b.EnemyShieldHP/(float)b.EnemyShieldMaxHP:0;
      Ring(floor,source,116,28,4,A(Cyan,strength));Ring(air,source+Vector2.up*85,120,105,4,A(Blue,strength*.55f));
      for(int i=0;i<6;i++){float x=(i-2.5f)*41;Crystal(source+new Vector2(x,8),18, (72+Mathf.Abs(x)*.55f)*strength,.65f,damage);}
      // Gathering healing particles, not an opaque screen-sized shield.
      for(int i=0;i<10;i++){float a=i*2.39996f+time;var p=source+Vector2.up*90+new Vector2(Mathf.Cos(a)*(70-35*t),Mathf.Sin(a)*45);air.Diamond(p,2,4,A(Pale,.45f+.35f*t));}
     }
     if(b.BossState==BossSkillState.Recovery&&t<.6f){if(b.BossBarrierOutcome==BossGimmickOutcome.Broken)Shards(source+Vector2.up*45,t*.7f,130);else{for(int i=0;i<12;i++){var p=source+new Vector2(Mathf.Sin(i*7)*80,25+Mathf.Repeat(i*14+t*180,150));air.Line(p+Vector2.left*5,p+Vector2.right*5,3,A(Pale,1-t));air.Line(p+Vector2.down*5,p+Vector2.up*5,3,A(Cyan,1-t));}}}
    }else if(s.attackVfx=="frost_resonance"){
     if(b.BossPreparation||b.BossState==BossSkillState.Travel){float growth=b.BossState==BossSkillState.Warning?t:1;float shake=b.BossState==BossSkillState.Warning?0:b.BossState==BossSkillState.Travel?5:1+3*t;for(int i=0;i<4;i++){float side=i<2?-1:1;float x=side*(76+(i%2)*28);float y=(i%2)*28;float jitter=Mathf.Sin(time*(12+shake*7)+i)*shake;Crystal(target+new Vector2(x+jitter,y),14, (52+(i%2)*22)*growth,1,Mathf.Min(1,shake/3));}Ring(floor,target,64,18,2,A(Cyan,.4f+shake*.1f));}
     if(b.BossSkillHasImpacted&&age>=0&&age<.45f)Shards(target,age,125);
    }else if(s.attackVfx=="frost_icicle"){
     var top=new Vector2(target.x,-235);
     if(b.BossPreparation){float shrink=b.BossState==BossSkillState.Warning?1:1-t;Ring(floor,target,63,18,3,Blue);Ring(floor,target,63+30*shrink,18+9*shrink,2,Pale);Crystal(top+Vector2.up*80,22,-80,.7f+.3f*t);}
     if(b.BossState==BossSkillState.Travel){var tip=Vector2.Lerp(top,target,t*t);for(int i=1;i<=3;i++)Crystal(tip+Vector2.up*(90+i*15),18, -90,.12f);Crystal(tip+Vector2.up*100,25,-100,1);Ring(floor,target,63,18,3,Cyan);}
     if(b.BossSkillHasImpacted&&age>=0&&age<.45f)Shards(target,age,145);
    }
   }
   float returned=(float)(b.Time-b.FrostCrystalReflectedAt);if(returned>=0&&returned<.55f){for(int i=0;i<5;i++){float t=Mathf.Clamp01(returned/.55f+i*.025f);var p=Vector2.Lerp(target+new Vector2((i-2)*25,0),source+Vector2.up*65,t);air.Diamond(p,3,8,A(Pale,1-t*.5f));}}
   // Preserve the existing status label's position, with contrast against bright ice.
   if(b.BossBarrierActive&&view.enemyStatus!=null){var p=view.enemyStatus.rectTransform.anchoredPosition;air.Quad(p+new Vector2(-154,-13),p+new Vector2(-154,13),p+new Vector2(154,13),p+new Vector2(154,-13),A(Dark,.92f));}
   floor.End();air.End();
  }
 }
}
