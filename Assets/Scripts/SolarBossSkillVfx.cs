using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // Fixed meshes: the mirror remains behind the boss, attack lanes below the HUD.
 public sealed class SolarBossSkillVfx {
  BattleView view;LightningMeshLayer back,floor,air;Text clock;
  static readonly Color Gold=new Color32(222,183,96,255),Ivory=new Color32(255,238,192,255),Ink=new Color32(31,28,53,255),Violet=new Color32(125,111,168,255);
  public int QuadCount=>(back==null?0:back.Count)+(floor==null?0:floor.Count)+(air==null?0:air.Count);
  public int DroppedQuads=>(back==null?0:back.Overflow)+(floor==null?0:floor.Overflow)+(air==null?0:air.Overflow);
  LightningMeshLayer Layer(string name){var g=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=g.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);var m=g.GetComponent<LightningMeshLayer>();m.raycastTarget=false;return m;}
  public void Initialize(BattleView v){view=v;back=Layer("Uriel sun mirror (pooled)");floor=Layer("Uriel noon midnight lanes (pooled)");air=Layer("Uriel reflected light (pooled)");clock=new GameObject("Noon midnight clock",typeof(RectTransform),typeof(Text),typeof(Outline)).GetComponent<Text>();var r=clock.rectTransform;r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-124);r.sizeDelta=new Vector2(270,30);clock.font=view.font;clock.fontSize=15;clock.alignment=TextAnchor.MiddleCenter;clock.raycastTarget=false;clock.GetComponent<Outline>().effectColor=Ink;Clear();}
  public void Clear(){if(back==null)return;back.Begin();floor.Begin();air.Begin();back.End();floor.End();air.End();clock.enabled=false;}
  public void Destroy(){if(back==null)return;Object.Destroy(back.gameObject);Object.Destroy(floor.gameObject);Object.Destroy(air.gameObject);Object.Destroy(clock.gameObject);}
  Color A(Color c,float a){c.a=a;return c;}
  void Depth(LightningMeshLayer m,Transform actor,bool after){int i=actor.GetSiblingIndex()+(after?1:0);if(m.transform.GetSiblingIndex()<actor.GetSiblingIndex())i--;m.transform.SetSiblingIndex(i);}
  Vector2 Polar(float a,float x,float y)=>new Vector2(Mathf.Cos(a)*x,Mathf.Sin(a)*y);
  void Arc(LightningMeshLayer m,Vector2 p,float rx,float ry,float width,Color c,float start=0,float portion=1){int n=48;for(int i=0;i<n*portion;i++){float a=start+i*Mathf.PI*2/n,z=start+(i+1)*Mathf.PI*2/n;m.Line(p+Polar(a,rx,ry),p+Polar(z,rx,ry),width,c);}}
  void Disc(LightningMeshLayer m,Vector2 p,float r,Color c){for(int i=0;i<48;i++){float a=i*Mathf.PI/24,z=(i+1)*Mathf.PI/24;m.Quad(p,p+Polar(a,r,r),p+Polar(z,r,r),p,c);}}
  void Sun(LightningMeshLayer m,Vector2 p,float radius,Color c,bool moon=false){Arc(m,p,radius,radius,2,c);if(moon){Arc(m,p+Vector2.right*(radius*.35f),radius*.7f,radius*.7f,3,c,Mathf.PI*.5f,.5f);}else{m.Diamond(p,radius*.42f,radius*.42f,c);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;m.Line(p+Polar(a,radius*.6f,radius*.6f),p+Polar(a,radius*.85f,radius*.85f),2,c);}}}
  void Ray(Vector2 start,Vector2 end,float width,Color c,float alpha){air.Line(start,end,width+4,A(Ink,alpha*.6f));air.Line(start,end,width,A(c,alpha));air.Line(start,end,2,A(Ivory,alpha));}
  public void Render(BattleModel b){back.Begin();floor.Begin();air.Begin();Depth(back,view.enemyArt.transform,false);Depth(floor,view.playerArt.transform,false);Depth(air,view.playerArt.transform,true);clock.enabled=true;clock.transform.SetAsLastSibling();
   var anim=view.GetComponent<BattleAnimation>();Vector2 origin=anim.BossSkillEnemyGround+Vector2.up*150,target=anim.BossSkillPlayerGround;float time=(float)b.Time;
   clock.color=b.SolarMidnight?new Color(.8f,.77f,.96f):Ivory;clock.text=(b.SolarMidnight?"자정":"정오")+"  "+b.SolarPhaseRemaining.ToString("0.0")+"초"+(b.SolarTransitionPending?(b.SolarMidnight?"  → 정오":"  → 자정"):"");
   air.Quad(new Vector2(-135,-126),new Vector2(135,-126),new Vector2(135,-153),new Vector2(-135,-153),A(Ink,.78f));
   var dial=new Vector2(-158,-139);Sun(air,dial,15,b.SolarMidnight?Violet:Gold,b.SolarMidnight);Arc(air,dial,20,20,2,b.SolarMidnight?Ivory:Gold,Mathf.PI/2,(float)(b.SolarPhaseRemaining/b.SolarPhaseDuration));
   if(b.SolarMidnight&&!b.SolarMirrorActive){Disc(back,origin,122,A(Ink,.75f));Arc(back,origin,124,124,3,A(Violet,.8f));}
   if(b.SolarMidnight||b.SolarTransitionPending){float a=b.SolarMidnight?.38f:.38f*(1-(float)b.SolarPhaseRemaining/3);for(int i=0;i<12;i++){float angle=i*Mathf.PI/6;back.Line(origin+Polar(angle,72,72),origin+Polar(angle,95,95),3,A(Violet,a));}Arc(back,origin,82,82,5,A(Violet,a));}
   float transition=(float)(b.Time-b.SolarTransitionAt);if(b.SolarTransitionPending||transition>=0&&transition<1){float t=b.SolarTransitionPending?1-(float)b.SolarPhaseRemaining/3:1-transition;Sun(back,origin,88,A(Gold,.6f));float offset=b.SolarMidnight?88*(1-t):-88*(1-t);Disc(back,origin+Vector2.right*offset,78,A(Ink,.85f));Arc(back,origin+Vector2.right*offset,78,78,3,A(Violet,.7f));}
   if(b.BossSkillBusy&&b.ActiveBossSkill.solarMirror){float p=b.BossPhaseProgress;bool preparing=b.SolarMirrorPreparing,active=b.SolarMirrorActive;float fade=preparing?Mathf.Clamp01(p):active?1:1-p;float radius=146+(preparing?(1-p)*35:active?0:p*40);
    for(int i=0;i<16;i++){float a=i*Mathf.PI/8+Mathf.PI/16;var q=origin+Polar(a,radius,radius);back.Diamond(q,6,9,A(Gold,fade));back.Diamond(q,2,5,A(Ivory,fade));if(active||preparing&&i<=p*16){back.Line(origin+Polar(a-.075f,139,139),origin+Polar(a+.075f,139,139),6,A(Gold,fade));back.Line(origin+Polar(a-.075f,132,132),origin+Polar(a+.075f,132,132),2,A(Ivory,fade));}}
    if(active){Disc(back,origin,127,new Color(.16f,.25f,.32f,.85f));Arc(back,origin,127,127,3,Gold);Sun(back,origin,76,A(Gold,.85f));for(int i=-2;i<=2;i++){float x=i*25;back.Line(origin+new Vector2(x-15,-55),origin+new Vector2(x+15,55),2,A(Ivory,.2f));}}
    if(!preparing&&!active){for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var q=origin+Polar(a,139+p*50,139+p*50);back.Line(q,q+new Vector2(7,-10),3,A(Ivory,1-p));}}
   }
   float reflected=(float)(b.Time-b.SolarReflectionAt);if(reflected>=0&&reflected<.38f){float t=reflected/.38f;Ray(origin,Vector2.Lerp(origin,target,Mathf.Min(1,t*2)),5,Gold,1-t);Arc(floor,target,40+t*35,12+t*10,3,A(Gold,1-t));}
   if(!b.BossSkillBusy&&b.EnemyWindupProgress>0){float t=b.EnemyWindupProgress;bool night=b.SolarAttackMidnight;Color c=night?Violet:Gold;
    if(night){for(int sign=-1;sign<=1;sign+=2){var side=target+Vector2.right*sign*90;Arc(floor,side,36,16,3,c);for(int i=0;i<3;i++){var q=Vector2.Lerp(origin,side,.3f+i*.23f);floor.Line(q+new Vector2(-7,6),q+Vector2.down*6,3,A(c,.5f+t*.5f));floor.Line(q+new Vector2(7,6),q+Vector2.down*6,3,A(c,.5f+t*.5f));}}Arc(floor,target,43,15,2,A(Ivory,.65f));Sun(floor,target+Vector2.down*25,9,Violet,true);}
    else{Arc(floor,target,67-24*t,21-6*t,3,Gold);for(int i=0;i<3;i++){var q=Vector2.Lerp(origin,target,.3f+i*.23f);floor.Line(q+new Vector2(-9,7),q+Vector2.down*7,3,A(Gold,.4f+t*.6f));floor.Line(q+new Vector2(9,7),q+Vector2.down*7,3,A(Gold,.4f+t*.6f));}Sun(floor,target+Vector2.down*25,9,Gold);}
   }
   float age=(float)(b.Time-b.SolarAttackImpactAt);if(age>=0&&age<.3f){float f=1-age/.3f;if(b.SolarLastAttackMidnight){for(int sign=-1;sign<=1;sign+=2){var side=target+Vector2.right*sign*90;Ray(origin,side,8,Violet,f);Arc(floor,side,40+age*80,16+age*20,4,A(Violet,f));}}else{Ray(origin,target,8,Gold,f);Arc(floor,target,45+age*100,15+age*25,4,A(Gold,f));}}
   back.End();floor.End();air.End();
  }
 }
}
