using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public sealed class LunarVfx:MonoBehaviour
    {
        const int Capacity=192;BattleView view;RectTransform layer,front,rear,enemy;Image arena,enemyImage,flash;Image[] pool;
        readonly Sprite[] moon=new Sprite[32],ice=new Sprite[48];IceVfxStyle style;Material glow,domain,originalArena;
        SkillData skill;BattleModel battle;float age=99,impact=-1,bonus=-1,flashAge=99;bool night,offsetApplied;Vector2 target,enemyNormal,arenaNormal,heldEnemy;int used;
        public bool DamageFromSkill{get;set;}public bool Configured=>skill!=null&&skill.lunarPresentation;
        public bool Synergy{get;private set;}public int BaseHits{get;private set;}public int BonusHits{get;private set;}public int PeakImages{get;private set;}
        float Duration=>skill==null?0:skill.vfxDuration+(battle==null?0:Mathf.Max(0,battle.ScheduledSkillHits-skill.hitCount)*skill.hitInterval);
        public bool Active=>Configured&&age<Duration;
        public bool HoldCasting=>Active&&impact<0;
        public bool Clean=>used==0&&!night&&!offsetApplied&&(flash==null||!flash.gameObject.activeSelf);
        public void Initialize(BattleView owner,RectTransform parent){view=owner;layer=parent;}
        public void BindBattle(BattleModel model){battle=model;}
        void EnsurePool()
        {
            if(pool!=null)return;var a=Resources.LoadAll<Sprite>("VFX/Lunar/Frames");for(int r=0;r<4;r++)for(int f=0;f<8;f++)foreach(var s in a)if(s.name=="Lunar"+r+"_"+f){moon[r*8+f]=s;break;}
            a=Resources.LoadAll<Sprite>("IceSkills/BallDepth/DepthFrames");for(int r=0;r<6;r++)for(int f=0;f<8;f++)foreach(var s in a)if(s.name=="Depth"+r+"_"+f){ice[r*8+f]=s;break;}
            style=Resources.Load<IceSkillLibrary>("IceSkills/Library").style;glow=Resources.Load<Material>("IceSkills/BallDepth/Highlight");domain=new Material(Resources.Load<Shader>("VFX/LunarDomain"));
            front=Root("Lunar foreground",layer);rear=Root("Lunar moon behind enemy",view.frame);rear.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex());
            pool=new Image[Capacity];for(int i=0;i<Capacity;i++){var go=new GameObject("Lunar pooled "+i,typeof(RectTransform),typeof(Image));pool[i]=go.GetComponent<Image>();pool[i].raycastTarget=false;pool[i].rectTransform.SetParent(front,false);go.SetActive(false);}
            var fg=new GameObject("Moonlight silhouette",typeof(RectTransform),typeof(Image));flash=fg.GetComponent<Image>();flash.raycastTarget=false;flash.material=Resources.Load<Material>("VFX/Wisp/Silhouette");fg.SetActive(false);arena=view.frame.Find("Arena").GetComponent<Image>();
        }
        RectTransform Root(string name,RectTransform parent){var go=new GameObject(name,typeof(RectTransform),typeof(RectMask2D));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-130);r.sizeDelta=new Vector2(480,530);return r;}
        public void Configure(SkillData data,Graphic enemyGraphic)
        {
            Cancel();skill=data;if(!Configured)return;EnsurePool();enemy=view.enemyArt.rectTransform;enemyImage=enemyGraphic as Image;var r=flash.rectTransform;r.SetParent(enemyGraphic.transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;flash.preserveAspect=enemyImage!=null&&enemyImage.preserveAspect;
        }
        public void Cast(Vector2 point)
        {
            if(!Configured||!isActiveAndEnabled)return;ClearVisual();target=point;Synergy=battle!=null&&battle.SlowSynergyAtCast;age=0;BaseHits=BonusHits=PeakImages=0;originalArena=arena.material;arena.material=domain;night=true;Render();
        }
        public void Hit(bool extra)
        {
            if(!Active)return;flashAge=0;if(extra){bonus=0;BonusHits++;}else{impact=0;BaseHits++;heldEnemy=enemy.anchoredPosition;}Render();
        }
        Sprite M(int r,float phase,bool loop=false)=>moon[r*8+Mathf.Clamp((int)((loop?Mathf.Repeat(phase,1):Mathf.Clamp01(phase))*8),0,7)];
        Sprite I(int r,float phase)=>ice[r*8+Mathf.Clamp((int)(Mathf.Clamp01(phase)*8),0,7)];
        static Color Tint(float alpha,float lavender=0)=>new Color(1-lavender*.06f,1-lavender*.18f,1,alpha);
        void Show(Sprite sprite,Vector2 p,Vector2 size,Color color,float angle=0,bool back=false,bool additive=false)
        {
            if(sprite==null||color.a<=0||used>=Capacity)return;var im=pool[used++];im.sprite=sprite;im.color=color;im.material=additive?glow:null;var r=im.rectTransform;var parent=back?rear:front;if(r.parent!=parent)r.SetParent(parent,false);r.SetAsLastSibling();r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round((p.y+130)/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);
        }
        void Beam(Vector2 from,Vector2 to,float width,float alpha){var delta=to-from;Show(M(1,age*1.5f,true),(from+to)*.5f,new Vector2(width,delta.magnitude+18),Tint(alpha),Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90,false,true);}
        void Render()
        {
            if(pool==null)return;foreach(var im in pool)im.gameObject.SetActive(false);used=0;if(!Active)return;
            float recovery=Mathf.Clamp01((Duration-age)/.5f);domain.SetFloat("_Darkness",Mathf.Clamp01(age/.18f)*recovery*.82f);
            var moonPoint=new Vector2(-25,-302);float emerge=Mathf.Clamp01((age-.12f)/.30f);float moonSize=Mathf.Lerp(310,368,emerge);float moonAlpha=emerge*recovery;
            Show(M(0,age*.8f,true),moonPoint,Vector2.one*(moonSize*1.13f),new Color(.25f,.31f,.68f,moonAlpha*.65f),-18,true);
            Show(M(0,age*.8f+.2f,true),moonPoint+new Vector2(4,1),Vector2.one*(moonSize*1.06f),Tint(moonAlpha*.65f,.9f),-18,true,true);
            Show(M(0,age*.8f,true),moonPoint,Vector2.one*moonSize,Tint(moonAlpha),-18,true);
            if(impact>=0&&impact<.18f)Show(M(0,0),moonPoint,Vector2.one*moonSize,Tint(1-impact/.18f),-18,true,true);
            view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,layer,out var staff);
            float stillAt=skill.initialHitDelay-.08f;bool still=age>=stillAt&&impact<0;float motion=still?stillAt:age;
            if(impact<0){
                float connect=Mathf.Clamp01((age-.2f)/.15f);Beam(staff,moonPoint+new Vector2(-50,70),18+connect*10,connect*.8f);Show(I(5,0),staff,Vector2.one*(70+age*28),Tint(connect),0,false,true);
                for(int j=0;j<12;j++){float a=j*2.4f-motion*2;float radius=(1-Mathf.Repeat(motion*1.6f+j/12f,1))*110;var p=staff+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.65f)*radius;Show(j%3==0?style.Get(IceStyleModule.ShardSmall):I(5,.2f),p,Vector2.one*(j%3==0?23:17),Tint(connect*.8f,j%2*.4f),j*30,false,j%3!=0);}
                float compression=Mathf.Clamp01((age-.4f)/Mathf.Max(.1f,stillAt-.4f));
                if(compression>0){Show(M(2,motion,true),target+new Vector2(0,-65),new Vector2(330,95),Tint(compression,.5f),0,true);Show(I(3,1-compression),target,new Vector2(340,240),Tint(compression,.6f),0,true);
                    int crystals=Synergy?14:8;for(int j=0;j<crystals;j++){float a=j*2.4f+motion*.5f;float radius=Mathf.Lerp(170,42,compression);var p=target+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.6f)*radius;Show(style.Get(IceStyleModule.ShardLarge),p,new Vector2(26,52),Tint(.85f),j*31,j%3==0);}
                    if(Synergy){Show(style.Get(IceStyleModule.CrackC),target,new Vector2(190,160),Tint(compression),0,false,true);Show(M(2,motion,true),target+new Vector2(0,-68),new Vector2(290,70),Tint(compression),0,false,true);for(int j=0;j<3;j++)Beam(target+new Vector2((j-1)*50,-45),moonPoint+new Vector2(-50,50),7,compression*.6f);}
                    Show(I(5,0),target,Vector2.one*Mathf.Lerp(180,45,compression),Tint(1),0,false,true);
                }
            }
            if(impact>=0)Explosion(impact,moonPoint);
            if(bonus>=0)BonusShatter(bonus);
            PeakImages=Mathf.Max(PeakImages,used);
        }
        void Explosion(float h,Vector2 moonPoint)
        {
            float t=Mathf.Max(0,h-.06f);
            if(t<.18f){Beam(moonPoint+new Vector2(-60,75),target,150*(1-t/.20f),1);Show(I(5,t/.18f),target,new Vector2(430,370),Tint(1),0,false,true);}
            if(t<.45f){Show(I(4,t/.45f),target,new Vector2(490,380),Tint(1,.8f),0,true);Show(M(3,t/.45f),target,new Vector2(465,415),Tint(1,.5f),0,true);Show(I(3,t/.45f),target+new Vector2(0,-40),new Vector2(480,300),Tint(1,.5f));}
            if(t<.3f){Show(I(2,t/.3f),target,new Vector2(355,330),Tint(1));Show(I(2,t/.3f),target,new Vector2(255,220),Tint(.8f),20,false,true);}
            for(int j=0;j<12;j++){float a=j*2.4f;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.7f);bool back=j%3==0;float size=back?55:80;
                if(t<.10f)Show(style.Get(IceStyleModule.ShardLarge),target+d*90,new Vector2(size*.6f,size*1.5f),Tint(1),-a*Mathf.Rad2Deg,back);
                else if(t<.65f){float life=(t-.1f)/.55f,perspective=back?1-life*.4f:1+life;var p=target+d*(70+life*170)*perspective+Vector2.down*life*life*80;Show(style.Get(IceStyleModule.ShardLarge),p,new Vector2(size*.55f*perspective*(.35f+.65f*Mathf.Abs(Mathf.Cos(j+t*9))),size*perspective),Tint(Mathf.Clamp01((.65f-t)/.23f)),j*31+t*140,back);}
            }
            if(t<.72f)for(int j=0;j<12;j++){float a=j*2.4f;var p=target+new Vector2(Mathf.Cos(a)*155,Mathf.Sin(a)*90)*Mathf.Min(1,t*3)+Vector2.down*t*65;Show(j%2==0?I(5,Mathf.Repeat(age+j*.13f,1)):style.Get(IceStyleModule.FrostA),p,new Vector2(35,28),Tint((.72f-t)*.9f,j%3*.3f),j*30,false,j%2==0);}
        }
        void BonusShatter(float h)
        {
            if(h>.4f)return;float t=h/.4f;
            if(h<.09f){Show(style.Get(IceStyleModule.CrackC),target,new Vector2(270,240),Tint(1),0,false,true);Show(M(2,0),target,new Vector2(260,210),Tint(1,.4f));}
            for(int j=0;j<16;j++){float a=j*2.4f;float r=35+t*170;var p=target+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*.65f);float size=(j%3==0?62:38)*(1+t*.5f);Show(style.Get(IceStyleModule.ShardMedium),p,new Vector2(size*.6f,size),Tint(1-t),j*41+t*100,j%4==0);}
            if(h<.18f)Show(I(5,h/.18f),target,new Vector2(240,190),Tint(.8f),45,false,true);
        }
        public void Step(BattleModel model,float delta)
        {
            battle=model;if(offsetApplied&&arena!=null)arena.rectTransform.anchoredPosition=arenaNormal;offsetApplied=false;
            if(!Configured)return;if(model.Result!=BattleResult.Fighting){Cancel();return;}
            age+=delta;if(impact>=0)impact+=delta;if(bonus>=0)bonus+=delta;flashAge+=delta;
            if(!Active){ClearVisual();return;}Render();
            flash.gameObject.SetActive(flashAge<.12f);if(flashAge<.12f){flash.sprite=enemyImage!=null?enemyImage.sprite:null;flash.color=new Color(.91f,.96f,1,.95f);}
            if(impact>=0&&impact<.22f){enemyNormal=enemy.anchoredPosition;arenaNormal=arena.rectTransform.anchoredPosition;float decay=1-impact/.22f;var shake=new Vector2(Mathf.Round(Mathf.Sin(impact*135)*9*decay),Mathf.Round(Mathf.Cos(impact*105)*5*decay));enemy.anchoredPosition=(impact<.06f?heldEnemy:enemyNormal)+shake;arena.rectTransform.anchoredPosition+=shake;offsetApplied=true;}
        }
        void ClearVisual()
        {
            if(offsetApplied){if(enemy!=null)enemy.anchoredPosition=enemyNormal;if(arena!=null)arena.rectTransform.anchoredPosition=arenaNormal;}offsetApplied=false;
            if(night&&arena!=null)arena.material=originalArena;night=false;age=99;impact=bonus=-1;flashAge=99;used=0;if(pool!=null)foreach(var im in pool)if(im!=null)im.gameObject.SetActive(false);if(flash!=null)flash.gameObject.SetActive(false);
        }
        public void Cancel(){ClearVisual();battle?.CancelSlowSignature();}
        void OnDisable()=>Cancel();
        void OnDestroy(){Cancel();if(front!=null)Destroy(front.gameObject);if(rear!=null)Destroy(rear.gameObject);if(flash!=null)Destroy(flash.gameObject);if(domain!=null)Destroy(domain);}
    }
}
