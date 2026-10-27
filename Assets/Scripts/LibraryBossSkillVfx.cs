using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // Bounded meshes and two reusable touch targets; no changes to the lower combat controls.
 public sealed class LibraryBossSkillVfx {
  BattleView view;LightningMeshLayer floor,air;readonly Button[] targets=new Button[2];readonly Text[] labels=new Text[2];
  static readonly Color Ink=new Color32(37,29,52,255),Purple=new Color32(116,77,153,255),Pale=new Color32(209,180,243,255),Gold=new Color32(210,174,105,255),Paper=new Color32(229,216,184,255);
  public int QuadCount=>(floor==null?0:floor.Count)+(air==null?0:air.Count);
  public int DroppedQuads=>(floor==null?0:floor.Overflow)+(air==null?0:air.Overflow);
  public static Vector2 BookPosition(int i)=>new Vector2(i==0?-114:114,-285);
  public void Initialize(BattleView v){view=v;floor=Layer("Azazel spell floor (pooled)");air=Layer("Azazel books (pooled)");for(int i=0;i<2;i++){
   int index=i;var g=new GameObject("Boss book target "+i,typeof(RectTransform),typeof(Image),typeof(Button));var r=g.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=BookPosition(i);r.sizeDelta=new Vector2(136,150);g.GetComponent<Image>().color=Color.clear;targets[i]=g.GetComponent<Button>();targets[i].onClick.AddListener(()=>Object.FindFirstObjectByType<BattleController>()?.RequestBossBook(index));
   var t=new GameObject("Book name and HP",typeof(RectTransform),typeof(Text),typeof(Outline)).GetComponent<Text>();t.rectTransform.SetParent(r,false);t.rectTransform.anchoredPosition=new Vector2(0,-54);t.rectTransform.sizeDelta=new Vector2(136,46);t.font=view.font;t.fontSize=14;t.alignment=TextAnchor.MiddleCenter;t.color=Paper;t.raycastTarget=false;t.GetComponent<Outline>().effectColor=Ink;labels[i]=t;
  }Clear();}
  LightningMeshLayer Layer(string n){var g=new GameObject(n,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=g.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);var m=g.GetComponent<LightningMeshLayer>();m.raycastTarget=false;return m;}
  public void Clear(){if(floor!=null){floor.Begin();floor.End();air.Begin();air.End();}foreach(var b in targets)if(b!=null)b.gameObject.SetActive(false);}
  public void Destroy(){if(floor!=null)Object.Destroy(floor.gameObject);if(air!=null)Object.Destroy(air.gameObject);foreach(var b in targets)if(b!=null)Object.Destroy(b.gameObject);}
  Color A(Color c,float a){c.a=a;return c;}
  void Depth(LightningMeshLayer m,Transform actor,bool after){int i=actor.GetSiblingIndex()+(after?1:0);if(m.transform.GetSiblingIndex()<actor.GetSiblingIndex())i--;m.transform.SetSiblingIndex(i);}
  void Box(Vector2 p,float w,float h,Color c){air.Quad(p+new Vector2(-w,-h),p+new Vector2(-w,h),p+new Vector2(w,h),p+new Vector2(w,-h),c);}
  void Ring(LightningMeshLayer m,Vector2 p,float rx,float ry,float width,Color c,bool broken=false){for(int i=0;i<32;i++){if(broken&&i%4>=2)continue;float a=i*Mathf.PI/16,z=(i+1)*Mathf.PI/16;m.Line(p+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),p+new Vector2(Mathf.Cos(z)*rx,Mathf.Sin(z)*ry),width,c);}}
  void Book(Vector2 p,float size,bool open,bool ghost,float time){float w=size*(open?1:.64f),h=size*.65f;
   if(ghost){for(int sign=-1;sign<=1;sign+=2){for(int i=0;i<4;i++){float x=sign*(8+i*w/4);air.Line(p+new Vector2(x,h-6),p+new Vector2(x+sign*6,h-3),2,A(Pale,.65f));air.Line(p+new Vector2(x,-h+5),p+new Vector2(x+sign*5,-h+8),2,A(Pale,.45f));}for(int i=0;i<3;i++){var q=p+new Vector2(sign*(12+i*10),Mathf.Sin(time*3+i)*9);air.Line(q,q+new Vector2(sign*8,3),2,A(Pale,.35f));}}return;}
   if(open){for(int sign=-1;sign<=1;sign+=2){Vector2 a=p+new Vector2(sign*3,-h),b=p+new Vector2(sign*3,h-9),c=p+new Vector2(sign*w,h),d=p+new Vector2(sign*w,-h+9);air.Quad(a+Vector2.down*4,b+Vector2.up*3,c+new Vector2(sign*4,3),d+new Vector2(sign*4,-4),Ink);air.Quad(a,b,c,d,sign<0?Paper:Gold);air.Line(d+Vector2.up*3,a+Vector2.up*3,3,Paper);for(int i=0;i<4;i++){float y=h-18-i*10;air.Line(p+new Vector2(sign*10,y),p+new Vector2(sign*(w-9),y+6),2,Purple);}}air.Line(p+Vector2.down*h,p+Vector2.up*(h-9),4,Ink);float flip=Mathf.Sin(time*8)*w*.6f;air.Line(p+Vector2.up*(h-9),p+new Vector2(flip,-h+6),3,Paper);}
   else {Box(p+new Vector2(3,-4),w+3,h+2,Ink);Box(p+new Vector2(3,-3),w,h,Gold);Box(p+new Vector2(5,-1),w-3,h-3,Paper);for(int y=0;y<3;y++)air.Line(p+new Vector2(-w+6,-h+2+y*2),p+new Vector2(w+1,-h+2+y*2),1,Purple);Box(p+new Vector2(-2,3),w,h,Ink);Box(p+new Vector2(-2,3),w-3,h-3,Purple);Box(p+new Vector2(-w+3,3),3,h-3,Gold);Box(p+new Vector2(-w+3,3),1,h-3,Paper);for(int sx=-1;sx<=1;sx+=2)for(int sy=-1;sy<=1;sy+=2){var c=p+new Vector2(sx*(w-7)-2,sy*(h-7)+3);air.Line(c,c+new Vector2(-sx*7,0),2,Gold);air.Line(c,c+new Vector2(0,-sy*7),2,Gold);}air.Diamond(p+new Vector2(0,3),11,15,Ink);air.Diamond(p+new Vector2(0,3),8,12,Gold);air.Diamond(p+new Vector2(0,3),4,8,Pale);}
  }
  void Column(Vector2 target,float power,float alpha){Ring(floor,target,60+power*20,19,4,A(Gold,alpha));Ring(floor,target,42,13,2,A(Pale,alpha));for(int i=-2;i<=2;i++){float x=i*16;var foot=target+new Vector2(x,8);air.Line(foot,foot+Vector2.up*(90+power*80-Mathf.Abs(i)*22),i==0?7:3,A(i==0?Pale:Purple,alpha*(i==0?.85f:.55f)));}for(int i=0;i<10;i++){float a=i*2.39996f;air.Diamond(target+new Vector2(Mathf.Cos(a)*65,20+Mathf.Sin(a)*22),3,6,A(Pale,alpha));}}
  public void Render(BattleModel b){
   floor.Begin();air.Begin();foreach(var button in targets)button.gameObject.SetActive(false);Depth(floor,view.enemyArt.transform,false);Depth(air,view.playerArt.transform,true);
   var anim=view.GetComponent<BattleAnimation>();var source=anim.BossSkillEnemyGround+Vector2.up*110;var target=anim.BossSkillPlayerGround;float time=(float)b.Time;
   if(b.BookBindingActive){Ring(floor,target,52,15,2,A(Purple,.65f),true);for(int i=0;i<4;i++){float a=time+i*Mathf.PI/2;air.Diamond(target+new Vector2(Mathf.Cos(a)*58,Mathf.Sin(a)*16+10),3,5,A(Gold,.55f));}}
   if(b.BossSkillBusy){var s=b.ActiveBossSkill;float t=b.BossPhaseProgress,age=(float)(b.Time-b.BossImpactAt);
    if(s.bookDurability>0){
     if(b.BossState==BossSkillState.Warning||b.BookChoiceActive||b.BossPreparation){for(int i=0;i<2;i++){var p=BookPosition(i);bool broken=b.BossBrokenBook==i;Book(p,46,false,broken,time);
       if(b.BookChoiceActive){targets[i].gameObject.SetActive(true);targets[i].interactable=b.BossBookHP(i)>0;targets[i].transform.SetAsLastSibling();labels[i].text=(i==0?"파괴의 서":"속박의 서")+" · "+b.BossBookHP(i)+"\n"+(i==b.SelectedBossBook?"선택됨":"터치하여 선택");if(i==b.SelectedBossBook){Ring(air,p,55,42,3,Gold);air.Diamond(p+Vector2.up*52,6,6,Gold);}float hp=b.BossBookHP(i)/(float)s.bookDurability;air.Line(p+new Vector2(-42,-38),p+new Vector2(42,-38),5,Ink);air.Line(p+new Vector2(-42,-38),p+new Vector2(-42+84*hp,-38),3,Pale);}
       if(b.BossPreparation&&b.BossBookSpell==i)Ring(air,p,53,42,3,Pale);
      }}
     float hit=(float)(b.Time-b.BossBookHitAt);if(hit>=0&&hit<.2f){var p=BookPosition(b.BossLastHitBook);air.Line(source,p,3,A(Gold,1-hit/.2f));Ring(air,p,48+hit*70,37+hit*45,3,A(Pale,1-hit/.2f));}
     if(b.BossPreparation&&b.BossBookSpell==0)Ring(floor,target,70,20,3,Gold);
     if(b.BossState==BossSkillState.Travel){if(b.BossBookSpell==0)Column(target,t,.7f);else Ring(air,target+Vector2.up*45,68,24,3,Purple,true);}
     if(b.BossSkillHasImpacted&&age>=0&&age<.3f&&b.BossBookSpell==0)Column(target,1,1-age/.3f);
    }else if(s.illusionShots>0){bool fake=b.BossIllusionFake;var origin=source+Vector2.up*55;Book(origin,46,true,fake,time);
     if(b.BossPreparation){Ring(floor,target,70,20,3,A(fake?Pale:Gold,fake?.4f:1),fake);for(int i=1;i<6;i++){if(fake&&i%2==0)continue;var p=Vector2.Lerp(origin,target,i/6f);floor.Line(p,p+Vector2.up*10,2,A(Pale,fake?.35f:.7f));}}
     if(b.BossState==BossSkillState.Travel){var p=Vector2.Lerp(origin,target,t);if(fake){for(int i=0;i<6;i++)air.Diamond(p+new Vector2(Mathf.Sin(i*7)*35,Mathf.Cos(i*3)*20),2,3,A(Pale,(1-t)*.55f));}else{air.Diamond(p,20,31,Ink);air.Diamond(p,16,27,Purple);air.Diamond(p,6,18,Pale);}}
     if(!fake&&b.BossSkillHasImpacted&&age>=0&&age<.25f)Ring(floor,target,60+age*180,18+age*45,4,A(Pale,1-age/.25f));for(int i=0;i<3;i++)air.Diamond(origin+new Vector2((i-1)*16,50),3,3,i<b.BossShotNumber?Gold:Ink);
    }else {var book=source+Vector2.up*55;if(b.BossPreparation){Book(book,64,true,false,time);Ring(floor,target,68,21,3,Gold);Ring(floor,target,68+24*(1-t),21+7*(1-t),2,Pale);}if(b.BossState==BossSkillState.Travel){Book(book,64,false,false,time);Column(target,t,.65f);}if(b.BossSkillHasImpacted&&age>=0&&age<.35f)Column(target,1,1-age/.35f);}
   }
   floor.End();air.End();
  }
 }
}

