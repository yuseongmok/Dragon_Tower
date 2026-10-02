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
        public bool Active=>Configured&&age<skill.initialHitDelay+skill.hitInterval+.34f;
        public int HitsShown=>hits;public int Capacity=>Count;
        public bool CrossVisible=>pool!=null&&pool[3].gameObject.activeSelf&&pool[4].gameObject.activeSelf&&hits>=2;
        public void Initialize(RectTransform parent)
        {
            claw=Resources.Load<Sprite>("VFX/WindClawSharp");spark=Resources.Load<Sprite>("VFX/GaleSlashImpact");ring=Resources.Load<Sprite>("VFX/GaleSlashShockwave");shared=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");
            var go=new GameObject("Wind Claw modules (pooled)",typeof(RectTransform),typeof(RectMask2D));root=go.GetComponent<RectTransform>();root.SetParent(parent,false);root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-132);root.sizeDelta=new Vector2(480,518);
            pool=new Image[Count];for(int i=0;i<Count;i++){var obj=new GameObject(i==0?"WindGather":i<3?"WindClawTrail":i<5?"WindClawSlash":i<7?"WindClawCrossAfterimage":i<13?"WindClawImpact":i==13?"WindShockwave":i==24?"WindClawFinalImpact":i>=26?"WindClawTipDebris":"WindPixelDebris",typeof(RectTransform),typeof(Image));var im=obj.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;pool[i]=im;if(i==3||i==4){im.type=Image.Type.Filled;im.fillMethod=Image.FillMethod.Vertical;im.fillOrigin=(int)Image.OriginVertical.Top;}obj.SetActive(false);}
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
                float rotation=cut==0?45:-45,scale=cut==0?1:1.12f;
                Vector2 direction=cut==0?new Vector2(.707f,-.707f):new Vector2(-.707f,-.707f);
                if(travel>=0&&travel<1&&h<0)
                {
                    Show(1,shared.Frame(WindModule.WindTrail,travel),Vector2.Lerp(source,center,travel),new Vector2(44,150),new Color(.5f,1,.9f,.8f),-Mathf.Atan2((center-source).x,(center-source).y)*Mathf.Rad2Deg);
                    Show(2,claw,source+direction*20,new Vector2(68,80),new Color(.6f,1,.9f,1-travel),rotation);
                    Show(3+cut,claw,center,new Vector2(250,290)*scale,Color.white,rotation);pool[3+cut].fillAmount=Mathf.Clamp01(travel);
                    for(int n=0;n<3;n++)
                    {
                        Vector2 local=new Vector2((n-1)*65,(.5f-travel)*290)*scale;float a=rotation*Mathf.Deg2Rad;
                        Vector2 tip=center+new Vector2(local.x*Mathf.Cos(a)-local.y*Mathf.Sin(a),local.x*Mathf.Sin(a)+local.y*Mathf.Cos(a));
                        Show(7+cut*3+n,spark,tip,Vector2.one*(cut==0?24:32),Color.white,rotation);
                        Show(26+n,null,tip-direction*12,new Vector2(4,9),new Color(.5f,1,.9f),rotation);
                    }
                }
                if(h>=0&&(final<0||final<.23f))
                {
                    float fade=final<0?1:1-Mathf.Clamp01((final-.085f)/.145f);
                    Color color=h<.15f?Color.white:new Color(.32f,.91f,.80f,.85f);color.a*=fade;
                    Show(3+cut,claw,center,new Vector2(250,290)*scale,color,rotation);pool[3+cut].fillAmount=1;
                    if(h<.12f)Show(5+cut,claw,center-direction*12,new Vector2(234,274)*scale,new Color(.1f,.63f,.58f,(1-h/.12f)*.3f),rotation);
                    if(h<.12f)for(int n=0;n<3;n++)
                    {
                        Vector2 line=new Vector2((n-1)*43,0);float a=rotation*Mathf.Deg2Rad;line=new Vector2(line.x*Mathf.Cos(a),line.x*Mathf.Sin(a));
                        Show(7+cut*3+n,spark,target+line,new Vector2(cut==0?55:90,cut==0?68:105),new Color(1,1,1,1-Mathf.Max(0,h-(cut==0?.02f:.06f))/.07f),rotation);
                    }
                }
            }
            if(final>=0&&final<.19f)
            {
                if(final<.11f)root.anchoredPosition=new Vector2(Mathf.Round(4*Mathf.Sin(final/.11f*Mathf.PI)),-132);
                if(final<.12f)Show(24,spark,target,new Vector2(175,195),new Color(1,1,1,final<.05f?1:1-(final-.05f)/.07f),0);
                float t=final/.19f;Show(13,ring,target,Vector2.one*Mathf.Lerp(65,265,Mathf.Clamp01(t*1.8f)),new Color(.65f,1,.92f,1-t));
            }
            for(int i=14;i<24;i++)
            {
                int n=i-14;float a=n*2.39996f;Vector2 dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                if(age<gather&&n<8)Show(i,null,source+dir*38*(1-age/gather),Vector2.one*3,new Color(.4f,1,.85f));
                if(final>=0&&final<.08f)Show(i,null,target+dir*(8+final*340),new Vector2(6,10),new Color(.75f,1,.94f,1-final/.08f),a*Mathf.Rad2Deg);
                else if(final>=.08f&&final<.32f)
                {
                    float t=(final-.08f)/.24f;Vector2 origin=center+new Vector2((n%3-1)*55,(n/3-1)*47);
                    Show(i,null,origin+dir*(t*75)+Vector2.down*(t*t*24),new Vector2(n%2==0?9:7,n%3==0?20:14)*(1-t*.4f),new Color(.3f,.96f,.82f,1-t),45+n*31+t*70);
                }
                else if(final<0&&hitAt[0]>=0&&age-hitAt[0]<.09f&&n<6)
                {float h=age-hitAt[0];Show(i,null,target+dir*(12+h*200),new Vector2(4,8),new Color(.5f,1,.86f,1-h/.09f),a*Mathf.Rad2Deg);}
            }
        }
    }
}
