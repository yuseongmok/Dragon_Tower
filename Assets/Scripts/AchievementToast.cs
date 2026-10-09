using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Separate overlay survives room screens and never blocks input.
 public sealed class AchievementToast:MonoBehaviour {
  readonly Queue<string> queue=new Queue<string>();Text text;CanvasGroup group;float remaining;
  public static AchievementToast Create(Font font){
   var go=new GameObject("Achievement notifications",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(CanvasGroup),typeof(AchievementToast));
   var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
   var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(480,850);scaler.matchWidthOrHeight=1;
   var t=go.GetComponent<AchievementToast>();t.group=go.GetComponent<CanvasGroup>();t.group.blocksRaycasts=false;t.group.alpha=0;
   var panel=new GameObject("Toast",typeof(RectTransform),typeof(Image));panel.transform.SetParent(go.transform,false);var r=(RectTransform)panel.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-92);r.sizeDelta=new Vector2(408,64);panel.GetComponent<Image>().color=new Color(.07f,.1f,.16f,.96f);panel.GetComponent<Image>().raycastTarget=false;DragonTowerTheme.Frame(panel.GetComponent<Image>(),DragonTowerTheme.Gold);
   var label=new GameObject("Text",typeof(RectTransform),typeof(Text));label.transform.SetParent(panel.transform,false);var lr=(RectTransform)label.transform;lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=new Vector2(8,4);lr.offsetMax=new Vector2(-8,-4);t.text=label.GetComponent<Text>();t.text.font=font;t.text.fontSize=18;t.text.color=new Color(1,.87f,.5f);t.text.alignment=TextAnchor.MiddleCenter;t.text.raycastTarget=false;return t;
  }
  public void Show(AchievementDefinition a){queue.Enqueue("업적 달성 · "+a.title+"\n+"+a.reward+"점");}
  void Update(){if(remaining<=0&&queue.Count>0){text.text=queue.Dequeue();remaining=3.2f;}if(remaining>0){remaining-=Time.unscaledDeltaTime;group.alpha=Mathf.Clamp01(remaining/.3f);}else group.alpha=0;}
 }
 public sealed partial class CollectionFlow {
  AchievementToast achievementToast;
  void OnDestroy(){if(achievementToast!=null){if(Session!=null)Session.AchievementUnlocked-=achievementToast.Show;Destroy(achievementToast.gameObject);}}
  void SetupAchievements(){achievementToast=AchievementToast.Create(battleView.font);Session.AchievementUnlocked+=achievementToast.Show;}
  void ShowProgressRecords(){
   Screen("업적 · 도전 기록");var career=Session.Profile.achievementProgress??new AchievementProgress();int y=195;
   foreach(var a in Session.Rewards.achievements){bool done=career.unlocked!=null&&career.unlocked.Contains(a.id);Label((done?"✓ ":"")+a.title+"   "+Mathf.Min(career.Value(a.metric),a.target)+" / "+a.target+"\n"+(done?"달성 완료":"최초 달성 +"+a.reward+"점"),0,y,420,65,18,done?Gold:Color.white);y+=75;}
   var p=Session.HasSavedRun?Session.Profile.activeRun.progress:Session.Profile.lastRun?.progress;
   if(p!=null&&!string.IsNullOrEmpty(p.runId))Label((Session.HasSavedRun?"현재 도전":"최근 도전")+" · "+p.score+"점\n"+(string.IsNullOrEmpty(p.customDragonName)?p.originalDragonName:p.customDragonName+" ("+p.originalDragonName+")")+"\n최대 "+p.maxFloor+"층 · 유대 "+p.maxBond+" · 처치 "+(p.normalKills+p.bossKills)+" (보스 "+p.bossKills+")",0,629,420,115,18,Gold);
   Button("로비로 돌아가기",0,744,380,48,ShowLobby);notice.text="업적은 누적됩니다. 달성 점수는 최초 한 번만 지급됩니다.";
  }
 }
}

