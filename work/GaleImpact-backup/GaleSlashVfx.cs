using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Presentation only: Hit is driven by the existing confirmed damage cue.
    public sealed class GaleSlashVfx : MonoBehaviour
    {
        Image[] pool; RectTransform root; Sprite arc; WindSkillRecipe shared; SkillData skill;
        Vector2 source,target; float age=10,impact=-1;
        public bool Configured=>skill!=null&&skill.StableId=="skill_gale_slash";
        public bool Active=>Configured&&age<skill.initialHitDelay+.28f;
        public int Capacity=>15;
        public float HitAge=>impact<0?10:age-impact;
        public Vector2 VisualOffset=>Active&&HitAge<.1f?new Vector2(3*(1-HitAge/.1f),0):Vector2.zero;
        public void Initialize(RectTransform parent)
        {
            arc=Resources.Load<Sprite>("VFX/GaleSlashArc");shared=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");
            var go=new GameObject("Gale Slash pooled visual layer",typeof(RectTransform),typeof(RectMask2D));root=go.GetComponent<RectTransform>();root.SetParent(parent,false);root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-132);root.sizeDelta=new Vector2(480,518);
            pool=new Image[Capacity];for(int i=0;i<pool.Length;i++){var obj=new GameObject(i==0?"GaleSlashTrail":i==1?"GaleSlashArc":i==2?"GaleSlashImpact":i==3?"WindGather":"WindPixelDebris",typeof(RectTransform),typeof(Image));var im=obj.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;pool[i]=im;obj.SetActive(false);}
        }
        public void Configure(SkillData value){skill=value;Clear();}
        public void Clear(){age=10;impact=-1;if(root!=null)root.anchoredPosition=new Vector2(0,-132);if(pool!=null)foreach(var im in pool)im.gameObject.SetActive(false);}
        void OnDisable(){Clear();}
        public void Cast(Vector2 from,Vector2 to){Clear();source=from+new Vector2(15,35);target=to;age=0;Render();}
        public void Hit(){impact=age;Render();}
        public void Step(float delta){age+=delta;Render();}
        void Show(int i,Sprite sprite,Vector2 pos,Vector2 size,Color color,float angle=0)
        {var im=pool[i];im.sprite=sprite;im.color=color;im.rectTransform.anchoredPosition=new Vector2(Mathf.Round(pos.x/2)*2,Mathf.Round((pos.y+132)/2)*2);im.rectTransform.sizeDelta=size;im.rectTransform.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);}
        void Render()
        {
            if(pool==null)return;foreach(var im in pool)im.gameObject.SetActive(false);root.anchoredPosition=new Vector2(0,-132);if(!Active)return;
            float gather=Mathf.Min(.12f,skill.initialHitDelay*.55f),h=HitAge;
            if(age<gather){float t=age/Mathf.Max(.001f,gather);Show(3,shared.Frame(WindModule.WindGather,t),source,new Vector2(110,90),Color.white,t*120);}
            if(age>=gather)
            {
                // A rotating arc sweeps around a pivot between both actors; never a projectile.
                float t=impact>=0?1:Mathf.Clamp01((age-gather)/Mathf.Max(.001f,skill.initialHitDelay-gather));
                float rotation=Mathf.Lerp(-125,0,t),fade=impact<0?1:1-Mathf.Clamp01((h-.045f)/.16f);
                Vector2 pivot=Vector2.Lerp(source,target,.5f),radial=target-pivot;
                float a=rotation*Mathf.Deg2Rad;Vector2 p=pivot+new Vector2(radial.x*Mathf.Cos(a)-radial.y*Mathf.Sin(a),radial.x*Mathf.Sin(a)+radial.y*Mathf.Cos(a));
                p+=new Vector2(0,-40);
                if(fade>0){Show(0,arc,p+new Vector2(-12,-12),new Vector2(318,210),new Color(.12f,.72f,.65f,fade*.45f),rotation-18);Show(1,arc,p,new Vector2(310,205),new Color(1,1,1,fade),rotation);}
            }
            if(h<.19f){float phase=Mathf.Max(0,h-.045f)/.145f;Show(2,shared.Frame(WindModule.WindHitSpark,phase),target,new Vector2(205,170),new Color(1,1,1,1-phase));root.anchoredPosition=new Vector2(h<.1f?Mathf.Round(Mathf.Sin(h*130)*2*(1-h/.1f)):0,-132);}
            for(int i=4;i<Capacity;i++)
            {
                float a=i*2.39996f;Vector2 dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                if(age<gather)Show(i,null,source+new Vector2(28,8)+dir*(52*(1-age/gather)),Vector2.one*3,new Color(.4f,1,.85f));
                else if(h<.23f)Show(i,null,target+dir*(12+h*220)+new Vector2(h*60,0),Vector2.one*(i%2==0?4:6),new Color(.7f,1,.92f,1-h/.23f));
            }
        }
    }
}
