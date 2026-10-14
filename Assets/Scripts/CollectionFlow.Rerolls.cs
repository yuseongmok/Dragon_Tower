using System;using System.Linq;using System.Collections;using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  Button rerollButton;bool rerollBusy;
  RandomReward[] CurrentRerollRewards(bool levelReward) {
   var saved=towerRun.RerollFor(levelReward?RewardRerollKind.Level:RewardRerollKind.Room);
   if(saved==null)return PickRandomRewards(towerRun.RewardSeed^towerRun.Level^(towerRun.Floor<<8),3);
   return saved.candidates.Select(id=>id.StartsWith("a:")?new RandomReward{augment=database.augments.First(x=>x.StableId==id.Substring(2))}:new RandomReward{skill=database.skills.First(x=>x.StableId==id.Substring(2))}).ToArray();
  }
  void AddRerollButton(RewardRerollKind kind,float y,Action redraw) {
   var owner=body;string key=towerRun.RewardKey(kind);long revision=Session.Profile.bondRevision;
   var b=Button("",-72,y,272,44,()=>{
    bool mixed=kind==RewardRerollKind.Room||kind==RewardRerollKind.Level;
    if(body!=owner||rerollBusy||choosingReward||rewardDetail!=null||(mixed&&!rewardsReady))return;
    rerollBusy=true;
    try {
     if(Session.TryRerollReward(towerRun,database,kind,key,revision,UnityEngine.Random.Range(0,int.MaxValue),out string message))StartCoroutine(RerollTransition(owner,redraw));
     else {notice.text=message;rerollBusy=false;}
    }catch(Exception e){rerollBusy=false;notice.text="저장하지 못했습니다. 포인트와 보상은 유지됩니다.";Debug.LogException(e);}
   });rerollButton=b;b.name="Bond reward reroll";
   StyleCard(b,new Color(.22f,.13f,.065f),Gold);
   // Pixel-native arrows avoid missing glyphs in the Korean font.
   var icon=Rect("Reroll arrows",b.transform,-108,22,24,24).gameObject.AddComponent<RerollArrows>();icon.color=Gold;icon.raycastTarget=false;
   var text=CardText(b.transform,"Reroll label","",14,22,214,40,16,GoldenUi.Ivory,TextAnchor.MiddleCenter);
   BondPointSymbol.Add(body,new Vector2(87,-y),22);
   var balance=Label(Session.Profile.bondPoints+"P",153,y,108,40,16,Gold);
   StartCoroutine(RefreshRerollButton(owner,b,text,balance,kind));
  }
  IEnumerator RefreshRerollButton(RectTransform owner,Button b,Text text,Text balance,RewardRerollKind kind) {
   while(owner==body&&b!=null) {
    int cost=BondEconomy.Load().rerollCost,points=Session.Profile.bondPoints;
    bool used=towerRun.RerollFor(kind)!=null,mixed=kind==RewardRerollKind.Room||kind==RewardRerollKind.Level;
    text.text=rerollBusy?"재추첨 중…":used?"재추첨 완료":points<cost?(cost-points)+"P 부족 · "+cost+"P":"재추첨 · "+cost+"P";
    balance.text=points+"P";
    b.interactable=!used&&points>=cost&&!rerollBusy&&!choosingReward&&rewardDetail==null&&(!mixed||rewardsReady)&&towerRun.CanRerollScreen(kind);
    yield return null;
   }
  }
  static CanvasGroup RerollGroup(Button button){var g=button.GetComponent<CanvasGroup>();return g!=null?g:button.gameObject.AddComponent<CanvasGroup>();}
  IEnumerator RerollTransition(RectTransform owner,Action redraw) {
   var cards=ChoiceButtons;var groups=cards==null?Array.Empty<CanvasGroup>():cards.Where(x=>x!=null).Select(RerollGroup).ToArray();
   foreach(var g in groups){g.interactable=false;g.blocksRaycasts=false;}
   if(RoomButton!=null)RoomButton.interactable=false;if(SecondRoomButton!=null)SecondRoomButton.interactable=false;
   float time=0;while(time<.18f&&body==owner){time+=Time.unscaledDeltaTime;foreach(var g in groups)if(g!=null)g.alpha=1-Mathf.Clamp01(time/.18f);yield return null;}
   if(body!=owner)yield break;rerollBusy=false;
   redraw();
   // Mixed rewards already have their existing staggered reveal.
   if(ChoiceButtons!=null&&!ScreenName.EndsWith(" 증강")&&ScreenName!="증강방") {
    owner=body;groups=ChoiceButtons.Where(x=>x!=null).Select(RerollGroup).ToArray();
    foreach(var g in groups){g.interactable=false;g.blocksRaycasts=false;}
    time=0;while(time<.22f&&body==owner){time+=Time.unscaledDeltaTime;foreach(var g in groups)if(g!=null)g.alpha=Mathf.Clamp01(time/.22f);yield return null;}
    if(body==owner)foreach(var g in groups)if(g!=null){g.alpha=1;g.interactable=g.blocksRaycasts=true;}
   }
  }
 }
 [RequireComponent(typeof(CanvasRenderer))] public sealed class RerollArrows:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper v) {
   v.Clear();string[] rows={"..######....",".##....##...","##......##..","##.....####.","........###.","............",".###........",".####.....##","..##......##","...##....##.","....######.."};var rect=GetPixelAdjustedRect();
   for(int y=0;y<rows.Length;y++)for(int x=0;x<12;x++)if(rows[y][x]=='#'){
    float l=rect.x+x*rect.width/12,t=rect.yMax-y*rect.height/11,w=rect.width/12,h=rect.height/11;int i=v.currentVertCount;
    v.AddVert(new Vector3(l,t),color,Vector2.zero);v.AddVert(new Vector3(l+w,t),color,Vector2.zero);v.AddVert(new Vector3(l+w,t-h),color,Vector2.zero);v.AddVert(new Vector3(l,t-h),color,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);
   }
  }
 }
}
