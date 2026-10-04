using UnityEngine;
namespace DragonTower
{
    // Presentation only. Every contact is driven by the existing combat cue.
    public sealed partial class EarthSkillVfx : MonoBehaviour
    {
        BattleView view; EarthSkillLibrary library; EarthStylePrototype painter;
        SkillData skill; EarthSkillKind kind;
        Vector2 origin,sourceGround,target,ground;
        float age=100,lastHit=100;
        readonly float[] hitTimes=new float[64];
        public bool DamageFromSkill {get;set;}
        public int ScheduledHits {get;set;}
        public int HitsShown {get;private set;}
        public bool Configured {get;private set;}
        public int ActiveImages=>painter==null?0:painter.ActiveCount;
        public int PeakImages=>painter==null?0:painter.PeakImages;
        public int DroppedLayers=>painter==null?0:painter.DroppedLayers;
        public Vector2 LastOrigin=>origin;
        int Count=>Mathf.Max(skill==null?1:skill.hitCount,ScheduledHits);
        float End=>skill==null?0:skill.initialHitDelay+(Count-1)*skill.hitInterval;
        public bool Active=>Configured&&age<End+2.4f;

        public void Initialize(BattleView owner){view=owner;library=Resources.Load<EarthSkillLibrary>("EarthSkills/Library");}
        public void Configure(SkillData data)
        {
            Clear();skill=data;Configured=library!=null&&library.Find(data,out kind);
            if(Configured&&painter==null){painter=gameObject.AddComponent<EarthStylePrototype>();painter.style=library.style;painter.Initialize(view);}
        }
        public void Cast()
        {
            if(!Configured)return;Clear();
            view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out origin);
            view.TryGetCharacterAnchor(DragonVisualAnchor.GroundPosition,view.frame,out sourceGround);
            target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));
            ground=target+new Vector2(0,-70);painter.Play(EarthSample.GroundImpact,origin,ground);age=0;Render();
        }
        public void Hit(bool first)
        {
            if(!Configured)return;if(first||!Active)Cast();
            if(HitsShown<hitTimes.Length)hitTimes[HitsShown++]=age;
            lastHit=0;Render();
        }
        public void Step(BattleModel battle,float delta)
        {
            if(!Configured)return;
            if(battle==null||battle.Result!=BattleResult.Fighting){Clear();return;}
            age+=delta;lastHit+=delta;Render();
        }
        public void Clear(){age=lastHit=100;HitsShown=0;if(painter!=null)painter.Clear();}
        void OnDisable()=>Clear();
        static float Fade(float t,float start,float end)=>Mathf.Clamp01((end-t)/(end-start));
        void Shape(int row,float phase,Vector2 p,Vector2 size,float alpha=1,float angle=0,int depth=1,float reveal=1)
            =>painter.SpriteLayer(library.Shape(row,phase),p,size,alpha,depth,angle,reveal);
        void Layer(EarthModule m,float phase,Vector2 p,Vector2 size,float a=1,int depth=1,float angle=0)
            =>painter.Layer(m,phase,p,size,a,depth,angle);
        void Contact(float h,float size,int seed)
        {
            if(h<0||h>.16f)return;
            Layer(EarthModule.Impact,h/.16f,target,new Vector2(size,size*.8f),Fade(h,.035f,.16f),1,seed*37);
        }
        void Render()
        {
            if(painter==null)return;painter.BeginLayers(age,kind==EarthSkillKind.Screw);
            if(Active)
            {
                if(HitsShown==0)painter.Crack(ground,age,.8f);
                switch(kind){case EarthSkillKind.Slam:RenderSlam();break;case EarthSkillKind.Claw:RenderClaw();break;case EarthSkillKind.Quake:RenderQuake();break;case EarthSkillKind.Screw:RenderScrew();break;}
                bool final=HitsShown>=Count;
                float amp=kind==EarthSkillKind.Screw?(final?10:1.8f):kind==EarthSkillKind.Quake?(final?6:2):kind==EarthSkillKind.Claw?(final?4:1.4f):3.5f;
                if(HitsShown>0)painter.Impulse(lastHit,amp,final?.16f:.10f);
            }
            painter.EndLayers();
        }
        void RenderSlam()
        {
            for(int i=0;i<HitsShown;i++)
            {
                float h=age-hitTimes[i];if(h>1.75f)continue;
                Contact(h,110,i);
                // Immediate data has no pre-cast window. The blade already touches the
                // enemy on T=0 and finishes its short growth without delaying damage.
                float held=Mathf.Max(0,h-.033f),grow=.72f+.28f*Mathf.Clamp01(held/.065f);
                float fade=Fade(h,.55f,1.2f);
                painter.Crack(ground,h,1.05f);
                Shape(0,0,ground+new Vector2(7,108),new Vector2(145,260),fade,-9,1,grow);
                painter.Impact(ground,held,.8f,i);
                for(int j=0;j<3;j++)painter.Rock(ground+new Vector2((j-1)*39,14),new Vector2(52,32),j*.13f,fade,1,j*23);
            }
        }
        // Implemented in the remaining batch stages; no cross-skill fallback.
        void RenderClaw()
        {
            if(HitsShown==0)return;
            float a=age-hitTimes[0],slashFade=Fade(a,.28f,.79f);
            Vector2 direction=new Vector2(-.76f,.65f),normal=new Vector2(-direction.y,direction.x);
            for(int j=0;j<3;j++)
            {
                float t=Mathf.Max(0,a-j*.012f),phase=Mathf.Clamp01(t/.075f);
                var p=target+normal*((j-1)*43)+direction*(j==1?5:-9);
                var size=new Vector2(j==1?100:82,j==1?324:278);
                if(a>.055f)Shape(1,1,p-direction*17,size,slashFade*.36f,49);
                Shape(1,phase,p,size,slashFade,49);
                Shape(2,phase,p+normal*3,size*.98f,slashFade,49);
                Shape(3,phase,p+normal*5,size*.97f,slashFade,49);
                // Dust follows the stone edge, never leads it. Staggered lumps
                // retain direction and expose the dark rock thickness.
                for(int k=0;k<6;k++)
                {
                    float h=a-.045f-k*.016f;if(h<0||h>.55f)continue;
                    var q=p+direction*((k/5f-.5f)*225)-normal*(15+h*37);
                    painter.Dust(q,new Vector2(44+h*35,27+h*21),h/.55f,Fade(h,.1f,.55f)*.70f,0);
                }
            }
            for(int i=0;i<HitsShown;i++)
            {
                float h=age-hitTimes[i];bool final=i==Count-1;
                Contact(h,final?150:85,i);
                var p=target+direction*((i/(float)Mathf.Max(1,Count-1)-.5f)*150);
                painter.Debris(p,h-.025f,final?.72f:.46f,i+7,false);
                if(final)painter.Shockwave(ground,Mathf.Max(0,h-.04f),.90f,8);
            }
        }
        void RenderQuake()
        {
            if(HitsShown==0)return;
            float a=age-hitTimes[0];
            painter.Crack(ground,a,1.45f);
            for(int i=0;i<HitsShown;i++)
            {
                float h=age-hitTimes[i];bool final=i==Count-1;
                if(h>1.8f)continue;
                Contact(h,final?185:130,i);
                float t=Mathf.Max(0,h-(final?.045f:.02f));
                painter.Impact(ground+new Vector2((i%3-1)*35,0),t,final?1.3f:.85f,20+i);
                // Vertical force: broad ground plates rise first, the narrow fast
                // debris follows, and gravity returns it. No flame-shaped envelope.
                for(int j=0;j<22;j++)
                {
                    float q=t-j%4*.018f;if(q<0||q>1.15f)continue;
                    float vx=Mathf.Sin(j*2.399f+i)*83;
                    float vy=260+j%5*38+(final?70:0);
                    var p=ground+new Vector2((j%7-3)*25+vx*q,18+vy*q-300*q*q);
                    if(p.y<ground.y-12)continue;
                    float size=j%5==0?53:15+j%4*8;
                    painter.Rock(p,new Vector2(size,size*(j%3==0?.60f:1)),q+j*.16f,Fade(q,.75f,1.15f),j%4==0?0:1,j*23+q*85);
                }
                for(int j=0;j<5;j++)
                {
                    float q=t-.025f-j*.018f;if(q<0||q>.86f)continue;
                    var p=ground+new Vector2((j-2)*(41+q*17),q*145);
                    painter.Dust(p,new Vector2(78+q*75,56+q*133),q/.86f,Fade(q,.27f,.86f)*.70f,j%2==0?0:1);
                }
                if(final)for(int j=0;j<3;j++)painter.Rise(ground+new Vector2((j-1)*90,-6),t+.14f,.6f+j%2*.15f,30+j);
            }
        }
        void RenderScrew()
        {
            float a=HitsShown>0?age-hitTimes[0]:age;
            Vector2 from=origin+new Vector2(0,12),to=target;
            Vector2 axis=(to-from).normalized,normal=new Vector2(-axis.y,axis.x);
            float lead=Mathf.Clamp01(a/.19f),tail=Mathf.Max(0,lead-.72f);
            float bodyFade=Fade(a,.18f,.35f);
            if(bodyFade>0)
            {
                // A perspective helix of solids, pressure arcs and dust. Back faces
                // are smaller/darker; foreground stones cross in front of the tube.
                for(int ring=0;ring<7;ring++)
                {
                    float u=ring/6f,along=Mathf.Lerp(tail,lead,u);
                    var center=Vector2.Lerp(from,to,along);
                    float radius=(40+Mathf.Sin(u*Mathf.PI)*60)*( .72f+.28f*lead);
                    float rotation=a*27-ring*.84f;
                    for(int j=0;j<14;j++)
                    {
                        float angle=j*Mathf.PI*2/14+rotation;
                        float z=Mathf.Sin(angle);bool front=z<0;
                        var p=center+normal*(Mathf.Cos(angle)*radius)+axis*(z*radius*.30f);
                        int depth=front?2:0;
                        float size=(front?35:23)*(1-u*.26f);
                        if(j%2==0)painter.Rock(p,new Vector2(size*1.45f,size),a*1.7f+j*.1f,bodyFade*(front?1:.68f),depth,angle*Mathf.Rad2Deg);
                        else painter.Dust(p,new Vector2(size*1.9f,size*.94f),Mathf.Repeat(a*2+j*.12f,1),bodyFade*(front?.76f:.4f),depth);
                        if(j%3==0)Layer(EarthModule.Grain,0,p+normal*3,new Vector2(front?15:8,5),bodyFade*(front?.90f:.5f),depth,angle*Mathf.Rad2Deg);
                    }
                    Shape(4,Mathf.Repeat(a*4+u*.2f,1),center,new Vector2(radius*2.3f,radius*.80f),bodyFade*.95f,Mathf.Atan2(normal.y,normal.x)*Mathf.Rad2Deg,1);
                }
            }
            // Persistent ground path, revealed towards the target. This secondary
            // deformation never generates damage, numbers, statuses or combat cues.
            for(int j=0;j<8;j++)
            {
                float h=a-j*.018f;if(h<0)continue;
                var p=Vector2.Lerp(sourceGround,ground,(j+1)/8f);
                painter.Crack(p,h,.63f,Fade(a,1.25f,2.1f));
            }
            for(int i=0;i<HitsShown;i++)
            {
                float h=age-hitTimes[i];bool final=i==Count-1;
                Contact(h,final?232:82,i);
                float u=(i+1)/(float)Count;
                var p=Vector2.Lerp(sourceGround,ground,u);
                float t=h-(i==0?.075f:0);if(t<0)continue;
                if(final)
                {
                    float held=Mathf.Max(0,t-.055f);
                    painter.Impact(ground,held,1.78f,61);
                    for(int j=0;j<3;j++)painter.Rise(ground+new Vector2((j-1)*87,j%2*15),held+.14f-j*.025f,j==1?1.32f:.90f,61+j);
                    // Foreground slabs continue outward after the central mass stops.
                    for(int j=0;j<6;j++)
                    {
                        float q=held-.065f-j*.012f;if(q<0||q>.68f)continue;
                        float side=j%2==0?-1:1;
                        var near=ground+new Vector2(side*(70+q*(190+j*18)),35+q*(110+j*14)-240*q*q);
                        painter.Rock(near,new Vector2(58,43)*(1+q),q+j*.16f,Fade(q,.36f,.68f),2,j*31+q*130);
                    }
                }
                else
                {
                    painter.Impact(p,t,.58f+u*.46f,50+i);
                    painter.Rise(p,t+.14f,.38f+u*.28f,50+i);
                }
            }
        }
    }
}
