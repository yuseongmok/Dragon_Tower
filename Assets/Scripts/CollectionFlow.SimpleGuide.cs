using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public sealed partial class CollectionSession {
  public void MarkSimpleCombatGuide(){var n=ProfileStore.Clone(Profile);n.simpleCombatGuideSeen=true;store.Save(n);Profile=n;}
 }
 public sealed partial class CollectionFlow {
  Text simpleGuide;float simpleGuideUntil;
  public void ShowSimpleCombatGuide(){
   if(Session==null||Session.Profile.simpleCombatGuideSeen||towerRun==null)return;
   Session.MarkSimpleCombatGuide();
   var r=Rect("First combat controls",battleView.frame,0,420,436,110);
   simpleGuide=r.gameObject.AddComponent<Text>();simpleGuide.font=battleView.font;simpleGuide.fontSize=20;simpleGuide.alignment=TextAnchor.MiddleCenter;simpleGuide.color=Color.white;simpleGuide.raycastTarget=false;
   simpleGuide.text="탭해서 공격\n슬라이드해서 회피\n오른쪽 아래 버튼을 눌러 스킬 사용";
   r.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(2,-2);r.SetAsLastSibling();simpleGuideUntil=Time.unscaledTime+7;
  }
  void TickSimpleGuide(){if(simpleGuide!=null&&(Time.unscaledTime>=simpleGuideUntil||controller.CurrentBattle==null||controller.CurrentBattle.Result!=BattleResult.Fighting)){Destroy(simpleGuide.gameObject);simpleGuide=null;}}
 }
}
