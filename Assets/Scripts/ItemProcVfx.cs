using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Item presentations share the approved element atlases. Damage always comes from the model cue.
 public sealed partial class ItemProcVfx:MonoBehaviour {
  sealed class Cast {public string id;public ItemAutoTrigger rule;public double start,impact=-1;public Vector2 from,to;public readonly List<double> impacts=new List<double>();}
  readonly List<Cast> casts=new List<Cast>();readonly List<Image> images=new List<Image>();int used;float spriteScale=1;
  BattleView view;FireSkillLibrary fire;IceSkillLibrary ice;Sprite[] wind;RectTransform root;
  public int PeakLayers{get;private set;}public int ActiveLayers=>used+ElementLayers;
  public void Initialize(BattleView owner){view=owner;fire=Resources.Load<FireSkillLibrary>("FireSkills/Library");ice=Resources.Load<IceSkillLibrary>("IceSkills/Library");wind=WindVFXStyle.Load();var go=new GameObject("Item effects",typeof(RectTransform));root=go.GetComponent<RectTransform>();root.SetParent(view.frame,false);root.anchorMin=root.anchorMax=root.pivot=Vector2.one*.5f;root.sizeDelta=new Vector2(480,850);}
  public void Cue(ItemProcVisual cue,double time){
   if(cue.effect.art==ItemProcArt.None)return;EnsureElement(cue.effect.art);
   if(cue.started){view.TryGetCharacterAnchor(cue.effect.trigger==ItemTrigger.BasicHit?DragonVisualAnchor.AttackOrigin:DragonVisualAnchor.SkillOrigin,view.frame,out var from);var to=(Vector2)view.frame.InverseTransformPoint(view.enemyArt.rectTransform.position);if(cue.effect.art==ItemProcArt.Guard)view.TryGetCharacterAnchor(DragonVisualAnchor.CharacterCenter,view.frame,out to);
    casts.Add(new Cast{id=cue.itemId,rule=cue.effect,start=time,from=from,to=to,impact=cue.effect.art==ItemProcArt.Guard?time:-1});
   }else {var c=casts.FindLast(x=>x.id==cue.itemId&&x.impacts.Count<System.Math.Max(1,x.rule.hitCount));if(c!=null){c.impact=time;c.impacts.Add(time);}}
   Step(time);
  }
  public void Step(double time){if(root==null)return;used=0;BeginElements((float)time);
   for(int n=casts.Count-1;n>=0;n--){var c=casts[n];float age=(float)(time-c.start),h=c.impact<0?-1:(float)(time-c.impact);
    if((c.impacts.Count>=System.Math.Max(1,c.rule.hitCount)&&h>1.3f)||age>c.rule.initialDelay+System.Math.Max(1,c.rule.hitCount)*c.rule.hitInterval+2){casts.RemoveAt(n);continue;}
    spriteScale=Mathf.Max(.35f,c.rule.visualScale);
    if(c.rule.art==ItemProcArt.Guard){Guard(c.to,h);continue;}
    if(c.rule.art>=ItemProcArt.Earth){spriteScale=1;ElementCast(c,age,time);continue;}
    if(c.impacts.Count<System.Math.Max(1,c.rule.hitCount)){float t=Mathf.Clamp01((age-c.impacts.Count*c.rule.hitInterval)/Mathf.Max(.001f,c.rule.initialDelay));if(c.rule.art==ItemProcArt.Tornado)Tornado(c.to,age,t);else if(c.rule.art==ItemProcArt.Meteor)Meteor(c,age,t);else Projectile(c,age,t);}
    foreach(var at in c.impacts){float k=(float)(time-at);if(k>.7f)continue;
     if(c.rule.art==ItemProcArt.FrostSpear)IceImpact(c.to,k);else if(c.rule.art==ItemProcArt.Tornado){Tornado(c.to,age,1-k/.7f);WindBurst(c.to,k);}else FireImpact(c.to,k,c.rule.art==ItemProcArt.Meteor?1.35f:1);
    }
   }
   EndElements();for(int i=used;i<images.Count;i++)images[i].gameObject.SetActive(false);PeakLayers=Mathf.Max(PeakLayers,ActiveLayers);
  }
  public void CancelMissing(BattleItem[] equipped){casts.RemoveAll(c=>System.Array.Find(equipped??System.Array.Empty<BattleItem>(),x=>x!=null&&x.id==c.id)==null);}
  public void Clear(){ClearElements();casts.Clear();used=0;foreach(var i in images)if(i!=null)i.gameObject.SetActive(false);}
  void OnDestroy(){DestroyElements();if(root!=null)Destroy(root.gameObject);}
  void Draw(Sprite sprite,Vector2 p,Vector2 size,Color color,float angle=0,bool glow=false){if(sprite==null||color.a<=0)return;Image im;if(used==images.Count){var go=new GameObject("Item layer",typeof(RectTransform),typeof(Image));im=go.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.raycastTarget=false;images.Add(im);}else im=images[used];used++;im.gameObject.SetActive(true);im.sprite=sprite;im.color=color;im.material=glow&&fire!=null?fire.style.highlight:null;im.rectTransform.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round(p.y/2)*2);im.rectTransform.sizeDelta=size*spriteScale;im.rectTransform.localRotation=Quaternion.Euler(0,0,angle);}
  static Color A(float a)=>new Color(1,1,1,Mathf.Clamp01(a));
  void F(FireStyleModule m,Vector2 p,Vector2 size,float age,float a=1,float angle=0,bool glow=false)=>Draw(fire==null?null:fire.style.Get(m,age),p,size,A(a),angle,glow);
  void I(IceStyleModule m,Vector2 p,Vector2 size,float a=1,float angle=0)=>Draw(ice==null?null:ice.style.Get(m),p,size,A(a),angle);
  void Projectile(Cast c,float age,float t){var p=Vector2.Lerp(c.from,c.to,t);var d=c.to-c.from;float angle=Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg-90;
   for(int i=5;i>=1;i--){var q=Vector2.Lerp(c.from,c.to,Mathf.Max(0,t-i*.045f));if(c.rule.art==ItemProcArt.Fireball)F(FireStyleModule.Tongue,q,new Vector2(36-i*4,62),age+i*.11f,(1-i/7f)*.65f,angle+180);else I(IceStyleModule.Refraction,q,new Vector2(17,49),.5f*(1-i/7f),angle);}
   if(c.rule.art==ItemProcArt.Fireball){F(FireStyleModule.DarkFlame,p,new Vector2(60,85),age,1,angle+180);F(FireStyleModule.Body,p,new Vector2(48,76),age,1,angle+180);F(FireStyleModule.Inner,p,new Vector2(35,60),age,1,angle+180);F(FireStyleModule.Core,p,new Vector2(23,42),age,1,angle+180,true);}
   else {I(IceStyleModule.Interior,p,new Vector2(43,106),1,angle);I(IceStyleModule.Body,p,new Vector2(35,100),1,angle);I(IceStyleModule.Edge,p,new Vector2(33,100),1,angle);I(IceStyleModule.Highlight,p,new Vector2(26,92),1,angle);}
  }
  void Meteor(Cast c,float age,float t){var sky=c.to+new Vector2(-72,100);float charge=Mathf.Clamp01(t/.45f);F(FireStyleModule.Shockwave,sky,new Vector2(128,38)*charge,age,.9f,age*20,true);F(FireStyleModule.Heat,sky,new Vector2(96,29)*charge,age,.7f,-age*35);
   for(int i=0;i<8;i++){float a=i*Mathf.PI/4+age;F(FireStyleModule.Ember,sky+new Vector2(Mathf.Cos(a)*53,Mathf.Sin(a)*15),new Vector2(7,13)*charge,age,charge,i*45,true);}
   if(t<.38f)return;float flight=Mathf.Pow(Mathf.InverseLerp(.38f,1,t),1.6f);var p=Vector2.Lerp(sky,c.to,flight);for(int i=6;i>0;i--)F(FireStyleModule.Tongue,Vector2.Lerp(sky,p,Mathf.Max(0,1-i*.11f)),new Vector2(34+i*2,72),age+i*.1f,1-i/8f,20);
   F(FireStyleModule.DarkFlame,p,new Vector2(90,112),age);F(FireStyleModule.Body,p,new Vector2(73,96),age);Draw(fire==null?null:fire.meteor,p,Vector2.one*67,Color.white,-25);F(FireStyleModule.Core,p+new Vector2(0,14),new Vector2(25,52),age,.65f,20,true);
  }
  void FireImpact(Vector2 p,float h,float scale){float f=Mathf.Clamp01((.7f-h)/.45f),e=1-Mathf.Pow(1-Mathf.Clamp01(h/.23f),3);F(FireStyleModule.Burst,p,new Vector2(160,130)*scale*(.35f+e),h,f);F(FireStyleModule.Inner,p,Vector2.one*96*scale*(.3f+e),h,f,20);if(h<.13f)F(FireStyleModule.Core,p,Vector2.one*95*scale,h,1-h/.13f,0,true);F(FireStyleModule.Shockwave,p+new Vector2(0,-42),new Vector2(210,47)*scale*(.4f+e),h,f,0,true);
   for(int i=0;i<22;i++){float a=i*2.399f;var q=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(10+h*(140+i%4*24)*scale)+Vector2.down*h*h*120;F(FireStyleModule.Ember,q,new Vector2(7,18)*(1+i%3*.2f),h+i*.1f,f,i*37,true);}
   for(int i=0;i<3;i++)F(FireStyleModule.Smoke,p+new Vector2((i-1)*37,h*40),Vector2.one*(50+h*70)*scale,h+i*.1f,f*.35f,i*80);
  }
  void IceImpact(Vector2 p,float h){float f=Mathf.Clamp01(1-h/.7f),e=Mathf.Clamp01(h/.18f);I(IceStyleModule.Impact,p,Vector2.one*(75+e*65),f);I(IceStyleModule.FrostRing,p,new Vector2(185,60)*(.35f+e),f);for(int i=0;i<18;i++){float a=i*2.399f;var q=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(14+h*(150+i%3*35))+Vector2.down*h*h*95;I(i%2==0?IceStyleModule.ShardSmall:IceStyleModule.Refraction,q,new Vector2(10,23),f,i*31);}I(IceStyleModule.FrostA,p,Vector2.one*(75+h*70),f*.55f);}
  void Tornado(Vector2 p,float age,float intensity){float a=Mathf.Clamp01(intensity*3);int frame=Mathf.FloorToInt(age*16)%8;foreach(var layer in new[]{"StormBack","StormFront","StormCore"})Draw(WindVFXStyle.Find(wind,layer+frame),p,new Vector2(136,181),A(a),0);for(int i=0;i<10;i++){float z=age*11+i*.6f;var q=p+new Vector2(Mathf.Sin(z)*(30+i*3),-55+i*12);I(IceStyleModule.Refraction,q,new Vector2(5,12),a*.75f,z*Mathf.Rad2Deg);}}
  void WindBurst(Vector2 p,float h){var ring=WindVFXStyle.Find(wind,"StormFront"+(Mathf.FloorToInt(h*14)%8));Draw(ring,p,new Vector2(180,125)*(1+h),A(1-h/.7f),h*100);}
  void Guard(Vector2 p,float h){float f=Mathf.Clamp01(1-h/.65f);I(IceStyleModule.FrostRing,p,new Vector2(126,94)*(1+h*.5f),f);for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5+1.1f;Draw(ice==null?null:ice.style.Get(IceStyleModule.Interior),p+new Vector2(Mathf.Cos(a)*43,Mathf.Sin(a)*42),new Vector2(24,37),new Color(.65f,.85f,.73f,f),a*Mathf.Rad2Deg);I(IceStyleModule.Edge,p+new Vector2(Mathf.Cos(a)*43,Mathf.Sin(a)*42),new Vector2(24,37),f,a*Mathf.Rad2Deg);}}
 }
}
