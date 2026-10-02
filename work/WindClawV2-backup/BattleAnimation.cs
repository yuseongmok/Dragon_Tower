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
            public Text text; public Vector2 start,drift; public Color color;public bool bladeNumber;
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
        ZephyrWindVfx windVfx;
        WindBladeVfx bladeVfx;Vector2 bladeEnemyPose;
        GaleStrikeVfx galeVfx; GaleSlashVfx slashVfx; GaleSlashFeedback slashFeedback; WindClawVfx clawVfx;WindHitFeedback clawFeedback;
        bool zephyrWind;
        public void SetZephyrWind(bool value){zephyrWind=value;}
        public bool UsesPixelWindSkill=>zephyrWind&&skillEffect==SkillEffectKind.Wind;
        DragonIdleFrames idleFrames;
        DragonIdleFrames attackFrames;
        DragonIdleFrames skillFrames;
        DragonIdleFrames dodgeFrames;
        DragonIdleFrames hitFrames;
        DragonIdleFrames deathFrames;
        public void SetDeathFrames(DragonIdleFrames frames) { deathFrames=frames; }
        public void SetHitFrames(DragonIdleFrames frames) { hitFrames=frames; }
        int dodgeDirection=-1;
        public void SetDodgeFrames(DragonIdleFrames frames) { dodgeFrames=frames; }
        // Presentation direction only; the controller must still ask BattleModel.Dodge().
        public void SetDodgeDirection(int direction) { dodgeDirection=direction>0?1:-1; }
        public void SetSkillFrames(DragonIdleFrames frames) { skillFrames=frames; }
        public void SetAttack(DragonIdleFrames frames) { attackFrames=frames; }
        Sprite restSprite;
        public void SetIdle(DragonIdleFrames frames,Sprite fallback) { idleFrames=frames;restSprite=fallback; }
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
            windVfx=gameObject.AddComponent<ZephyrWindVfx>();windVfx.Initialize(layer,player);
            galeVfx=gameObject.AddComponent<GaleStrikeVfx>();galeVfx.Initialize(layer);
            bladeVfx=gameObject.AddComponent<WindBladeVfx>();bladeVfx.Initialize(layer);slashVfx=gameObject.AddComponent<GaleSlashVfx>();slashVfx.Initialize(layer);slashFeedback=gameObject.AddComponent<GaleSlashFeedback>();slashFeedback.Initialize();clawVfx=gameObject.AddComponent<WindClawVfx>();clawVfx.Initialize(layer);clawFeedback=gameObject.AddComponent<WindHitFeedback>();clawFeedback.Initialize();
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
            dodgeDirection=-1;
            attackAge=skillAge=dodgeAge=enemyAttackAge=playerHitAge=enemyHitAge=10;
            playerGraphic=ActiveGraphic(player);enemyGraphic=ActiveGraphic(enemy);
            if(playerGraphic is Image idleImage) { idleImage.sprite=idleFrames!=null ? idleFrames.At(0) : restSprite;AlignIdle(idleImage); }
            // Images are untinted; procedural fallback uses its original configured color.
            playerColor=playerGraphic is Image?Color.white:playerGraphic.color;
            if(enemyGraphic is Image) enemyColor=Color.white;
            skillEffect=effect;
            skillColor=EffectColor(effect);
            assetVfx.Configure(UsesPixelWindSkill||(skill!=null&&(skill.StableId=="skill_falling_flower"||skill.StableId=="skill_gale_slash"||skill.StableId=="skill_wind_claw"))?null:skill);
            windVfx.Clear();galeVfx.Configure(zephyrWind,skill);bladeVfx.Configure(skill);slashVfx.Configure(skill);slashFeedback.Bind(player,enemy,playerGraphic,enemyGraphic);slashVfx.SetTargetWidth(enemyGraphic.rectTransform.rect.width);bladeEnemyPose=enemyHome;clawVfx.Configure(skill);clawFeedback.Bind(player,enemy,playerGraphic,enemyGraphic);
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
                    if(zephyrWind)windVfx.Attack(enemyHome);
                    else {Burst(enemyHome,Color.white,10,.06f,90);Slash(enemyHome,.06f);}
                    Damage(enemyHome,damage.ToString(),zephyrWind?new Color(.7f,1,.8f):new Color(1,.91f,.62f),.06f);
                    break;
                case CombatCue.SkillCast:
                    if(clawVfx.Configured){attackAge=10;skillAge=0;clawVfx.Cast(playerHome,enemyHome);break;}
                    if(slashVfx.Configured){attackAge=10;skillAge=0;slashVfx.Cast(playerHome,enemyHome);break;}
                    if(bladeVfx.Configured){attackAge=10;skillAge=0;bladeVfx.Cast(playerHome,enemyHome);}break;
                case CombatCue.Skill:
                    if(clawVfx.Configured){if(!clawVfx.Active){clawVfx.Cast(playerHome,enemyHome);skillAge=0;}enemyHitAge=0;clawVfx.Hit();bool final=clawVfx.HitsShown%2==0;clawFeedback.Hit(final);Damage(enemyHome,damage.ToString(),skillColor,final?.067f:.025f,0,false,clawVfx.HitsShown);break;}
                    if(slashVfx.Configured){if(!slashVfx.Active){slashVfx.Cast(playerHome,enemyHome);skillAge=0;}enemyHitAge=0;slashVfx.Hit();slashFeedback.Hit();Damage(enemyHome,damage.ToString()+"!",skillColor,.067f,0,true);break;}
                    if(attackFrames!=null)attackAge=10;
                    if(bladeVfx.Configured){if(!bladeVfx.Active)bladeVfx.Cast(playerHome,enemyHome);enemyHitAge=0;bladeVfx.Hit();Damage(enemyHome,damage.ToString()+"!",skillColor,0,bladeVfx.HitsShown);break;}
                    skillAge=0;enemyHitAge=-.18f;
                    var from=playerHome+new Vector2(55,35);var to=enemyHome;
                    // Imported effects keep their full composite hierarchy. A small pooled pixel
                    // accent guarantees readable impact feedback if a third-party texture or
                    // animation has a delayed first frame.
                    if(UsesPixelWindSkill){if(!galeVfx.Play(playerHome,to,enemyGraphic.rectTransform.rect.size))windVfx.Skill(to);}
                    else if(!assetVfx.PlayAt(to))
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
                    if(clawVfx.Configured){if(!clawVfx.Active){clawVfx.Cast(playerHome,enemyHome);skillAge=0;}enemyHitAge=0;clawVfx.Hit();bool finalClaw=clawVfx.HitsShown%2==0;clawFeedback.Hit(finalClaw);Damage(enemyHome,damage.ToString(),skillColor,finalClaw?.067f:.025f,0,false,clawVfx.HitsShown);break;}
                    if(slashVfx.Configured){enemyHitAge=0;slashVfx.Hit();slashFeedback.Hit();Damage(enemyHome,damage.ToString()+"!",skillColor,.067f,0,true);break;}
                    if(bladeVfx.Configured){if(!bladeVfx.Active)bladeVfx.Cast(playerHome,enemyHome);enemyHitAge=0;bladeVfx.Hit();Damage(enemyHome,damage.ToString()+"!",skillColor,0,bladeVfx.HitsShown);break;}
                    enemyHitAge=-.06f;
                    if(UsesPixelWindSkill)windVfx.SkillHit(enemyHome);else Burst(enemyHome,skillColor,8,0,70);
                    Damage(enemyHome,damage.ToString()+"!",skillColor,0);
                    break;
                case CombatCue.Dodge:
                    if(skillFrames!=null)skillAge=10;
                    if(attackFrames!=null)attackAge=10;
                    dodgeAge=0;
                    if(zephyrWind)windVfx.Dodge(playerGraphic as Image,player,dodgeDirection);
                    else for(int i=0;i<6;i++) Emit(playerHome+new Vector2(-20*dodgeDirection,i*8-20),new Vector2(-120*dodgeDirection,15),new Vector2(24,3),new Color(.45f,.95f,1),.28f,i*.025f);
                    break;
                case CombatCue.EnemyHit:
                    if(dodgeFrames!=null)dodgeAge=10;
                    if(skillFrames!=null)skillAge=10;
                    if(attackFrames!=null)attackAge=10;
                    enemyAttackAge=0;playerHitAge=0;
                    if(zephyrWind)windVfx.Hit(playerHome+new Vector2(35,25));else Burst(playerHome,new Color(1,.42f,.35f),12,0,110);
                    Damage(playerHome,damage.ToString(),new Color(1,.48f,.40f),0);
                    break;
                case CombatCue.PlayerStatusHit:
                    if(dodgeFrames!=null)dodgeAge=10;
                    if(skillFrames!=null)skillAge=10;
                    if(attackFrames!=null)attackAge=10;
                    playerHitAge=0;
                    if(zephyrWind)windVfx.Hit(playerHome+new Vector2(35,25));else Burst(playerHome,new Color(1,.55f,.25f),5,0,45);
                    Damage(playerHome,damage.ToString(),new Color(1,.65f,.35f),0);
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
        void Damage(Vector2 at,string text,Color color,float delay,int bladeHit=0,bool heavy=false,int clawHit=0)
        {
            var n=numbers[numberIndex++%numbers.Length];n.active=true;n.start=at+new Vector2((numberIndex%3-1)*22,65);
            n.bladeNumber=bladeHit>0;n.drift=new Vector2(0,65);
            if(n.bladeNumber){int lane=(bladeHit-1)%3-1;n.start=at+new Vector2(lane*70,78+(bladeHit%2)*12);n.drift=new Vector2(lane*25,55);}
            if(heavy){n.start=at+new Vector2(-52,78);n.drift=new Vector2(-8,52);}
            n.text.fontSize=heavy?30:26;if(clawHit>0){bool final=clawHit%2==0;n.start=at+new Vector2(final?62:-64,78);n.drift=new Vector2(final?12:-12,52);n.text.fontSize=final?30:25;}n.age=-delay;n.color=color;n.text.text=text;n.text.gameObject.SetActive(false);
        }
        static float Pulse(float age,float length) => age>=0&&age<length?Mathf.Sin(age/length*Mathf.PI):0;
        public void Step(BattleModel battle,float delta)
        {
            if(delta<=0 || player==null) return;
            clock+=delta;attackAge+=delta;skillAge+=delta;dodgeAge+=delta;enemyAttackAge+=delta;playerHitAge+=delta;enemyHitAge+=delta;
            assetVfx.Step(delta);
            windVfx.Step(delta);if(battle.Result!=BattleResult.Fighting)galeVfx.Clear();else galeVfx.Step(delta);
            if(battle.Result!=BattleResult.Fighting)bladeVfx.Clear();else bladeVfx.Step(delta);
            if(battle.Result!=BattleResult.Fighting)slashVfx.Clear();else slashVfx.Step(delta);
            if(battle.Result!=BattleResult.Fighting)clawVfx.Clear();else clawVfx.Step(delta);
            bool fighting=battle.Result==BattleResult.Fighting;
            if(!fighting) resultAge+=delta;
            bool hasIdle=idleFrames!=null && idleFrames.frames!=null && idleFrames.frames.Length>0 && playerGraphic is Image;
            bool hasAttack=attackFrames!=null && attackFrames.frames!=null && attackFrames.frames.Length>0 && playerGraphic is Image;
            bool hasSkill=skillFrames!=null && skillFrames.frames!=null && skillFrames.frames.Length>0 && playerGraphic is Image;
            bool hasDodge=dodgeFrames!=null && dodgeFrames.frames!=null && dodgeFrames.frames.Length>0 && playerGraphic is Image;
            bool hasHit=hitFrames!=null && hitFrames.frames!=null && hitFrames.frames.Length>0 && playerGraphic is Image;
            bool spriteDeath=battle.Result==BattleResult.Defeat && deathFrames!=null && deathFrames.frames!=null && deathFrames.frames.Length>0 && playerGraphic is Image;
            bool spriteHit=fighting && hasHit && playerHitAge<hitFrames.Length;
            float dodgeLength=battle.Dragon.dodgeDuration>0?battle.Dragon.dodgeDuration:BattleModel.DodgeDuration;
            bool spriteDodge=fighting && hasDodge && dodgeAge<dodgeLength && playerHitAge>=.24f;
            bool skillActive=skillAge<(hasSkill?skillFrames.Length:.4f);
            bool defensiveAction=dodgeAge<dodgeLength || playerHitAge<.24f;
            bool spriteSkill=fighting && hasSkill && skillActive && !defensiveAction;
            bool priorityAction=skillActive || defensiveAction;
            bool spriteAttack=fighting && hasAttack && !priorityAction && attackAge<attackFrames.Length;
            if(spriteDeath)
            {
                var image=(Image)playerGraphic;image.sprite=deathFrames.Once(resultAge);AlignFrames(image,deathFrames);
            }
            else if(spriteHit)
            {
                var image=(Image)playerGraphic;image.sprite=hitFrames.Once(playerHitAge);AlignFrames(image,hitFrames);
            }
            else if(spriteDodge)
            {
                var image=(Image)playerGraphic;image.sprite=dodgeFrames.Once(dodgeAge/dodgeLength*dodgeFrames.Length);AlignFrames(image,dodgeFrames);
            }
            else if(spriteSkill)
            {
                var image=(Image)playerGraphic;image.sprite=skillFrames.Once(skillAge);AlignFrames(image,skillFrames);
            }
            else if(spriteAttack)
            {
                var image=(Image)playerGraphic;image.sprite=attackFrames.Once(attackAge);AlignFrames(image,attackFrames);
            }
            else if(hasIdle)
            {
                bool acting=attackAge<.24f || skillAge<.4f || dodgeAge<BattleModel.DodgeDuration || playerHitAge<.24f;
                ((Image)playerGraphic).sprite=idleFrames.At(fighting&&!acting ? clock : 0);
                AlignIdle((Image)playerGraphic);
            }
            else if(hasAttack || hasSkill || hasDodge || hasHit)
            {
                var image=(Image)playerGraphic;image.sprite=restSprite;AlignFrames(image,null);
            }
            float breath=fighting&&!hasIdle?Mathf.Sin(clock*3.6f):0;
            float bounce=fighting?Mathf.Abs(Mathf.Sin(clock*3.3f)):0;
            float windup=fighting?battle.EnemyWindupProgress:0;
            float attack=Pulse(attackAge,.24f),cast=Pulse(skillAge,.4f),dodge=Pulse(dodgeAge,dodgeLength);
            if(spriteSkill)attack=0;
            if(spriteDodge){attack=0;cast=0;}
            if(spriteHit){attack=0;cast=0;dodge=0;}
            float enemyAttack=Pulse(enemyAttackAge,.28f);
            if(battle.EnemyStunned){enemyAttack=0;enemyAttackAge=10;bounce=0;}
            var playerOffset=new Vector2(28*attack+66*dodge*dodgeDirection-9*cast,4*breath+48*attack+12*dodge);
            var enemyOffset=new Vector2(-20*enemyAttack,9*bounce-46*enemyAttack-12*windup);
            if(playerHitAge>=0&&playerHitAge<.24f) playerOffset.x+=Mathf.Sin(playerHitAge*95)*10*(1-playerHitAge/.24f);
            if(!slashVfx.Active&&!clawVfx.Active&&enemyHitAge>=0&&enemyHitAge<.24f) enemyOffset.x+=Mathf.Sin(enemyHitAge*95)*12*(1-enemyHitAge/.24f);
            player.anchoredPosition=playerHome+Snap(playerOffset);enemy.anchoredPosition=enemyHome+Snap(enemyOffset);
            float attackStretch=hasAttack?0:attack;
            float castStretch=hasSkill?0:cast;
            player.localScale=new Vector3(1-.018f*breath+.07f*attackStretch-.06f*castStretch,1+.025f*breath-.05f*attackStretch+.08f*castStretch,1);
            enemy.localScale=new Vector3(1+.04f*(1-bounce)+.16f*windup-.12f*enemyAttack,1-.06f*(1-bounce)-.19f*windup+.16f*enemyAttack,1);
            player.localRotation=Quaternion.Euler(0,0,-9*attack-13*dodge*dodgeDirection+4*cast);
            enemy.localRotation=Quaternion.Euler(0,0,windup*Mathf.Sin(clock*40)*3+10*enemyAttack);
            playerGraphic.color=HitColor(playerColor,playerHitAge);
            enemyGraphic.color=HitColor(enemyColor,enemyHitAge);
            if(fighting)
            {
                float tint=.40f+.12f*Mathf.Sin((float)battle.Time*5);
                if(BattleStatusHud.HasStatus(battle,true))playerGraphic.color=Color.Lerp(playerGraphic.color,BattleStatusHud.ActorTint(battle,true),tint);
                if(BattleStatusHud.HasStatus(battle,false))enemyGraphic.color=Color.Lerp(enemyGraphic.color,BattleStatusHud.ActorTint(battle,false),tint);
            }
            if(fighting&&bladeVfx.FinalFreeze)enemy.anchoredPosition=bladeEnemyPose;
            bladeEnemyPose=enemy.anchoredPosition;
            slashFeedback.Step(delta,fighting,!defensiveAction);clawFeedback.Step(delta,fighting,!defensiveAction);
            if(dodge>0){var c=playerGraphic.color;c.a=1-.45f*dodge;playerGraphic.color=c;}
            if(!fighting)
            {
                var defeated=battle.Result==BattleResult.Victory?enemy:player;
                var graphic=battle.Result==BattleResult.Victory?enemyGraphic:playerGraphic;
                if(spriteDeath)
                {
                    Restore(player,playerHome);
                    // Hold the final collapsed pose under the existing result screen.
                    playerGraphic.color=playerColor;
                }
                else
                {
                    float t=Mathf.Clamp01((resultAge-.18f)/.45f);
                    defeated.localRotation=Quaternion.Euler(0,0,-18*t);defeated.localScale=Vector3.one*(1-.22f*t);
                    var c=graphic.color;c.a=1-t;graphic.color=c;
                }
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
                n.text.gameObject.SetActive(true);n.text.rectTransform.anchoredPosition=n.start+n.drift*n.age;
                if(n.bladeNumber){var pos=n.text.rectTransform.anchoredPosition;pos.y=Mathf.Min(pos.y,-142);n.text.rectTransform.anchoredPosition=pos;}
                n.text.rectTransform.localScale=Vector3.one*(1+.25f*Pulse(n.age,.18f));
                var c=n.color;c.a=1-Mathf.Clamp01((n.age-.35f)/.35f);n.text.color=c;
            }
        }
        void AlignIdle(Image image)
        {
            AlignFrames(image,idleFrames);
        }
        void AlignFrames(Image image,DragonIdleFrames frames)
        {
            var offset=frames!=null ? frames.OffsetFor(image.sprite) : Vector2.zero;
            image.rectTransform.localScale=Vector3.one*(frames!=null?Mathf.Max(.1f,frames.displayScale):1);
            image.rectTransform.anchoredPosition=Vector2.Scale(offset,image.rectTransform.rect.size);
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








