using UnityEngine;
namespace DragonTower
{
    public sealed partial class IceSkillVfx
    {
        readonly Sprite[] elementFrames=new Sprite[24];
        void InitializeElementDepth()
        {
            var frames=Resources.LoadAll<Sprite>("IceSkills/ElementDepth/Frames");
            for(int row=0;row<3;row++)for(int f=0;f<8;f++)foreach(var sprite in frames)
                if(sprite.name=="Element"+row+"_"+f){elementFrames[row*8+f]=sprite;break;}
        }
        Sprite ElementFrame(int row,float phase)=>elementFrames[row*8+Mathf.Clamp((int)(Mathf.Repeat(phase,1)*8),0,7)];
        // Real hit events own the clock. These layers never produce damage, status or hit cues.
        void DepthImpact(float h,float size,int count,int seed,bool strong)
        {
            if(h<0||h>.43f)return;
            if(h<.22f)BallLayer(BallFrame(3,h/.22f),target+new Vector2(0,-12),new Vector2(size*1.4f,size*.85f),.9f,0,true);
            if(h<.15f){BallLayer(BallFrame(2,h/.15f),target,new Vector2(size,size*.8f),1);BallLayer(BallFrame(5,h/.15f),target,Vector2.one*size,.85f,0,false,true);}
            for(int j=0;j<count;j++){
                bool back=j%3==0;float t=h/.43f,a=(j*137.5f+seed*29)*Mathf.Deg2Rad,perspective=back?1-t*.35f:1+t*.8f;
                var p=target+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.65f)*(12+t*size*.65f*perspective)+Vector2.down*(t*t*28);
                float width=.3f+.7f*Mathf.Abs(Mathf.Cos(j+h*15));
                BallLayer(library.style.Get(IceStyleModule.ShardMedium),p,new Vector2(22*perspective*width,(strong?42:30)*perspective),Mathf.Clamp01((.43f-h)/.16f),j*43+h*180,back,false,back?.6f:1);
            }
        }
        void RenderShot()
        {
            if(elementFrames[0]==null){RenderShotLegacy();return;}
            for(int i=0;i<Mathf.Min(skill.hitCount,12);i++){
                float due=skill.initialHitDelay+i*skill.hitInterval,flight=Mathf.Min(.18f,skill.initialHitDelay),start=due-flight;
                if(age<start){origins[i]=Origin();continue;}if(age<=start+.034f)origins[i]=Origin();
                float h=hitAt[i]<0?-1:age-hitAt[i];
                if(h<0){float t=Mathf.Clamp01((age-start)/flight);var from=origins[i]+new Vector2((i%3-1)*25,0);var delta=target-from;var dir=delta.normalized;var side=new Vector2(-dir.y,dir.x);float bend=(i%3-1)*23*Mathf.Sin(t*Mathf.PI);var p=Vector2.Lerp(from,target,t*t*(3-2*t))+side*bend;float angle=Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90;float width=(i==2?80:68)*(1+.15f*Mathf.Sin(t*Mathf.PI));
                    for(int k=2;k>=1;k--)BallLayer(ElementFrame(0,age*2+k*.1f),p-dir*(k*24),new Vector2(width*(1-k*.18f),125),.26f/k,angle,false,false,.55f);
                    BallLayer(ElementFrame(0,age*2+i*.3f),p,new Vector2(width,146),1,angle);
                    BallLayer(BallFrame(5,Mathf.Repeat(age*4,1)),p+dir*38,new Vector2(width*.8f,85),.7f,angle,false,true);
                    for(int j=0;j<2;j++)BallLayer(library.style.Get(IceStyleModule.ShardSmall),p-dir*(35+j*17)+side*Mathf.Sin(age*35+j*3)*18,new Vector2(15,32),.8f,angle);
                }else DepthImpact(h,i==2?132:108,i==2?6:4,i,i==2);
            }
        }
        void RenderBreath()
        {
            if(elementFrames[8]==null){RenderBreathLegacy();return;}
            var from=Origin();var delta=target-from;var dir=delta.normalized;var side=new Vector2(-dir.y,dir.x);float angle=Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90;
            float end=skill.initialHitDelay+(skill.hitCount-1)*skill.hitInterval,fade=Mathf.Clamp01((end+.14f-age)/.14f)*Mathf.Clamp01(age/.06f),growth=Mathf.Clamp01(age/Mathf.Max(.05f,skill.initialHitDelay));
            if(fade>0){
                // Moving cross-sections make a turbulent volume, widening toward the enemy.
                for(int j=0;j<7;j++){float t=(j+.5f)/7;if(t>growth)continue;float pulse=1+.14f*Mathf.Sin(age*27-j*1.7f);var p=Vector2.Lerp(from,target,t)+side*Mathf.Sin(age*19-j)*t*12;float width=(28+t*150)*pulse;
                    BallLayer(ElementFrame(1,age*1.8f-j*.13f),p,new Vector2(width,delta.magnitude/5+45),fade*.9f,angle);
                    if(j%2==0)BallLayer(ElementFrame(1,age*1.8f-j*.13f+.3f),p-side*width*.1f,new Vector2(width*.40f,delta.magnitude/5+25),fade*.48f,angle,false,true);
                }
                for(int j=0;j<5;j++){float t=Mathf.Repeat(age*2+j*.2f,1);if(t>growth)continue;var p=Vector2.Lerp(from,target,t)+side*Mathf.Sin(age*15+j*2)*(12+t*45);BallLayer(library.style.Get(j%2==0?IceStyleModule.FrostA:IceStyleModule.FrostB),p,new Vector2(40+t*90,42+t*45),fade*.5f,angle+j*39);}
                for(int j=0;j<8;j++){float t=Mathf.Repeat(age*2.4f+j/8f,1);if(t>growth)continue;float z=Mathf.Sin(t*10+j);var p=Vector2.Lerp(from,target,t)+side*z*(15+t*60);float scale=(20+t*18)*(1+z*.24f);
                    BallLayer(library.style.Get(IceStyleModule.ShardMedium),p,new Vector2(scale*.6f,scale*1.5f),fade,angle+j*17,z<0&&t>.8f,false,z<0?.6f:1);}
                BallLayer(BallFrame(3,Mathf.Repeat(age*4,1)),from,new Vector2(100,55),fade,angle);
            }
            // Five short contact pulses, without five oversized overlapping explosions.
            for(int j=0;j<HitsShown;j++){float h=age-hitAt[j];if(j==skill.hitCount-1)DepthImpact(h,195,9,j,true);
                else if(h<.15f){BallLayer(BallFrame(4,h/.15f),target,new Vector2(215,175),1,0,true);BallLayer(BallFrame(2,h/.15f),target,new Vector2(124,110),1);BallLayer(BallFrame(5,h/.15f),target,Vector2.one*130,.7f,0,false,true);}}
        }
        void RenderWall()
        {
            if(elementFrames[16]==null){RenderWallLegacy();return;}
            view.TryGetCharacterAnchor(DragonVisualAnchor.GroundPosition,root.parent as RectTransform,out var ground);ground+=new Vector2(25,8);var foot=target+new Vector2(0,-65);
            float final=hitAt[2]<0?-1:age-hitAt[2];
            if(final<.25f){for(int j=0;j<6;j++){float t=(j+1)/6f;if(t>age/.16f)continue;var at=Vector2.Lerp(ground,foot,t);BallLayer(library.style.Get(IceStyleModule.CrackB),at,new Vector2(65+t*35,38),.9f,-15);if(j%2==0)BallLayer(BallFrame(5,Mathf.Repeat(age*3+j*.2f,1)),at,new Vector2(75,32),.55f,0,false,true);}}
            // Individual faceted spires grow from fixed feet; rear spires pass behind the enemy.
            for(int j=0;j<5;j++){
                int hit=j%3;float due=skill.initialHitDelay+hit*skill.hitInterval,grow=Mathf.Clamp01((age-(due-.09f))/.09f);if(grow<=0)continue;
                bool back=j<2;float height=j==4?240:170+j*13;var basePoint=foot+new Vector2((j-2)*49,back?22:-25);float w=j==4?104:84;
                if(final<.11f){float pulse=final<0?1:1+.06f*Mathf.Sin(final*40);var size=new Vector2(w*pulse,height*grow);var center=basePoint+new Vector2(0,size.y*.46f);
                    BallLayer(ElementFrame(2,age*.8f+j*.19f),center,size,1,(j-2)*-7,back,false,back?.68f:1);
                    float h=hitAt[hit]<0?-1:age-hitAt[hit];if(h>=0&&h<.09f)BallLayer(BallFrame(5,h/.09f),center,new Vector2(w*1.2f,height*.8f),.9f,0,back,true);
                    if(final>=0)BallLayer(library.style.Get(IceStyleModule.CrackC),center,size,.95f,0,back,true);
                }
                if(final>=.11f&&final<.58f){float t=(final-.11f)/.47f;for(int k=0;k<3;k++){float a=(j*67+k*115)*Mathf.Deg2Rad,perspective=back?1-t*.4f:1+t;var at=basePoint+new Vector2(0,height*.55f)+new Vector2(Mathf.Cos(a)*150*t*perspective,Mathf.Sin(a)*90*t-t*t*130);float size=(32+k*9)*perspective;
                    BallLayer(library.style.Get(IceStyleModule.ShardLarge),at,new Vector2(size*(.35f+.65f*Mathf.Abs(Mathf.Cos(t*7+j))),size*1.5f),Mathf.Clamp01((.58f-final)/.18f),j*50+k*30+t*170,back,false,back?.65f:1);}}
            }
            for(int j=0;j<Mathf.Min(HitsShown,2);j++)DepthImpact(age-hitAt[j],115+j*20,3,j,false);
            if(final>=0){DepthImpact(final,300,8,2,true);if(final<.3f){BallLayer(BallFrame(4,final/.3f),target,new Vector2(395,290),1,0,true);BallLayer(BallFrame(3,final/.3f),foot,new Vector2(410,235),1);}}
        }
    }
}
