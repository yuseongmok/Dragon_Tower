using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  static readonly string[] ImprintNames={"패시브","일반공격","스킬 피해","체력"};
  int imprintAbility; bool imprintAnimating;
  string ImprintEffect(DragonData d,int ability,int level){
   if(ability>0)return ImprintNames[ability]+" +"+(level*2)+"%";
   var k=d.passiveMechanic;float chance=PassiveImprintRules.Chance(k,level),duration=PassiveImprintRules.Duration(k,level);
   switch(k){
    case DragonPassiveMechanic.JetStream:return "남은 일반공격 대기시간 -"+PassiveImprintRules.Value(level,15,40)+"%";
    case DragonPassiveMechanic.VoidAccelerator:return "재시전 최대 HP 소비 "+PassiveImprintRules.OvercastCost(level)+"%";
    case DragonPassiveMechanic.HydraVenom:return PassiveImprintRules.PoisonPeriod(level).ToString("0.0")+"초마다 공격력 "+PassiveImprintRules.DotPercent(level)+"% 중독";
    case DragonPassiveMechanic.HardenedShell:return "증가 HP 10%당 스킬 +"+PassiveImprintRules.Value(level,1,3).ToString("0.0")+"% (상한 30%)";
    case DragonPassiveMechanic.FrozenBlade:return "얼음 스킬 허용 · 전기 약점 추가 피해 -"+(level*5)+"%";
    case DragonPassiveMechanic.Mirage:return "회피 공격력 +2% × 5 · "+duration.ToString("0.0")+"초";
    case DragonPassiveMechanic.HolyLight:return "스킬·자동 아이템 대기시간 -"+PassiveImprintRules.CooldownReduction(level)+"%";
    case DragonPassiveMechanic.RuyiOrb:return chance+"% 확률 · 원본의 30% 추가타";
    case DragonPassiveMechanic.IronArmor:return chance+"% · HP 10% 보호막 5초 / 피해 +10%";
    case DragonPassiveMechanic.Phoenix:return chance+"% 확률 · 최대 HP "+PassiveImprintRules.Value(level,10,30)+"% 회복";
    case DragonPassiveMechanic.Tentacle:return chance+"% · 촉수 "+duration.ToString("0.0")+"초 / 초당 공격력 20%";
    case DragonPassiveMechanic.FlameBreath:return chance+"% 확률 · 화상 "+duration.ToString("0.0")+"초 / 초당 "+PassiveImprintRules.DotPercent(level)+"%";
    default:return chance+"% 확률 · "+duration.ToString("0.0")+"초 "+(k==DragonPassiveMechanic.TimeRift?"기절":k==DragonPassiveMechanic.FreezingGaze?"둔화 20%":"마비");
   }
  }
  void ImprintBase(){
   ShowStatusDetails();
   // Expand only the lower status section. Keep portrait, name, anchors and lobby button.
   foreach(Transform child in body){var r=child as RectTransform;if(r!=null&&-r.anchoredPosition.y>=463&&-r.anchoredPosition.y<724)child.gameObject.SetActive(false);}
      portrait.anchoredPosition=new Vector2(0,-245);
   var frame=body.Find("Dragon portrait frame") as RectTransform;if(frame!=null){frame.anchoredPosition=new Vector2(0,-245);frame.sizeDelta=new Vector2(420,220);}
   foreach(Transform child in body){var r=child as RectTransform;if(r==null)continue;if(Mathf.Approximately(-r.anchoredPosition.y,440))r.anchoredPosition=new Vector2(0,-383);if(child.name=="로비로 돌아가기")r.anchoredPosition=new Vector2(0,-784);}
   notice.rectTransform.anchoredPosition=new Vector2(0,-825);notice.rectTransform.sizeDelta=new Vector2(430,22);
   Panel("유대 각인",body,0,579,422,352,GoldenUi.Dark,GoldenUi.Gold);
  }
  public void ShowImprintPanel(string message="",bool? succeeded=null){
   if(imprintAnimating)return;ImprintBase();var d=Session.Selected;var x=Session.GetImprint(d.StableId);var c=BondEconomy.Load();int a=imprintAbility,level=x.levels[a];long revision=Session.Profile.bondRevision;
   Label("유대 각인 "+x.Total+" / 10",-91,422,200,27,18,Gold);
   BondPointSymbol.Add(body,new Vector2(38,-422));
   var balance=Label(Session.Profile.bondPoints.ToString("N0"),119,422,128,27,18,Gold);balance.resizeTextForBestFit=true;balance.resizeTextMinSize=12;balance.resizeTextMaxSize=18;
   for(int i=0;i<4;i++){
    int selected=i;float y=458+i*38;
    var row=Rect("Imprint row "+i,body,0,y,396,34);var bg=row.gameObject.AddComponent<Image>();bg.color=i==a?new Color(.35f,.27f,.17f,.9f):new Color(.12f,.10f,.10f,.6f);
    var b=row.gameObject.AddComponent<Button>();b.targetGraphic=bg;b.onClick.AddListener(()=>{if(imprintAnimating)return;imprintAbility=selected;ShowImprintPanel();});
    Label(ImprintNames[i],-144,y,98,30,18,i==a?Gold:Color.white);
    for(int n=0;n<10;n++){var cell=Rect("Gauge "+i+" "+n,row,-78+n*23,17,19,20).gameObject.AddComponent<Image>();cell.color=n<x.levels[i]?new Color(.95f,.73f,.32f):new Color(.17f,.15f,.14f);cell.raycastTarget=false;var edge=cell.gameObject.AddComponent<Outline>();edge.effectColor=n<x.levels[i]?new Color(1,.89f,.58f):new Color(.46f,.38f,.29f);edge.effectDistance=new Vector2(1,-1);}
    Label(x.levels[i].ToString(),175,y,32,30,20,Gold);
   }   var effectText=Label("현재  "+ImprintEffect(d,a,level)+"\n"+(level<10?"다음  "+ImprintEffect(d,a,level+1):"최대 단계"),0,616,396,48,16,Color.white);effectText.resizeTextForBestFit=true;effectText.resizeTextMinSize=12;effectText.resizeTextMaxSize=16;
   Label("연속 실패 "+x.failures[a]+"회 · "+(x.failures[a]>=5?"다음 확정 성공":"2회부터 회당 +10%p / 5회 후 확정"),0,653,394,22,14,Muted);
   string title=x.Total>=10?"투자 한도 도달":c.costs[level]+"P · 성공 "+c.Chance(level,x.failures[a])+"% · 강화";
   var upgrade=Button(title,0,686,386,42,()=>BeginImprintUpgrade(d.StableId,a,revision));
   upgrade.GetComponentInChildren<Text>().fontSize=17;upgrade.interactable=x.Total<10&&level<10&&Session.Profile.bondPoints>=c.costs[level];
   Button("재분배 · "+c.redistributionCost+"P",-101,731,188,36,()=>ShowImprintRedistribution(x.levels)).GetComponentInChildren<Text>().fontSize=16;
   Button("상세 정보",101,731,188,36,ShowStatusDetails).GetComponentInChildren<Text>().fontSize=16;
   notice.text=string.IsNullOrEmpty(message)?"영구 저장 · 다음 전투부터 적용 · 행을 눌러 강화할 능력을 선택":message;

  }
  void BeginImprintUpgrade(string species,int ability,long revision){
   if(imprintAnimating)return;
   try{if(!Session.TryUpgradeImprint(species,ability,revision,out bool ok)){notice.text="포인트 또는 투자 한도를 확인해주세요.";return;}imprintAnimating=true;StartCoroutine(AnimateImprintUpgrade(ok));}
   catch(Exception){imprintAnimating=false;notice.text="저장 실패 · 비용과 단계는 유지됩니다.";}
  }
  IEnumerator AnimateImprintUpgrade(bool success){
   var owner=body;var lockGroup=body.gameObject.AddComponent<CanvasGroup>();lockGroup.interactable=false;
   var dragon=portrait.GetComponent<Graphic>();var original=dragon!=null?dragon.color:Color.white;
   var glowEdge=portrait.gameObject.AddComponent<Outline>();glowEdge.effectDistance=new Vector2(3,-3);glowEdge.useGraphicAlpha=true;
   try{
    for(float t=0;t<1.05f&&body==owner;t+=Mathf.Min(Time.unscaledDeltaTime,.05f)){float glow=Mathf.Sin(t/1.05f*Mathf.PI);glowEdge.effectColor=new Color(1,.84f,.32f,glow*.9f);if(dragon!=null)dragon.color=Color.Lerp(original,new Color(1,.94f,.72f),glow*.5f);yield return null;}
   }finally{if(dragon!=null)dragon.color=original;if(glowEdge!=null)Destroy(glowEdge);imprintAnimating=false;if(lockGroup!=null)Destroy(lockGroup);}
   if(body!=owner)yield break;
   ShowImprintPanel();owner=body;var group=body.gameObject.AddComponent<CanvasGroup>();group.interactable=false;imprintAnimating=true;
   var result=Label(success?"강화 성공!":"강화 실패",0,325,370,48,30,success?Gold:new Color(.9f,.62f,.56f));var outline=result.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.08f,.04f,.04f);outline.effectDistance=new Vector2(2,-2);if(success)DragonTowerAudio.PlayLevelUp();
   notice.text=success?"한 칸이 채워졌습니다.":"단계는 유지됩니다. 실패 보호가 누적됩니다.";
   try{for(float t=0;t<1.25f&&body==owner;t+=Mathf.Min(Time.unscaledDeltaTime,.05f))yield return null;}
   finally{imprintAnimating=false;if(result!=null)Destroy(result.gameObject);if(group!=null)Destroy(group);}
  }  void ShowImprintRedistribution(int[] source){
   int[] values=(int[])source.Clone();ImprintBase();var d=Session.Selected;var x=Session.GetImprint(d.StableId);long revision=Session.Profile.bondRevision;
   int total=0;foreach(var n in values)total+=n;
   Label("각인 재분배 · "+total+" / "+x.Total+" 단계",0,482,390,28,18,Gold);
   for(int i=0;i<4;i++){int index=i;float y=521+i*41;Label(ImprintNames[i]+"   "+values[i],-38,y,240,32,17,Color.white);Button("−",112,y,43,34,()=>{if(values[index]>0)values[index]--;ShowImprintRedistribution(values);});Button("+",166,y,43,34,()=>{if(total<x.Total&&values[index]<10)values[index]++;ShowImprintRedistribution(values);});}
   var apply=Button("변경 확인 · "+BondEconomy.Load().redistributionCost+"P",-73,693,252,38,()=>ShowImprintConfirmation(values,revision));apply.GetComponentInChildren<Text>().fontSize=15;apply.interactable=total==x.Total&&Session.Profile.bondPoints>=BondEconomy.Load().redistributionCost;
   Button("취소",135,693,122,38,()=>ShowImprintPanel()).GetComponentInChildren<Text>().fontSize=16;notice.text="단계 총합을 유지합니다. 실패 보호는 초기화됩니다.";
  }
  void ShowImprintConfirmation(int[] values,long revision){
   if(imprintAnimating)return;ImprintBase();var d=Session.Selected;var x=Session.GetImprint(d.StableId);string rows="";for(int i=0;i<4;i++)rows+=ImprintNames[i]+"   "+x.levels[i]+" → "+values[i]+"\n";
   Label("재분배를 확정할까요?",0,487,390,30,20,Gold);Label(rows,0,565,370,112,19,Color.white);
   Label(BondEconomy.Load().redistributionCost+"P 소비 · 총 "+x.Total+"단계 유지\n실패 보호 초기화",0,647,385,45,16,Muted);
   Button("확정",-101,697,188,38,()=>{try{ShowImprintPanel(Session.TryRedistributeImprint(d.StableId,values,revision)?"재분배 완료":"변경할 단계와 포인트를 확인해주세요.");}catch(Exception){ShowImprintPanel("저장 실패 · 이전 배치를 유지합니다.");}});
   Button("취소",101,697,188,38,()=>ShowImprintPanel());
  }
 }
}
