using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Menu-only palette. Element HUD themes and the approved lobby remain independent.
 public static class GoldenUi {
  public static readonly Color Ivory=AncientUi.Hex(0xFFF0CF),Gold=AncientUi.Hex(0xF6B847),Brown=AncientUi.Hex(0xA66B3C),Dark=AncientUi.Hex(0x3B2119),Stone=AncientUi.Hex(0x57463C),Muted=AncientUi.Hex(0xD6B794),Danger=AncientUi.Hex(0xA75A4D);
  public static Color TextColor(Color c){if(c==Color.white||c==AncientUi.Ivory)return Ivory;if(c==AncientUi.Gold||c==DragonTowerTheme.Gold)return Gold;if(c==AncientUi.Muted)return Muted;return c;}
  public static bool Primary(string n)=>n=="계속"||n=="계속하기"||n=="확인"||n=="저장"||n=="이름 저장"||n=="이 선택으로 성장"||n=="장착"||n.Contains("다음 층");
  public static bool Destructive(string n)=>n.Contains("초기화")||n.Contains("삭제");
 }
}
