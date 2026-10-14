using UnityEngine;
namespace DragonTower {
 // Shared wind palette and 2px snapped geometry. Rendering never changes combat state.
 public sealed class GarudaBossSkillVfx {
  BattleView view;LightningMeshLayer floor,air;
  public int QuadCount=>(floor==null?0:floor.Count)+(air==null?0:air.Count);
  public int DroppedQuads=>(floor==null?0:floor.Overflow)+(air==null?0:air.Overflow);
  public void Initialize(BattleView v){view=v;floor=Layer("Garuda wind floor (pooled)");air=Layer("Garuda wind blades (pooled)");}
  LightningMeshLayer Layer(string name){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);var m=go.GetComponent<LightningMeshLayer>();m.raycastTarget=false;return m;}
  public void Clear(){if(floor!=null){floor.Begin();floor.End();air.Begin();air.End();}}
  public void Destroy(){if(floor!=null)Object.Destroy(floor.gameObject);if(air!=null)Object.Destroy(air.gameObject);}
  Color A(Color c,float a)=>WindVFXStyle.Alpha(c,a);
  void Depth(LightningMeshLayer m,Transform actor,bool after){int i=actor.GetSiblingIndex()+(after?1:0);if(m.transform.GetSiblingIndex()<actor.GetSiblingIndex())i--;m.transform.SetSiblingIndex(i);}
  void Arc(LightningMeshLayer m,Vector2 p,float rx,float ry,float begin,float length,float width,Color c){for(int i=0;i<18;i++){float a=begin+length*i/18,z=begin+length*(i+1)/18;m.Line(p+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),p+new Vector2(Mathf.Cos(z)*rx,Mathf.Sin(z)*ry),width,c);}}
  void Crescent(Vector2 p,float size,float tilt,float alpha){
   // Broad swept crescent with a stepped dark contour, restrained white edge.
   for(int i=0;i<24;i++){float x=(i-12)/12f,z=(i-11)/12f;var a=p+new Vector2(x*size,(-x*x*.42f+tilt*x)*size);var b=p+new Vector2(z*size,(-z*z*.42f+tilt*z)*size);float w=5+18*(1-Mathf.Abs(x));air.Line(a,b,w+6,A(WindVFXStyle.Deep,alpha));air.Line(a,b,w,A(WindVFXStyle.Teal,alpha));air.Line(a+Vector2.down*3,b+Vector2.down*3,Mathf.Max(2,w*.45f),A(WindVFXStyle.Cyan,alpha));air.Line(a+Vector2.down*6,b+Vector2.down*6,2,A(WindVFXStyle.Pale,alpha));}
  }
  public void Render(BattleModel b){
   floor.Begin();air.Begin();Depth(floor,view.enemyArt.transform,false);Depth(air,view.playerArt.transform,true);
   var motion=view.GetComponent<BattleAnimation>();var origin=motion.BossSkillEnemyGround+Vector2.up*70;var target=motion.BossSkillPlayerGround;float time=(float)b.Time;
   bool busy=b.BossSkillBusy;bool stormCharge=busy&&b.ActiveBossSkill.stormDuration>0&&b.BossPreparation;
   if(b.BossStormActive||stormCharge){float power=stormCharge?b.BossPhaseProgress:Mathf.Min(1,(float)b.BossStormRemaining*3);
    for(int side=-1;side<=1;side+=2){for(int i=0;i<12;i++){float y=-205-Mathf.Repeat(time*115+i*39,470),x=side*(199+Mathf.Sin(time*2+i)*15);var p=new Vector2(x,y);air.Line(p,p+new Vector2(-side*8,27),3,A(WindVFXStyle.Cyan,power*.38f));if(i%3==0){air.Diamond(p+Vector2.up*35,5,2,A(new Color(.66f,.77f,.47f),power*.7f));air.Line(p+Vector2.up*35,p+new Vector2(5,37),2,A(WindVFXStyle.Deep,power));}}
     Arc(air,origin+new Vector2(side*50,25),100,35,time*2+side,2,3,A(WindVFXStyle.Mint,power*.6f));}
    Arc(floor,target,60,13,time*2,4,3,A(WindVFXStyle.Cyan,power*.6f));Arc(floor,target+Vector2.up*16,45,10,-time*2,3,2,A(WindVFXStyle.Mint,power*.4f));
   }
   if(busy&&b.ActiveBossSkill.directionalShots>0){float t=b.BossPhaseProgress;int side=b.BossDirection==BossBladeDirection.Left?-1:b.BossDirection==BossBladeDirection.Right?1:0;
    var landing=target+new Vector2(side*152,side==0?0:12);var launch=origin+new Vector2(side*74,0);
    if(b.BossPreparation){
     for(int k=0;k<3;k++){float a=time*2+k*2.094f;Arc(air,origin,54+k*16,20+k*7,a,1.6f,3,A(WindVFXStyle.Mint,.6f));}
     // The occupied lane is a dashed trajectory; side attacks leave a bounded center gap.
     for(int i=1;i<=7;i++){var p=Vector2.Lerp(launch,landing,i/8f);floor.Line(p,p+Vector2.up*12,4,A(WindVFXStyle.Teal,.75f));floor.Line(p,p+new Vector2(-8,9),2,A(WindVFXStyle.Cyan,.8f));floor.Line(p,p+new Vector2(8,9),2,A(WindVFXStyle.Cyan,.8f));}
     if(side!=0){for(int sign=-1;sign<=1;sign+=2){var p=target+new Vector2(sign*63,0);floor.Line(p,p+Vector2.up*30,3,WindVFXStyle.Mint);floor.Line(p,p+new Vector2(-sign*13,0),3,WindVFXStyle.Mint);}}
     Crescent(launch,48+20*t,side*.3f,.5f+.3f*t);
    }
    if(b.BossState==BossSkillState.Travel){var p=Vector2.Lerp(launch,landing,t);for(int i=3;i>0;i--)Crescent(p+Vector2.up*i*17,(side==0?98:70)-i*5,side*.5f,.13f);Crescent(p,side==0?98:70,side*.5f,1);for(int i=0;i<14;i++){float x=Mathf.Sin(i*8.1f)*55;air.Diamond(p+new Vector2(x,20+Mathf.Repeat(i*11+t*90,85)),2,5,A(WindVFXStyle.Mint,.65f));}}
    float age=(float)(b.Time-b.BossImpactAt);if(b.BossSkillHasImpacted&&age<.28f){int hitSide=b.BossLastImpactDirection==BossBladeDirection.Left?-1:b.BossLastImpactDirection==BossBladeDirection.Right?1:0;var p=target+new Vector2(hitSide*152,hitSide==0?0:12);Crescent(p+Vector2.down*age*35,hitSide==0?108:75,hitSide*.5f,1-age/.28f);}
    // Three small pips communicate cadence without adding instructions or a panel.
    for(int i=0;i<3;i++)air.Diamond(origin+new Vector2((i-1)*18,95),4,4,i<b.BossShotNumber?WindVFXStyle.Mint:WindVFXStyle.Deep);
   }
   floor.End();air.End();
  }
 }
}
