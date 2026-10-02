using System;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Wisp-only pooled presentation. The combat model owns all contacts and buff time.
    public sealed class WispVfx : MonoBehaviour
    {
        const int Capacity=192;
        BattleView view;RectTransform layer,front,rear;Image[] pool;readonly Sprite[] frames=new Sprite[40];
        IceVfxStyle iceStyle;Material glow,silhouette;Image flash,enemyImage,arena;RectTransform enemy;SkillData skill;BattleModel battle;
        Vector2 target,source,enemyNormal,arenaNormal;bool offsetApplied;float age=99,finalAge=-1,flashAge=99;int used;
        readonly float[] contacts=new float[32];
        public bool DamageFromSkill{get;set;}public int HitsShown{get;private set;}public int PeakImages{get;private set;}
        public int HitCount=>battle!=null&&battle.ScheduledSkillHits>0?battle.ScheduledSkillHits:skill==null?0:skill.hitCount;
        float Duration=>battle!=null&&battle.DodgeReleaseVisualDuration>0?battle.DodgeReleaseVisualDuration:skill.dodgeFreeCastDuration;
        public bool Configured=>skill!=null&&skill.wispPresentation;
        public bool Active=>Configured&&age<Duration;
        public bool Clean=>used==0&&!offsetApplied&&(flash==null||!flash.gameObject.activeSelf);
        public void Initialize(BattleView owner,RectTransform parent){view=owner;layer=parent;}
        public void BindBattle(BattleModel model){battle=model;}
        void EnsurePool()
        {
            if(pool!=null)return;
            var sprites=Resources.LoadAll<Sprite>("VFX/Wisp/Frames");for(int row=0;row<5;row++)for(int f=0;f<8;f++)foreach(var s in sprites)if(s.name=="Wisp"+row+"_"+f){frames[row*8+f]=s;break;}
            iceStyle=Resources.Load<IceSkillLibrary>("IceSkills/Library").style;glow=Resources.Load<Material>("IceSkills/BallDepth/Highlight");silhouette=Resources.Load<Material>("VFX/Wisp/Silhouette");
            front=Root("Wisp foreground",layer);rear=Root("Wisp background",view.frame);rear.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex());
            pool=new Image[Capacity];for(int i=0;i<Capacity;i++){var go=new GameObject("Wisp pooled "+i,typeof(RectTransform),typeof(Image));var im=go.GetComponent<Image>();im.rectTransform.SetParent(front,false);im.raycastTarget=false;go.SetActive(false);pool[i]=im;}
            var fg=new GameObject("Wisp spirit silhouette",typeof(RectTransform),typeof(Image));flash=fg.GetComponent<Image>();flash.material=silhouette;flash.raycastTarget=false;fg.SetActive(false);
            arena=view.frame.Find("Arena").GetComponent<Image>();
        }
        RectTransform Root(string name,RectTransform parent){var go=new GameObject(name,typeof(RectTransform),typeof(RectMask2D));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-140);r.sizeDelta=new Vector2(476,515);return r;}
        public void Configure(SkillData data,Graphic enemyGraphic)
        {
            Cancel();skill=data;if(!Configured)return;EnsurePool();enemy=view.enemyArt.rectTransform;enemyImage=enemyGraphic as Image;
            flash.rectTransform.SetParent(enemyGraphic.transform,false);flash.rectTransform.anchorMin=Vector2.zero;flash.rectTransform.anchorMax=Vector2.one;flash.rectTransform.offsetMin=flash.rectTransform.offsetMax=Vector2.zero;
            flash.sprite=enemyImage!=null?enemyImage.sprite:null;flash.preserveAspect=enemyImage!=null&&enemyImage.preserveAspect;
        }
        public void Cast(Vector2 to){if(!Configured||!isActiveAndEnabled)return;ClearVisual();target=to;view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,layer,out source);age=0;HitsShown=PeakImages=0;for(int i=0;i<contacts.Length;i++)contacts[i]=-1;Render();}
        public void Hit(){if(!Active||HitsShown>=contacts.Length)return;contacts[HitsShown++]=age;flashAge=0;if(HitsShown>=HitCount)finalAge=0;Render();}
        Sprite Frame(int row,float phase,bool loop=false)=>frames[row*8+Mathf.Clamp((int)((loop?Mathf.Repeat(phase,1):Mathf.Clamp01(phase))*8),0,7)];
        void Show(Sprite sprite,Vector2 p,Vector2 size,Color c,float angle=0,bool back=false,bool additive=false)
        {
            if(c.a<=0||used>=Capacity)return;var im=pool[used++];im.sprite=sprite;im.material=additive?glow:null;im.color=c;var r=im.rectTransform;var parent=back?rear:front;if(r.parent!=parent)r.SetParent(parent,false);r.SetAsLastSibling();r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round((p.y+140)/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);
        }
        static Color Tint(float a,float hue=0)=>new Color(1-hue*.12f,1-hue*.26f,1,a);
        Vector2 Spawn(int i){switch(i%6){case 0:return new Vector2(-202,-220);case 1:return new Vector2(205,-365);case 2:return new Vector2(-150,-570);case 3:return new Vector2(130,-155);case 4:return source+new Vector2(-90,-20);default:return new Vector2(210,-540);}}
        Vector2 Path(int i,float now,out float scale,out bool back)
        {
            int n=Mathf.Max(2,HitCount-1),spirits=(n+1)/2;int id=i%spirits;float hit1=skill.initialHitDelay+id*skill.hitInterval,hit2=hit1+spirits*skill.hitInterval;
            float hit=now<hit1+.13f?hit1:hit2;bool second=hit==hit2;var start=Spawn(id);if(second)start=target+(target-start).normalized*170+new Vector2((id%2==0?1:-1)*35,20);
            float dt=now-hit;var direction=(target-start).normalized;var side=new Vector2(-direction.y,direction.x);
            Vector2 p;if(dt<=0){float t=Mathf.Clamp01((now-(hit-.28f))/.28f);float eased=t*t*(3-2*t);p=Vector2.Lerp(start,target,eased)+side*Mathf.Sin(t*Mathf.PI)*(id%2==0?75:-75);}
            else p=target+direction*Mathf.Min(185,dt*1000)+side*Mathf.Sin(Mathf.Min(1,dt/.25f)*Mathf.PI)*45;
            back=id%3==0;scale=(back?42:66)+(id%3==2?22:0)+Mathf.Sin(Mathf.Clamp01((dt+.28f)/.5f)*Mathf.PI)*(back?10:36);
            float finalDue=skill.initialHitDelay+(HitCount-1)*skill.hitInterval;
            if(now>finalDue-.1f){float t=Mathf.Clamp01((now-(finalDue-.1f))/.1f);p=Vector2.Lerp(p,target,t*t);scale*=1-t*.7f;}
            return p;
        }
        void Spirit(Vector2 p,Vector2 direction,float size,float opacity,int id,bool back,float now,bool trail=true)
        {
            float angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90;
            if(trail)Show(Frame(1,now*1.8f+id*.13f,true),p-direction.normalized*size*.65f,new Vector2(size*.95f,size*2.6f),Tint(opacity*.7f,id%3*.4f),angle,back);
            Show(Frame(0,now*1.8f+id*.17f,true),p,new Vector2(size,size*1.35f),Tint(opacity,id%3*.3f),angle,back);
            Show(Frame(4,now*2+id*.1f,true),p+direction.normalized*size*.16f,Vector2.one*size*.8f,Tint(opacity*.8f),angle,back,true);
        }
        void Render()
        {
            if(pool==null)return;foreach(var im in pool)im.gameObject.SetActive(false);used=0;if(!Configured)return;
            float due=skill.initialHitDelay+(HitCount-1)*skill.hitInterval;
            if(Active){
                float dim=.28f*Mathf.Clamp01(age/.12f)*Mathf.Clamp01((Duration-age)/.35f);
                Show(null,new Vector2(0,-398),new Vector2(476,515),new Color(.03f,.025f,.14f,dim),0,true);
                if(finalAge<0){int count=Mathf.Max(1,HitCount/2);for(int i=0;i<count;i++){
                    float alpha=Mathf.Clamp01((age-i*.023f)/.12f);float size;bool back;var p=Path(i,age,out size,out back);float ignore;bool b;var prev=Path(i,age-.022f,out ignore,out b);var dir=p-prev;if(dir.sqrMagnitude<.1f)dir=target-p;
                    for(int k=6;k>=1;k--){var old=Path(i,age-k*.024f,out ignore,out b);var next=Path(i,age-(k-1)*.024f,out ignore,out b);var tangent=next-old;if(tangent.sqrMagnitude<4)continue;Show(Frame(1,age*2+i*.2f,true),(old+next)*.5f,new Vector2(size*(.7f-k*.065f),tangent.magnitude+75),Tint(alpha*(.90f-k*.09f),.8f),Mathf.Atan2(tangent.y,tangent.x)*Mathf.Rad2Deg-90,back); }
                    Spirit(p,dir,size,alpha,i,back,age);
                    Spirit(p+new Vector2(Mathf.Sin(age*14+i)*24,Mathf.Cos(age*11+i)*18),dir,size*.32f,alpha*.7f,i+1,!back,age,false);
                }
                if(age>due-.1f){float t=Mathf.Clamp01((age-(due-.1f))/.1f);Show(Frame(4,0),target,Vector2.one*Mathf.Lerp(160,46,t),Tint(1),0,false,true);}
                }
                for(int i=0;i<HitsShown- (finalAge>=0?1:0);i++){float h=age-contacts[i];if(h<.14f){Show(Frame(2,h/.14f),target+new Vector2((i%3-1)*12,0),new Vector2(95,85),Tint(1,i%2*.6f),i*47);Show(Frame(4,h/.14f),target,Vector2.one*80,Tint(.8f),0,false,true);}}
                if(finalAge>=0)Explosion(finalAge);
            }
            if(battle!=null&&battle.DodgeCooldownReleased){view.TryGetCharacterAnchor(DragonVisualAnchor.CharacterCenter,layer,out var center);float fade=Mathf.Clamp01((float)battle.DodgeReleaseRemaining/.5f);for(int i=0;i<3;i++){float a=age*2+i*2.094f;var p=center+new Vector2(Mathf.Cos(a)*78,Mathf.Sin(a)*30);Spirit(p,new Vector2(-Mathf.Sin(a),Mathf.Cos(a)*.35f),28+Mathf.Sin(a)*6,fade,i,Mathf.Sin(a)>0,age);if(fade<.4f)Show(Frame(4,1-fade),p,Vector2.one*44,Tint(fade),0,false,true);}}
            PeakImages=Mathf.Max(PeakImages,used);
        }
        void Explosion(float h)
        {
            float t=Mathf.Max(0,h-.05f); // 50 ms VFX-only hit hold; simulation/cooldowns/swipes keep running.
            if(t<.46f){Show(Frame(2,t/.46f),target,new Vector2(460,405),Tint(1,.9f),12,true);Show(Frame(3,t/.46f),target+new Vector2(0,-35),new Vector2(470,305),Tint(1,.6f),0,true);}
            if(t<.3f){Show(Frame(2,t/.3f),target,new Vector2(355,335),Tint(1,.35f),-22);Show(Frame(2,t/.3f),target,new Vector2(245,225),Tint(.8f),18,false,true);}
            if(t<.12f)Show(Frame(4,t/.12f),target,new Vector2(390,330),Tint(1),0,false,true);
            for(int i=0;i<18;i++){float a=i*2.4f,z=Mathf.Sin(a),life=Mathf.Clamp01((t+.035f)/.55f),depth=i%3==0?.65f:1+life*.9f;var p=target+new Vector2(Mathf.Cos(a)*200,Mathf.Sin(a)*125)*life*depth;float alpha=Mathf.Clamp01((.55f-t)/.23f);Spirit(p,new Vector2(Mathf.Cos(a),z),((i%3==0?26:38)*depth),alpha*Mathf.Clamp01(t/.05f),i,i%3==0,age);}
            if(t<.5f)for(int i=0;i<8;i++){float a=i*2.4f,life=t/.5f;var p=target+new Vector2(Mathf.Cos(a)*220*life,Mathf.Sin(a)*120*life-50*life*life);Show(iceStyle.Get(IceStyleModule.ShardMedium),p,new Vector2(24+life*20,42+life*28),Tint(Mathf.Clamp01((.5f-t)/.18f)),i*37+t*170,i%3==0);}
            if(t<.55f)for(int i=0;i<7;i++){float a=i*.897f;var p=target+new Vector2(Mathf.Cos(a)*150,Mathf.Sin(a)*65)*t;Show(Frame(4,Mathf.Repeat(age+i*.17f,1)),p,Vector2.one*35,Tint((.55f-t)*1.2f),0,false,true);}
        }
        public void Step(BattleModel model,float delta)
        {
            battle=model;if(offsetApplied&&arena!=null)arena.rectTransform.anchoredPosition=arenaNormal;offsetApplied=false;
            if(!Configured)return;if(model.Result!=BattleResult.Fighting){Cancel();return;}
            age+=delta;if(finalAge>=0)finalAge+=delta;flashAge+=delta;Render();
            if(flash!=null){bool on=flashAge<(finalAge>=0?.12f:.045f);flash.gameObject.SetActive(on);if(on){flash.sprite=enemyImage!=null?enemyImage.sprite:null;flash.color=new Color(.82f,.96f,1,finalAge>=0?.95f:.7f);}}
            if(finalAge>=0&&finalAge<.19f){enemyNormal=enemy.anchoredPosition;arenaNormal=arena.rectTransform.anchoredPosition;float decay=1-finalAge/.19f;var shift=new Vector2(Mathf.Round(Mathf.Sin(finalAge*140)*7*decay),Mathf.Round(Mathf.Cos(finalAge*113)*4*decay));enemy.anchoredPosition+=shift;arena.rectTransform.anchoredPosition+=shift;offsetApplied=true;}
        }
        void ClearVisual(){if(offsetApplied){if(enemy!=null)enemy.anchoredPosition=enemyNormal;if(arena!=null)arena.rectTransform.anchoredPosition=arenaNormal;}offsetApplied=false;age=99;finalAge=-1;flashAge=99;used=0;if(pool!=null)foreach(var im in pool)if(im!=null)im.gameObject.SetActive(false);if(flash!=null)flash.gameObject.SetActive(false);}
        public void Cancel(){ClearVisual();battle?.CancelDodgeRelease();}
        void OnDisable()=>Cancel();
        void OnDestroy(){Cancel();if(front!=null)Destroy(front.gameObject);if(rear!=null)Destroy(rear.gameObject);if(flash!=null)Destroy(flash.gameObject);}
    }
}
