using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Presentation only: impacts are triggered by existing real damage cues.
    public sealed partial class IceSkillVfx : MonoBehaviour
    {
        const int Capacity=64;
        Image[] pool;RectTransform root;BattleView view;IceSkillLibrary library;SkillData skill;
        float age=100;readonly float[] hitAt=new float[12];readonly Vector2[] origins=new Vector2[12];
        Vector2 target;int used;bool slowed;float slowPower;
        public bool Configured=>skill!=null&&skill.icePresentation&&library!=null;
        public bool Active=>Configured&&age<skill.initialHitDelay+(skill.hitCount-1)*skill.hitInterval+.65f;
        public bool HoldCasting=>Active&&skill.iceKind==IceSkillKind.Breath&&age<skill.initialHitDelay+(skill.hitCount-1)*skill.hitInterval;
        public bool DamageFromSkill {get;set;}
        public int HitsShown{get;private set;}
        public int ActiveImages=>used;
        public int PeakImages{get;private set;}
        public Vector2 LastOrigin{get;private set;}
        public float LastImpactAge{get;private set;}
        public void Initialize(BattleView owner,RectTransform layer)
        {
            view=owner;library=Resources.Load<IceSkillLibrary>("IceSkills/Library");
            var go=new GameObject("Ice skill safe area",typeof(RectTransform),typeof(RectMask2D));root=go.GetComponent<RectTransform>();root.SetParent(layer,false);
            root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.sizeDelta=new Vector2(456,490);root.anchoredPosition=new Vector2(0,-155);
            pool=new Image[Capacity];for(int i=0;i<Capacity;i++){var obj=new GameObject("Ice pooled "+i,typeof(RectTransform),typeof(Image));var im=obj.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.raycastTarget=false;pool[i]=im;obj.SetActive(false);}
            InitializeBallDepth();
            InitializeElementDepth();
        }
        public void Configure(SkillData data){Clear();skill=data;HitsShown=0;PeakImages=0;}
        public void Cast(Vector2 at){Clear();target=at;age=0;HitsShown=0;for(int i=0;i<hitAt.Length;i++){hitAt[i]=-1;origins[i]=Origin();}Render();}
        Vector2 Origin(){view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,root.parent as RectTransform,out var p);LastOrigin=p;return p;}
        public void Hit(){if(!Configured)return;if(!Active)Cast(target);if(HitsShown>=hitAt.Length)return;hitAt[HitsShown++]=age;LastImpactAge=age;Render();}
        public void Step(BattleModel battle,float delta)
        {
            if(!Configured)return;if(battle.Result!=BattleResult.Fighting){Clear();return;}
            age+=delta;slowed=battle.EnemySlowed;slowPower=battle.EnemySlowPower;Render();
        }
        public void Clear(){age=100;slowed=false;used=0;if(pool!=null)foreach(var p in pool)if(p!=null)p.gameObject.SetActive(false);}
        void OnDisable()=>Clear();void OnDestroy(){if(root!=null)Destroy(root.gameObject);if(ballRear!=null)Destroy(ballRear.gameObject);}
        void Draw(Sprite sprite,Vector2 at,Vector2 size,float alpha=1,float angle=0,float reveal=1)
        {
            if(sprite==null||used>=Capacity||alpha<=0)return;var im=pool[used++];im.sprite=sprite;im.color=new Color(ballTint.r,ballTint.g,ballTint.b,Mathf.Clamp01(alpha));im.material=ballGlow&&ballHighlight!=null?ballHighlight:Graphic.defaultGraphicMaterial;im.type=reveal<1?Image.Type.Filled:Image.Type.Simple;im.fillMethod=Image.FillMethod.Vertical;im.fillOrigin=0;im.fillAmount=reveal;
            var r=im.rectTransform;var parent=ballBack?ballRear:root;if(r.parent!=parent)r.SetParent(parent,false);r.SetAsLastSibling();r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(Mathf.Round(at.x/2)*2,Mathf.Round((at.y+155)/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);
        }
        void Module(IceStyleModule m,Vector2 p,Vector2 size,float a=1,float rotation=0,float reveal=1)=>Draw(library.style.Get(m),p,size,a,rotation,reveal);
        void Render()
        {
            if(pool==null||!Configured)return;foreach(var p in pool)p.gameObject.SetActive(false);used=0;
            if(Active){if(skill.iceKind==IceSkillKind.Ball)RenderBall();else if(skill.iceKind==IceSkillKind.Breath)RenderBreath();else if(skill.iceKind==IceSkillKind.Wall)RenderWall();else RenderShot();}
            if(slowed){var feet=target+new Vector2(0,-65);Module(IceStyleModule.FrostRing,feet,new Vector2(110,32),.65f);Module(IceStyleModule.FrostA,feet+new Vector2(-25,9),new Vector2(46,26),.65f);Module(IceStyleModule.FrostB,feet+new Vector2(25,9),new Vector2(46,26),.6f);Module(IceStyleModule.StatusCrystal,feet+new Vector2(-47,15),Vector2.one*(22+slowPower*.08f),.8f);}
            PeakImages=Mathf.Max(PeakImages,used);
        }
        void RenderShotLegacy()
        {
            for(int i=0;i<Mathf.Min(skill.hitCount,12);i++)
            {
                float due=skill.initialHitDelay+i*skill.hitInterval,flight=Mathf.Min(.18f,skill.initialHitDelay),start=due-flight;
                if(age<start){origins[i]=Origin();continue;}
                if(age<=start+.034f)origins[i]=Origin();
                float h=hitAt[i]<0?-1:age-hitAt[i];
                if(h<0){float t=Mathf.Clamp01((age-start)/flight);var from=origins[i]+new Vector2((i%3-1)*13,0);var point=Vector2.Lerp(from,target,t);var dir=target-from;float angle=Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg-90;
                    Draw(library.Get(0),point,new Vector2(52,112),1,angle);Module(IceStyleModule.Refraction,point,new Vector2(36,76),.8f,angle);Module(IceStyleModule.FrostA,point-dir.normalized*24,new Vector2(38,48),.65f,angle);
                }else Impact(h,78,4,i);
            }
        }
        void RenderBallLegacy()
        {
            float first=hitAt[0]<0?-1:age-hitAt[0],second=hitAt[1]<0?-1:age-hitAt[1];
            if(first<0){float gather=Mathf.Min(.10f,skill.initialHitDelay*.4f);if(age<=gather+.034f)origins[0]=Origin();var from=origins[0];float t=Mathf.Clamp01((age-gather)/Mathf.Max(.01f,skill.initialHitDelay-gather));var p=Vector2.Lerp(from,target,t*t);Draw(library.Get(2),p,Vector2.one*Mathf.Lerp(42,120,Mathf.Clamp01(age/gather)));for(int j=0;j<3;j++){float a=age*7+j*2.094f;Module(IceStyleModule.ShardMedium,p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*17,Vector2.one*35,.9f,-a*Mathf.Rad2Deg);}Module(IceStyleModule.Refraction,p,Vector2.one*90,.95f);}
            else if(second<0){Draw(library.Get(2),target,Vector2.one*(120+Mathf.Min(.24f,first)*46));Module((IceStyleModule)((int)IceStyleModule.CrackA+Mathf.Min(2,(int)(first*15))),target,Vector2.one*140);Impact(first,94,4,0);}
            else{Impact(second,170,10,1);if(second<.18f){float t=second/.18f;Module(IceStyleModule.FrostRing,target,new Vector2(108+158*t,86+98*t),1-t);for(int j=0;j<3;j++){float a=j*2.094f;Module(IceStyleModule.StatusCrystal,target+new Vector2(Mathf.Cos(a)*54,Mathf.Sin(a)*43),new Vector2(65,94),1-t,j*40);}}}
        }
        void RenderBreathLegacy()
        {
            var from=Origin();var delta=target-from;float end=skill.initialHitDelay+(skill.hitCount-1)*skill.hitInterval;float fade=Mathf.Clamp01((end+.12f-age)/.12f)*Mathf.Clamp01(age/.08f);float angle=Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90;
            for(int j=0;j<6;j++){float t=(j+.5f)/6;float growth=Mathf.Clamp01(age/Mathf.Max(.05f,skill.initialHitDelay));if(t>growth)continue;var p=Vector2.Lerp(from,target,t);Draw(library.Get(4),p,new Vector2(34+t*105,delta.magnitude/5+34),fade*.85f,angle);}
            for(int j=0;j<5;j++){float t=Mathf.Repeat(age*2+j*.2f,1);var p=Vector2.Lerp(from,target,t)+new Vector2(Mathf.Sin(j*2+age*12)*t*12,0);Module(j%2==0?IceStyleModule.FrostA:IceStyleModule.ShardSmall,p,new Vector2(25+t*24,30),fade*.85f,angle);}
            for(int j=0;j<HitsShown;j++){float h=age-hitAt[j];Impact(h,j==skill.hitCount-1?126:70,j==skill.hitCount-1?8:3,j);if(h<.14f)Module(IceStyleModule.FrostB,target+new Vector2((j%2==0?-1:1)*25,0),new Vector2(88,66),1-h/.14f);}
        }
        void RenderWallLegacy()
        {
            view.TryGetCharacterAnchor(DragonVisualAnchor.GroundPosition,root.parent as RectTransform,out var ground);ground+=new Vector2(25,8);var end=target+new Vector2(0,-65);
            float final=hitAt[2]<0?-1:age-hitAt[2];
            // A short VFX-only hold: never changes global time, damage or animation clocks.
            float visualFinal=final<.05f?0:final-.05f;
            if(final<.35f){for(int j=0;j<8;j++){float t=(j+1)/8f;if(t>age/.16f)continue;var at=Vector2.Lerp(ground,end,t);Module(IceStyleModule.CrackB,at,new Vector2(48,40),final<0?.7f:1-final/.35f,-20);Module(IceStyleModule.FrostA,at,new Vector2(60,30),.5f);}}
            for(int j=0;j<3;j++){
                float due=skill.initialHitDelay+j*skill.hitInterval,growth=Mathf.Clamp01((age-(due-.065f))/.065f);if(growth<=0)continue;
                var foot=Vector2.Lerp(ground,end,(j==0?.65f:j==1?.83f:1f));float height=136+j*22;var at=foot+new Vector2(j==1?-20:10,height*.38f);var size=new Vector2(126+j*32,height);float h=hitAt[j]<0?-1:age-hitAt[j];
                if(final<.12f){Draw(library.Get(6+j),at,size,1,0,growth);if(h>=0&&h<.12f)Module(IceStyleModule.Refraction,at,size,.9f);if(final>=0)Module(IceStyleModule.CrackC,at,size,1);}
                if(final>=.12f&&final<.5f){float t=(final-.12f)/.38f;for(int k=0;k<4;k++){float a=(k*137+j*25)*Mathf.Deg2Rad;Module(k%2==0?IceStyleModule.ShardLarge:IceStyleModule.ShardMedium,at+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(15+t*90)+new Vector2(0,-t*t*25),Vector2.one*(32+j*9),1-t,k*45+t*60);}}
                if(h>=0&&j<2)Impact(h,90+j*20,4,j);
            }
            if(final>=0){Impact(visualFinal,205,10,2);if(visualFinal<.17f)Module(IceStyleModule.FrostRing,target,new Vector2(135+visualFinal*760,78+visualFinal*340),1-visualFinal/.17f);}
        }
        void Impact(float h,float size,int fragments,int seed)
        {
            if(size>=100&&h<.14f)Module(IceStyleModule.FrostRing,target,new Vector2(size*(.7f+h*4),size*(.5f+h*2)),.7f*(1-h/.14f));
            if(h<.067f)Module(IceStyleModule.Impact,target,Vector2.one*size,1-h/.09f);
            if(h<.10f)Module((IceStyleModule)((int)IceStyleModule.CrackA+Mathf.Min(2,(int)(h*30))),target,Vector2.one*size,.95f);
            if(h>=.07f&&h<.32f){float t=(h-.07f)/.25f;for(int j=0;j<fragments;j++){float a=(j*137.5f+seed*23)*Mathf.Deg2Rad;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Module(j%2==0?IceStyleModule.ShardMedium:IceStyleModule.ShardSmall,target+d*(8+t*(size*.65f))+new Vector2(0,-t*t*12),Vector2.one*(16+j%3*5),1-t,Mathf.Round((j*40+t*90)/15)*15);}}
        }
    }
}
