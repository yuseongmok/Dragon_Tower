using UnityEngine;
namespace DragonTower {
 public sealed partial class FireSkillVfx {
  void RenderMeteor(){
   float h=HitsShown>0?age-hits[0]:-1;var sky=target+new Vector2(-110,235);float t=h<0?Mathf.Clamp01(age/Mathf.Max(.001f,skill.initialHitDelay)):1;var p=Vector2.Lerp(sky,target,t*t);
   if(h<.14f){float fade=h<0?1:1-h/.14f;
    L(FireStyleModule.Shockwave,ground,new Vector2(160,40),fade*.65f,0,true,false,.4f);
    Trail(sky,p,110,fade,7);Flame(p,new Vector2(160,195),age,fade,25,true);
    painter.SpriteLayer(library.meteor,p,Vector2.one*132,fade,-24);
    for(int i=0;i<9;i++){var smoke=Vector2.Lerp(p,sky,i/9f);L(FireStyleModule.Smoke,smoke,Vector2.one*(60+i*7),fade*.4f,i*35,true);}
   }
   
   for(int i=0;i<HitsShown;i++){float impact=age-hits[i];Impact(impact,i==0?255:i==Count-1?300:205,i==Count-1?48:28,i==Count-1);}
   if(h>=.033f&&h<.24f)painter.SpriteLayer(library.meteor,target+new Vector2(-24,35),Vector2.one*148,Mathf.Clamp01((.24f-h)/.12f),-24,false,false,true);
  }
 }
}



