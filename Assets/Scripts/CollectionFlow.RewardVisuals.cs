using UnityEngine;using UnityEngine.UI;
namespace DragonTower {public sealed partial class CollectionFlow {
 void RewardAccent(Button b,Color accent){var r=Rect("Rarity accent",b.transform,-b.GetComponent<RectTransform>().rect.width*.5f+8,b.GetComponent<RectTransform>().rect.height*.5f,3,b.GetComponent<RectTransform>().rect.height-28);var im=r.gameObject.AddComponent<Image>();im.color=accent;im.raycastTarget=false;}
 string CompactInventory(){if(towerRun==null)return "";string s="장착 · "+(towerRun.ItemSlots.Count==0?"없음":"");for(int i=0;i<towerRun.ItemSlots.Count;i++)s+=(i>0?" / ":"")+towerRun.ItemSlots[i].DisplayName;var c=towerRun.ItemAt(3);return s+"\n소모품 · "+(c==null?"없음":c.displayName+" ×"+towerRun.ItemCountAt(3));}
}}
