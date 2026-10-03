using UnityEngine;
namespace DragonTower {
 public sealed partial class FireSkillVfx {
  Vector2 feedbackHome;Color feedbackColor;bool feedbackApplied;
  void RestoreFeedback(){if(feedbackApplied&&view!=null&&view.enemyArt!=null){view.enemyArt.rectTransform.anchoredPosition=feedbackHome;view.enemyArt.color=feedbackColor;}feedbackApplied=false;}
  void Feedback(){feedbackApplied=false;if(lastHit>.10f||HitsShown==0)return;bool strong=(kind==FireSkillKind.Meteor)||(kind==FireSkillKind.Purgatory&&HitsShown==Count);if(!strong)return;var im=view.enemyArt;feedbackHome=im.rectTransform.anchoredPosition;feedbackColor=im.color;feedbackApplied=true;im.rectTransform.anchoredPosition=feedbackHome+new Vector2(Mathf.Sin(lastHit*90)*3*(1-lastHit/.1f),0);if(lastHit<.05f)im.color=new Color(1,1,.78f);}
  void RenderPurgatory(){float final=HitsShown>=Count?age-hits[Count-1]:-1;float sustain=final<0?1:Mathf.Clamp01((.24f-final)/.24f);var gate=target+new Vector2(0,85);
   // Existing data begins at T=0: gate/beam contact exist immediately, then grow outward.
   float open=HitsShown>0?1:Mathf.Clamp01(age/Mathf.Max(.001f,skill.initialHitDelay));
   if(sustain>0){
    // One continuous volume, with separate turbulent outer/body/core sheets.
    float pulse=1+.045f*Mathf.Sin(age*27);var basePoint=ground+new Vector2(0,9);float height=Mathf.Max(211,450-basePoint.y);
    var middle=basePoint+new Vector2(0,height*.5f);
    if(library.infernoColumn!=null&&library.infernoColumn.Length==24){
     int outer=Mathf.FloorToInt(age*14)%8,body=Mathf.FloorToInt(age*18+2)%8,core=Mathf.FloorToInt(age*23+5)%8;
     painter.SpriteLayer(library.infernoColumn[outer],middle,new Vector2(300*pulse,height+12)*open,sustain,0,true);
     painter.SpriteLayer(library.infernoColumn[8+body],middle+new Vector2(Mathf.Sin(age*16)*4,0),new Vector2(259*pulse,height)*open,sustain);
     painter.SpriteLayer(library.infernoColumn[16+core],middle+new Vector2(Mathf.Sin(age*22)*5,0),new Vector2(137*pulse,height)*open,sustain*.88f,0,false,true);
    }
    // Small aperture and a broad floor flare reinforce top-to-bottom continuity.
    L(FireStyleModule.Burst,basePoint+new Vector2(0,height-5),new Vector2(250,53),sustain,0,false,true,.35f);
    L(FireStyleModule.Burst,basePoint,new Vector2(306,82),sustain,0,false,false,.35f);
    L(FireStyleModule.Shockwave,basePoint,new Vector2(322,65),sustain*.85f,0,false,true,.35f);
    for(int i=0;i<10;i++){float u=Mathf.Repeat(age*2.8f+i*.1f,1);float side=i%2==0?-1:1;
     var p=basePoint+new Vector2(side*(82+Mathf.Sin(i+age*11)*14),height*(1-u));
     L(FireStyleModule.Tongue,p,new Vector2(45,78),sustain*.75f,180+side*17);
     L(FireStyleModule.Ember,p+new Vector2(side*(15+u*25),-14),new Vector2(20,39),sustain*(1-u*.5f),side*20,false,true);
    }
    for(int i=0;i<5;i++)L(FireStyleModule.Smoke,basePoint+new Vector2((i-2)*59,25+Mathf.Sin(age*6+i)*8),new Vector2(98,84),sustain*.45f,i*30,true);
    L(FireStyleModule.Heat,middle,new Vector2(325,height),sustain*.2f,0,true);
   }
   for(int i=0;i<HitsShown;i++){float h=age-hits[i];if(i==Count-1){
     // VFX-only 50ms impact hold. No change to battle clocks or character Animator.
     float held=h<.05f?0:h-.05f;Impact(held,385,60,true);
     if(h<.24f){float t=Mathf.Clamp01((h-.05f)/.19f);L(FireStyleModule.Burst,gate,new Vector2(380,125)*(1-t*.4f),1-t,0,false,true,t);}
    }else if(h<.24f)Impact(h*2.5f,133,10,false);
   }
  }
 }
}


