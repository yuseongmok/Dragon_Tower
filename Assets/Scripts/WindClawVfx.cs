using System;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public sealed class WindClawVfx : MonoBehaviour
    {
        const int Count=32;Image[] pool;RectTransform root;Sprite claw,spark,ring;readonly Sprite[] bodyFrames=new Sprite[8],goldFrames=new Sprite[8];WindSkillRecipe shared;SkillData skill;
        float age=10;readonly float[] hitAt={-1,-1};int hits;Vector2 source,target;
        public bool Configured=>skill!=null&&skill.StableId=="skill_wind_claw";
        public bool Active=>Configured&&age<skill.initialHitDelay+skill.hitInterval+.34f;
        public bool Drawing {get;private set;}
        public bool FinalLayersVisible=>pool!=null&&pool[24].gameObject.activeSelf&&pool[30].gameObject.activeSelf&&pool[31].gameObject.activeSelf&&pool[13].gameObject.activeSelf;
        public int HitsShown=>hits;public int Capacity=>Count;
        public bool CrossVisible=>pool!=null&&pool[3].gameObject.activeSelf&&pool[4].gameObject.activeSelf&&hits>=2;
        public void Initialize(RectTransform parent)
        {
            var frames=Resources.LoadAll<Sprite>("VFX/WindClawLayered");for(int f=0;f<8;f++){int index=f;bodyFrames[f]=Array.Find(frames,s=>s.name=="ClawBody"+index);goldFrames[f]=Array.Find(frames,s=>s.name=="ClawGold"+index);}claw=bodyFrames[7];spark=Resources.Load<Sprite>("VFX/GaleSlashImpact");ring=Resources.Load<Sprite>("VFX/GaleSlashShockwave");shared=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");
            var go=new GameObject("Wind Claw modules (pooled)",typeof(RectTransform),typeof(RectMask2D));root=go.GetComponent<RectTransform>();root.SetParent(parent,false);root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-132);root.sizeDelta=new Vector2(480,518);
            pool=new Image[Count];for(int i=0;i<Count;i++){var obj=new GameObject(i==0?"WindGather":i<3?"WindClawTrail":i<5?"WindClawSlash":i<7?"WindClawCrossAfterimage":i<13?"WindClawImpact":i==13?"WindShockwave":i==24?"WindClawFinalImpact":i==25||i==29?"WindClawWarmRim":i==30?"WindClawMintBurst":i==31?"WindClawCyanBurst":i>=26?"WindClawTipDebris":"WindPixelDebris",typeof(RectTransform),typeof(Image));var im=obj.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;pool[i]=im;obj.SetActive(false);}pool[5].transform.SetSiblingIndex(0);pool[6].transform.SetSiblingIndex(0);
        }
        public void Configure(SkillData data){skill=data;Clear();}
        public void Clear(){Drawing=false;age=10;hits=0;hitAt[0]=hitAt[1]=-1;if(root!=null)root.anchoredPosition=new Vector2(0,-132);if(pool!=null)foreach(var im in pool)im.gameObject.SetActive(false);}
        void OnDisable(){Clear();}
        public void Cast(Vector2 from,Vector2 to){Clear();source=from+new Vector2(20,30);target=to;age=0;Render();}
        public void Hit(){if(!Configured)return;hitAt[hits%2]=age;hits++;Render();}
        public void Step(float delta){age+=delta;Render();}
        void Show(int i,Sprite sprite,Vector2 p,Vector2 size,Color color,float angle=0)
        {var im=pool[i];im.sprite=sprite;im.color=color;im.rectTransform.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round((p.y+132)/2)*2);im.rectTransform.sizeDelta=size;im.rectTransform.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);}
        void Layer(int i,Sprite sprite,Vector2 point,Vector2 size,Color color,float angle,float mirror)
        {Show(i,sprite,point,size,color,angle);pool[i].rectTransform.localScale=new Vector3(mirror,1,1);}
        static Vector2 ClawPoint(int claw,float t)
        {
            float x=claw*20;Vector2 a=new Vector2(8+x,claw==1?123:claw==0?111:103),b=new Vector2(-2+x,65),c=new Vector2(16+x,-13),d=new Vector2(49+x,claw==1?18:claw==0?25:32);
            float u=1-t;return a*u*u*u+b*3*u*u*t+c*3*u*t*t+d*t*t*t;
        }
        void Render()
        {
            Drawing=false;if(pool==null)return;foreach(var im in pool)im.gameObject.SetActive(false);root.anchoredPosition=new Vector2(0,-132);if(!Active)return;
            float gather=Mathf.Min(.10f,skill.initialHitDelay*.6f),final=hitAt[1]<0?-1:age-hitAt[1];
            if(age<gather)Show(0,shared.Frame(WindModule.WindGather,age/Mathf.Max(.001f,gather)),source,new Vector2(105,85),Color.white);
            Vector2 center=target+new Vector2(0,12);
            for(int cut=0;cut<2;cut++)
            {
                float due=skill.initialHitDelay+cut*skill.hitInterval,flight=Mathf.Min(.08f,skill.initialHitDelay),travel=(age-(due-flight))/Mathf.Max(.001f,flight),h=hitAt[cut]<0?-1:age-hitAt[cut];
                float baseRotation=cut==0?42:-42,scale=cut==0?1:1.12f,mirror=cut==0?1:-1;
                Vector2 direction=cut==0?new Vector2(.707f,-.707f):new Vector2(-.707f,-.707f);
                if(travel>=0&&travel<1&&h<0)
                {
                    Drawing=true;
                    float sweep=1-Mathf.Pow(1-travel,1.7f),angle=baseRotation+(cut==0?-26:26)*(1-sweep);
                    Vector2 size=new Vector2(260*Mathf.Lerp(.68f,1,sweep),290)*scale,position=center-direction*(26*(1-sweep));
                    int frame=Mathf.Clamp((int)(sweep*7),0,6);
                    Show(1,shared.Frame(WindModule.WindTrail,travel),Vector2.Lerp(source,center,sweep),new Vector2(46,150),new Color(.3f,.9f,1,.65f),-Mathf.Atan2((center-source).x,(center-source).y)*Mathf.Rad2Deg);
                    Layer(2,bodyFrames[frame],source+direction*20,new Vector2(70,80),new Color(.45f,.9f,1,1-travel),angle,mirror);
                    Layer(5+cut,bodyFrames[frame],position+new Vector2(-mirror*7,-5),size*1.04f,new Color(.08f,.35f,.68f,.65f),angle-8*mirror,mirror);
                    Layer(3+cut,bodyFrames[frame],position,size,Color.white,angle,mirror);
                    Layer(cut==0?25:29,goldFrames[Mathf.Max(0,frame-1)],position+new Vector2(mirror*9,-5),size,new Color(1,1,1,.72f),angle-11*mirror,mirror);
                    for(int n=0;n<3;n++)
                    {
                        Vector2 tip=ClawPoint(n,(frame+1)/8f);tip=new Vector2((tip.x/96-.5f)*size.x*mirror,(tip.y/128-.5f)*size.y);
                        float a=angle*Mathf.Deg2Rad;tip=position+new Vector2(tip.x*Mathf.Cos(a)-tip.y*Mathf.Sin(a),tip.x*Mathf.Sin(a)+tip.y*Mathf.Cos(a));
                        Show(7+cut*3+n,spark,tip,Vector2.one*(cut==0?24:32),Color.white,angle);
                        Show(26+n,null,tip-direction*10,new Vector2(3,8),new Color(.5f,.95f,1),angle);
                    }
                }
                if(h>=0&&(final<0||final<.23f))
                {
                    float fade=final<0?1:1-Mathf.Clamp01((final-.085f)/.145f);
                    Color color=h<.06f?Color.white:new Color(.45f,.93f,.95f,.82f);color.a*=fade;
                    Vector2 size=new Vector2(260,290)*scale;
                    Layer(3+cut,claw,center,size,color,baseRotation,mirror);
                    if(h<.10f)Layer(5+cut,claw,center+new Vector2(-mirror*7,-5),size*1.04f,new Color(.08f,.35f,.68f,(1-h/.1f)*.6f),baseRotation-8*mirror,mirror);
                    if(h<.16f)Layer(cut==0?25:29,goldFrames[7],center+new Vector2(mirror*(9+h*40),-5-h*15),size,new Color(1,1,1,(1-h/.16f)*.72f),baseRotation-mirror*(11+h*55),mirror);
                    if(h<.12f)for(int n=0;n<3;n++)
                    {
                        Vector2 line=new Vector2((n-1)*43,0);float a=baseRotation*Mathf.Deg2Rad;line=new Vector2(line.x*Mathf.Cos(a),line.x*Mathf.Sin(a));
                        Show(7+cut*3+n,spark,target+line,new Vector2(cut==0?55:90,cut==0?68:105),new Color(1,1,1,1-Mathf.Max(0,h-(cut==0?.02f:.05f))/.07f),baseRotation);
                    }
                }
            }
            // The confirmed final hit keeps its contact spark; the burst follows 20ms later.
            // Presentation only: no extra damage, hit cue or global time change.
            if(final>=0&&final<.22f)
            {
                if(final<.11f)root.anchoredPosition=new Vector2(Mathf.Round(4*Mathf.Sin(final/.11f*Mathf.PI)),-132);
                if(final<.12f)Show(24,spark,target,new Vector2(175,195),new Color(1,1,1,final<.05f?1:1-(final-.05f)/.07f),0);
                float burst=final-.02f;
                if(burst>=0)
                {
                    if(burst<.12f)Show(30,spark,target,Vector2.one*Mathf.Lerp(80,205,Mathf.Clamp01(burst/.055f)),new Color(.65f,1,.84f,(1-burst/.12f)*.85f),25);
                    if(burst<.16f)Show(31,shared.Frame(WindModule.WindHitSpark,.4f),target,Vector2.one*Mathf.Lerp(95,240,Mathf.Clamp01(burst/.075f)),new Color(.06f,.73f,.91f,(1-burst/.16f)*.65f),-15);
                    Show(13,ring,target,Vector2.one*Mathf.Lerp(65,275,Mathf.Clamp01(burst/.085f)),new Color(.65f,1,.92f,1-burst/.20f));
                }
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
