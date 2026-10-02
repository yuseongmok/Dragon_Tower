using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public sealed partial class IceSkillVfx
    {
        readonly Sprite[] ballFrames=new Sprite[48];Material ballHighlight;RectTransform ballRear;
        bool ballBack,ballGlow;Color ballTint=Color.white;
        void InitializeBallDepth()
        {
            var frames=Resources.LoadAll<Sprite>("IceSkills/BallDepth/DepthFrames");for(int row=0;row<6;row++)for(int f=0;f<8;f++){string key="Depth"+row+"_"+f;foreach(var sprite in frames)if(sprite.name==key){ballFrames[row*8+f]=sprite;break;}}
            ballHighlight=Resources.Load<Material>("IceSkills/BallDepth/Highlight");
            var go=new GameObject("Ice ball rear depth",typeof(RectTransform),typeof(RectMask2D));ballRear=go.GetComponent<RectTransform>();ballRear.SetParent(view.frame,false);ballRear.anchorMin=ballRear.anchorMax=new Vector2(.5f,1);ballRear.pivot=new Vector2(.5f,1);ballRear.anchoredPosition=new Vector2(0,-155);ballRear.sizeDelta=new Vector2(456,490);ballRear.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex());
        }
        Sprite BallFrame(int row,float phase,bool loop=false)=>ballFrames[row*8+Mathf.Clamp((int)((loop?Mathf.Repeat(phase,1):Mathf.Clamp01(phase))*8),0,7)];
        void BallLayer(Sprite sprite,Vector2 point,Vector2 size,float alpha=1,float angle=0,bool back=false,bool glow=false,float brightness=1)
        {
            ballBack=back;ballGlow=glow;ballTint=new Color(brightness,brightness,1,1);Draw(sprite,point,size,alpha,angle);ballBack=ballGlow=false;ballTint=Color.white;
        }
        void BallOrbit(Vector2 point,float radius,float phase,bool near,float alpha=1,bool behindEnemy=false)
        {
            for(int j=0;j<6;j++){float a=phase+j*Mathf.PI/3,z=Mathf.Sin(a);if((z>=0)!=near)continue;float perspective=1+z*.28f;var p=point+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.30f);float width=Mathf.Max(.32f,Mathf.Abs(Mathf.Cos(a*.7f)));
                BallLayer(library.style.Get(IceStyleModule.ShardLarge),p,new Vector2(42*perspective*width,62*perspective),alpha,Mathf.Round((-a*Mathf.Rad2Deg+25)/15)*15,behindEnemy,false,near?1:.58f);}
        }
        void RenderBall()
        {
            if(ballFrames[0]==null){RenderBallLegacy();return;}
            float first=hitAt[0]<0?-1:age-hitAt[0],second=hitAt[1]<0?-1:age-hitAt[1];
            float spin=Mathf.Floor(age*15)/15*13;
            if(first<0)
            {
                float gather=Mathf.Min(.10f,skill.initialHitDelay*.4f);if(age<=gather+.034f)origins[0]=Origin();var from=origins[0];float travel=Mathf.Clamp01((age-gather)/Mathf.Max(.01f,skill.initialHitDelay-gather));float t=travel*travel*(3-2*travel);var point=Vector2.Lerp(from,target,t)+new Vector2(-28*Mathf.Sin(t*Mathf.PI),0);float charge=Mathf.Clamp01(age/gather);float size=Mathf.Lerp(26,158,charge)+Mathf.Sin(t*Mathf.PI)*30;
                for(int k=4;k>=1;k--){float old=Mathf.Clamp01(t-k*.065f);var p=Vector2.Lerp(from,target,old)+new Vector2(-28*Mathf.Sin(old*Mathf.PI),0);BallLayer(library.style.Get(k%2==0?IceStyleModule.FrostA:IceStyleModule.FrostB),p,new Vector2(size*.8f,size*.42f),charge*.20f*(1-k*.16f),k*29);}
                BallOrbit(point,size*.39f,spin,false,charge);BallLayer(BallFrame(5,0),point,Vector2.one*size,.42f,0,false,true);BallLayer(BallFrame(0,age*1.8f,true),point,new Vector2(size,size*(1-.16f*Mathf.Sin(t*Mathf.PI))),.9f);
                BallOrbit(point,size*.39f,spin,true,charge);BallLayer(BallFrame(5,Mathf.Repeat(age*4,1)),point+new Vector2(-size*.15f,size*.2f),Vector2.one*(size*.75f),.78f,0,false,true);
                if(age<gather)for(int j=0;j<5;j++){float a=j*2.4f;var p=point+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(80*(1-charge)+12);BallLayer(library.style.Get(IceStyleModule.ShardSmall),p,Vector2.one*22,charge);}
            }
            else if(second<0)
            {
                float phase=Mathf.Clamp01(first/Mathf.Max(.03f,skill.hitInterval));float squeeze=Mathf.Exp(-first*28);float size=Mathf.Lerp(156,124,phase);
                BallLayer(BallFrame(4,phase*.65f),target,new Vector2(252,214),.85f,0,true);BallOrbit(target,85-18*phase,spin,false,1,true);
                BallLayer(BallFrame(1,phase),target,new Vector2(size*(1+.48f*squeeze),size*(1-.38f*squeeze)),1);
                BallOrbit(target,85-18*phase,spin,true);BallLayer(BallFrame(5,phase*.5f),target,Vector2.one*(110+phase*70),.55f+phase*.3f,0,false,true);
                if(first<.10f)BallLayer(BallFrame(2,first/.10f),target,new Vector2(180,120),1,0,false,true);
            }
            else
            {
                float t=second;
                if(t<.30f){BallLayer(BallFrame(4,t/.30f),target,new Vector2(360,280),1,0,true);BallLayer(BallFrame(3,t/.30f),target+new Vector2(0,-28),new Vector2(360,230),1,12,true);}
                // Depth groups cross the monster's silhouette, not just an orb's own painted shadow.
                for(int pass=0;pass<2;pass++)for(int j=0;j<12;j++){
                    bool back=j%3==0;if(back!=(pass==0)||t>.56f)continue;float a=(j*137.5f+25)*Mathf.Deg2Rad;float life=t/.56f;float perspective=back?1-life*.50f:1+life*1.15f;float distance=(15+life*(back?125:175))*perspective;var p=target+new Vector2(Mathf.Cos(a)*distance,Mathf.Sin(a)*distance*.58f-life*life*42);float width=Mathf.Max(.3f,Mathf.Abs(Mathf.Cos(j+t*9)));float size=(j%3==1?48:34)*perspective;
                    BallLayer(library.style.Get(j%2==0?IceStyleModule.ShardLarge:IceStyleModule.ShardMedium),p,new Vector2(size*width,size*1.35f),Mathf.Clamp01((.56f-t)/.23f),Mathf.Round((j*37+t*(back?-150:190))/15)*15,back,false,back?.58f:1);}
                if(t<.22f){BallLayer(BallFrame(2,t/.22f),target,new Vector2(320,280),1);BallLayer(BallFrame(2,t/.22f),target,new Vector2(238,215),.6f,18,false,true);}
                if(t<.32f)BallLayer(BallFrame(3,t/.32f),target+new Vector2(0,-15),new Vector2(360,245),.85f,-8);
                if(t<.09f)BallLayer(BallFrame(5,t/.09f),target,new Vector2(250,210),1,0,false,true);
                if(t>.10f&&t<.58f)for(int j=0;j<5;j++){float a=j*1.257f;var p=target+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.5f)*(50+t*100);BallLayer(library.style.Get(j%2==0?IceStyleModule.FrostA:IceStyleModule.FrostB),p,new Vector2(90,48),(.58f-t)*.9f,j*31);}
            }
        }
    }
}
