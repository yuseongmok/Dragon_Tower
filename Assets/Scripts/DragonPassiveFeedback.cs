using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Read-only feedback. No RNG calls, gameplay callbacks or saved state.
 public sealed class DragonPassiveFeedback:MonoBehaviour {
  BattleView view;BattleModel owner;LightningMeshLayer mesh;BattleVfxHudVisibility materials;Text label;RawImage dragon;
  int proc,cast;double started=-100;DragonPassiveMechanic kind;
  public int PulseCount{get;private set;}public int QuadCount=>mesh==null?0:mesh.Count;
  public string Label=>label==null?"":label.text;
  public static bool Supports(DragonPassiveMechanic k)=>k==DragonPassiveMechanic.JetStream||k==DragonPassiveMechanic.HardenedShell||k==DragonPassiveMechanic.RuyiOrb;
  RectTransform Rect(string name,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=new Vector2(w,h);return r;}
  public void Initialize(BattleView v){view=v;materials=new BattleVfxHudVisibility();var r=Rect("Passive feedback geometry",480,850);r.gameObject.AddComponent<CanvasRenderer>();mesh=r.gameObject.AddComponent<LightningMeshLayer>();mesh.raycastTarget=false;mesh.material=materials.alpha;
   var lr=Rect("Passive feedback text",330,25);lr.anchoredPosition=new Vector2(0,-75);label=lr.gameObject.AddComponent<Text>();label.font=v.font;label.fontSize=14;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;var outline=lr.gameObject.AddComponent<Outline>();outline.effectColor=new Color(0,0,0,.85f);outline.effectDistance=new Vector2(1,-1);
   var dr=Rect("Mir passive dragon afterimage",125,42);dragon=dr.gameObject.AddComponent<RawImage>();dragon.texture=Resources.Load<Texture2D>("MirLegendary/Dragon");dragon.raycastTarget=false;dragon.material=materials.alpha;dragon.enabled=false;BattleVfxHudVisibility.BringHudForward(v);
  }
  public void Render(BattleModel b){if(mesh==null)return;mesh.Begin();dragon.enabled=false;label.enabled=false;label.text="";
   if(owner!=b){owner=b;started=-100;proc=b==null?0:b.PassiveProcCount;cast=b==null?0:b.PassivePresentationCastSerial;PulseCount=0;}
   if(b==null){mesh.End();return;}kind=b.Dragon.passiveMechanic;
   bool pulse=kind==DragonPassiveMechanic.HardenedShell?b.PassivePresentationCastSerial>cast&&b.TitanPassiveBonus>0:b.PassiveProcCount>proc;
   proc=b.PassiveProcCount;cast=b.PassivePresentationCastSerial;
   if(!b.NewPassives||!Supports(kind)||b.Result!=BattleResult.Fighting||!view.IsBattleUncovered){started=-100;mesh.End();return;}
   if(pulse){started=b.Time;PulseCount++;}float age=(float)(b.Time-started),u=Mathf.Clamp01(age/.55f),fade=1-u;
   Color accent=kind==DragonPassiveMechanic.HardenedShell?new Color(1,.8f,.40f):new Color(.5f,1,.87f);
   label.color=accent;label.rectTransform.anchoredPosition=new Vector2(0,kind==DragonPassiveMechanic.JetStream?-30:-75);
   if(kind==DragonPassiveMechanic.HardenedShell){label.enabled=true;label.text="체력 연동 · 스킬 피해 +"+b.TitanPassiveBonus.ToString("0.#")+"%";}
   else if(kind==DragonPassiveMechanic.JetStream&&b.Dragon.combatRulesVersion>=18){label.enabled=b.JetStreamRemaining>0;label.text=label.enabled?"공격 쿨타임 -"+PassiveImprintRules.Value(b.PassiveLevel,15,40).ToString("0.#")+"% · "+b.JetStreamRemaining.ToString("0.0")+"초":"";}
   else if(age<.85f){label.enabled=true;label.text=kind==DragonPassiveMechanic.JetStream?"공격 대기시간 감소":"추가타";}
   if(age>=.55f){mesh.End();return;}materials.Update(view);
   view.TryGetCharacterAnchor(DragonVisualAnchor.CharacterCenter,view.frame,out var player); view.TryGetCharacterAnchor(DragonVisualAnchor.GroundPosition,view.frame,out var ground);
   Vector2 enemy=(Vector2)view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));accent.a=fade;
   if(kind==DragonPassiveMechanic.JetStream){var center=player+Vector2.down*55;for(int j=0;j<2;j++)for(int i=0;i<26;i++){float a=i*.18f+u*5+j*3.14f,c=a+.18f;float radius=32+u*26+j*7;mesh.Line(center+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.22f),center+new Vector2(Mathf.Cos(c)*radius,Mathf.Sin(c)*radius*.22f),j==0?3:2,accent);}}
   else if(kind==DragonPassiveMechanic.HardenedShell){for(int j=-1;j<=1;j++){var a=player+new Vector2(j*15,-12);for(int i=0;i<4;i++){var next=a+new Vector2((i%2==0?8:-6)*j,9);mesh.Line(a,next,3,accent);a=next;}}}
   else{if(dragon.texture!=null){dragon.enabled=true;dragon.color=new Color(.6f,1,1,fade);dragon.rectTransform.anchoredPosition=enemy+new Vector2(-55+u*100,12-Mathf.Sin(u*Mathf.PI)*25);dragon.rectTransform.localEulerAngles=new Vector3(0,0,-18);}
    for(int i=0;i<6;i++){float a=i*Mathf.PI/3;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));mesh.Line(enemy+d*(7+u*8),enemy+d*(16+u*20),2,accent);}}
   mesh.End();
  }
  void OnDisable(){started=-100;if(mesh!=null){mesh.Begin();mesh.End();}if(label!=null)label.enabled=false;if(dragon!=null)dragon.enabled=false;}
  void OnDestroy(){if(mesh!=null)Destroy(mesh.gameObject);if(label!=null)Destroy(label.gameObject);if(dragon!=null)Destroy(dragon.gameObject);materials?.Dispose();}
 }
}

