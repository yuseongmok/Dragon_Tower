using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public enum AncientSurfaceKind { Panel,Button,Primary,Title,Portrait,Information,Collection,Popup,Icon }
 public static class AncientUi {
  public static readonly Color Background=Hex(0x24252A),Stone=Hex(0x393A3F),Raised=Hex(0x414249),Gold=Hex(0xC6A16A),DarkGold=Hex(0x8D785D),Ivory=Hex(0xF3E8D7),Muted=Hex(0xB3ACA0);
  public static Color Hex(int rgb)=>new Color(((rgb>>16)&255)/255f,((rgb>>8)&255)/255f,(rgb&255)/255f,1);
  public static AncientStoneSurface Frame(Image image,AncientSurfaceKind kind=AncientSurfaceKind.Panel){
   foreach(var effect in image.GetComponents<Shadow>())effect.enabled=false;
   var legacy=image.transform.Find("Pixel edge trim");if(legacy!=null)legacy.gameObject.SetActive(false);
   image.color=Color.clear;
   var child=image.transform.Find("Ancient stone surface");AncientStoneSurface art;
   if(child==null){var go=new GameObject("Ancient stone surface",typeof(RectTransform),typeof(AncientStoneSurface));var r=go.GetComponent<RectTransform>();r.SetParent(image.transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;r.SetAsFirstSibling();art=go.GetComponent<AncientStoneSurface>();}else art=child.GetComponent<AncientStoneSurface>();
   art.kind=kind;art.raycastTarget=false;art.SetVerticesDirty();return art;
  }
  public static void Button(Button button,bool primary=false,bool selected=false,bool muted=false){
   var art=Frame((Image)button.targetGraphic,primary?AncientSurfaceKind.Primary:AncientSurfaceKind.Button);art.muted=muted;
   button.transition=Selectable.Transition.None;var feedback=button.GetComponent<AncientButtonFeedback>()??button.gameObject.AddComponent<AncientButtonFeedback>();feedback.Bind(button,art,selected);
   foreach(var text in button.GetComponentsInChildren<Text>()){text.color=Ivory;foreach(var shadow in text.GetComponents<Shadow>()){shadow.effectColor=new Color(0,0,0,.35f);shadow.effectDistance=new Vector2(0,-1);}}
  }
 }
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class AncientStoneSurface:MaskableGraphic {
  public AncientSurfaceKind kind;public bool selected,muted,disabled,pressed;
  static Texture2D atlas;
  public override Texture mainTexture { get { if(atlas==null)atlas=Resources.Load<Texture2D>("UI/ancient-charcoal-atlas");return atlas!=null?atlas:base.mainTexture; } }
  void AtlasMesh(VertexHelper v,Rect r){
   int cell=kind==AncientSurfaceKind.Primary?0:kind==AncientSurfaceKind.Button?1:kind==AncientSurfaceKind.Title?2:kind==AncientSurfaceKind.Collection?(muted?5:4):kind==AncientSurfaceKind.Portrait?7:kind==AncientSurfaceKind.Information?3:6;
   float[] rows={0,.201f,.39f,.658f,1};int row=cell/2;
   float ux=(cell%2)*.5f,uy=1-rows[row+1],uh=rows[row+1]-rows[row];
   if(cell==7){int n=v.currentVertCount;v.AddVert(new Vector3(r.xMin,r.yMin),Color.white,new Vector2(ux,uy));v.AddVert(new Vector3(r.xMin,r.yMax),Color.white,new Vector2(ux,uy+uh));v.AddVert(new Vector3(r.xMax,r.yMax),Color.white,new Vector2(ux+.5f,uy+uh));v.AddVert(new Vector3(r.xMax,r.yMin),Color.white,new Vector2(ux+.5f,uy));v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);return;}
   float bx=Mathf.Min(22,r.width*.15f),by=Mathf.Min(16,r.height*.24f);
   float[] xs={r.xMin,r.xMin+bx,r.xMax-bx,r.xMax};float[] ys={r.yMin,r.yMin+by,r.yMax-by,r.yMax};
   float[] us={ux,ux+.075f,ux+.425f,ux+.5f};float[] vs={uy,uy+.05f,uy+uh-.05f,uy+uh};
   Color tint=disabled?new Color(.46f,.46f,.46f):pressed?new Color(.8f,.8f,.8f):Color.white;
   if(kind==AncientSurfaceKind.Collection&&!selected&&!muted)tint=new Color(.78f,.78f,.78f);
   for(int y=0;y<3;y++)for(int x=0;x<3;x++){int n=v.currentVertCount;v.AddVert(new Vector3(xs[x],ys[y]),tint,new Vector2(us[x],vs[y]));v.AddVert(new Vector3(xs[x],ys[y+1]),tint,new Vector2(us[x],vs[y+1]));v.AddVert(new Vector3(xs[x+1],ys[y+1]),tint,new Vector2(us[x+1],vs[y+1]));v.AddVert(new Vector3(xs[x+1],ys[y]),tint,new Vector2(us[x+1],vs[y]));v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
  }
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();var r=rectTransform.rect;if(mainTexture==atlas&&atlas!=null){AtlasMesh(vh,r);return;}float x=r.xMin,y=r.yMin,w=r.width,h=r.height;bool button=kind==AncientSurfaceKind.Button||kind==AncientSurfaceKind.Primary;
   Color shadow=AncientUi.Hex(0x121316),metal=disabled||muted?AncientUi.Hex(0x55565A):AncientUi.DarkGold,gold=disabled||muted?AncientUi.Hex(0x68696B):AncientUi.Gold;
   Color face=kind==AncientSurfaceKind.Primary?AncientUi.Hex(0x235344):button?AncientUi.Raised:AncientUi.Stone;if(disabled||muted)face=Color.Lerp(face,AncientUi.Background,.62f);
   Cut(vh,x,y-(button?(pressed?2:6):3),w,h,6,shadow);
   Cut(vh,x,y,w,h,6,metal);Cut(vh,x+2,y+2,w-4,h-4,5,AncientUi.Hex(0x1C1D21));
   Cut(vh,x+4,y+4,w-8,h-8,4,face);
   Box(vh,x+9,y+h-6,w-18,2,Color.Lerp(face,AncientUi.Ivory,.25f));
   Box(vh,x+8,y+4,w-16,3,shadow);Box(vh,x+4,y+9,2,h-18,Color.Lerp(face,AncientUi.Ivory,.12f));
   Box(vh,x+w-6,y+9,2,h-18,shadow);
   // Carved inset stays calm beneath text; stone chips stay on the margin.
   if(!button){Cut(vh,x+10,y+10,w-20,h-20,3,kind==AncientSurfaceKind.Portrait?AncientUi.Hex(0x202125):Color.Lerp(face,AncientUi.Background,.48f));Box(vh,x+12,y+h-12,w-24,1,shadow);}
   for(int i=0;i<(int)(w/30);i++){float px=x+13+i*29;Box(vh,px,y+h-8,4+(i%3)*2,1,Color.Lerp(face,AncientUi.Ivory,.13f));if(i%2==0)Box(vh,px+5,y+8,5,1,shadow);}
   if(w>45&&h>30)foreach(float cx in new[]{x+5,x+w-13})foreach(float cy in new[]{y+5,y+h-13}){Box(vh,cx,cy,8,2,metal);Box(vh,cx,cy,2,8,gold);Box(vh,cx+2,cy+5,3,2,Color.Lerp(gold,AncientUi.Ivory,.3f));}
   if(kind==AncientSurfaceKind.Title||kind==AncientSurfaceKind.Primary){Diamond(vh,x+15,y+h*.5f,6,gold);Diamond(vh,x+w-15,y+h*.5f,6,gold);Diamond(vh,x+15,y+h*.5f,3,face);Diamond(vh,x+w-15,y+h*.5f,3,face);}
   if(selected&&!disabled){Box(vh,x+12,y+h-2,w-24,2,gold);Box(vh,x+12,y,w-24,2,gold);Diamond(vh,x+w-13,y+h-13,4,AncientUi.Ivory);}
  }
  static void Cut(VertexHelper v,float x,float y,float w,float h,float c,Color color){if(w<=0||h<=0)return;Box(v,x+c,y,w-2*c,h,color);Box(v,x+2,y+3,w-4,h-6,color);Box(v,x,y+c,w,h-2*c,color);}
  static void Diamond(VertexHelper v,float x,float y,int radius,Color color){for(int i=-radius;i<=radius;i+=2){int half=radius-Mathf.Abs(i);Box(v,x-half,y+i,Mathf.Max(2,half*2),2,color);}}
  static void Box(VertexHelper v,float x,float y,float w,float h,Color color){if(w<=0||h<=0)return;int n=v.currentVertCount;v.AddVert(new Vector3(x,y),color,Vector2.zero);v.AddVert(new Vector3(x,y+h),color,Vector2.zero);v.AddVert(new Vector3(x+w,y+h),color,Vector2.zero);v.AddVert(new Vector3(x+w,y),color,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
 }
}
