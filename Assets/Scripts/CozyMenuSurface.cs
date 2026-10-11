using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Keeps existing selection, press, reveal and navigation owners; swaps only their surface.
 public sealed class CozyMenuSurface:MonoBehaviour {
  AncientStoneSurface source;Image surface;Outline highlight;string asset;Button button;
  public Image Surface=>surface;
  public static bool InScope(Transform t){
   for(var p=t;p!=null;p=p.parent)if(p.name=="드래곤 로비"||p.name=="시작 화면")return false;
   if(t.GetComponentInParent<DragonTowerSettings>(true)!=null||t.GetComponentInParent<BattleResultPresentation>(true)!=null||t.GetComponentInParent<AchievementToast>(true)!=null)return true;
   for(;t!=null;t=t.parent)if(t.name=="Collection screens")return true;
   return false;
  }
  void Awake(){source=GetComponent<AncientStoneSurface>();button=GetComponentInParent<Button>();var go=new GameObject("Golden Adventure surface",typeof(RectTransform),typeof(Image));var r=go.GetComponent<RectTransform>();r.SetParent(transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;surface=go.GetComponent<Image>();surface.raycastTarget=false;highlight=go.AddComponent<Outline>();highlight.effectColor=GoldenUi.Gold;highlight.effectDistance=Vector2.one;highlight.enabled=false;Sync();}
  void LateUpdate(){Sync();}
  void Sync(){if(source==null||surface==null)return;source.enabled=false;
   bool card=source.kind==AncientSurfaceKind.Collection||(button!=null&&string.IsNullOrEmpty(button.name));
   bool isButton=source.kind==AncientSurfaceKind.Button||source.kind==AncientSurfaceKind.Primary;
   string next=!card&&(source.kind==AncientSurfaceKind.Primary||(isButton&&button!=null&&GoldenUi.Primary(button.name)))?"primary":!card&&(isButton||source.kind==AncientSurfaceKind.Title)?"secondary":"panel";
   if(next!=asset){asset=next;surface.sprite=Resources.Load<Sprite>((asset=="panel"?"UI/GoldenAdventure/":"UI/LobbyGuide/")+asset);surface.type=Image.Type.Sliced;}
   // Cap corner size on tall cards and dialogs; do not stretch a button's large ornaments across a menu.
   if(surface.sprite!=null){float h=Mathf.Max(1,((RectTransform)transform).rect.height);surface.pixelsPerUnitMultiplier=asset=="panel"?surface.sprite.border.x/(card?12f:18f):surface.sprite.rect.height/Mathf.Min(h,65);}
   Color tint=Color.white;
   if(card&&!source.selected)tint=source.muted?new Color(.40f,.38f,.35f,1):new Color(.81f,.76f,.67f,1);
   if(button!=null&&GoldenUi.Destructive(button.name))tint=new Color(1,.56f,.45f,1);
   if(source.disabled)tint=Color.Lerp(tint,new Color(.4f,.36f,.31f,1),.65f);
   if(source.pressed)tint*=new Color(.8f,.8f,.8f,1);
   surface.color=tint;highlight.enabled=source.selected&&!source.disabled;
  }
 }
}
