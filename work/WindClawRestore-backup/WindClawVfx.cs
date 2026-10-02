using System;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public sealed class WindClawVfx : MonoBehaviour
    {
        const int Count=32;Material highlight;Image[] pool;RectTransform root;Sprite claw,spark,ring;readonly Sprite[] bodyFrames=new Sprite[8],coreFrames=new Sprite[8];WindSkillRecipe shared;SkillData skill;
        float age=10;readonly float[] hitAt={-1,-1};int hits;Vector2 source,target;
        public bool Configured=>skill!=null&&skill.StableId=="skill_wind_claw";
        public bool Active=>Configured&&age<skill.initialHitDelay+skill.hitInterval+.34f;
        public bool Drawing {get;private set;}
        public bool FinalLayersVisible=>pool!=null&&pool[24].gameObject.activeSelf&&pool[30].gameObject.activeSelf&&pool[31].gameObject.activeSelf&&pool[13].gameObject.activeSelf;
        public int HitsShown=>hits;public int Capacity=>Count;
        public bool CrossVisible=>pool!=null&&pool[3].gameObject.activeSelf&&pool[4].gameObject.activeSelf&&hits>=2;
        public void Initialize(RectTransform parent)
        {
            var frames=Resources.LoadAll<Sprite>("VFX/WindClawHierarchy");for(int f=0;f<8;f++){int index=f;bodyFrames[f]=Array.Find(frames,s=>s.name=="ClawBody"+index);coreFrames[f]=Array.Find(frames,s=>s.name=="ClawCore"+index);}claw=bodyFrames[7];spark=Resources.Load<Sprite>("VFX/GaleSlashImpact");ring=Resources.Load<Sprite>("VFX/GaleSlashShockwave");shared=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");
            var go=new GameObject("Wind Claw modules (pooled)",typeof(RectTransform),typeof(RectMask2D));root=go.GetComponent<RectTransform>();root.SetParent(parent,false);root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-132);root.sizeDelta=new Vector2(480,518);
            highlight=new Material(Resources.Load<Shader>("VFX/WindClawHighlight"));pool=new Image[Count];for(int i=0;i<Count;i++){var obj=new GameObject(i==0?"WindGather":i<3?"WindClawTrail":i<5?"WindClawSlash":i<7?"WindClawCrossAfterimage":i<13?"WindClawImpact":i==13?"WindShockwave":i==24?"WindClawFinalImpact":i==25||i==29?"WindClawHotCore":i==30?"WindClawMintBurst":i==31?"WindClawCyanBurst":i>=26?"WindClawTipDebris":"WindPixelDebris",typeof(RectTransform),typeof(Image));var im=obj.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;pool[i]=im;if(i==24||i==25||i==29)im.material=highlight;obj.SetActive(false);}pool[5].transform.SetSiblingIndex(0);pool[6].transform.SetSiblingIndex(0);pool[24].transform.SetAsLastSibling();
        }
        public void Configure(SkillData data){skill=data;Clear();}
        public void Clear(){Drawing=false;age=10;hits=0;hitAt[0]=hitAt[1]=-1;if(root!=null)root.anchoredPosition=new Vector2(0,-132);if(pool!=null)foreach(var im in pool)im.gameObject.SetActive(false);}
        void OnDisable(){Clear();}
        void OnDestroy(){if(highlight!=null)Destroy(highlight);}
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
                    Vector2 size=new Vector2(260*Mathf.Lerp(.80f,1,sweep),290*Mathf.Lerp(.88f,1,sweep))*scale,position=center-direction*(26*(1-sweep));
                    int frame=Mathf.Clamp((int)(sweep*7),0,6);
                    Show(1,shared.Frame(WindModule.WindTrail,travel),Vector2.Lerp(source,center,sweep),new Vector2(46,150),new Color(.3f,.9f,1,.65f),-Mathf.Atan2((center-source).x,(center-source).y)*Mathf.Rad2Deg);
                    Layer(2,bodyFrames[frame],source+direction*20,new Vector2(70,80),new Color(.45f,.9f,1,1-travel),angle,mirror);
                    Layer(5+cut,bodyFrames[Mathf.Max(0,frame-2)],position+new Vector2(-mirror*3,-3),size*1.015f,new Color(.08f,.30f,.52f,cut==0?.27f:.36f),angle-3*mirror,mirror);
                    Layer(3+cut,bodyFrames[Mathf.Max(0,frame-1)],position,size,new Color(.82f,1,1,cut==0?.9f:1),angle,mirror);
                    Layer(cut==0?25:29,coreFrames[frame],position+direction*2,size*1.004f,new Color(1,1,1,cut==0?.65f:.95f),angle+mirror,mirror);
                    for(int n=0;n<3;n++)
                    {
                        Vector2 tip=ClawPoint(n,(frame+1)/8f);tip=new Vector2((tip.x/96-.5f)*size.x*mirror,(tip.y/128-.5f)*size.y);
                        float a=angle*Mathf.Deg2Rad;tip=position+new Vector2(tip.x*Mathf.Cos(a)-tip.y*Mathf.Sin(a),tip.x*Mathf.Sin(a)+tip.y*Mathf.Cos(a));
                        Show(7+cut*3+n,spark,tip,Vector2.one*(cut==0?24:32),Color.white,angle);
                        Show(26+n,null,tip-direction*10,new Vector2(3,8),new Color(.5f,.95f,1),angle);
                    }
                }
                if(h>=0&&(final<0||final<.18f))
                {
                    float fade=final<0?1:1-Mathf.Clamp01((final-.04f)/.14f);
                    Color color;
                    if(final<0)color=h<.04f?Color.white:new Color(.35f,.85f,.89f,.66f);
                    else if(final<.04f)color=Color.white;
                    else if(final<.09f)color=Color.Lerp(new Color(.65f,1,.85f),new Color(.16f,.78f,.94f),(final-.04f)/.05f);
                    else color=Color.Lerp(new Color(.16f,.78f,.94f),new Color(.025f,.28f,.38f),(final-.09f)/.09f);
                    color.a*=fade;
                    Vector2 size=new Vector2(260,290)*scale;
                    Layer(3+cut,claw,center,size,color,baseRotation,mirror);
                    float trailLife=cut==0?.11f:.15f;if(h<trailLife)Layer(5+cut,claw,center+new Vector2(-mirror*3,-3-h*10),size*1.015f,new Color(.08f,.30f,.52f,(1-h/trailLife)*.32f),baseRotation-3*mirror,mirror);
                    float coreAge=final>=0?final:h;float coreLife=cut==0?.035f:.055f;if(coreAge<coreLife)Layer(cut==0?25:29,coreFrames[7],center,size,new Color(1,1,1,(1-coreAge/coreLife)*(cut==0?.65f:.95f)),baseRotation,mirror);
                    if(h<.12f)for(int n=0;n<3;n++)
                    {
                        Vector2 line=new Vector2((n-1)*43,0);float a=baseRotation*Mathf.Deg2Rad;line=new Vector2(line.x*Mathf.Cos(a),line.x*Mathf.Sin(a));
                        Show(7+cut*3+n,spark,target+line,new Vector2(cut==0?55:90,cut==0?68:105),new Color(1,1,1,1-Mathf.Max(0,h-(cut==0?.02f:.05f))/.07f),baseRotation);
                    }
                }
            }
            if(final>=0&&final<.20f)
            {
                if(final<.11f)root.anchoredPosition=new Vector2(Mathf.Round(4*Mathf.Sin(final/.11f*Mathf.PI)),-132);
                // All layers begin on the confirmed second hit, with staggered expansion/decay.
                if(final<.065f)Show(24,spark,target,Vector2.one*Mathf.Lerp(105,140,Mathf.Clamp01(final/.025f)),new Color(1,1,1,1-final/.065f),0);
                if(final<.12f){float t=final/.12f;Show(30,spark,target,Vector2.one*Mathf.Lerp(75,190,Mathf.Clamp01((final-.008f)/.05f)),new Color(.65f,1,.84f,(1-t)*.78f),25);}
                if(final<.16f){float t=final/.16f;Show(31,shared.Frame(WindModule.WindHitSpark,.4f),target,Vector2.one*Mathf.Lerp(90,225,Mathf.Clamp01((final-.016f)/.075f)),new Color(.06f,.73f,.91f,(1-t)*.56f),-15);}
                float wave=final/.20f;Show(13,ring,target,Vector2.one*Mathf.Lerp(90,270,Mathf.Clamp01((final-.012f)/.10f)),new Color(.025f,.52f,.55f,(1-wave)*.85f));
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
