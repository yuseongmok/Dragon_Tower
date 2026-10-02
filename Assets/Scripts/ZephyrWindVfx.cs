using UnityEngine;
using UnityEngine.UI;

namespace DragonTower
{
    // Fixed pools and UI geometry: no world-space lighting, textures or per-cast allocations.
    public sealed class ZephyrWindVfx : MonoBehaviour
    {
        const int Count=10, GhostCount=3;
        sealed class Effect { public PixelWindGraphic graphic;public Vector2 origin;public float age,delay,life;public bool active; }
        readonly Effect[] effects=new Effect[Count];
        readonly Image[] ghosts=new Image[GhostCount];
        float ghostAge=10;int cursor;
        RectTransform effectRoot,hitRoot;
        public int Capacity=>Count+GhostCount;
        public int ActiveCount { get { int n=ghostAge<.3f?GhostCount:0;foreach(var e in effects)if(e!=null&&e.active)n++;return n; } }
        public void Initialize(RectTransform parent,RectTransform actor)
        {
            var root=new GameObject("Zephyr pixel wind",typeof(RectTransform),typeof(RectMask2D));
            var r=root.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-170);r.sizeDelta=new Vector2(480,455);
            effectRoot=r;
            var foreground=new GameObject("Zephyr hit foreground",typeof(RectTransform),typeof(RectMask2D));
            hitRoot=foreground.GetComponent<RectTransform>();hitRoot.SetParent(parent.parent,false);hitRoot.anchorMin=hitRoot.anchorMax=new Vector2(.5f,1);hitRoot.pivot=new Vector2(.5f,1);hitRoot.anchoredPosition=new Vector2(0,-170);hitRoot.sizeDelta=new Vector2(480,455);hitRoot.SetSiblingIndex(actor.GetSiblingIndex()+1);
            for(int i=0;i<Count;i++)
            {
                var go=new GameObject("Wind "+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(PixelWindGraphic));
                var graphic=go.GetComponent<PixelWindGraphic>();Setup(graphic.rectTransform,r);graphic.raycastTarget=false;go.SetActive(false);effects[i]=new Effect{graphic=graphic};
            }
            for(int i=0;i<GhostCount;i++)
            {
                var go=new GameObject("Zephyr afterimage "+i,typeof(RectTransform),typeof(Image));
                var image=go.GetComponent<Image>();Setup(image.rectTransform,r);image.preserveAspect=true;image.raycastTarget=false;go.SetActive(false);ghosts[i]=image;
            }
        }
        static void Setup(RectTransform r,Transform parent){r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,.5f);}
        public void Clear(){cursor=0;ghostAge=10;foreach(var e in effects){e.active=false;e.graphic.gameObject.SetActive(false);}foreach(var g in ghosts)g.gameObject.SetActive(false);}
        void Emit(int kind,Vector2 point,float size,float delay,float life)
        {
            var e=effects[cursor++%Count];e.origin=point+new Vector2(0,170);e.age=0;e.delay=delay;e.life=life;e.active=true;
            e.graphic.rectTransform.SetParent(kind==2?hitRoot:effectRoot,false);
            e.graphic.kind=kind;e.graphic.phase=0;e.graphic.rectTransform.sizeDelta=Vector2.one*size;e.graphic.rectTransform.anchoredPosition=e.origin;e.graphic.color=Color.white;e.graphic.gameObject.SetActive(false);
        }
        public void Attack(Vector2 at){Emit(0,at,148,.06f,.22f);}
        public void Skill(Vector2 at){Emit(1,at+new Vector2(0,-4),196,.04f,.56f);Emit(0,at,154,.18f,.24f);}
        public void SkillHit(Vector2 at){Emit(0,at,126,0,.22f);}
        public void Hit(Vector2 at){Emit(2,at,122,0,.23f);}
        public void Dodge(Image source,RectTransform actor,int dir)
        {
            ghostAge=0;
            for(int i=0;i<GhostCount;i++)
            {
                var g=ghosts[i];g.sprite=source==null?null:source.sprite;
                var rect=g.rectTransform;rect.sizeDelta=source==null?actor.rect.size:source.rectTransform.rect.size;
                rect.localScale=source==null?actor.localScale:Vector3.Scale(actor.localScale,source.rectTransform.localScale);
                rect.localRotation=actor.localRotation;
                rect.anchoredPosition=actor.anchoredPosition+(source==null?Vector2.zero:source.rectTransform.anchoredPosition)+new Vector2(-dir*i*14,170);
                g.color=new Color(.32f,1,.77f,.3f-i*.06f);g.gameObject.SetActive(g.sprite!=null);
            }
        }
        public void Step(float dt)
        {
            foreach(var e in effects)
            {
                if(!e.active)continue;e.age+=dt;
                if(e.age<e.delay)continue;
                float t=(e.age-e.delay)/e.life;
                if(t>=1){e.active=false;e.graphic.gameObject.SetActive(false);continue;}
                e.graphic.gameObject.SetActive(true);e.graphic.phase=Mathf.Floor(t*8)/8;
                e.graphic.color=new Color(1,1,1,Mathf.Min(1,(1-t)*3));
                e.graphic.rectTransform.anchoredPosition=e.origin+new Vector2(0,e.graphic.kind==1?Mathf.Round(t*12/4)*4:0);e.graphic.SetVerticesDirty();
            }
            if(ghostAge<.3f)
            {
                ghostAge+=dt;
                for(int i=0;i<GhostCount;i++){var g=ghosts[i];var c=g.color;c.a=Mathf.Max(0,(.30f-i*.06f)*(1-ghostAge/.3f));g.color=c;if(ghostAge>=.3f)g.gameObject.SetActive(false);}
            }
        }
    }

    // Quantized 40x40 geometry makes the effect's pixel scale stable on the portrait canvas.
    public sealed class PixelWindGraphic : MaskableGraphic
    {
        public int kind;public float phase;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float cell=r.width/40f;
            for(int y=0;y<40;y++)for(int x=0;x<40;x++)
            {
                float px=(x-19.5f)/20,py=(y-19.5f)/20;float strength=0;
                if(kind==0)
                {
                    float angle=Mathf.Atan2(py,px);float radius=Mathf.Sqrt(px*px+py*py);
                    float sweep=Mathf.DeltaAngle(angle*Mathf.Rad2Deg,35+phase*95);
                    float outer=.82f-phase*.09f;
                    if(sweep>-135&&sweep<100&&radius<outer&&radius>outer-(.09f+.12f*(1-Mathf.Abs(sweep)/150)))strength=radius>outer-.05f?1:.65f;
                    if(sweep>-95&&sweep<65&&radius>.48f&&radius<.54f)strength=.45f;
                }
                else if(kind==1)
                {
                    for(int band=0;band<3;band++)
                    {
                        float cy=-.48f+band*.42f,width=.36f+band*.19f;
                        float radius=Mathf.Sqrt(px*px/(width*width)+(py-cy)*(py-cy)/.038f);
                        float angle=Mathf.Atan2((py-cy)/.195f,px/width)*Mathf.Rad2Deg;
                        float gap=Mathf.DeltaAngle(angle,phase*360+band*105);
                        if(radius>.73f&&radius<1&&Mathf.Abs(gap)>32)strength=Mathf.Max(strength,py>cy?.95f:.55f);
                    }
                }
                else
                {
                    float radial=Mathf.Sqrt(px*px+py*py),angle=Mathf.Atan2(py,px);
                    float spoke=Mathf.Abs(Mathf.Sin(angle*4));
                    if(radial>.18f+phase*.22f&&radial<.40f+phase*.46f&&spoke<.23f)strength=1;
                    if(phase<.3f&&Mathf.Abs(px)+Mathf.Abs(py)<.23f)strength=.8f;
                }
                if(strength<=0)continue;
                var tint=strength>.85f?new Color(.87f,1,.83f):strength>.6f?new Color(.32f,.97f,.72f):new Color(.10f,.60f,.59f);
                if(kind==2)tint=Color.Lerp(new Color(1,.77f,.43f),tint,.45f);
                tint.a=color.a;int start=vh.currentVertCount;float left=r.xMin+x*cell,bottom=r.yMin+y*cell;
                vh.AddVert(new Vector3(left,bottom),tint,Vector2.zero);vh.AddVert(new Vector3(left,bottom+cell),tint,Vector2.zero);
                vh.AddVert(new Vector3(left+cell,bottom+cell),tint,Vector2.zero);vh.AddVert(new Vector3(left+cell,bottom),tint,Vector2.zero);
                vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
            }
        }
    }
}
