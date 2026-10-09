using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  void DragonStatsRow(DragonData d,float y){
   StatChip("체력",d.maxHP.ToString(),2,-104,y);StatChip("공격력",d.attackDamage.ToString(),0,104,y);
  }
  void StatChip(string title,string value,int icon,float x,float y){
   var panel=Panel(title,body,x,y,198,48,DragonTowerTheme.Slate,DragonTowerTheme.GoldDim);
   CardIcon(panel.transform,icon,-72,24,28);
   CardText(panel.transform,"Stat title",title,-22,24,65,32,16,Muted,TextAnchor.MiddleLeft);
   CardText(panel.transform,"Stat value",value,61,24,58,32,23,Color.white,TextAnchor.MiddleRight);
  }
  void CardIcon(Transform parent,int icon,float x,float y,float size){var art=Rect("Info symbol",parent,x,y,size,size).gameObject.AddComponent<Image>();art.sprite=RewardIcon(icon);art.preserveAspect=true;art.raycastTarget=false;}
  void DragonInfoCard(string category,string title,string detail,int icon,float y,float height){
   var p=Panel(category,body,0,y,410,height,DragonTowerTheme.Slate,DragonTowerTheme.GoldDim);
   CardIcon(p.transform,icon,-174,26,30);
   CardText(p.transform,"Category",category,-89,19,120,22,13,Muted,TextAnchor.MiddleLeft);
   if(!string.IsNullOrEmpty(title))CardText(p.transform,"Title",title,64,19,202,24,18,Gold,TextAnchor.MiddleRight);
   var t=CardText(p.transform,"Description",detail,18,39+(height-45)*.5f,344,height-43,17,Color.white,TextAnchor.UpperLeft);
   t.resizeTextForBestFit=true;t.resizeTextMinSize=14;t.resizeTextMaxSize=17;t.lineSpacing=1.1f;
  }
 }
}
