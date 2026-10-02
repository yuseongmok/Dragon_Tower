using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Presentation-only samples. No BattleModel, SkillData, cues, damage or status writes.
    public sealed class IceStylePrototype : MonoBehaviour
    {
        public IceVfxStyle style;
        const int PoolSize=28;
        Image[] pool;RectTransform root;Vector2 source,center,feet;float age=10;
        public int VisualIntensity {get;set;}=1; // Preview art parameter, NOT a gameplay stack.
        public int Capacity=>pool==null?0:pool.Length;
        public int ActiveCount {get{int n=0;if(pool!=null)foreach(var p in pool)if(p.gameObject.activeSelf)n++;return n;}}
        public bool Active=>age<2.8f;
        public void Initialize(RectTransform frame)
        {
            if(pool!=null)return;
            var go=new GameObject("Ice style safe area",typeof(RectTransform),typeof(RectMask2D));root=go.GetComponent<RectTransform>();root.SetParent(frame,false);
            root.anchorMin=root.anchorMax=Vector2.one*.5f;root.pivot=Vector2.one*.5f;root.anchoredPosition=new Vector2(0,32);root.sizeDelta=new Vector2(448,510);
            pool=new Image[PoolSize];
            for(int i=0;i<PoolSize;i++)
            {
                var child=new GameObject("Ice module "+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));var image=child.GetComponent<Image>();image.rectTransform.SetParent(root,false);image.raycastTarget=false;image.maskable=true;pool[i]=image;child.SetActive(false);
            }
        }
        public void Play(Vector2 castingOrigin,Vector2 crystalCenter,Vector2 enemyFeet)
        {Clear();source=castingOrigin;center=crystalCenter;feet=enemyFeet;age=0;Render();}
        public void Step(float delta){if(!Active)return;age+=Mathf.Max(0,delta);if(age>=2.8f){Clear();return;}Render();}
        public void Clear(){age=10;if(pool!=null)foreach(var p in pool)if(p!=null)p.gameObject.SetActive(false);}
        void OnDisable()=>Clear();
        void OnDestroy(){if(root!=null)Destroy(root.gameObject);}
        void Draw(int slot,IceStyleModule module,Vector2 point,Vector2 size,float alpha=1,float angle=0,float reveal=1)
        {
            var image=pool[slot];image.sprite=style.Get(module);image.color=new Color(1,1,1,Mathf.Clamp01(alpha));
            image.type=reveal<1?Image.Type.Filled:Image.Type.Simple;image.fillMethod=Image.FillMethod.Vertical;image.fillOrigin=0;image.fillAmount=Mathf.Clamp01(reveal);
            var r=image.rectTransform;r.anchorMin=r.anchorMax=Vector2.one*.5f;r.pivot=Vector2.one*.5f;
            var p=point-root.anchoredPosition;r.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round(p.y/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);image.gameObject.SetActive(true);
        }
        void Render()
        {
            foreach(var p in pool)p.gameObject.SetActive(false);
            if(age<.12f)Draw(0,IceStyleModule.Highlight,center-new Vector2(0,42),Vector2.one*18,1-age/.12f);
            float growth=Mathf.Clamp01((age-.10f)/style.growthDuration);
            if(age>=.10f&&age<.82f)
            {
                // Different painted faces, never copies of a tinted full silhouette.
                Draw(0,IceStyleModule.Interior,center,new Vector2(128,128),1,0,Mathf.Lerp(.12f,1,growth));
                Draw(1,IceStyleModule.Body,center,new Vector2(128,128),.92f,0,Mathf.Clamp01(growth*1.15f-.08f));
                Draw(2,IceStyleModule.Edge,center,new Vector2(128,128),1,0,Mathf.Clamp01(growth*1.15f-.16f));
                if(age>=.37f&&age<.58f)Draw(3,IceStyleModule.Refraction,center+new Vector2(0,Mathf.Lerp(-14,18,(age-.37f)/.21f)),new Vector2(80,80),Mathf.Sin((age-.37f)/.21f*Mathf.PI)*.8f);
            }
            // Tiny anchor sample only: no projectile, no skill motion or skill call.
            if(age<.48f)
            {
                Draw(4,IceStyleModule.StatusCrystal,source,new Vector2(42,42),Mathf.Clamp01(age/.10f)*(1-Mathf.Clamp01((age-.35f)/.13f)));
                Draw(5,IceStyleModule.FrostA,source-new Vector2(4,10),new Vector2(32,20),.5f);
            }
            if(age>=.68f&&age<.76f)Draw(6,IceStyleModule.Impact,center,new Vector2(48,48),1-(age-.68f)/.08f);
            if(age>=.72f&&age<.72f+style.crackDuration)
            {
                float t=(age-.72f)/style.crackDuration;Draw(7,(IceStyleModule)((int)IceStyleModule.CrackA+Mathf.Min(2,(int)(t*3))),center,new Vector2(100,112));
            }
            if(age>=.82f&&age<.82f+style.shatterDuration)
            {
                float t=(age-.82f)/style.shatterDuration;
                for(int i=0;i<Mathf.Min(15,style.shardCount);i++)
                {
                    float a=(i*137.5f+18)*Mathf.Deg2Rad;var direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                    var at=center+direction*(12+t*(38+(i%4)*11))+new Vector2(0,-t*t*25);
                    float size=i<3?34:i<7?24:14;var module=i<3?IceStyleModule.ShardLarge:i<7?IceStyleModule.ShardMedium:IceStyleModule.ShardSmall;
                    float angle=Mathf.Round((i*39+t*(i%2==0?90:-100))/15)*15;Draw(8+i,module,at,Vector2.one*size,1-t*t,angle);
                }
            }
            if(age>=1.12f)
            {
                float fade=Mathf.Clamp01((age-1.12f)/.18f)*Mathf.Clamp01((2.8f-age)/.35f);int intensity=Mathf.Clamp(VisualIntensity,1,5);
                Draw(23,IceStyleModule.FrostRing,feet,new Vector2(94,30),fade*(.55f+.035f*intensity));
                for(int i=0;i<3;i++)
                {
                    float phase=Mathf.Floor(age*12)/12;var p=feet+new Vector2(-36+i*34,7+Mathf.Sin(phase*3+i)*3);
                    Draw(24+i,i%2==0?IceStyleModule.FrostA:IceStyleModule.FrostB,p,new Vector2(30,18),fade*.45f);
                }
                Draw(27,IceStyleModule.StatusCrystal,feet+new Vector2(-44,12),Vector2.one*(16+intensity*2),fade*.8f);
            }
        }
    }
}
