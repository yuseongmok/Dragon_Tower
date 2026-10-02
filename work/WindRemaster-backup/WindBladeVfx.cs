using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public sealed class WindBladeVfx : MonoBehaviour
    {
        RectTransform visualRoot;Image[] images;Sprite blade;WindSkillRecipe shared;SkillData skill;
        Vector2 from,to;float age=10;int hits;readonly float[] hitAge={10,10,10};
        public bool Configured=>skill!=null&&skill.StableId=="skill_falling_flower";
        public bool Active=>Configured&&age<skill.initialHitDelay+skill.hitInterval*2+.22f;
        public int Capacity=>17;
        public int HitsShown=>hits;
        public bool FinalFreeze=>Active&&hits>=3&&age-hitAge[2]>=0&&age-hitAge[2]<.033f;
        public void Initialize(RectTransform parent)
        {
            blade=Resources.Load<Sprite>("VFX/WindBlade");shared=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");
            var go=new GameObject("Wind Blade modules (pooled)",typeof(RectTransform),typeof(RectMask2D));var root=go.GetComponent<RectTransform>();visualRoot=root;root.SetParent(parent,false);root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-132);root.sizeDelta=new Vector2(480,518);
            images=new Image[17];for(int i=0;i<images.Length;i++){var obj=new GameObject(i<3?"WindBladeProjectile":i<6?"WindBladeTrail":i<9?"WindSlashImpact":"WindPixelDebris",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));var im=obj.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;images[i]=im;obj.SetActive(false);}
            var edge=images[2].gameObject.AddComponent<Outline>();edge.effectColor=new Color(.72f,1,.94f,.65f);edge.effectDistance=new Vector2(2,-2);
        }
        public void Configure(SkillData value){skill=value;Clear();}
        public void Clear(){if(visualRoot!=null)visualRoot.anchoredPosition=new Vector2(0,-132);age=10;hits=0;for(int i=0;i<3;i++)hitAge[i]=10;if(images!=null)foreach(var im in images)im.gameObject.SetActive(false);}
        void OnDisable(){Clear();}
        public void Cast(Vector2 source,Vector2 target){Clear();from=source+new Vector2(24,38);to=target;age=0;Render();}
        public void Hit(){if(!Configured)return;hitAge[hits%3]=age;hits++;Render();}
        public void Step(float delta){if(!Active){if(images!=null)foreach(var im in images)im.gameObject.SetActive(false);if(visualRoot!=null)visualRoot.anchoredPosition=new Vector2(0,-132);return;}age+=delta;Render();}
        void Show(int index,Sprite sprite,Vector2 point,Vector2 size,Color color,float angle=0)
        {var im=images[index];im.sprite=sprite;im.color=color;im.rectTransform.anchoredPosition=new Vector2(Mathf.Round(point.x/2)*2,Mathf.Round((point.y+132)/2)*2);im.rectTransform.sizeDelta=size;im.rectTransform.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);}
        void Render()
        {
            if(images==null)return;foreach(var im in images)im.gameObject.SetActive(false);if(!Active)return;
            float finalAge=age-hitAge[2];
            visualRoot.anchoredPosition=new Vector2(finalAge>=0&&finalAge<.1f?Mathf.Round(Mathf.Sin(finalAge*120)*2*(1-finalAge/.1f)):0,-132);
            float flight=Mathf.Min(.08f,skill.initialHitDelay*.5f),gather=Mathf.Max(0,skill.initialHitDelay-flight);
            for(int i=0;i<3;i++)
            {
                float t=(age-gather-i*skill.hitInterval)/Mathf.Max(.01f,flight);
                Vector2 offset=new Vector2((i-1)*52,(1-i)*32),start=from+offset,end=to+new Vector2((i-1)*12,(1-i)*14);
                if(t>=0&&t<1)
                {
                    Vector2 p=Vector2.Lerp(start,end,t)+new Vector2((i-1)*24*Mathf.Sin(t*Mathf.PI),0);float angle=-Mathf.Atan2((end-start).x,(end-start).y)*Mathf.Rad2Deg;
                    Show(i+3,blade,Vector2.Lerp(start,end,Mathf.Max(0,t-.24f)),new Vector2(186,234)*(i==2?1.12f:1),new Color(.24f,.83f,.77f,.62f),angle);
                    Show(i,blade,p,new Vector2(204,252)*(i==2?1.12f:1),Color.white,angle);
                }
                float h=age-hitAge[i],life=i==2?.18f:.12f;
                if(i==2&&h>=0)h=Mathf.Max(0,h-.033f);
                if(h>=0&&h<life)Show(i+6,shared.Frame(WindModule.WindHitSpark,h/life),end,Vector2.one*(i==2?220:90+i*18),new Color(1,1,1,1-h/life));
            }
            for(int i=0;i<8;i++)
            {
                float angle=i*2.39996f;var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                if(age<gather)Show(i+9,null,from+direction*(38*(1-age/Mathf.Max(.01f,gather))),Vector2.one*3,new Color(.35f,.97f,.83f));
                float final=age-hitAge[2];if(final>=0&&final<.16f)Show(i+9,null,to+direction*(8+final*250),Vector2.one*(i%2==0?6:4),new Color(.6f,1,.9f,1-final/.16f));
            }
        }
    }
}


