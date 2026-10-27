using UnityEngine;
namespace DragonTower {
 public partial class BattleView {
  UnityEngine.UI.Image awakenedArena,roarDimmer;
  UnityEngine.UI.Image ArenaLayer(string name){var g=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));var r=g.GetComponent<RectTransform>();r.SetParent(arenaImage.transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;var im=g.GetComponent<UnityEngine.UI.Image>();im.raycastTarget=false;im.color=Color.clear;return im;}
  void ShowAwakenedArena(BattleModel b){
   if(arenaImage==null)return;
   if(awakenedArena==null){awakenedArena=ArenaLayer("Awakened primordial sky");awakenedArena.sprite=Resources.Load<Sprite>("Environments/Throne/arena-awakened");awakenedArena.transform.SetAsFirstSibling();}
   if(roarDimmer==null)roarDimmer=ArenaLayer("Brief roar darkness");
   float reveal=b.PrimordialPhase==2?1:b.PrimordialTransitioning?Mathf.SmoothStep(0,1,Mathf.Clamp01((b.PrimordialTransitionProgress-.40f)/.35f)):0;
   if(b.Result==BattleResult.Victory)reveal*=1-(motion==null?1:motion.PrimordialReleaseProgress);
   awakenedArena.color=new Color(1,1,1,b.PrimordialAnimated&&awakenedArena.sprite!=null?reveal:0);
   float darkness=0;
   if(b.PrimordialAnimated&&b.PrimordialCinematic){bool intro=b.PrimordialIntroducing;float p=intro?b.PrimordialIntroProgress:b.PrimordialTransitionProgress;float elapsed=(p-(intro?.48f:.60f))*(intro?4.2f:5.2f)-.08f;
    if(elapsed>=0&&elapsed<1.08f)darkness=.62f*Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.08f))*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-.48f)/.60f)));
   }
   if(b.PrimordialAnimated&&b.BossSkillBusy&&b.ActiveBossSkill.attackVfx=="contract_fear"&&!b.BossPreparation){
    float t=b.BossState==BossSkillState.Travel?b.BossPhaseProgress*b.ActiveBossSkill.damageTime:(float)(b.Time-b.BossImpactAt)+b.ActiveBossSkill.damageTime;
    darkness=.5f*Mathf.Clamp01(t/.06f)*(1-Mathf.Clamp01((t-.18f)/.35f));
   }
   roarDimmer.color=new Color(.015f,.01f,.035f,darkness);
  }
  PrimordialThroneEnvironment primordialEnvironment;
  Sprite contractNormal,contractCorrupted;
  Sprite[] guardianFrames,awakenedFrames;
  PrimordialBossCinema primordialCinema;
  static Sprite[] LoadBossFrames(string name){var frames=Resources.LoadAll<Sprite>("Enemies/Throne/Animated/"+name);System.Array.Sort(frames,(a,b)=>string.CompareOrdinal(a.name,b.name));return frames;}
  BattleModel primordialShown;
  BondHeartState lastHeartState;
  void ResetPrimordialPresentation(bool enabled){
   primordialShown=null;
   if(awakenedArena!=null)awakenedArena.color=Color.clear;if(roarDimmer!=null)roarDimmer.color=Color.clear;
   primordialCinema?.Hide();HideFinalMemory();
   if(pixelEnemy!=null){pixelEnemy.enabled=true;pixelEnemy.rectTransform.localScale=Vector3.one;}
   if(enabled&&guardianFrames==null){guardianFrames=LoadBossFrames("guardian");awakenedFrames=LoadBossFrames("awakened");}
   if(enabled&&primordialEnvironment==null&&arenaImage!=null)primordialEnvironment=PrimordialThroneEnvironment.Create(arenaImage.rectTransform,Resources.Load<Sprite>("Environments/Throne/heart"));
   if(primordialEnvironment!=null){primordialEnvironment.gameObject.SetActive(enabled);if(enabled)primordialEnvironment.SetStateImmediate(BondHeartState.Normal);}
   if(enabled){contractNormal=guardianFrames.Length==4?guardianFrames[0]:Resources.Load<Sprite>("Enemies/Throne/contract_guardian");contractCorrupted=awakenedFrames.Length==4?awakenedFrames[0]:Resources.Load<Sprite>("Enemies/Throne/contract_corrupted");}
  }
  void ShowPrimordialPresentation(BattleModel b){
   if(!b.PrimordialBossBattle)return;
   ShowAwakenedArena(b);ShowFinalMemory(b);
   bool corrupt=b.PrimordialPhase==2&&!b.PrimordialRescued||b.PrimordialTransitioning&&b.PrimordialTransitionProgress>=.55f;
   if(pixelEnemy!=null){
    pixelEnemy.sprite=corrupt?contractCorrupted:contractNormal;
    if(b.PrimordialAnimated&&guardianFrames.Length==4&&awakenedFrames.Length==4){
     // The rescued heart does not instantly turn the body back into phase one.
     bool released=b.Result==BattleResult.Victory;bool second=b.PrimordialPhase==2&&!(released&&(motion==null||motion.PrimordialReleaseProgress>=.45f));int pose=released?0:(int)(b.Time*3)%4;
     if(b.EnemyStunned)pose=0;else if(b.BossPreparation)pose=b.BossPhaseProgress>.55f?2:1;else if(b.BossState==BossSkillState.Travel)pose=2;
     if(pixelEnemy.GetComponent<PrimordialOriginalFrames>()==null)pixelEnemy.gameObject.AddComponent<PrimordialOriginalFrames>();
     pixelEnemy.sprite=(second?awakenedFrames:guardianFrames)[pose];
     pixelEnemy.rectTransform.localScale=Vector3.one*(second?1.16f:1.05f);
     pixelEnemy.enabled=!b.PrimordialCinematic&&!(b.BossSkillBusy&&b.ActiveBossSkill.attackVfx=="contract_fear");
     if(primordialCinema==null)primordialCinema=new PrimordialBossCinema(pixelEnemy);
     primordialCinema.Show(b,guardianFrames,awakenedFrames);
    }
   }
   enemyName.text=b.Result==BattleResult.Victory?"해방된 최초의 계약자":(b.PrimordialPhase==1&&!(b.PrimordialAnimated&&b.PrimordialTransitioning&&b.PrimordialTransitionProgress>=.5f))?"최초의 계약자 · 1페이즈":"잠식된 계약자 · 2페이즈";
   var state=b.Result==BattleResult.Victory||b.PrimordialRescued?BondHeartState.Recovering:corrupt?BondHeartState.Corrupted:BondHeartState.Normal;
   if(primordialEnvironment!=null){if(primordialShown!=b)primordialEnvironment.SetStateImmediate(state);else if(lastHeartState!=state)primordialEnvironment.TransitionTo(state,1.4f);}
   primordialShown=b;lastHeartState=state;
   if(b.PrimordialTransitioning&&!b.PrimordialAnimated){enemyHP.text="봉인 붕괴";message.text="최초의 계약자가 당신을 보호합니다";warning.text="";skillButton.interactable=false;windupFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,0);}
   if(b.PrimordialAnimated){
    float hp=b.PrimordialCinematic?b.PrimordialHealthReveal:(float)b.EnemyHP/b.CurrentEnemyMaxHP;
    enemyFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,400*hp);
    enemyHP.text=(b.PrimordialCinematic?Mathf.CeilToInt(hp*b.CurrentEnemyMaxHP):b.EnemyHP)+" / "+b.CurrentEnemyMaxHP+(!b.PrimordialCinematic&&b.EnemyShieldHP>0?"  + 방벽 "+b.EnemyShieldHP:"");
    if(b.PrimordialCinematic){message.text="";warning.text="";skillButton.interactable=false;windupFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,0);}
   }
  }
 }
}
