using UnityEngine;
using UnityEngine.UI;

namespace DragonTower
{
    public enum BondHeartState { Normal, Corrupted, Recovering }

    // Presentation only. The caller owns placement behind actors and HUD.
    // Deliberately has no combat, progression, save, or ending dependencies.
    public sealed class PrimordialThroneEnvironment : MonoBehaviour
    {
        public BondHeartState State { get; private set; }
        public float TransitionProgress { get; private set; } = 1;
        public int ActiveGlyphCount => Mathf.RoundToInt(glyphs);
        public float Corruption => corruption;
        Image heart;
        ThroneEnvironmentGraphic accents;
        float elapsed, duration, corruption, glyphs=9, startCorruption, startGlyphs;
        static readonly Color Warm = new Color(.91f,.80f,.56f,1);
        static readonly Color Dark = new Color(.25f,.20f,.37f,1);

        public static PrimordialThroneEnvironment Create(RectTransform backgroundLayer, Sprite heartSprite)
        {
            var go=new GameObject("Primordial throne environment",typeof(RectTransform));
            var rect=(RectTransform)go.transform;rect.SetParent(backgroundLayer,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var result=go.AddComponent<PrimordialThroneEnvironment>();
            var h=new GameObject("Bond heart",typeof(RectTransform),typeof(Image));
            var hr=(RectTransform)h.transform;hr.SetParent(rect,false);hr.anchorMin=hr.anchorMax=new Vector2(.5f,1);
            hr.anchoredPosition=new Vector2(0,-180);hr.sizeDelta=new Vector2(108,108);
            result.heart=h.GetComponent<Image>();result.heart.sprite=heartSprite;result.heart.preserveAspect=true;result.heart.raycastTarget=false;
            var a=new GameObject("Nine region seals and ambient motes",typeof(RectTransform),typeof(ThroneEnvironmentGraphic));
            var ar=(RectTransform)a.transform;ar.SetParent(rect,false);ar.anchorMin=Vector2.zero;ar.anchorMax=Vector2.one;ar.offsetMin=ar.offsetMax=Vector2.zero;
            result.accents=a.GetComponent<ThroneEnvironmentGraphic>();result.accents.raycastTarget=false;
            result.SetStateImmediate(BondHeartState.Normal);return result;
        }
        public void SetStateImmediate(BondHeartState state)
        {
            State=state;elapsed=duration=0;TransitionProgress=1;
            corruption=state==BondHeartState.Corrupted?1:0;glyphs=state==BondHeartState.Corrupted?3:9;
            Apply();
        }
        public void TransitionTo(BondHeartState state,float seconds=2.4f)
        {
            if(float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<=0){SetStateImmediate(state);return;}
            State=state;startCorruption=corruption;startGlyphs=glyphs;elapsed=0;duration=Mathf.Clamp(seconds,.1f,10);TransitionProgress=0;
        }
        void Update()
        {
            if(duration>0)
            {
                elapsed+=Time.unscaledDeltaTime;TransitionProgress=Mathf.Clamp01(elapsed/duration);
                float t=Mathf.SmoothStep(0,1,TransitionProgress);
                corruption=Mathf.Lerp(startCorruption,State==BondHeartState.Corrupted?1:0,t);
                glyphs=Mathf.Lerp(startGlyphs,State==BondHeartState.Corrupted?3:9,t);
                if(TransitionProgress>=1)duration=0;
            }
            Apply();
        }
        void Apply()
        {
            if(heart==null||accents==null)return;
            float pulse=.94f+.035f*Mathf.Sin(Time.unscaledTime*1.5f);
            heart.color=Color.Lerp(Warm,Dark,corruption)*new Color(pulse,pulse,pulse,1);
            accents.corruption=corruption;accents.activeGlyphs=ActiveGlyphCount;accents.age=Time.unscaledTime;
            accents.SetVerticesDirty();
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ThroneEnvironmentGraphic : MaskableGraphic
    {
        public float corruption,age;
        public int activeGlyphs=9;
        static readonly string[] Seals = {
            "11111/10101/10101/10101/11111", // prison bars
            "10001/01010/00100/01010/10001", // aqueduct channels
            "00100/01100/01110/11111/01110", // forge flame
            "00000/11110/00001/01110/00000", // sky wind
            "00110/01100/11110/00100/01000", // thunder
            "10101/01110/11111/01110/10101", // ice
            "11011/10101/10101/10101/11011", // book
            "00100/01110/11111/01110/00100", // sunlight
            "00100/01000/00100/00010/00100"  // void rift
        };
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();var r=rectTransform.rect;float sx=r.width/480,sy=r.height/850;
            for(int i=0;i<9;i++)
            {
                float angle=(i*40+10)*Mathf.Deg2Rad;
                float x=Mathf.Round(Mathf.Cos(angle)*215/2)*2,y=-62+Mathf.Round(Mathf.Sin(angle)*133/2)*2;
                var c=i<activeGlyphs?new Color(.78f,.66f,.42f,.56f):new Color(.24f,.25f,.32f,.65f);
                var pattern=Seals[i];int px=0,py=0;
                foreach(char ch in pattern){if(ch=='/'){px=0;py++;continue;}if(ch=='1')Quad(v,(x+px*2-5)*sx,(y-py*2+5)*sy,2*sx,2*sy,c);px++;}
            }
            // Eight slow edge motes; never a transparent fullscreen overlay.
            for(int i=0;i<8;i++)
            {
                float x=(i%2==0?-1:1)*(154+i%3*10),y=35+Mathf.Repeat(i*37+age*8,230);
                Quad(v,x*sx,Mathf.Round(y/2)*2*sy,2*sx,3*sy,new Color(.78f,.66f,.42f,.3f*(1-corruption)));
            }
            if(corruption>.001f)
            {
                var c=new Color(.20f,.13f,.31f,.85f*corruption);
                for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)
                {
                    float x=side*(35+(i%2)*5),y=209+i*13;
                    Quad(v,x*sx,y*sy,3*sx,14*sy,c);Quad(v,(x-4)*sx,(y+11)*sy,7*sx,3*sy,c);
                }
            }
        }
        static void Quad(VertexHelper v,float x,float y,float w,float h,Color c)
        {
            int n=v.currentVertCount;v.AddVert(new Vector3(x,y),c,Vector2.zero);v.AddVert(new Vector3(x+w,y),c,Vector2.zero);
            v.AddVert(new Vector3(x+w,y+h),c,Vector2.zero);v.AddVert(new Vector3(x,y+h),c,Vector2.zero);
            v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
        }
    }
}
