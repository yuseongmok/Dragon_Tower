using UnityEngine;
namespace DragonTower {
 public sealed partial class WaterSkillVfx {
  // Perspective projection of six independently phased reservoirs around the target.
  static readonly Vector3[] orbSpace={new Vector3(-145,65,110),new Vector3(65,155,80),new Vector3(170,40,35),new Vector3(-125,-70,-110),new Vector3(100,-85,-90),new Vector3(-60,155,-30)};
  partial void RenderSplash(){var s=library.style;float end=Mathf.Max(.12f,End),fire=end*.46f;bool final=HitsShown>=Count;float release=final?age-hits[Count-1]:-1;
   for(int i=0;i<orbSpace.Length;i++){var world=orbSpace[i];float perspective=600/(600+world.z);var pos=target+new Vector2(world.x,world.y)*perspective;int depth=world.z>40?0:2;float born=i*.037f,open=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-born)/.12f));float fade=release<0?1:Fade(release,0,.16f);float radius=(34+i%3*6)*perspective*open;
    if(open>0&&fade>0){p.Mass(pos,radius,age*(2.4f+i*.21f)+i,fade,depth);p.Disc(pos+new Vector2(-radius*.12f,radius*.12f),radius*(.22f+.18f*Mathf.Clamp01(age/end)),.8f,s.pale,fade,depth);p.Flow(pos,radius*1.1f,.62f,age*(i%2==0?7:-6),fade,depth,i,3,3.4f);}
    if(age>=fire&&release<.10f){float pressure=Mathf.Clamp01((age-fire)/.08f)* (release<0?1:Fade(release,0,.10f));Stream(pos,target,(14+i%3*3)*perspective,age*1.6f+i,pressure,depth);}
    if(release>=0){p.Flow(pos,(radius+release*180)*perspective,.7f,release*6,Fade(release,0,.28f),depth,i,3,4.8f);p.Droplets(pos,release,.7f*perspective,1,14);}
   }
   // Each existing damage event has a contact pulse; only the last opens the full burst.
   for(int i=0;i<HitsShown;i++){float t=age-hits[i];if(i==Count-1)continue;float f=Fade(t,.018f,.105f);p.Disc(target,14+t*85,.75f,s.pale,f,1);p.Flow(target,25+t*140,.55f,t*8,f,1,i,5,4);}
   if(final){float t=release<.04f?0:release-.04f;var floor=target+Vector2.down*58;SharpBurst(floor,t,2.85f,true);for(int j=0;j<3;j++)p.Flow(floor+Vector2.up*(release*260+j*16),110+release*280+j*14,.28f,release*5+j,Fade(release,.09f,.42f),2,j,5,4.2f);p.Disc(floor,28+Mathf.Clamp01(release/.065f)*90,.24f,s.pale,Fade(release,.03f,.10f),2);p.Impulse(release,8,.13f);}
  }
 }
}
