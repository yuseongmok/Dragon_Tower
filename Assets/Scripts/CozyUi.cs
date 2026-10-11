using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace DragonTower {
 public enum CozySurfaceKind { Primary, Secondary, Information, Selection, Popup, Text }
 // Opt-in only. Never replaces DragonTowerTheme or AncientUi globally.
 public static class CozyUi {
  public static readonly Color Charcoal=AncientUi.Hex(0x24262B),Stone=AncientUi.Hex(0x35383D),Raised=AncientUi.Hex(0x484B50),Gold=AncientUi.Hex(0xC5A574),Ivory=AncientUi.Hex(0xF1E9DC),Green=AncientUi.Hex(0x326B53);
  public static CozySurface Apply(Image image,CozySurfaceKind kind){
   foreach(var effect in image.GetComponents<Shadow>())effect.enabled=false;
   foreach(Transform child in image.transform)if(child.name=="Ancient stone surface"||child.name=="Pixel edge trim")child.gameObject.SetActive(false);
   image.color=Color.clear;var go=new GameObject("Cozy pixel surface",typeof(RectTransform),typeof(CanvasRenderer),typeof(CozySurface));var r=go.GetComponent<RectTransform>();r.SetParent(image.transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;r.SetAsFirstSibling();var art=go.GetComponent<CozySurface>();art.kind=kind;art.raycastTarget=false;return art;
  }
  public static void Style(Button b,CozySurfaceKind kind){var old=b.GetComponent<AncientButtonFeedback>();if(old!=null){old.enabled=false;Object.Destroy(old);}var art=Apply((Image)b.targetGraphic,kind);b.transition=Selectable.Transition.None;var feedback=b.gameObject.AddComponent<CozyButtonFeedback>();feedback.Bind(b,art);foreach(var t in b.GetComponentsInChildren<Text>()){t.color=Ivory;foreach(var s in t.GetComponents<Shadow>()){s.effectColor=new Color(0,0,0,.45f);s.effectDistance=new Vector2(0,-1);}}}
 }
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class CozySurface:MaskableGraphic {
  public CozySurfaceKind kind;public bool pressed,selected,disabled;
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();if(kind==CozySurfaceKind.Text)return;var r=rectTransform.rect;float x=Mathf.Round(r.xMin),y=Mathf.Round(r.yMin),w=Mathf.Round(r.width),h=Mathf.Round(r.height);bool button=kind==CozySurfaceKind.Primary||kind==CozySurfaceKind.Secondary;Color face=kind==CozySurfaceKind.Primary?CozyUi.Green:button?CozyUi.Raised:kind==CozySurfaceKind.Selection?CozyUi.Stone:CozyUi.Charcoal;if(disabled)face=Color.Lerp(face,CozyUi.Charcoal,.65f);if(pressed)face=Color.Lerp(face,Color.black,.16f);if(kind==CozySurfaceKind.Information)face.a=.88f;
   if(kind==CozySurfaceKind.Primary){Primary(v,x,y,w,h);return;}
   if(kind==CozySurfaceKind.Secondary){Secondary(v,x,y,w,h);return;}
   Cut(v,x,y-(button&&!pressed?3:1),w,h,new Color(.04f,.05f,.06f,.6f));Cut(v,x,y,w,h,selected?CozyUi.Gold:Color.Lerp(CozyUi.Charcoal,CozyUi.Raised,.7f));Cut(v,x+1,y+1,w-2,h-2,face);
   if(button){Box(v,x+4,y+h-3,w-8,2,Color.Lerp(face,CozyUi.Ivory,.22f));Box(v,x+4,y+1,w-8,2,Color.Lerp(face,Color.black,.25f));}
   if(kind==CozySurfaceKind.Popup)Box(v,x+18,y+h-52,w-36,1,CozyUi.Raised);
   if(!button){Box(v,x+5,y+h-2,w-10,1,Finish(0x5C5A51));Box(v,x+3,y+3,1,h-6,Finish(0x3E4140));Box(v,x+6,y+2,w-12,1,Finish(0x191F20));}
  }
  void Secondary(VertexHelper v,float x,float y,float w,float h){
   Cut(v,x,y-(pressed?1:4),w,h,Finish(0x191E1E));Cut(v,x,y,w,h,Finish(selected?0xC5A574:0x696657));
   Cut(v,x+1,y+1,w-2,h-2,Finish(0x282F2F));Cut(v,x+3,y+4,w-6,h-7,Finish(0x414A48));
   Box(v,x+5,y+h-11,w-10,6,Finish(0x4A5550));Box(v,x+5,y+4,w-10,3,Finish(0x333D3B));
   Box(v,x+4,y+h-3,w-8,1,Finish(0x798071));Box(v,x+4,y+2,w-8,1,Finish(0x202A28));
   for(int i=0;i<(int)(w/31);i++){Box(v,x+9+i*31,y+h-7,4+i%3,1,Finish(0x566059));Box(v,x+12+i*31,y+8,5,1,Finish(0x394340));}
  }
  Color Finish(int hex){var c=AncientUi.Hex(hex);if(disabled)c=Color.Lerp(CozyUi.Charcoal,c,.35f);else if(pressed)c=Color.Lerp(c,Color.black,.18f);return c;}
  void Primary(VertexHelper v,float x,float y,float w,float h){
   // Hand-cut bronze rim and enamelled teal inset. No atlas or stretched corners.
   Cut(v,x,y-(pressed?2:5),w,h,Finish(0x171B1B));
   Cut(v,x,y,w,h,Finish(0x272D2A));
   Cut(v,x+1,y+1,w-2,h-2,Finish(selected?0xC5A574:0x897251));
   Cut(v,x+3,y+3,w-6,h-6,Finish(0x1A3934));
   Cut(v,x+5,y+6,w-10,h-11,Finish(0x285448));
   Box(v,x+6,y+h-17,w-12,10,Finish(0x326353));
   Box(v,x+7,y+7,w-14,9,Finish(0x23493F));
   // Broad, quiet centre for the label; small deterministic grain near the margins.
   for(int i=0;i<(int)(w/17);i++){
    float px=x+12+i*17;
    Box(v,px,y+h-12-(i%3)*3,3+i%4,1,Finish(i%2==0?0x3B6A57:0x2C5A4D));
    Box(v,px+4,y+10+(i%2)*3,4,1,Finish(0x2B5548));
   }
   Box(v,x+7,y+h-5,w-14,1,Finish(0x54816A));
   Box(v,x+6,y+5,w-12,2,Finish(0x142F2B));
   Box(v,x+6,y+h-2,w-12,1,Finish(0xB2986E));
   Box(v,x+6,y+1,w-12,1,Finish(0x5B503D));
   // Small worn bronze corner caps instead of jewels, wings or ornamental scrolls.
   foreach(float cx in new[]{x+3,x+w-11}){
    Box(v,cx,y+h-5,8,2,Finish(0xAA9168));
    Box(v,cx,y+3,8,1,Finish(0x84714F));
    Box(v,cx+2,y+h-7,2,2,Finish(0xC1AC83));
   }
  }
  static void Cut(VertexHelper v,float x,float y,float w,float h,Color c){Box(v,x+2,y,w-4,1,c);Box(v,x+1,y+1,w-2,1,c);Box(v,x,y+2,w,h-4,c);Box(v,x+1,y+h-2,w-2,1,c);Box(v,x+2,y+h-1,w-4,1,c);}
  static void Box(VertexHelper v,float x,float y,float w,float h,Color c){if(w<=0||h<=0)return;int n=v.currentVertCount;v.AddVert(new Vector3(x,y),c,Vector2.zero);v.AddVert(new Vector3(x,y+h),c,Vector2.zero);v.AddVert(new Vector3(x+w,y+h),c,Vector2.zero);v.AddVert(new Vector3(x+w,y),c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
 }
 public sealed class CozyButtonFeedback:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler,ISubmitHandler {
  Button button;CozySurface art;bool down;float release;
  public void Bind(Button b,CozySurface s){button=b;art=s;}
  void Down(bool value){if(down==value)return;down=value;foreach(Transform c in transform)if(c.gameObject.activeSelf)((RectTransform)c).anchoredPosition+=Vector2.down*(value?2:-2);if(art!=null){art.pressed=value;art.SetVerticesDirty();}}
  void LateUpdate(){if(button==null)return;if(!button.IsInteractable()||(release>0&&Time.unscaledTime>=release)){Down(false);release=0;}if(art.disabled==button.IsInteractable()){art.disabled=!button.IsInteractable();art.SetVerticesDirty();}}
  void OnDisable(){Down(false);}
  public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&button.IsInteractable())Down(true);}
  public void OnPointerUp(PointerEventData e){Down(false);}public void OnPointerExit(PointerEventData e){Down(false);}
  public void OnSelect(BaseEventData e){art.selected=true;art.SetVerticesDirty();}public void OnDeselect(BaseEventData e){art.selected=false;art.SetVerticesDirty();}
  public void OnSubmit(BaseEventData e){if(button.IsInteractable()){Down(true);release=Time.unscaledTime+.1f;}}
 }
}
