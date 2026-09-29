using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonTower
{
    public enum MusicMood { Lobby, Battle, Boss }

    /// <summary>Persistent, self-contained audio for WebGL and desktop builds.</summary>
    public sealed class DragonTowerAudio : MonoBehaviour
    {
        const int Rate=22050;
        static DragonTowerAudio instance;
        AudioSource music,sfx;
        AudioListener fallbackListener;
        AudioClip lobby,battle,boss,ui,attack,impact,skill,dodge,hurt,bossSkill,chest,victory,defeat,evolution;
        MusicMood mood;float scanAt;
        public static DragonTowerAudio Instance
        {
            get
            {
                if(instance!=null)return instance;
                var go=new GameObject("Dragon Tower Audio");instance=go.AddComponent<DragonTowerAudio>();DontDestroyOnLoad(go);return instance;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap(){var unused=Instance;}
        void Awake()
        {
            if(instance!=null&&instance!=this){Destroy(gameObject);return;}instance=this;DontDestroyOnLoad(gameObject);
            music=gameObject.AddComponent<AudioSource>();music.loop=true;music.playOnAwake=false;music.volume=PlayerPrefs.GetFloat("DragonTower.MusicVolume",.28f);
            sfx=gameObject.AddComponent<AudioSource>();sfx.playOnAwake=false;sfx.volume=PlayerPrefs.GetFloat("DragonTower.SfxVolume",.72f);
            fallbackListener=gameObject.AddComponent<AudioListener>();
            BuildClips();SetMusic(MusicMood.Lobby);
        }
        void Update()
        {
            if(Time.unscaledTime<scanAt)return;scanAt=Time.unscaledTime+1;
            EnsureListener();
            foreach(var button in FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(button.GetComponent<DragonTowerUiSound>()==null)button.gameObject.AddComponent<DragonTowerUiSound>();
        }
        void EnsureListener()
        {
            bool hasOther=false;
            foreach(var listener in FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
                if(listener!=fallbackListener&&listener.enabled){hasOther=true;break;}
            fallbackListener.enabled=!hasOther;
        }
        public static void SetMusic(MusicMood value)
        {
            var a=Instance;if(a.mood==value&&a.music.isPlaying)return;a.mood=value;
            a.music.clip=value==MusicMood.Boss?a.boss:value==MusicMood.Battle?a.battle:a.lobby;
            a.music.pitch=1;a.music.Play();
        }
        public static void SetVolumes(float musicVolume,float sfxVolume)
        {
            var a=Instance;a.music.volume=Mathf.Clamp01(musicVolume);a.sfx.volume=Mathf.Clamp01(sfxVolume);
            PlayerPrefs.SetFloat("DragonTower.MusicVolume",a.music.volume);PlayerPrefs.SetFloat("DragonTower.SfxVolume",a.sfx.volume);PlayerPrefs.Save();
        }
        public static void PlayUi(){var a=Instance;a.UnlockWebAudio();a.One(a.ui,.72f,UnityEngine.Random.Range(.96f,1.05f));}
        public static void PlayChest(){Instance.One(Instance.chest,1);}
        public static void PlayEvolution(){Instance.One(Instance.evolution,1);}
        public static void PlayResult(bool won){var a=Instance;a.One(won?a.victory:a.defeat,1);}
        public static void PlayCombat(CombatCue cue,SkillEffectKind element)
        {
            var a=Instance;
            switch(cue)
            {
                case CombatCue.Attack:a.One(a.attack,.85f,UnityEngine.Random.Range(.96f,1.07f));break;
                case CombatCue.Skill:a.One(a.skill,1,ElementPitch(element));break;
                case CombatCue.SkillHit:a.One(a.impact,.7f,UnityEngine.Random.Range(.9f,1.1f));break;
                case CombatCue.Dodge:case CombatCue.EnemyMiss:a.One(a.dodge,.8f);break;
                case CombatCue.EnemyHit:a.One(a.hurt,.9f,UnityEngine.Random.Range(.94f,1.04f));break;
                case CombatCue.BossSkill:a.One(a.bossSkill,1);break;
            }
        }
        static float ElementPitch(SkillEffectKind element)
        {switch(element){case SkillEffectKind.Frost:return 1.2f;case SkillEffectKind.Wind:return 1.12f;case SkillEffectKind.Earth:return .78f;case SkillEffectKind.Lightning:return 1.35f;case SkillEffectKind.Water:return .92f;case SkillEffectKind.Dark:return .7f;case SkillEffectKind.Light:return 1.28f;default:return 1;}}
        void UnlockWebAudio(){if(music.clip!=null&&!music.isPlaying)music.Play();}
        void One(AudioClip clip,float volume,float pitch=1){if(clip==null)return;sfx.pitch=pitch;sfx.PlayOneShot(clip,volume);}

        void BuildClips()
        {
            ui=Tone("UI touch",.075f,720,1080,.22f,Wave.Triangle);
            attack=Sweep("Claw attack",.16f,760,170,.42f,Wave.Saw,17);
            impact=NoiseHit("Enemy impact",.13f,.46f,91);
            skill=LayeredSkill();dodge=Sweep("Dodge",.22f,320,1280,.32f,Wave.Sine,5);
            hurt=NoiseHit("Dragon hurt",.25f,.58f,37);
            bossSkill=BossWarning();chest=ChestClip();victory=Jingle("Victory",new[]{523f,659f,784f,1047f},.16f,.34f);
            defeat=Jingle("Defeat",new[]{392f,330f,262f,196f},.2f,.30f);evolution=Jingle("Evolution",new[]{392f,523f,659f,784f,1047f},.18f,.36f);
            lobby=Music("Lobby music",new[]{220f,277.18f,329.63f,440f},84,false);
            battle=Music("Battle music",new[]{146.83f,174.61f,220f,261.63f},132,true);
            boss=Music("Boss music",new[]{110f,130.81f,164.81f,196f},154,true);
        }
        enum Wave{Sine,Triangle,Saw,Square}
        static float Osc(Wave w,double phase)
        {
            double p=phase-Math.Floor(phase);
            switch(w){case Wave.Triangle:return (float)(1-4*Math.Abs(p-.5));case Wave.Saw:return (float)(2*p-1);case Wave.Square:return p<.5?1:-1;default:return (float)Math.Sin(phase*Math.PI*2);}
        }
        static AudioClip Tone(string name,float seconds,float from,float to,float gain,Wave wave)
        {
            int n=Mathf.CeilToInt(seconds*Rate);var data=new float[n];double phase=0;
            for(int i=0;i<n;i++){float t=i/(float)Math.Max(1,n-1),env=Mathf.Sin(Mathf.PI*t);float f=Mathf.Lerp(from,to,t);phase+=f/Rate;data[i]=Osc(wave,phase)*env*gain;}
            return Clip(name,data);
        }
        static AudioClip Sweep(string name,float seconds,float from,float to,float gain,Wave wave,int seed)
        {
            int n=Mathf.CeilToInt(seconds*Rate);var data=new float[n];var random=new System.Random(seed);double phase=0;
            for(int i=0;i<n;i++){float t=i/(float)n,env=(1-t)*(1-t),f=Mathf.Lerp(from,to,t);phase+=f/Rate;data[i]=(Osc(wave,phase)*.72f+((float)random.NextDouble()*2-1)*.28f)*env*gain;}
            return Clip(name,data);
        }
        static AudioClip NoiseHit(string name,float seconds,float gain,int seed)
        {
            int n=Mathf.CeilToInt(seconds*Rate);var data=new float[n];var random=new System.Random(seed);double phase=0;
            for(int i=0;i<n;i++){float t=i/(float)n,env=Mathf.Exp(-t*8);phase+=(95+210*(1-t))/Rate;data[i]=(((float)random.NextDouble()*2-1)*.65f+Osc(Wave.Square,phase)*.35f)*env*gain;}
            return Clip(name,data);
        }
        static AudioClip LayeredSkill()
        {
            int n=Mathf.CeilToInt(.48f*Rate);var data=new float[n];var random=new System.Random(73);double p1=0,p2=0;
            for(int i=0;i<n;i++){float t=i/(float)n,env=Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.16f))*Mathf.Pow(1-t,.7f);p1+=(180+920*t)/Rate;p2+=(360+1500*t*t)/Rate;data[i]=(Osc(Wave.Saw,p1)*.22f+Osc(Wave.Sine,p2)*.36f+((float)random.NextDouble()*2-1)*.12f)*env;}
            return Clip("Dragon skill",data);
        }
        static AudioClip BossWarning()
        {
            int n=Mathf.CeilToInt(.75f*Rate);var data=new float[n];double p=0;
            for(int i=0;i<n;i++){float t=i/(float)n,beat=(t<.28f||t>.42f&&t<.7f)?1:0;p+=(92+35*Mathf.Sin(t*18))/Rate;data[i]=Osc(Wave.Square,p)*beat*(1-t)*.34f;}
            return Clip("Boss skill",data);
        }
        static AudioClip ChestClip()
        {
            int n=Mathf.CeilToInt(.55f*Rate);var data=new float[n];double p=0;
            for(int i=0;i<n;i++){float t=i/(float)n;float f=t<.32f?180:Mathf.Lerp(520,1180,(t-.32f)/.68f);p+=f/Rate;float env=t<.32f?(1-t*2):Mathf.Pow(1-t,.6f);data[i]=(Osc(Wave.Triangle,p)*.32f+Osc(Wave.Sine,p*2.01)*.17f)*env;}
            return Clip("Chest open",data);
        }
        static AudioClip Jingle(string name,float[] notes,float noteLength,float gain)
        {
            int each=Mathf.CeilToInt(noteLength*Rate),n=each*notes.Length;var data=new float[n];double phase=0;
            for(int i=0;i<n;i++){int note=Math.Min(notes.Length-1,i/each);float t=(i%each)/(float)each,env=Mathf.Sin(Mathf.PI*t);phase+=notes[note]/Rate;data[i]=(Osc(Wave.Triangle,phase)*.7f+Osc(Wave.Sine,phase*2)*.3f)*env*gain;}
            return Clip(name,data);
        }
        static AudioClip Music(string name,float[] chord,float bpm,bool drums)
        {
            const float seconds=12;int n=(int)(seconds*Rate);var data=new float[n];var random=new System.Random(name.GetHashCode());double pad=0,bass=0,lead=0;float beat=60/bpm;
            for(int i=0;i<n;i++)
            {
                float t=i/(float)Rate;int step=(int)(t/(beat*.5f));float note=chord[step%chord.Length];pad+=chord[(step/4)%chord.Length]/Rate;bass+=chord[(step/8)%chord.Length]*.5/Rate;lead+=note/Rate;
                float v=Osc(Wave.Sine,pad)*.075f+Osc(Wave.Triangle,bass)*.065f+Osc(Wave.Triangle,lead)*.035f;
                if(drums){float within=t%beat;float kick=Mathf.Exp(-within*24)*Osc(Wave.Sine,t*(78-38*within));float hat=within>beat*.48f&&within<beat*.58f?((float)random.NextDouble()*2-1)*.09f:0;v+=kick*.10f+hat;}
                float edge=Mathf.Min(1,Mathf.Min(t,seconds-t)*2);data[i]=v*edge;
            }
            return Clip(name,data);
        }
        static AudioClip Clip(string name,float[] data){var clip=AudioClip.Create(name,data.Length,1,Rate,false);clip.SetData(data,0);return clip;}
    }

    public sealed class DragonTowerUiSound : MonoBehaviour,IPointerDownHandler
    {public void OnPointerDown(PointerEventData eventData){if(GetComponent<Button>()?.interactable==true)DragonTowerAudio.PlayUi();}}
}
