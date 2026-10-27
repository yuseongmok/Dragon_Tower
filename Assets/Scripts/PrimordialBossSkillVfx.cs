using UnityEngine;
namespace DragonTower {
 // Three fixed meshes and one articulated legendary texture; all animation uses the replay model clock.
 public sealed partial class PrimordialBossSkillVfx {
  BattleView view;LightningMeshLayer rear,floor,front;PrimordialDragonApparition dragons;LegacyPrimordialBossSkillVfx legacy;
  readonly Color ink=new Color(.10f,.11f,.19f),stone=new Color(.38f,.43f,.56f),gold=new Color(.91f,.67f,.29f),ivory=new Color(1,.94f,.76f),cyan=new Color(.43f,.84f,.94f),purple=new Color(.61f,.38f,.83f);
  public int QuadCount=>(rear?.Count??0)+(floor?.Count??0)+(front?.Count??0)+(legacy?.QuadCount??0);
  public int DroppedQuads=>(rear?.Overflow??0)+(floor?.Overflow??0)+(front?.Overflow??0)+(legacy?.DroppedQuads??0);
  public void Initialize(BattleView v){view=v;legacy=new LegacyPrimordialBossSkillVfx();legacy.Initialize(v);rear=Layer("Contract grand architecture");floor=Layer("Contract landing marks");front=Layer("Contract attack ribbons");dragons=new GameObject("Legendary ancestral dragons",typeof(RectTransform),typeof(CanvasRenderer),typeof(PrimordialDragonApparition)).GetComponent<PrimordialDragonApparition>();dragons.Initialize(v.frame);}
  LightningMeshLayer Layer(string n){var m=new GameObject(n,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer)).GetComponent<LightningMeshLayer>();m.rectTransform.SetParent(view.frame,false);m.rectTransform.anchorMin=m.rectTransform.anchorMax=m.rectTransform.pivot=new Vector2(.5f,1);m.rectTransform.sizeDelta=new Vector2(480,850);m.raycastTarget=false;return m;}
  static void Empty(LightningMeshLayer m){if(m!=null){m.Begin();m.End();}}
  public void Clear(){legacy?.Clear();Empty(rear);Empty(floor);Empty(front);dragons?.Clear();}
  public void Destroy(){legacy?.Destroy();foreach(var m in new[]{rear,floor,front})if(m!=null)Object.Destroy(m.gameObject);if(dragons!=null)Object.Destroy(dragons.gameObject);}
  static Color A(Color c,float a){c.a=Mathf.Clamp01(a);return c;}
  static Vector2 Polar(float a,float r,float squash=1)=>new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*squash);
  void Ring(LightningMeshLayer m,Vector2 p,float r,float squash,float w,Color c,int n=64,float rotate=0){for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n+rotate,z=(i+1)*Mathf.PI*2/n+rotate;m.Line(p+Polar(a,r,squash),p+Polar(z,r,squash),w,c);}}
  void Depth(Transform t,Transform anchor,bool after){int n=anchor.GetSiblingIndex()+(after?1:0);if(t.GetSiblingIndex()<anchor.GetSiblingIndex())n--;t.SetSiblingIndex(n);}
  void Glyph(LightningMeshLayer m,Vector2 p,int i,float size,Color c){float a=i*.7f;m.Diamond(p,size,size,ink);Ring(m,p,size,1,2,c,8,a);m.Line(p+Polar(a,size*.7f),p-Polar(a,size*.7f),2,c);m.Line(p+Polar(a+1.2f,size*.7f),p-Polar(a+1.2f,size*.7f),2,c);if(i%2==0)m.Diamond(p,2,3,ivory);}
  void Circle(Vector2 p,float radius,float clock,Color c,float alpha,int glyphs=9){
   Ring(rear,p,radius,1,7,A(ink,alpha));Ring(rear,p,radius,1,2,A(c,alpha));Ring(rear,p,radius-12,1,2,A(c,alpha*.6f));
   for(int i=0;i<glyphs;i++){float a=i*Mathf.PI*2/glyphs+clock*.12f;Glyph(rear,p+Polar(a,radius),i,9,A(c,alpha));var q=p+Polar(a,radius-22);rear.Line(q,p+Polar(a+.55f,radius-22),1,A(c,alpha*.35f));}
   Ring(rear,p,radius*.65f,1,2,A(c,alpha*.5f),9,-clock*.09f);
  }
  void Telegraph(Vector2 p,float u,Color c,bool side){
   if(side){for(int sign=-1;sign<=1;sign+=2){floor.Line(p+new Vector2(sign*52,-20),p+new Vector2(sign*52,42),3,cyan);floor.Line(p+new Vector2(sign*52,42),p+new Vector2(sign*32,42),3,cyan);}Ring(floor,p,48,.32f,2,A(cyan,.7f));}
   else{Ring(floor,p,48,.3f,4,ink);Ring(floor,p,46,.3f,2,ivory);Ring(floor,p,48+80*(1-u),.3f,4,c);for(int i=-1;i<=1;i++)floor.Line(p+new Vector2(i*28,27),p+new Vector2(i*28,35),2,c);}
  }
  void Spear(Vector2 p,float scale,Color c){
   front.Line(p+Vector2.up*130*scale,p,13*scale,ink);front.Line(p+Vector2.up*125*scale,p,7*scale,c);front.Line(p+Vector2.up*115*scale,p,2*scale,ivory);
   front.Quad(p+new Vector2(-25,28)*scale,p+Vector2.up*60*scale,p+new Vector2(25,28)*scale,p+Vector2.down*28*scale,ink);
   front.Quad(p+new Vector2(-17,28)*scale,p+Vector2.up*48*scale,p+new Vector2(17,28)*scale,p+Vector2.down*20*scale,c);
   front.Quad(p+new Vector2(-17,28)*scale,p+Vector2.up*48*scale,p+Vector2.up*25*scale,p+Vector2.down*20*scale,new Color(.62f,.35f,.12f));
   front.Quad(p+Vector2.up*48*scale,p+new Vector2(17,28)*scale,p+Vector2.down*20*scale,p+Vector2.up*25*scale,new Color(1,.84f,.44f));
   front.Line(p+Vector2.up*46*scale,p+Vector2.down*18*scale,3*scale,ivory);
   front.Diamond(p+Vector2.up*29*scale,5*scale,10*scale,ink);
   front.Diamond(p+Vector2.up*29*scale,2*scale,6*scale,cyan);
   for(int k=0;k<5;k++){var q=p+Vector2.up*(65+k*11)*scale;front.Line(q+Vector2.left*6*scale,q+Vector2.right*6*scale,2*scale,ivory);}
   for(int sign=-1;sign<=1;sign+=2){front.Line(p+Vector2.up*55*scale,p+new Vector2(sign*37,78)*scale,5*scale,c);front.Line(p+new Vector2(sign*37,78)*scale,p+new Vector2(sign*25,42)*scale,3*scale,ivory);}
  }
  void Blade(Vector2 p,int side,float u){
   for(int i=0;i<22;i++){float x=-110+i*10;float y=x*x*.003f;var a=p+new Vector2(x,y);var z=p+new Vector2(x+10,(x+10)*(x+10)*.003f);front.Line(a+Vector2.up*10,z+Vector2.up*10,18,A(purple,.35f));front.Line(a,z,12,purple);front.Line(a+Vector2.down*3,z+Vector2.down*3,3,ivory);}
   for(int i=0;i<8;i++)front.Line(p+new Vector2(side*20+i*3,20+i*12),p+new Vector2(side*40+i*3,45+i*12),2,A(purple,(1-i*.1f)*.6f));
  }
  void Beam(Vector2 start,Vector2 target,float u,Color c,bool giant){
   var end=Vector2.Lerp(start,target,u);var delta=end-start;var normal=new Vector2(-delta.y,delta.x).normalized;float w=(giant?70:49)*(Mathf.Sin(Mathf.Clamp01(u)*Mathf.PI*.5f)*.5f+.5f);
   front.Quad(start-normal*14,start+normal*14,end+normal*w,end-normal*w,A(ink,.85f));
   front.Quad(start-normal*9,start+normal*9,end+normal*(w-5),end-normal*(w-5),A(c,.8f));
   front.Line(start,end,giant?17:11,ivory);
   for(int j=0;j<5;j++){float t=Mathf.Repeat(u*2-j*.16f,1);var q=Vector2.Lerp(start,end,t);Ring(front,q,18+t*w,.27f,3,A(c,.7f));}
   for(int side=-1;side<=1;side+=2)for(int i=0;i<7;i++){float t=i/7f;var p=Vector2.Lerp(start,end,t)+normal*side*(w+8+Mathf.Sin(t*31+u*16)*5);front.Line(p,p+delta.normalized*20,3,A(c,.6f));}
  }
  void Debris(Vector2 p,float scale,float rotation){
   Vector2 x=Polar(rotation,1),y=new Vector2(-x.y,x.x);float w=24*scale,h=45*scale;
   // A broken palace column: bevelled drums, a cornice, fluting and a fractured lower end.
   front.Quad(p-x*(w+8)-y*(h+7),p-x*(w+5)+y*(h+10),p+x*(w+11)+y*(h+4),p+x*(w+9)-y*(h-6),ink);
   for(int i=0;i<4;i++){float low=-h+i*h*.5f,high=low+h*.5f-3;float offset=(i%2==0?-2:2)*scale;
    front.Quad(p+x*(-w+offset)+y*low,p+x*(-w+offset)+y*high,p+x*(w+offset)+y*high,p+x*(w+offset)+y*low,stone);
    front.Quad(p+x*(-w+offset)+y*low,p+x*(-w+offset)+y*high,p+x*(-w+8*scale+offset)+y*high,p+x*(-w+8*scale+offset)+y*low,new Color(.67f,.67f,.66f));
    front.Quad(p+x*(w-7*scale+offset)+y*low,p+x*(w-7*scale+offset)+y*high,p+x*(w+offset)+y*high,p+x*(w+offset)+y*low,new Color(.23f,.26f,.37f));
    front.Line(p+x*(-w+offset)+y*high,p+x*(w+offset)+y*high,3,ivory);
    for(int j=-1;j<=1;j++)front.Line(p+x*j*9*scale+y*(low+3),p+x*j*9*scale+y*(high-3),2,ink);
   }
   var cap=p+y*(h+6*scale);front.Quad(cap-x*(w+10*scale)-y*5*scale,cap-x*(w+7*scale)+y*8*scale,cap+x*(w+9*scale)+y*8*scale,cap+x*(w+12*scale)-y*5*scale,new Color(.69f,.68f,.62f));
   front.Line(cap-x*(w+7*scale)+y*8*scale,cap+x*(w+9*scale)+y*8*scale,4,ivory);
   front.Line(p-x*5-y*h,p+x*9,3,gold);front.Line(p+x*9,p-x*8+y*h,3,gold);
   Glyph(front,p,4,12*scale,gold);
   for(int i=0;i<4;i++){var q=p+x*(i-1.5f)*16*scale-y*(h+12+i%2*9);front.Diamond(q,6*scale,10*scale,stone);front.Line(q,q+Vector2.up*6*scale,2,ivory);}
  }
  void Burst(Vector2 p,float t,Color c,bool huge){float r=huge?215:155;Ring(floor,p,25+r*t,.34f,7*(1-t),A(c,1-t));Ring(floor,p,15+r*t*.65f,.34f,3*(1-t),A(ivory,1-t));for(int i=0;i<32;i++){float a=i*2.39996f;var q=p+new Vector2(Mathf.Cos(a)*(20+r*t),Mathf.Sin(a)*30+Mathf.Sin(t*Mathf.PI)*(30+i%4*17));front.Diamond(q,(3+i%3)*(1-t),(7+i%5*2)*(1-t),A(i%3==0?ivory:c,1-t));}}
  public void Render(BattleModel b){
   if(b.PrimordialAnimated&&b.PrimordialCinematic){Clear();return;}
   if(!b.PrimordialSpectacle){legacy.Render(b);return;}rear.Begin();floor.Begin();front.Begin();dragons.Clear();
   Depth(rear.transform,view.enemyArt.transform,false);Depth(dragons.transform,view.enemyArt.transform,false);Depth(floor.transform,view.playerArt.transform,false);Depth(front.transform,view.playerArt.transform,true);
   var motion=view.GetComponent<BattleAnimation>();var target=motion.BossSkillPlayerGround;var source=motion.BossSkillEnemyGround;var center=source+Vector2.up*140;float clock=(float)b.Time;
   Color c=b.PrimordialPhase==1?gold:purple;
   if(b.PrimordialTransitioning){float t=b.PrimordialTransitionProgress;Circle(center,166,clock,gold,1-t);Circle(center,175,clock,purple,t);dragons.Show(center+new Vector2(0,-80),clock,Mathf.Sin(t*Mathf.PI)*.8f,true,.85f);Ring(front,target+Vector2.up*40,78,1.15f,5,A(gold,Mathf.Sin(t*Mathf.PI)));for(int i=0;i<9;i++){float a=i*Mathf.PI*2/9;var p=center+Polar(a,160+30*t);rear.Line(p,p+Polar(a+.3f,40*t),4,A(purple,t));}End();return;}
   if(!b.BossSkillBusy){End();return;}
   float u=b.BossPhaseProgress;bool prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel,interval=b.BossState==BossSkillState.Interval;var s=b.ActiveBossSkill;bool finale=s.oncePerEncounter;bool side=s.directionalShots>0&&b.BossDirection!=BossBladeDirection.Front;bool breath=s.attackVfx=="contract_breath"||finale&&b.BossShotNumber==4;
   float fade=b.BossState==BossSkillState.Recovery?1-u:1;
   if(s.attackVfx=="contract_continuous_cross"){RenderContinuousCross(b,center,target,clock);End();return;}
   if(s.attackVfx=="contract_rotating_cross"){RenderRotatingCross(b,center,target,clock);End();return;}
   if(s.attackVfx=="contract_fear"){RenderDragonFear(b,center,target,clock);End();return;}
   if(s.attackVfx=="contract_end"){RenderContractEnd(b,center,target,clock);End();return;}
   if(s.attackVfx=="contract_big_bang"){RenderBigBang(b,center,target,clock);End();return;}
   if(s.attackVfx=="contract_apocalypse"||s.attackVfx=="contract_apocalypse_collision"){RenderContractPortals(b,target,clock);End();return;}
   if(s.attackVfx=="contract_void_claw"){RenderVoidClaw(b,target,clock);End();return;}
   if(s.attackVfx=="contract_spear_echo"){RenderSpearEcho(b,target,clock);End();return;}
   if(s.attackVfx=="contract_spear_judgment"){RenderSpearJudgment(b,center,target,clock);End();return;}
   if(finale||breath)rear.Quad(new Vector2(-240,-130),new Vector2(240,-130),new Vector2(240,-650),new Vector2(-240,-650),A(ink,(finale?.38f:.20f)*fade));
   Circle(center,finale?198:165,clock,finale?gold:c,.7f*fade);
   if(finale){Circle(center,120,-clock,purple,.85f*fade);Ring(rear,center,169,1,6,A(gold,.75f*fade));dragons.Show(center+new Vector2(0,-60),clock,.85f*fade,true,.9f);for(int i=0;i<9;i++){float a=i*Mathf.PI*2/9;var p=center+Polar(a,180);rear.Line(p,center+Polar(a,140),3,A(ivory,.6f*fade));}}
   if(s.attackVfx=="contract_seal"){
    if(b.BossBarrierActive||prep&&b.BossBarrierOutcome==BossGimmickOutcome.None){
     float ratio=b.BossBarrierActive?b.EnemyShieldHP/(float)Mathf.Max(1,b.EnemyShieldMaxHP):1;
     for(int i=0;i<9;i++){float a=i*Mathf.PI*2/9-Mathf.PI*.5f;var p=center+Polar(a,145);bool lit=i<Mathf.CeilToInt(ratio*9);Glyph(rear,p,i,15,lit?ivory:stone);if(s.id=="void_chains"){for(int j=0;j<9;j++){var q=Vector2.Lerp(p,center,j/9f);Ring(rear,q,5,1.6f,2,lit?purple:ink,8,j);}}else rear.Line(p,center+Polar(a,110),3,lit?gold:ink);}
     if(b.PrimordialBarrierStrikeRemaining>=0&&b.PrimordialBarrierStrikeRemaining<1.3){float t=1-(float)b.PrimordialBarrierStrikeRemaining/1.3f;Telegraph(target,t,purple,false);if(b.PrimordialBarrierStrikeRemaining<.55)Spear(Vector2.Lerp(center,target,1-(float)b.PrimordialBarrierStrikeRemaining/.55f),.9f,purple);}
    }
    if(b.BossBarrierOutcome==BossGimmickOutcome.Broken){Burst(center,u,gold,true);if(b.PrimordialRescued){Ring(front,target+Vector2.up*48,72,1.15f,3,A(gold,1-u));dragons.Show(center+Vector2.down*65,clock,(1-u)*.55f,false,.85f);}}
    if(b.BossBarrierOutcome==BossGimmickOutcome.Failed){if(prep)Telegraph(target,u,c,false);if(travel)Beam(center,target,u,c,true);}
   }else{
    if(prep||travel){Telegraph(target,travel?1:u,side?purple:gold,side);if(side){float x=b.BossDirection==BossBladeDirection.Left?-145:145;floor.Line(target+new Vector2(x,170),target+new Vector2(x,-35),5,A(purple,.5f));}}
    if(breath){
     if(!finale)dragons.Show(center+new Vector2(0,-70),clock,.82f*fade,b.PrimordialPhase==2,.9f);
     var mouth=center+new Vector2(b.PrimordialPhase==2&&b.BossShotNumber==2?65:-65,-25);
     if(prep){Circle(mouth,28+20*u,clock*2,c,.95f);for(int i=0;i<14;i++)rear.Diamond(mouth+Polar(i*2.4f+clock,90*(1-u)+22),3,7,ivory);}
     if(travel){Beam(mouth,target,u,finale?gold:b.BossShotNumber==2?purple:cyan,finale);if(finale)Beam(center+new Vector2(65,-25),target,u,purple,true);}
    }else if(s.attackVfx=="contract_debris"){
     if(prep||travel||interval){bool huge=b.BossShotNumber==3;float scale=huge?2.8f:1.5f;var home=target+new Vector2(huge?0:b.BossShotNumber==1?-90:90,700);var p=travel?Vector2.Lerp(home,target,u*u):home+Vector2.up*Mathf.Sin(clock*2)*4;Debris(p,scale,huge?.1f:Mathf.Sin(clock)*.12f);for(int i=0;i<6;i++)front.Diamond(p+new Vector2((i%2==0?-1:1)*(40+i*8)*scale,70+i*13),3,8,A(purple,.6f));}
    }else if(prep||travel||interval){
     if(side){int sign=b.BossDirection==BossBladeDirection.Left?-1:1;var home=center+new Vector2(sign*115,-20);var end=target+new Vector2(sign*145,0);if(travel)Blade(Vector2.Lerp(home,end,u),sign,u);else{Ring(rear,home,44,1,3,purple);rear.Line(home,end,2,A(purple,.3f));}}
     else{for(int i=0;i<3;i++){var home=new Vector2((i-1)*150,100);if(i==Mathf.Max(0,b.BossShotNumber-1)%3)Spear(travel?Vector2.Lerp(home,target,u*u):home,1.38f,gold);}}
    }
   }
   float age=(float)(b.Time-b.BossImpactAt);if(b.BossSkillHasImpacted&&age>=0&&age<.65f){var hit=target;if(s.directionalShots>0&&b.BossLastImpactDirection!=BossBladeDirection.Front)hit.x+=b.BossLastImpactDirection==BossBladeDirection.Left?-145:145;Burst(hit,age/.65f,c,finale||s.attackVfx=="contract_debris"&&b.BossShotNumber==3);}
   if(finale&&b.BossShotNumber==4&&b.BossSkillHasImpacted&&age>=0&&age<1.15f){
    float t=age/1.15f,opacity=1-t;Ring(floor,target,90+140*t,.35f,9*opacity,A(gold,opacity));Ring(floor,target,60+155*t,.40f,5*opacity,A(purple,opacity));
    for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var start=target+Polar(a,45+100*t,.35f);var end=start+Vector2.up*(55+Mathf.Sin(a*3+1)*25)*(1-t);front.Line(start,end,5*opacity,A(i%2==0?gold:purple,opacity*.8f));front.Line(end,end+new Vector2(i%2==0?14:-14,28)*opacity,2,A(ivory,opacity));}
   }
   GrandDetail(b,center,target,c,clock,fade);
   End();
  }
  void End(){rear.End();floor.End();front.End();}
 }
}



