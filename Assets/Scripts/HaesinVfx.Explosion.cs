using UnityEngine;
namespace DragonTower {public sealed partial class HaesinVfx {
 void Explosion(float t){if(t<0||t>1.7f)return;float held=Mathf.Max(0,t-.065f);ShowEnemy();SharkBreakup(t);SharpBurst(target,held,3.5f);SharpBurst(ground,held,2.8f,true);for(int i=0;i<5;i++)water.Flow(target,90+held*(420+i*50),.3f+i*.09f,t*3+i,F(t,.12f,.7f),2,i,9-i,4.8f);water.Disc(target,35+Mathf.Clamp01(t/.075f)*130,.8f,profile.style.foam,F(t,.03f,.15f),2);water.Impulse(t,18,.23f);if(t<.085f)LegendaryScreenDimming.Set(this,new Color(.75f,.98f,1,.33f*F(t,.025f,.085f)));for(int i=0;i<12;i++){float u=t-.09f-i*.01f;if(u<0||u>1.1f)continue;var pos=target+D(i*2.399f)*(80+u*240)+Vector2.down*u*u*100;water.Bubble(pos,10+i%4*5,F(u,.4f,1.1f)*.65f,2);}
 }
}}
