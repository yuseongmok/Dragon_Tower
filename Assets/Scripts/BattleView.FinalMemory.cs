using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 public partial class BattleView {
  Text finalSpeech;
  void HideFinalMemory(){if(finalSpeech!=null)finalSpeech.gameObject.SetActive(false);}
  void ShowFinalMemory(BattleModel b){
   if(!b.PrimordialAnimated||!b.PrimordialCinematic){HideFinalMemory();return;}
   // Keep the arena and the transforming boss visible. No story image interrupts combat.
   if(finalSpeech==null){
    finalSpeech=Label("",frame,0,612,422,90,21,new Color(1,.94f,.79f));
    finalSpeech.raycastTarget=false;
    finalSpeech.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(1,-2);
   }
   finalSpeech.gameObject.SetActive(true);
   finalSpeech.transform.SetAsLastSibling();
   finalSpeech.text=b.PrimordialIntroducing
    ?"다가오지 마라.\n이번에는…… 빼앗기지 않겠다."
    :b.PrimordialTransitionProgress<.48f
     ?"이 약속만은…… 빼앗길 수 없다."
     :"아직…… 약속은 끝나지 않았다!";
  }
 }
}
