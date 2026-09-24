using UnityEngine;
using UnityEngine.UI;

namespace DragonTower
{
    // A small presentation layer: no damage/cooldown logic and no per-hit Instantiate/Destroy.
    // All effect coordinates are in the existing 480 x 850 portrait frame.
    public sealed class BattleAnimation : MonoBehaviour
    {
        const int ParticleCount=72, NumberCount=12;
        sealed class Particle
        {
            public Image image; public Vector2 start, velocity, size;
            public Color color; public float age, life, gravity, spin; public bool active;
        }
        sealed class Number
        {
            public Text text; public Vector2 start; public Color color;
            public float age; public bool active;
        }
        readonly Particle[] particles=new Particle[ParticleCount];
        readonly Number[] numbers=new Number[NumberCount];
        RectTransform player,enemy,layer;
        Graphic playerGraphic,enemyGraphic;
        Vector2 playerHome,enemyHome;
        Color playerColor,enemyColor,skillColor;
        SkillEffectKind skillEffect;
        BattleAssetVfx assetVfx;
        float clock,attackAge,skillAge,dodgeAge,enemyAttackAge,playerHitAge,enemyHitAge,resultAge;
        int particleIndex,numberIndex;
        public bool ResultReady => resultAge>=.65f;
        public int PoolObjectCount => ParticleCount+NumberCount;

