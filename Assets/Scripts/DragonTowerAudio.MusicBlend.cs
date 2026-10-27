using UnityEngine;
namespace DragonTower {
 public sealed partial class DragonTowerAudio {
  AudioSource musicOther;float musicLevel,blendAge,blendDuration;bool musicBlending,softFinalLoop,audioPaused,introRoarPlayed;
  void InitializeMusicBlend(){musicLevel=music.volume;musicOther=gameObject.AddComponent<AudioSource>();musicOther.playOnAwake=false;musicOther.volume=0;}
  void CancelMusicBlend(){musicOther.Stop();musicOther.volume=0;musicBlending=false;softFinalLoop=false;music.volume=musicLevel;}
  void BlendFinalMusic(AudioClip clip,bool softLoop){
   if(clip==null)return;
   softFinalLoop=softLoop;
   if(music.clip==clip&&music.isPlaying)return;
   StartMusicBlend(clip,1.8f);music.loop=!softLoop;
  }
  void StartMusicBlend(AudioClip clip,float duration){
   musicOther.Stop();var previous=music;music=musicOther;musicOther=previous;
   music.clip=clip;music.loop=!softFinalLoop;music.pitch=1;music.volume=0;music.Play();
   blendAge=0;blendDuration=duration;musicBlending=true;ApplyMusicBlend();
  }
  void ApplyMusicBlend(){float t=musicBlending?Mathf.SmoothStep(0,1,blendAge/Mathf.Max(.01f,blendDuration)):1;music.volume=musicLevel*t;musicOther.volume=musicBlending?musicLevel*(1-t):0;}
  void TickMusicBlend(){
   if(audioPaused)return;
   if(musicBlending){blendAge+=Time.unscaledDeltaTime;ApplyMusicBlend();if(blendAge>=blendDuration){musicOther.Stop();musicBlending=false;ApplyMusicBlend();}}
   if(softFinalLoop&&!musicBlending&&music.clip!=null&&music.isPlaying&&music.clip.length>3&&music.time>=music.clip.length-1.5f)StartMusicBlend(music.clip,1.5f);
  }
 }
}
