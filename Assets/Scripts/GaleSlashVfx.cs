using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Presentation only: Hit is driven by the existing confirmed damage cue.
    public sealed class GaleSlashVfx : MonoBehaviour
    {
        Image[] pool; RectTransform root; Sprite arc,edgeSprite,coreSprite,impactSprite,shockwaveSprite;float impactWidth=166; WindSkillRecipe shared; SkillData skill;
        Vector2 source,target; float age=10,impact=-1;
        public bool Configured=>skill!=null&&skill.StableId=="skill_gale_slash";
        public bool Active=>Configured&&age<skill.initialHitDelay+.28f;
        public int Capacity=>19;
        public void SetTargetWidth(float width){impactWidth=width*.65f;}
        public float HitAge=>impact<0?10:age-impact;
        public Vector2 VisualOffset=>Active&&HitAge<.1f?new Vector2(3*(1-HitAge/.1f),0):Vector2.zero;
        public void Initialize(RectTransform parent)
        {
            var atlas=WindVFXStyle.Load();arc=WindVFXStyle.Find(atlas,"ArcBody");edgeSprite=WindVFXStyle.Find(atlas,"ArcEdge");coreSprite=WindVFXStyle.Find(atlas,"ArcCore");impactSprite=Resources.Load<Sprite>("VFX/GaleSlashImpact");shockwaveSprite=Resources.Load<Sprite>("VFX/GaleSlashShockwave");shared=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");
            var go=new GameObject("Gale Slash pooled visual layer",typeof(RectTransform),typeof(RectMask2D));root=go.GetComponent<RectTransform>();root.SetParent(parent,false);root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-132);root.sizeDelta=new Vector2(480,518);
            pool=new Image[Capacity];for(int i=0;i<pool.Length;i++){var obj=new GameObject(i==0?"GaleSlashTrail":i==1?"GaleSlashArc":i==2?"GaleSlashImpact":i==3?"WindGather":i==15?"WindShockwave":i==16?"GaleSlashCuttingEdge":i==17?"GaleSlashBrightCore":i==18?"GaleSlashCyanBurst":"WindPixelDebris",typeof(RectTransform),typeof(Image));var im=obj.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;pool[i]=im;if(i==2){var edge=obj.AddComponent<Outline>();edge.effectColor=new Color(.025f,.30f,.32f,.95f);edge.effectDistance=new Vector2(2,-2);}obj.SetActive(false);}pool[2].transform.SetAsLastSibling();
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
                if(fade>0)
                {
                    Show(0,arc,p+new Vector2(-12,-12),new Vector2(318,210),WindVFXStyle.Alpha(WindVFXStyle.Teal,fade*.56f),rotation-18);
                    Show(1,arc,p,new Vector2(310,205),new Color(1,1,1,fade),rotation);
                    Show(16,edgeSprite,p+new Vector2(0,2),new Vector2(310,205),new Color(1,1,1,fade),rotation+1);
                    // Thin leading core moves independently by a few pixels, not a larger arc.
                    float coreFade=impact<0?fade:fade*(1-Mathf.Clamp01((h-.04f)/.07f));
                    Show(17,coreSprite,p+new Vector2(2,3),new Vector2(310,205)*.985f,new Color(1,1,1,coreFade),rotation+2);
                }
            }
            if(h<.13f)
            {
                float fade=h<.05f?1:1-(h-.05f)/.08f;
                Show(2,impactSprite,target,Vector2.one*impactWidth,new Color(1,1,1,fade));
                if(h>=.025f){float burst=(h-.025f)/.105f;Show(18,shared.Frame(WindModule.WindHitSpark,.4f),target,Vector2.one*impactWidth*Mathf.Lerp(.55f,1.12f,Mathf.Clamp01(burst*1.7f)),WindVFXStyle.Alpha(WindVFXStyle.Cyan,(1-burst)*.70f),25);}
            }
            if(h>=.035f&&h<.16f)
            {
                float t=(h-.035f)/.125f;
                Show(15,shockwaveSprite,target,Vector2.one*impactWidth*Mathf.Lerp(.45f,1.25f,Mathf.Clamp01(t*1.7f)),WindVFXStyle.Alpha(WindVFXStyle.Teal,(1-t)*.85f));
            }
            // One small impulse; the HUD and camera do not move.
            if(h<.08f)root.anchoredPosition=new Vector2(Mathf.Round(2*Mathf.Sin(h/.08f*Mathf.PI)),-132);
            for(int i=4;i<15;i++)
            {
                float a=i*2.39996f;Vector2 dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                if(age<gather)Show(i,null,source+new Vector2(28,8)+dir*(52*(1-age/gather)),Vector2.one*3,new Color(.4f,1,.85f));
                else if(h<.18f)Show(i,null,target+dir*(10+h*320)+new Vector2(h*70,0),new Vector2(i%2==0?4:6,i%2==0?10:4),new Color(.50f,1,.87f,1-h/.18f),a*Mathf.Rad2Deg);
            }
        }
    }
}