        public void Initialize(BattleView view)
        {
            player=view.playerArt.rectTransform;enemy=view.enemyArt.rectTransform;
            playerHome=player.anchoredPosition;enemyHome=enemy.anchoredPosition;
            enemyColor=view.enemyArt.color;
            var root=new GameObject("Battle effects (pooled)",typeof(RectTransform),typeof(Canvas));
            layer=root.GetComponent<RectTransform>();layer.SetParent(view.frame,false);
            layer.anchorMin=layer.anchorMax=new Vector2(.5f,1);layer.pivot=new Vector2(.5f,1);
            layer.anchoredPosition=Vector2.zero;layer.sizeDelta=new Vector2(480,850);
            // A separate canvas limits effect geometry rebuilds; no GraphicRaycaster is needed here.
            root.GetComponent<Canvas>().overrideSorting=false;
            // Draw above the enemy art, then let warning text, timing bar, player HUD and buttons
            // created later in the frame draw over every effect.
            layer.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex()+1);
            assetVfx=gameObject.AddComponent<BattleAssetVfx>();
            assetVfx.Initialize(layer);
            for(int i=0;i<particles.Length;i++)
            {
                var go=new GameObject("Pixel "+i,typeof(RectTransform),typeof(Image));
                var image=go.GetComponent<Image>();SetupRect(image.rectTransform);image.raycastTarget=false;
                particles[i]=new Particle{image=image};go.SetActive(false);
            }
            for(int i=0;i<numbers.Length;i++)
            {
                var go=new GameObject("Damage "+i,typeof(RectTransform),typeof(Text),typeof(Shadow));
                var text=go.GetComponent<Text>();SetupRect(text.rectTransform);text.rectTransform.sizeDelta=new Vector2(180,48);
                text.font=view.font;text.fontSize=26;text.fontStyle=FontStyle.Bold;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
                go.GetComponent<Shadow>().effectColor=new Color(0,0,0,.9f);go.GetComponent<Shadow>().effectDistance=new Vector2(2,-2);
                numbers[i]=new Number{text=text};go.SetActive(false);
            }
        }
        void SetupRect(RectTransform rect)
        {
            rect.SetParent(layer,false);rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);
            rect.pivot=new Vector2(.5f,.5f);
        }
        Graphic ActiveGraphic(RectTransform actor)
        {
            var pixel=actor.Find("Pixel sprite");
            return pixel!=null&&pixel.gameObject.activeSelf?pixel.GetComponent<Image>():actor.GetComponent<Graphic>();
        }
        public void ResetBattle(SkillEffectKind effect)
        {
            ResetBattle(effect,null);
        }
        public void ResetBattle(SkillData skill)
        {
            ResetBattle(skill.effectKind,skill);
        }
        void ResetBattle(SkillEffectKind effect,SkillData skill)
        {
            clock=resultAge=0;particleIndex=numberIndex=0;
            attackAge=skillAge=dodgeAge=enemyAttackAge=playerHitAge=enemyHitAge=10;
            playerGraphic=ActiveGraphic(player);enemyGraphic=ActiveGraphic(enemy);
            // Images are untinted; procedural fallback uses its original configured color.
            playerColor=playerGraphic is Image?Color.white:playerGraphic.color;
            if(enemyGraphic is Image) enemyColor=Color.white;
            skillEffect=effect;
            skillColor=EffectColor(effect);
            assetVfx.Configure(skill);
            foreach(var p in particles){p.active=false;p.image.gameObject.SetActive(false);}
            foreach(var n in numbers){n.active=false;n.text.gameObject.SetActive(false);}
            Restore(player,playerHome);Restore(enemy,enemyHome);
            playerGraphic.color=playerColor;enemyGraphic.color=enemyColor;
        }
        static void Restore(RectTransform rect,Vector2 home)
        {rect.anchoredPosition=home;rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;}
        public void Play(CombatCue cue,int damage)
        {
            switch(cue)
            {
                case CombatCue.Attack:
                    attackAge=0;enemyHitAge=-.06f;
                    Burst(enemyHome,Color.white,10,.06f,90);
                    Slash(enemyHome,.06f);Damage(enemyHome,damage.ToString(),new Color(1,.91f,.62f),.06f);
                    break;
                case CombatCue.Skill:
                    skillAge=0;enemyHitAge=-.18f;
                    var from=playerHome+new Vector2(55,35);var to=enemyHome;
                    // Imported effects keep their full composite hierarchy. A small pooled pixel
                    // accent guarantees readable impact feedback if a third-party texture or
                    // animation has a delayed first frame.
                    if(!assetVfx.PlayAt(to))
                    {
                        if(skillEffect==SkillEffectKind.Frost||skillEffect==SkillEffectKind.Water)FrostSkill(from,to);
                        else if(skillEffect==SkillEffectKind.Wind||skillEffect==SkillEffectKind.Earth)WindSkill(from,to);
                        else FireSkill(from,to);
                    }
                    else
                    {
                        Burst(to,skillColor,8,.04f,72);
                        if(skillEffect==SkillEffectKind.Wind||skillEffect==SkillEffectKind.Earth)Slash(to,.04f);
                    }
                    Damage(enemyHome,damage.ToString()+"!",skillColor,.18f);
                    break;
                case CombatCue.SkillHit:
                    enemyHitAge=-.06f;
                    Burst(enemyHome,skillColor,8,0,70);
                    Damage(enemyHome,damage.ToString()+"!",skillColor,0);
                    break;
                case CombatCue.Dodge:
                    dodgeAge=0;
                    for(int i=0;i<6;i++) Emit(playerHome+new Vector2(20,i*8-20),new Vector2(120,15),new Vector2(24,3),new Color(.45f,.95f,1),.28f,i*.025f);
                    break;
                case CombatCue.EnemyHit:
                    enemyAttackAge=0;playerHitAge=0;
                    Burst(playerHome,new Color(1,.42f,.35f),12,0,110);
                    Damage(playerHome,damage.ToString(),new Color(1,.48f,.40f),0);
                    break;
                case CombatCue.EnemyMiss:
                    enemyAttackAge=0;
                    Damage(playerHome,"회피!",new Color(.40f,1,.88f),0);
                    Burst(playerHome+new Vector2(65,-25),new Color(.5f,.8f,.9f),6,0,55);
                    break;
            }
        }
        void Slash(Vector2 at,float delay)
        {
            for(int i=0;i<3;i++) Emit(at+new Vector2(i*14-16,12),new Vector2(18,-15),new Vector2(5,62-i*8),new Color(1,.93f,.67f),.16f,delay,0,-35);
        }
        void FireSkill(Vector2 from,Vector2 to)
        {
            for(int i=0;i<16;i++)
                Emit(from,(to-from)/.20f+new Vector2(Mathf.Sin(i*2)*28,0),new Vector2(12+i%3*3,12+i%3*3),
                    i%3==0?new Color(1,.95f,.65f):skillColor,.22f,i*.013f,-35,90);
            Burst(to,skillColor,22,.18f,150);
            for(int i=0;i<5;i++)Emit(to+new Vector2(i*14-28,10),new Vector2(i*8-16,-125-i*8),new Vector2(7,18),new Color(1,.72f,.12f),.38f,.18f,35,0);
        }
        void FrostSkill(Vector2 from,Vector2 to)
        {
            for(int i=0;i<14;i++)
                Emit(from+new Vector2(0,(i%4-2)*7),(to-from)/.23f,new Vector2(7+(i%2)*4,18),i%3==0?Color.white:skillColor,.26f,i*.012f,0,45);
            for(int i=0;i<7;i++)
                Emit(to+new Vector2(i*16-48,38),new Vector2(i*5-15,-45),new Vector2(9,38+i%3*10),i%2==0?Color.white:skillColor,.48f,.14f+i*.014f,0,0);
            Burst(to,new Color(.65f,.92f,1),16,.18f,105);
        }
        void WindSkill(Vector2 from,Vector2 to)
        {
            for(int i=0;i<15;i++)
                Emit(from+new Vector2(0,Mathf.Sin(i*1.3f)*34),(to-from)/.24f+new Vector2(0,Mathf.Cos(i)*65),
                    new Vector2(28-i%3*5,4+i%2*2),i%3==0?Color.white:skillColor,.28f,i*.012f,0,-18);
            for(int i=0;i<18;i++)
            {
                float angle=i*Mathf.PI*2/18;var radial=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                var tangent=new Vector2(-radial.y,radial.x);
                Emit(to+radial*(28+i%3*9),tangent*(120+i%4*15),new Vector2(20,5),i%4==0?Color.white:skillColor,.38f,.14f,0,angle*Mathf.Rad2Deg);
            }
        }
        void Burst(Vector2 at,Color color,int count,float delay,float speed)
        {
            for(int i=0;i<count;i++)
            {
                float angle=i*2.39996f;
                var velocity=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(speed*(.55f+(i%5)*.12f));
                float size=4+(i%3)*3;
                Emit(at,velocity,new Vector2(size,size),i%4==0?Color.white:color,.30f+(i%4)*.06f,delay,90,0);
            }
        }
        void Emit(Vector2 at,Vector2 velocity,Vector2 size,Color color,float life,float delay=0,float gravity=0,float spin=0)
        {
            var p=particles[particleIndex++%particles.Length];p.active=true;p.start=at;p.velocity=velocity;p.size=size;p.color=color;
            p.age=-delay;p.life=life;p.gravity=gravity;p.spin=spin;p.image.gameObject.SetActive(false);
        }
        void Damage(Vector2 at,string text,Color color,float delay)
        {
            var n=numbers[numberIndex++%numbers.Length];n.active=true;n.start=at+new Vector2((numberIndex%3-1)*22,65);
            n.age=-delay;n.color=color;n.text.text=text;n.text.gameObject.SetActive(false);
        }
        static float Pulse(float age,float length) => age>=0&&age<length?Mathf.Sin(age/length*Mathf.PI):0;
        public void Step(BattleModel battle,float delta)
        {
            if(delta<=0 || player==null) return;
            clock+=delta;attackAge+=delta;skillAge+=delta;dodgeAge+=delta;enemyAttackAge+=delta;playerHitAge+=delta;enemyHitAge+=delta;
            assetVfx.Step(delta);
            bool fighting=battle.Result==BattleResult.Fighting;
            if(!fighting) resultAge+=delta;
            float breath=fighting?Mathf.Sin(clock*3.6f):0;
            float bounce=fighting?Mathf.Abs(Mathf.Sin(clock*3.3f)):0;
            float windup=fighting?1-Mathf.Clamp01((float)(battle.NextEnemyStrike-battle.Time)/BattleModel.WindupDuration):0;
            float attack=Pulse(attackAge,.24f),cast=Pulse(skillAge,.4f),dodge=Pulse(dodgeAge,BattleModel.DodgeDuration);
            float enemyAttack=Pulse(enemyAttackAge,.28f);
            var playerOffset=new Vector2(28*attack-66*dodge-9*cast,4*breath+48*attack+12*dodge);
            var enemyOffset=new Vector2(-20*enemyAttack,9*bounce-46*enemyAttack-12*windup);
            if(playerHitAge>=0&&playerHitAge<.24f) playerOffset.x+=Mathf.Sin(playerHitAge*95)*10*(1-playerHitAge/.24f);
            if(enemyHitAge>=0&&enemyHitAge<.24f) enemyOffset.x+=Mathf.Sin(enemyHitAge*95)*12*(1-enemyHitAge/.24f);
            player.anchoredPosition=playerHome+Snap(playerOffset);enemy.anchoredPosition=enemyHome+Snap(enemyOffset);
            player.localScale=new Vector3(1-.018f*breath+.07f*attack-.06f*cast,1+.025f*breath-.05f*attack+.08f*cast,1);
            enemy.localScale=new Vector3(1+.04f*(1-bounce)+.16f*windup-.12f*enemyAttack,1-.06f*(1-bounce)-.19f*windup+.16f*enemyAttack,1);
            player.localRotation=Quaternion.Euler(0,0,-9*attack+13*dodge+4*cast);
            enemy.localRotation=Quaternion.Euler(0,0,windup*Mathf.Sin(clock*40)*3+10*enemyAttack);
            playerGraphic.color=HitColor(playerColor,playerHitAge);
            enemyGraphic.color=HitColor(enemyColor,enemyHitAge);
            if(dodge>0){var c=playerGraphic.color;c.a=1-.45f*dodge;playerGraphic.color=c;}
            if(!fighting)
            {
                var defeated=battle.Result==BattleResult.Victory?enemy:player;
                var graphic=battle.Result==BattleResult.Victory?enemyGraphic:playerGraphic;
                float t=Mathf.Clamp01((resultAge-.18f)/.45f);
                defeated.localRotation=Quaternion.Euler(0,0,-18*t);defeated.localScale=Vector3.one*(1-.22f*t);
                var c=graphic.color;c.a=1-t;graphic.color=c;
            }
            foreach(var p in particles)
            {
                if(!p.active)continue;p.age+=delta;
                if(p.age<0)continue;
                if(p.age>=p.life){p.active=false;p.image.gameObject.SetActive(false);continue;}
                p.image.gameObject.SetActive(true);float t=p.age/p.life;
                p.image.rectTransform.anchoredPosition=Snap(p.start+p.velocity*p.age+Vector2.down*(p.gravity*p.age*p.age*.5f));
                p.image.rectTransform.sizeDelta=p.size*(1-.55f*t);
                p.image.rectTransform.localRotation=Quaternion.Euler(0,0,p.spin);
                var c=p.color;c.a=1-t;p.image.color=c;
            }
            foreach(var n in numbers)
            {
                if(!n.active)continue;n.age+=delta;if(n.age<0)continue;
                if(n.age>=.7f){n.active=false;n.text.gameObject.SetActive(false);continue;}
                n.text.gameObject.SetActive(true);n.text.rectTransform.anchoredPosition=n.start+new Vector2(0,n.age*65);
                n.text.rectTransform.localScale=Vector3.one*(1+.25f*Pulse(n.age,.18f));
                var c=n.color;c.a=1-Mathf.Clamp01((n.age-.35f)/.35f);n.text.color=c;
            }
        }
        static Vector2 Snap(Vector2 v) => new Vector2(Mathf.Round(v.x/2)*2,Mathf.Round(v.y/2)*2);
        static Color EffectColor(SkillEffectKind effect)
        {
            switch(effect)
            {
                case SkillEffectKind.Frost:return new Color(.35f,.85f,1);
                case SkillEffectKind.Wind:return new Color(.3f,1,.65f);
                case SkillEffectKind.Earth:return new Color(.72f,.51f,.27f);
                case SkillEffectKind.Lightning:return new Color(.55f,.72f,1);
                case SkillEffectKind.Water:return new Color(.2f,.72f,1);
                case SkillEffectKind.Dark:return new Color(.63f,.3f,.92f);
                case SkillEffectKind.Light:return new Color(1,.91f,.5f);
                default:return new Color(1,.42f,.06f);
            }
        }
        static Color HitColor(Color normal,float age)
        {
            if(age<0||age>=.23f)return normal;
            return ((int)(age/.055f)%2)==0?new Color(1,.38f,.3f,normal.a):normal;
        }
    }
}
