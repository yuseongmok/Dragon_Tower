using System;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public sealed class WindClawVfx : MonoBehaviour
    {
        const int Count=30;Image[] pool;RectTransform root;Sprite claw,spark,ring;WindSkillRecipe shared;SkillData skill;
        float age=10;readonly float[] hitAt={-1,-1};int hits;Vector2 source,target;
        public bool Configured=>skill!=null&&skill.StableId=="skill_wind_claw";
        public bool Active=>Configured&&age<skill.initialHitDelay+skill.hitInterval+.26f;
        public int HitsShown=>hits;public int Capacity=>Count;
        public bool CrossVisible=>pool!=null&&pool[3].gameObject.activeSelf&&pool[4].gameObject.activeSelf&&hits>=2;
        public void Initialize(RectTransform parent)
        {
            claw=Array.Find(Resources.LoadAll<Sprite>("VFX/WindClaw"),s=>s.name=="WindClawSlash");spark=Resources.Load<Sprite>("VFX/GaleSlashImpact");ring=Resources.Load<Sprite>("VFX/GaleSlashShockwave");shared=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");
            var go=new GameObject("Wind Claw modules (pooled)",typeof(RectTransform),typeof(RectMask2D));root=go.GetComponent<RectTransform>();root.SetParent(parent,false);root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-132);root.sizeDelta=new Vector2(480,518);
            pool=new Image[Count];for(int i=0;i<Count;i++){var obj=new GameObject(i==0?"WindGather":i<3?"WindClawTrail":i<5?"WindClawSlash":i<7?"WindClawCrossAfterimage":i<13?"WindClawImpact":i==13?"WindShockwave":"WindPixelDebris",typeof(RectTransform),typeof(Image));var im=obj.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;pool[i]=im;obj.SetActive(false);}
        }
        public void Configure(SkillData data){skill=data;Clear();}
        public void Clear(){age=10;hits=0;hitAt[0]=hitAt[1]=-1;if(root!=null)root.anchoredPosition=new Vector2(0,-132);if(pool!=null)foreach(var im in pool)im.gameObject.SetActive(false);}
        void OnDisable(){Clear();}
        public void Cast(Vector2 from,Vector2 to){Clear();source=from+new Vector2(20,30);target=to;age=0;Render();}
        public void Hit(){if(!Configured)return;hitAt[hits%2]=age;hits++;Render();}
        public void Step(float delta){age+=delta;Render();}
        void Show(int i,Sprite sprite,Vector2 p,Vector2 size,Color color,float angle=0)
        {var im=pool[i];im.sprite=sprite;im.color=color;im.rectTransform.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round((p.y+132)/2)*2);im.rectTransform.sizeDelta=size;im.rectTransform.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);}
        void Render()
        {
            if(pool==null)return;foreach(var im in pool)im.gameObject.SetActive(false);root.anchoredPosition=new Vector2(0,-132);if(!Active)return;
            float gather=Mathf.Min(.10f,skill.initialHitDelay*.6f),final=hitAt[1]<0?-1:age-hitAt[1];
            if(age<gather)Show(0,shared.Frame(WindModule.WindGather,age/Mathf.Max(.001f,gather)),source,new Vector2(105,85),Color.white);
            Vector2 center=target+new Vector2(0,-28);
            for(int cut=0;cut<2;cut++)
            {
                float due=skill.initialHitDelay+cut*skill.hitInterval,flight=Mathf.Min(.08f,skill.initialHitDelay),travel=(age-(due-flight))/Mathf.Max(.001f,flight),h=hitAt[cut]<0?-1:age-hitAt[cut];
                float rotation=cut==0?45:-45,scale=cut==0?1:1.08f;
                Vector2 direction=cut==0?new Vector2(.707f,-.707f):new Vector2(-.707f,-.707f);
                if(travel>=0&&travel<1&&h<0)
                {
                    Show(1,shared.Frame(WindModule.WindTrail,travel),Vector2.Lerp(source,center,travel),new Vector2(44,150),new Color(.5f,1,.9f,.8f),-Mathf.Atan2((center-source).x,(center-source).y)*Mathf.Rad2Deg);
                    Show(2,claw,source+direction*20,new Vector2(68,80),new Color(.6f,1,.9f,1-travel),rotation);
                    Show(3+cut,claw,center-direction*(65*(1-travel)),new Vector2(230,270)*scale,Color.white,rotation);
                }
                if(h>=0&&(final<0||final<.22f))
                {
                    float fade=final<0?1:1-Mathf.Clamp01((final-.06f)/.16f);
                    Color color=h<.055f?Color.white:new Color(.32f,.91f,.80f,cut==0?.75f:.85f);color.a*=fade;
                    Show(3+cut,claw,center,new Vector2(230,270)*scale,color,rotation);
                    if(h<.12f)Show(5+cut,claw,center-direction*12,new Vector2(234,274)*scale,new Color(.1f,.63f,.58f,(1-h/.12f)*.3f),rotation);
                    if(h<.12f)for(int n=0;n<3;n++)
                    {
                        Vector2 line=new Vector2((n-1)*43,0);float a=rotation*Mathf.Deg2Rad;line=new Vector2(line.x*Mathf.Cos(a),line.x*Mathf.Sin(a));
                        Show(7+cut*3+n,spark,target+line,new Vector2(cut==0?55:90,cut==0?68:105),new Color(1,1,1,1-Mathf.Max(0,h-(cut==0?.02f:.06f))/.07f),rotation);
                    }
                }
            }
            if(final>=0&&final<.17f)
            {
                if(final<.11f)root.anchoredPosition=new Vector2(Mathf.Round(4*Mathf.Sin(final/.11f*Mathf.PI)),-132);
                if(final>=.025f){float t=(final-.025f)/.145f;Show(13,ring,target,Vector2.one*Mathf.Lerp(80,235,Mathf.Clamp01(t*1.5f)),new Color(.65f,1,.92f,1-t));}
            }
            for(int i=14;i<Count;i++)
            {
                float a=(i-14)*2.39996f;Vector2 dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                if(age<gather&&i<22)Show(i,null,source+dir*38*(1-age/gather),Vector2.one*3,new Color(.4f,1,.85f));
                float h=final>=0?final:(hitAt[0]<0?-1:age-hitAt[0]);
                if(h>=0&&h<.20f&&(final>=0||i<22))Show(i,null,target+dir*(12+h*(final>=0?360:180)),new Vector2(4,i%2==0?9:4),new Color(.5f,1,.86f,1-h/.20f),a*Mathf.Rad2Deg);
            }
        }
    }
}
