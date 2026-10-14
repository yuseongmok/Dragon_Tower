using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // Reuses player fire art, never casts a player skill or schedules a damage callback.
 public sealed class VulcanBossSkillVfx {
  BattleView view;LightningMeshLayer rear,front;FireSkillLibrary library;FireVfxStyle fire;
  readonly Image[] images=new Image[80];int used;public int PeakImages{get;private set;}
  readonly Sprite[] volcano=new Sprite[48];
  readonly Color dark=new Color(.18f,.055f,.075f),lava=new Color(1,.30f,.055f),gold=new Color(1,.76f,.23f);
  public int QuadCount=>(rear==null?0:rear.Count)+(front==null?0:front.Count);
  public int DroppedQuads=>(rear==null?0:rear.Overflow)+(front==null?0:front.Overflow);
  public void Initialize(BattleView owner){view=owner;library=Resources.Load<FireSkillLibrary>("FireSkills/Library");fire=library==null?null:library.style;var atlas=Resources.LoadAll<Sprite>("VFX/VolcanicMeteor/VolcanicAtlas");for(int row=0;row<6;row++)for(int f=0;f<8;f++)foreach(var s in atlas)if(s.name=="Volcanic"+row+"_"+f)volcano[row*8+f]=s;rear=Layer("Vulcan rear (pooled)");front=Layer("Vulcan front (pooled)");for(int i=0;i<images.Length;i++){var im=new GameObject("Vulcan fire fragment "+i,typeof(RectTransform),typeof(Image)).GetComponent<Image>();im.rectTransform.SetParent(front.transform,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;im.enabled=false;images[i]=im;}}
  Sprite Volcano(int row,float t)=>volcano[row*8+Mathf.FloorToInt(Mathf.Repeat(t*12,8))];
  LightningMeshLayer Layer(string name){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);var m=go.GetComponent<LightningMeshLayer>();m.raycastTarget=false;return m;}
  public void Clear(){if(rear!=null){rear.Begin();rear.End();}if(front!=null){front.Begin();front.End();}foreach(var im in images)if(im!=null)im.enabled=false;used=0;}
  public void Destroy(){foreach(var im in images)if(im!=null)Object.Destroy(im.gameObject);if(rear!=null)Object.Destroy(rear.gameObject);if(front!=null)Object.Destroy(front.gameObject);}
  static Color A(Color c,float alpha){c.a=alpha;return c;}
  void Draw(Sprite sprite,Vector2 p,Vector2 size,float alpha,float angle=0,bool back=false){if(sprite==null||used>=images.Length||alpha<=0)return;var im=images[used++];im.rectTransform.SetParent(back?rear.transform:front.transform,false);im.rectTransform.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round(p.y/2)*2);im.rectTransform.sizeDelta=size;im.rectTransform.localRotation=Quaternion.Euler(0,0,angle);im.sprite=sprite;im.color=A(Color.white,alpha);im.enabled=true;}
  void Flame(FireStyleModule kind,Vector2 p,Vector2 size,float t,float alpha=1,float angle=0,bool back=false){if(fire!=null)Draw(fire.Get(kind,t),p,size,alpha,angle,back);}
  void Ring(LightningMeshLayer m,Vector2 p,float radius,float tilt,float phase,Color color,float width=3){for(int i=0;i<40;i++){if(i%10==9)continue;float a=i*Mathf.PI/20+phase,z=a+.14f;m.Line(p+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*tilt),p+new Vector2(Mathf.Cos(z)*radius,Mathf.Sin(z)*radius*tilt),width,color);}}
  void Cracks(Vector2 p,float scale,float alpha){for(int i=0;i<6;i++){float a=i*1.0472f;var v=new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.36f);var bend=p+v*32*scale+new Vector2(7,-5);front.Line(p,bend,4,A(dark,alpha));front.Line(bend,p+v*65*scale,3,A(lava,alpha));}}
  void Burst(Vector2 p,float age,float scale){float t=Mathf.Clamp01(age/.65f),fade=1-t;Flame(FireStyleModule.Burst,p+Vector2.up*28*scale,new Vector2(180,140)*scale,age,fade);Flame(FireStyleModule.Shockwave,p,new Vector2(170+170*t,50+25*t)*scale,age,fade);Cracks(p,scale,fade);
   for(int i=0;i<16;i++){float a=i*2.39996f;var q=p+new Vector2(Mathf.Cos(a)*(20+90*t),Mathf.Sin(a)*18*t+Mathf.Sin(t*Mathf.PI)*(28+i%4*12))*scale;front.Diamond(q,(3+i%3)*scale,(3+i%2)*scale,A(i%3==0?gold:dark,fade));front.Diamond(q+Vector2.up*2,2,2,A(lava,fade));}}
  void Depth(LightningMeshLayer m,Transform actor,bool after){int index=actor.GetSiblingIndex()+(after?1:0);if(m.transform.GetSiblingIndex()<actor.GetSiblingIndex())index--;m.transform.SetSiblingIndex(index);}
  public void Render(BattleModel b){rear.Begin();front.Begin();Depth(rear,view.enemyArt.transform,false);Depth(front,view.playerArt.transform,true);
   var motion=view.GetComponent<BattleAnimation>();var enemy=motion.BossSkillEnemyGround;var target=motion.BossSkillPlayerGround;float t=b.BossPhaseProgress;
   if(b.ActiveBossSkill.kind==BossAttackKind.Gimmick)Barrier(b,enemy,target,t);else Meteor(b,enemy,target,t);
   rear.End();front.End();PeakImages=Mathf.Max(PeakImages,used);
  }
  void Barrier(BattleModel b,Vector2 p,Vector2 target,float t){
   p+=Vector2.up*30;
   if(b.BossPreparation||b.BossBarrierActive){float power=b.BossPreparation?.25f+.65f*t:1;float time=b.BossPreparation?t:(float)(b.ActiveBossSkill.barrierWindow-b.BossPhaseRemaining);
    Ring(rear,p,120,.28f,time*.3f,A(lava,power),5);Ring(front,p,114,.28f,-time*.2f,A(gold,power*.7f));
    // Side walls and crown leave the boss silhouette visible through the center.
    for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++){var q=p+new Vector2(side*(114-i*10),22+i*43);Flame(FireStyleModule.Body,q,new Vector2(62,94)*power,time+i*.21f,power*.85f,side*-10,i%2==0);Flame(FireStyleModule.Ember,q+new Vector2(-side*10,27),new Vector2(12,23),time+i*.13f,power);}
    for(int i=-2;i<=2;i++)Flame(FireStyleModule.Tongue,p+new Vector2(i*43,228-Mathf.Abs(i)*9),new Vector2(65,62)*power,time+i*.3f,power*.68f);
    for(int i=0;i<24;i++){float a=i*Mathf.PI/24,z=(i+.7f)*Mathf.PI/24;var center=p+Vector2.up*105;front.Line(center+new Vector2(Mathf.Cos(a)*143,Mathf.Sin(a)*143),center+new Vector2(Mathf.Cos(z)*143,Mathf.Sin(z)*143),3,A(lava,power*.65f));}
    if(b.BossBarrierActive){float damage=1-b.EnemyShieldHP/(float)b.EnemyShieldMaxHP;for(int i=0;i<5;i++)if(damage>i*.17f){var q=p+new Vector2((i%2==0?-1:1)*(85+i*4),30+i*34);front.Line(q,q+new Vector2(-13,18),4,gold);front.Line(q+new Vector2(-13,18),q+new Vector2(7,34),3,lava);}}
   }else if(b.BossState==BossSkillState.Recovery){
    float age=t*(b.BossBarrierOutcome==BossGimmickOutcome.Broken?b.ActiveBossSkill.breakRecovery:b.ActiveBossSkill.recovery);
    if(b.BossBarrierOutcome==BossGimmickOutcome.Broken&&age<.65f){Burst(p+Vector2.up*65,age,.8f);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var q=p+Vector2.up*95+new Vector2(Mathf.Cos(a)*(90+age*50),Mathf.Sin(a)*(85+age*20));Flame(FireStyleModule.Ember,q,new Vector2(22,46)*(1-age/.65f),age+i*.1f,1-age/.65f,i*45);}Cracks(p,1.6f,1-age/.65f);}
    if(b.BossBarrierOutcome==BossGimmickOutcome.Failed&&age<.8f){float travel=Mathf.Clamp01(age/.23f);var wave=Vector2.Lerp(p,target,travel);Flame(FireStyleModule.Shockwave,wave,new Vector2(260+60*travel,100),age,1-age/.8f);Burst(target,age,1.1f);}
   }
  }
  void Meteor(BattleModel b,Vector2 source,Vector2 target,float t){
   if(b.BossPreparation){bool early=b.BossState==BossSkillState.Warning;float charge=early?t*.3f:.3f+.7f*t;float r=early?78:Mathf.Lerp(90,43,Mathf.Clamp01((t-.65f)/.35f));Ring(rear,target,r,.3f,t*2, A(dark,.9f),8);Ring(rear,target,r,.3f,t*2,A(lava,.4f+.6f*charge),4);Ring(rear,target,r-12,.3f,-t*3,A(gold,charge*.8f),2);
    Flame(FireStyleModule.Body,source+new Vector2(-55,100),new Vector2(44,72),t,.75f);Flame(FireStyleModule.Body,source+new Vector2(55,100),new Vector2(44,72),t+.2f,.75f);
    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var q=target+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*.3f);rear.Diamond(q,3,4,A(gold,charge));}
   }
   if(b.BossState==BossSkillState.Travel){float fall=t*t;var start=new Vector2(165,-150);var pos=Vector2.Lerp(start,target,fall);var direction=(start-target).normalized;
    Ring(rear,target,43+12*t,.3f,0,A(lava,.9f),4);
    for(int i=5;i>=0;i--)Flame(FireStyleModule.Tongue,pos+direction*(18+i*18),new Vector2(58-i*5,94-i*7),t+i*.09f,.7f-i*.075f,-20);
    Draw(Volcano(3,t),pos+direction*40,new Vector2(74,130),.9f,-20);
    Draw(Volcano(0,t)??(library==null?null:library.meteor),pos,new Vector2(95,95),1,t*65);
    Draw(Volcano(1,t),pos,new Vector2(95,95),.65f,t*65);
   }
   if(b.BossState==BossSkillState.Recovery&&b.BossSkillHasImpacted)Burst(target,t*b.ActiveBossSkill.recovery,1.1f);
  }
 }
}
