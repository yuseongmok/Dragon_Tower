using System;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  BattleStats NamedBattleStats(){var stats=towerRun.BuildBattleStats(Session.Selected.Snapshot(towerRun.Level));stats.displayName=Session.SelectedName(DragonData.EvolutionStage(towerRun.Level));return stats;}
  void ShowRenameDragon(){
   string instance=Session.Profile.selectedInstanceId;Screen("파트너 이름 변경");
   Label("원래 이름 · "+Session.Selected.displayName,0,211,420,38,20,Muted);
   Label("함께할 이름을 지어주세요",0,270,420,42,25,Gold);
   var box=Rect("Dragon name input",body,0,350,410,64);var image=box.gameObject.AddComponent<Image>();image.color=new Color(.12f,.17f,.24f,1);DragonTowerTheme.Frame(image,DragonTowerTheme.GoldDim);
   var input=box.gameObject.AddComponent<InputField>();input.targetGraphic=image;input.lineType=InputField.LineType.SingleLine;input.characterValidation=InputField.CharacterValidation.None;
   var text=CardText(box,"Name text","",0,32,380,56,25,Color.white,TextAnchor.MiddleLeft);text.supportRichText=false;input.textComponent=text;input.text=Session.SelectedName();
   Label("최대 12자 · 글자, 숫자, 공백, -와 _\n무료 · 변경 횟수 제한 없음",0,425,420,70,17,Muted);
   var error=Label("",0,495,420,62,17,new Color(1,.64f,.48f));
   input.onValueChanged.AddListener(_=>error.text="");
   Button("이름 저장",0,580,380,60,()=>{try{if(Session.RenameDragon(instance,input.text,out var message))ShowStatus();else error.text=message;}catch(Exception){error.text="저장하지 못했습니다. 다시 시도해주세요.";}});
   Button("원래 이름으로 되돌리기",0,657,380,50,()=>{try{Session.ResetDragonName(instance);ShowStatus();}catch(Exception){error.text="저장하지 못했습니다. 다시 시도해주세요.";}});
   Button("취소",0,730,380,50,ShowStatus);notice.text="진화해도 이름은 그대로 유지됩니다.";
   input.ActivateInputField();
  }
 }
}
