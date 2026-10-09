using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DragonTower
{
    public sealed class TowerDoorVisual:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler
    {
        Image left,right,mark;TowerPortalLight light;RectTransform leftLeaf,rightLeaf;
        CanvasGroup crestGroup;Sprite leftSprite,rightSprite;Color accent;bool opening,hover;
        static Image Make(Transform parent,string name,float w,float h)
        {
            var g=new GameObject(name,typeof(RectTransform),typeof(Image));var r=g.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(w,h);var im=g.GetComponent<Image>();im.raycastTarget=false;return im;
        }
        public void Build(Sprite sprite,Sprite icon,Color color,bool boss,TowerRoomKind kind)
        {
            accent=color;float width=boss?178:122,height=boss?240:192;
            var glow=new GameObject("Light beyond door",typeof(RectTransform),typeof(TowerPortalLight));var gr=glow.GetComponent<RectTransform>();gr.SetParent(transform,false);gr.anchorMin=gr.anchorMax=gr.pivot=new Vector2(.5f,.5f);gr.sizeDelta=new Vector2(width*.90f,height*.86f);gr.anchoredPosition=new Vector2(0,-height*.05f);light=glow.GetComponent<TowerPortalLight>();light.raycastTarget=false;light.color=new Color(color.r,color.g,color.b,.10f);
            left=Make(transform,"Left door leaf",width*.5f,height);right=Make(transform,"Right door leaf",width*.5f,height);
            if(sprite!=null)
            {
                var r=sprite.rect;leftSprite=Sprite.Create(sprite.texture,new Rect(r.x,r.y,r.width*.5f,r.height),new Vector2(0,.5f),100);rightSprite=Sprite.Create(sprite.texture,new Rect(r.x+r.width*.5f,r.y,r.width*.5f,r.height),new Vector2(1,.5f),100);left.sprite=leftSprite;right.sprite=rightSprite;
            }
            leftLeaf=left.rectTransform;rightLeaf=right.rectTransform;leftLeaf.pivot=new Vector2(0,.5f);rightLeaf.pivot=new Vector2(1,.5f);leftLeaf.anchoredPosition=new Vector2(-width*.5f,0);rightLeaf.anchoredPosition=new Vector2(width*.5f,0);
            mark=Make(transform,"Room crest",boss?68:48,boss?68:48);mark.sprite=icon;mark.preserveAspect=true;mark.rectTransform.anchoredPosition=new Vector2(0,boss?-25:-14);crestGroup=mark.gameObject.AddComponent<CanvasGroup>();
            if(icon==null){mark.color=Color.clear;var symbol=new GameObject("Room symbol",typeof(RectTransform));var sr=symbol.GetComponent<RectTransform>();sr.SetParent(mark.transform,false);sr.anchorMin=Vector2.zero;sr.anchorMax=Vector2.one;sr.offsetMin=sr.offsetMax=Vector2.zero;Graphic graphic=kind==TowerRoomKind.Nest?(Graphic)symbol.AddComponent<EggGraphic>():symbol.AddComponent<TowerCoinEmblem>();graphic.raycastTarget=false;}
        }
        public void SetOpening(float amount)
        {
            opening=true;leftLeaf.localScale=rightLeaf.localScale=new Vector3(Mathf.Max(.015f,1-amount),1,1);
            light.color=new Color(accent.r,accent.g,accent.b,.12f+amount*.88f);crestGroup.alpha=1-amount;
            left.color=right.color=Color.Lerp(new Color(1,.92f,.72f),Color.white,amount);
        }
        void Update(){if(opening)return;float glow=.12f+Mathf.Sin(Time.unscaledTime*2)*.03f+(hover?.18f:0);light.color=new Color(accent.r,accent.g,accent.b,glow);left.color=right.color=hover?new Color(1,.94f,.78f):Color.white;}
        public void OnPointerEnter(PointerEventData e){hover=true;}
        public void OnPointerExit(PointerEventData e){hover=false;}
        public void OnPointerDown(PointerEventData e){hover=true;}
        void OnDestroy(){if(leftSprite!=null)Destroy(leftSprite);if(rightSprite!=null)Destroy(rightSprite);}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TowerCoinEmblem:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper v){v.Clear();float unit=rectTransform.rect.width/24;int[] widths={5,8,10,11,11,11,11,10,8,5};for(int row=0;row<10;row++){int w=widths[row];Box(v,-w,8-row*2,w*2,2,AncientUi.Hex(0x392719),unit);if(row>0&&row<9){Box(v,-w+1,8-row*2,w*2-2,2,AncientUi.Hex(0xCD872D),unit);Box(v,-w+2,8-row*2,w,2,AncientUi.Hex(0xF8D37A),unit);}}Box(v,-1,-7,3,14,AncientUi.Hex(0x9B581B),unit);Box(v,-3,5,7,2,AncientUi.Hex(0xFFF0AA),unit);Box(v,-3,-7,7,2,AncientUi.Hex(0xFFF0AA),unit);}
        static void Box(VertexHelper v,float x,float y,float w,float h,Color c,float s){int n=v.currentVertCount;v.AddVert(new Vector3(x*s,y*s),c,Vector2.zero);v.AddVert(new Vector3(x*s,(y+h)*s),c,Vector2.zero);v.AddVert(new Vector3((x+w)*s,(y+h)*s),c,Vector2.zero);v.AddVert(new Vector3((x+w)*s,y*s),c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TowerPortalLight:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();var r=rectTransform.rect;
            // A soft volume inside the arch, rather than a flat luminous rectangle.
            for(int layer=0;layer<3;layer++)
            {
                float scale=1-layer*.28f;int center=v.currentVertCount;
                Color core=Color.Lerp(color,new Color(1,.96f,.83f,color.a),layer*.3f);core.a=color.a*(.65f-layer*.1f);
                v.AddVert(new Vector3(0,-r.height*.1f),core,Vector2.zero);
                for(int i=0;i<=40;i++)
                {
                    float a=i*Mathf.PI/20;Color edge=core;edge.a=0;
                    v.AddVert(new Vector3(Mathf.Cos(a)*r.width*.5f*scale,Mathf.Sin(a)*r.height*.5f*scale),edge,Vector2.zero);
                    if(i>0)v.AddTriangle(center,center+i,center+i+1);
                }
            }
        }
    }
}
