using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Presentation only. Uses replayable model progress and the existing boss transform.
 public sealed class PrimordialBossCinema {
  readonly LightningMeshLayer roar;
  readonly RectTransform root;readonly Image target,body,next;readonly Sprite[] entrance,transform;
  static readonly float[] Intro={0,.07f,.16f,.26f,.32f,.38f,.48f,.82f};
  static readonly float[] Change={0,.10f,.20f,.30f,.40f,.50f,.60f,.88f};
  static Sprite[] Load(string n){var s=Resources.LoadAll<Sprite>("Enemies/Throne/Cinema/"+n);System.Array.Sort(s,(a,b)=>string.CompareOrdinal(a.name,b.name));return s;}
  public PrimordialBossCinema(Image boss){
   target=boss;root=new GameObject("Primordial in-place animation",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(boss.transform.parent,false);root.SetSiblingIndex(boss.transform.GetSiblingIndex()+1);
   var fx=new GameObject("Roar pressure waves",typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var fr=fx.GetComponent<RectTransform>();fr.SetParent(root,false);fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.offsetMin=fr.offsetMax=Vector2.zero;roar=fx.GetComponent<LightningMeshLayer>();roar.raycastTarget=false;
   body=Layer("Body");next=Layer("Frame transition");entrance=Load("entrance");transform=Load("transformation");Hide();
  }
  Image Layer(string name){var g=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));var r=g.GetComponent<RectTransform>();r.SetParent(root,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;var i=g.GetComponent<Image>();i.preserveAspect=true;i.raycastTarget=false;g.AddComponent<PrimordialOriginalFrames>();return i;}
  public void Hide(){root.gameObject.SetActive(false);}
  public void Show(BattleModel b,Sprite[] guardian,Sprite[] awakened){
   if(b.PrimordialAnimated&&b.BossSkillBusy&&b.ActiveBossSkill.attackVfx=="contract_fear"){ShowFear(b,guardian,awakened);return;}
   if(!b.PrimordialAnimated||!b.PrimordialCinematic){Hide();return;}
   var t=target.rectTransform;root.gameObject.SetActive(true);root.anchorMin=t.anchorMin;root.anchorMax=t.anchorMax;root.pivot=t.pivot;root.sizeDelta=t.sizeDelta;root.anchoredPosition=t.anchoredPosition;root.localRotation=t.localRotation;
   bool intro=b.PrimordialIntroducing;float p=intro?b.PrimordialIntroProgress:b.PrimordialTransitionProgress;float duration=intro?4.2f:5.2f;
   root.localScale=Vector3.one*(intro?1.05f:Mathf.Lerp(1.05f,1.16f,Mathf.SmoothStep(0,1,p)));
   var frames=intro?entrance:transform;var beats=intro?Intro:Change;int pose=0;while(pose<7&&p>=beats[pose+1])pose++;
   Sprite current=frames.Length==8?frames[pose]:(intro?guardian:awakened)[Mathf.Min(3,pose/2)];Sprite previous=frames.Length==8?frames[Mathf.Max(0,pose-1)]:current;
   // A short eased in-between removes hard cuts; motion remains in the drawn poses.
   float blend=Mathf.SmoothStep(0,1,Mathf.Clamp01((p-beats[pose])*duration/.12f));
   if(pose==0)blend=1;
   if(p>.93f){previous=current;current=(intro?guardian:awakened)[(int)(b.Time*3)%4];blend=Mathf.SmoothStep(0,1,(p-.93f)/.07f);}
   body.sprite=previous;next.sprite=current;body.color=new Color(1,1,1,1-blend);next.color=new Color(1,1,1,blend);
   float settle=intro?Mathf.Clamp01(p/.27f):1;float lift=intro?45*(1-Mathf.SmoothStep(0,1,settle)):0;
   root.anchoredPosition+=new Vector2(0,lift);
   RenderRoar(intro,(p-(intro?.48f:.60f))*duration-.08f);

  }
  void ShowFear(BattleModel b,Sprite[] guardian,Sprite[] awakened){
   var t=target.rectTransform;root.gameObject.SetActive(true);root.anchorMin=t.anchorMin;root.anchorMax=t.anchorMax;root.pivot=t.pivot;root.sizeDelta=t.sizeDelta;root.anchoredPosition=t.anchoredPosition;root.localRotation=t.localRotation;
   bool first=b.PrimordialPhase==1;root.localScale=Vector3.one*(first?1.05f:1.16f);
   var frames=first?entrance:transform;var idle=first?guardian:awakened;
   float u=b.BossPhaseProgress;int pose=b.BossPreparation?5:6;
   Sprite current=frames.Length==8?frames[pose]:idle[2];
   Sprite previous=b.BossPreparation?idle[1]:frames.Length==8?frames[5]:idle[1];
   float blend=Mathf.SmoothStep(0,1,Mathf.Clamp01(u*3));
   if(b.BossState==BossSkillState.Recovery){previous=current;current=idle[0];blend=Mathf.SmoothStep(0,1,u);}
   body.sprite=previous;next.sprite=current;body.color=new Color(1,1,1,1-blend);next.color=new Color(1,1,1,blend);
   float roarAge=b.BossPreparation?-1:b.BossState==BossSkillState.Travel?u*b.ActiveBossSkill.damageTime:(float)(b.Time-b.BossImpactAt)+b.ActiveBossSkill.damageTime;
   RenderRoar(first,roarAge);
  }
  // Deterministic, bounded mesh VFX; never changes camera, HUD, or combat state.
  void RenderRoar(bool intro,float elapsed){
   roar.Begin();
   if(elapsed<0||elapsed>1.12f){roar.End();return;}
   float power=1-elapsed/1.12f;Color col=intro?new Color(1,.76f,.32f):new Color(.70f,.43f,1);
   Rect r=root.rect;Vector2 center=r.center+new Vector2(0,18);float width=Mathf.Min(r.width,400);
   for(int wave=0;wave<3;wave++){
    float age=elapsed-wave*.14f;if(age<0||age>.78f)continue;float u=age/.78f;float radius=width*(.38f+.43f*u);col.a=(1-u)*.7f;
    for(int k=0;k<40;k++){float a=k*Mathf.PI/20,b=(k+1)*Mathf.PI/20;roar.Line(center+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.64f),center+new Vector2(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius*.64f),2+5*(1-u),col);}
    Vector2 floor=new Vector2(r.center.x,r.yMin+r.height*.12f);col.a=(1-u)*.8f;
    for(int k=0;k<32;k++){float a=k*Mathf.PI/16,b=(k+1)*Mathf.PI/16;roar.Line(floor+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.15f),floor+new Vector2(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius*.15f),3+3*(1-u),col);}
   }
   for(int k=0;k<24;k++){
    float a=k*Mathf.PI/12+.12f;var dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.65f);float rad=width*(.55f+elapsed*.28f);col.a=power*.55f;
    roar.Line(center+dir*rad,center+dir*(rad+12+power*24),2+power*2,col);
    if(k%2==0){col.a=power*.8f;roar.Diamond(center+dir*(rad+20),2+power*2,4+power*4,col);}
   }
   // Small recoil of the boss only; the battlefield and controls remain stable.
   float shake=Mathf.Max(0,1-elapsed/.35f);root.anchoredPosition+=new Vector2(Mathf.Sin(elapsed*85)*2.5f,Mathf.Sin(elapsed*67)*1.5f)*shake;
   roar.End();
  }
 }
}
