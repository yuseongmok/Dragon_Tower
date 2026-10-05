using System;
namespace DragonTower
{
    public enum AugmentMechanic
    {
        None,OverloadCore,ManaRampage,RapidFireInstinct,ConsecutiveSlash,LastStand,FirstAid,
        ExploitWeakness,FlameRemnant,StaticDischarge,FrostBarrier,Inferno,Tingly,DodgeMaster,
        IceCream,Transference,DoubleCasting,Vampire,IndomitableWill,Spread,Clone
    }
    [Serializable]
    public sealed class BattleAugment
    {
        public AugmentMechanic mechanic;public float primaryValue,secondaryValue,chancePercent,duration;public int triggerCount=1;
    }
    public enum SkillEffectKind { Fire, Frost, Wind, Earth, Lightning, Water, Dark, Light }
    public enum CombatStatusEffect { None, Burn, Paralyze, Slow, Stun, Poison }
    public enum ElementType { Neutral, Water, Fire, Ice, Wind, Earth, Lightning, Dark, Light }
    public static class ElementRules
    {
        public static bool HasAdvantage(ElementType attacker,ElementType defender)
        {
            return (attacker==ElementType.Water&&defender==ElementType.Fire)
                ||(attacker==ElementType.Fire&&defender==ElementType.Ice)
                ||(attacker==ElementType.Ice&&defender==ElementType.Wind)
                ||(attacker==ElementType.Wind&&defender==ElementType.Earth)
                ||(attacker==ElementType.Earth&&defender==ElementType.Lightning)
                ||(attacker==ElementType.Lightning&&defender==ElementType.Water)
                ||(attacker==ElementType.Dark&&defender==ElementType.Light)
                ||(attacker==ElementType.Light&&defender==ElementType.Dark);
        }
        public static int Damage(int baseDamage,ElementType attacker,ElementType defender)
        {
            if(baseDamage<0)throw new ArgumentOutOfRangeException(nameof(baseDamage));
            return HasAdvantage(attacker,defender)?(baseDamage*3+1)/2:baseDamage;
        }
        public static string DisplayName(ElementType element)
        {
            switch(element)
            {
                case ElementType.Water:return "물";case ElementType.Fire:return "불";case ElementType.Ice:return "얼음";
                case ElementType.Wind:return "바람";case ElementType.Earth:return "흙";case ElementType.Lightning:return "번개";
                case ElementType.Dark:return "어둠";case ElementType.Light:return "빛";default:return "중립";
            }
        }
    }
    public sealed class SkillStats
    {
        public bool celestialSignature;public int celestialExtraDamage=42;
        public bool timeDomain;public float timeDomainDelay=.24f,timeDomainDuration=5;
        public bool useSecondaryElement;public ElementType secondaryElement;
        public float attackEmpowerDuration,attackEmpowerDelay;public int attackEmpowerDamage;
        public bool chargedBeam;public float chargeDuration=5,beamIgnitionDelay=.18f,beamDuration=5;
        public bool statusOnFinalHit;public float targetCriticalBonus,targetCriticalDuration;public float[] hitTimeOffsets;
        public bool HasCustomTiming {get{if(hitTimeOffsets==null||hitTimeOffsets.Length!=hitCount)return false;float last=-1;foreach(float t in hitTimeOffsets){if(float.IsNaN(t)||float.IsInfinity(t)||t<0||t<=last)return false;last=t;}return Math.Abs(hitTimeOffsets[0]-initialHitDelay)<.0001f;}}
        public float HitOffset(int index){int count=Math.Max(1,hitCount);return HasCustomTiming?(index/count)*(hitTimeOffsets[count-1]+Math.Max(.03f,hitInterval))+hitTimeOffsets[index%count]:initialHitDelay+index*Math.Max(.03f,hitInterval);}
        public float completionHealPercent,completionHealDelay=.22f;
        public string exclusiveDragonId;public float protectedCastDuration,protectedCastDelay,completionDefenseDuration,completionDefensePercent;
        public float dodgeFreeCastDuration,dodgeFreeAfterDuration,finalHitDamageMultiplier=1;
        public float slowBonusDamagePercent,slowBonusDelay=.16f;
        public string displayName;public ElementType elementType;public int damage;public float cooldown;
        public bool statusOnHit;public float initialHitDelay;public int hitCount=1;public float hitInterval=.14f;public CombatStatusEffect statusEffect;
        public float statusChancePercent,statusDuration,statusPower;
    }
    public sealed class BattleStats
    {
        public string speciesId;
        public string displayName,element;public ElementType elementType;public int maxHP,attackDamage;public SkillEffectKind skillEffect;public SkillStats skill;
        public float attackCooldown=.3f,dodgeCooldown=1.4f,dodgeDuration=.42f;
        public float criticalChance=0,criticalDamage=1.5f,damageReductionPercent;
        public bool skillDisabled;
        public string passiveName,passiveDescription;public DragonPassiveMechanic passiveMechanic;public ElementType alternateSkillElement;
        public bool passiveAvailable=true;public int passiveStage;
        public BattleAugment[] augments=Array.Empty<BattleAugment>();
        public BattleItem[] items=Array.Empty<BattleItem>();
    }
    public sealed class BattleEnemyStats
    {
        public bool timeStopImmune;public float timeStopDurationMultiplier=1;
        public int floor=1;
        public float specialPatternInterval=11,specialPatternDuration=3,barrierHealPercent=12,reflectionPercent=20;
        public string displayName;public ElementType elementType;public int maxHP,damage;public float interval;
        public float timingVariancePercent,quickAttackChancePercent,quickAttackIntervalMultiplier=.6f;
        public EnemyBossPattern bossPattern;public int patternEveryAttacks=4;public float patternIntervalMultiplier=1,patternDamageMultiplier=1;
        public int barrierHP;public float barrierDuration=4.5f,barrierFailureDamagePercent=35;
        public static BattleEnemyStats Normal()=>new BattleEnemyStats{displayName="바위 슬라임",elementType=ElementType.Neutral,maxHP=240,damage=18,interval=2.4f};
        public const int FirstAreaCount=5;
        public static BattleEnemyStats FirstArea(int index)
        {
            var enemy=Normal();
            switch(index)
            {
                case 0:return enemy;
                case 1:enemy.displayName="소형 골렘";enemy.elementType=ElementType.Earth;return enemy;
                case 2:enemy.displayName="던전 좀비";enemy.elementType=ElementType.Dark;return enemy;
                case 3:enemy.displayName="동굴 박쥐";enemy.elementType=ElementType.Dark;return enemy;
                case 4:enemy.displayName="갑옷 해골 기사";enemy.elementType=ElementType.Earth;return enemy;
                default:throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
        public static BattleEnemyStats AncientGolem(int floor)=>new BattleEnemyStats
        {displayName="고대 룬 골렘",elementType=ElementType.Earth,maxHP=360+(floor/10-1)*60,damage=24+(floor/10-1)*2,interval=2.2f,
            bossPattern=EnemyBossPattern.EarthShatter,patternEveryAttacks=4,patternIntervalMultiplier=1.3f,patternDamageMultiplier=1.6f};
    }
    public enum BattleResult { Fighting, Victory, Defeat }
    public enum CombatCue { Attack, Skill, SkillHit, Dodge, EnemyHit, EnemyMiss, BossSkill, PlayerStatusHit, SkillCast, SkillBonusHit, SkillHeal, ChargeFailed, BeamPrimed, BeamStarted, BeamEnded, EmpowerStarted, EmpowerEnded, EmpoweredAttackHit, TimeStopped, TimeStored, TimeReleased, CelestialPrimed, CelestialExtraHit }
    // Pure combat rules: no scene dependencies; can be tested without rendering.
    public sealed partial class BattleModel
    {
        public const float AttackCooldown = .3f, DodgeCooldown = 1.4f, DodgeDuration = .42f;
        public const float EnemyInterval = 2.4f, WindupDuration = .85f;
        public int PlayerHP { get; private set; }
        public int EnemyHP { get; private set; }
        public int ShieldHP => Time<shieldUntil?shieldHP:0;
        public bool EnemyBurning => Time<burnUntil;
        public bool EnemySlowed => Time<slowUntil;
        public bool ResolvingSkillHit {get;private set;}
        public float EnemySlowPower=>EnemySlowed?slowPercent:0;
        public bool EnemyParalyzed => Time<paralyzeUntil;
        public bool EnemyStunned => Time<stunUntil;
        public bool EnemyPoisoned => poisonDamage>0;
        public int EnemyShieldHP => enemyShieldHP;
        public int EnemyShieldMaxHP { get; private set; }
        public double EnemyShieldRemaining => enemyShieldHP>0?Math.Max(0,enemyShieldUntil-Time):0;
        public bool PassiveConsumed { get; private set; }
        public bool CanUseSkill => Result==BattleResult.Fighting&&!TimeDomainBusy&&!ChargedBeamBusy&&ChargeCooldownReady&&!SignatureCastLocked&&(string.IsNullOrEmpty(Dragon.skill.exclusiveDragonId)||Dragon.skill.exclusiveDragonId==Dragon.speciesId)&&!PlayerStunned&&!PlayerSkillSealed&&!Dragon.skillDisabled&&(Time>=SkillReady||Dragon.passiveMechanic==DragonPassiveMechanic.VoidAccelerator);
        double protectedSkillUntil,protectedSkillFrom;
        public bool SignatureCastLocked=>Result==BattleResult.Fighting&&Time<protectedSkillUntil;
        public bool ProtectedSkillActive=>SignatureCastLocked&&Time>=protectedSkillFrom;
        public double ProtectedSkillRemaining=>ProtectedSkillActive?protectedSkillUntil-Time:0;
        public void CancelProtectedSkill(){if(SignatureCastLocked){pendingSkillHits=0;pendingFirstSkillHit=false;}protectedSkillUntil=protectedSkillFrom=0;CancelCompletionDefense();CancelChargedBeam();}
        public const int EnemyMaxHP = 240, EnemyDamage = 18;
        public int CurrentEnemyMaxHP => Enemy.maxHP;
        public BattleResult Result { get; private set; }
        public double Time { get; private set; }
        public double AttackReady, SkillReady, DodgeReady;
        public double DodgeUntil { get; private set; } = -1;
        public double NextEnemyStrike { get; private set; } = EnemyInterval;
        double enemyWindupDuration=WindupDuration;
        float strikeSlowMultiplier=1;
        public double CurrentEnemyWindupDuration=>enemyWindupDuration;
        public float EnemyWindupProgress=>EnemyStunned?0:(float)Math.Max(0,Math.Min(1,1-(NextEnemyStrike-Time)/enemyWindupDuration));
        public double EnemyStunRemaining=>Math.Max(0,stunUntil-Time);
        public double EnemySlowRemaining=>Math.Max(0,slowUntil-Time);
        public double EnemyBurnRemaining=>Math.Max(0,burnUntil-Time);
        public double EnemyParalyzeRemaining=>Math.Max(0,paralyzeUntil-Time);
        public string EnemyIntent { get; private set; } = "일반 공격";
        public BattleStats Dragon { get; }
        public BattleEnemyStats Enemy { get; }
        readonly Func<double> random;
        int attackHits,enemyAttackCount;int shieldHP,enemyShieldHP;int burnDamage,poisonDamage;int pendingSkillHits;bool pendingFirstSkillHit;double nextSkillHit;
        float nextEnemyDamageMultiplier=1;
        double shieldUntil,enemyShieldUntil,burnUntil,burnNext,slowUntil,paralyzeUntil,stunUntil,passiveRapidUntil,passivePoisonNext,rapidUntil,firstAidUntil,frostBarrierReady;
        double invulnerableUntil,noAttackCooldownUntil,noSkillCooldownUntil,skillDamageBuffUntil,criticalBuffUntil,attackDamageBuffUntil;
        double poisonNext=10,meteorNext=10;float slowPercent,skillDamageBuffPercent,criticalBuffPercent,attackDamageBuffPercent;
        bool firstAidUsed,dodgeAttemptedForStrike,phoenixUsed,echoReady;
        public event Action<string> Feedback;
        public event Action<CombatCue, int> Cue;
        public BattleModel(BattleStats dragon):this(dragon,BattleEnemyStats.Normal(),dragon==null?0:dragon.maxHP,null) {}
        public BattleModel(BattleStats dragon,BattleEnemyStats enemy):this(dragon,enemy,dragon==null?0:dragon.maxHP,null) {}
        public BattleModel(BattleStats dragon,BattleEnemyStats enemy,int initialHP):this(dragon,enemy,initialHP,null) {}
        public BattleModel(BattleStats dragon,BattleEnemyStats enemy,int initialHP,Func<double> randomSource)
        {
            if (dragon == null || dragon.skill == null) throw new ArgumentException("Dragon and skill data are required.");
            if(enemy==null||enemy.maxHP<=0||enemy.damage<0||enemy.interval<=0)throw new ArgumentException("Valid enemy data is required.");
            Dragon = dragon;Enemy=enemy;
            random=randomSource??new Random().NextDouble;
            PlayerHP = Math.Max(0,Math.Min(dragon.maxHP,initialHP));
            EnemyHP=enemy.maxHP;NextEnemyStrike=enemy.interval;
            InitializeAreaMechanics();
            if(Dragon.passiveMechanic==DragonPassiveMechanic.HydraVenom){poisonDamage=Math.Max(1,(int)Math.Round(Dragon.attackDamage*Passive(.18f)));passivePoisonNext=Time+1;}
        }
        public void Tick(float delta)
        {
            if(float.IsNaN(delta)||float.IsInfinity(delta)||delta<=0)return;
            double remaining=delta;
            while(remaining>0.0000001&&(Result==BattleResult.Fighting||CompletionHealPending))
            {double step=CelestialStep(TimeDomainStep(AttackEmpowerStep(ChargedBeamStep(Math.Min(.05,remaining)))));TickCompletionHeal(step);TickStep(step);if(Result!=BattleResult.Fighting){CancelTimeDomain();CancelCelestial();}remaining-=step;}
        }
        void TickStep(double delta)
        {
            if (Result != BattleResult.Fighting || delta <= 0) return;
            SlowPlayerCooldowns(delta);
            PauseEnemyTime(delta);
            Time += delta;
            TickTimeDomain();
            if(Result!=BattleResult.Fighting)return;
            TickAttackEmpower();
            TickAreaMechanics();
            if(Result!=BattleResult.Fighting)return;
            if(Time>=shieldUntil)shieldHP=0;
            if(enemyShieldHP>0&&Time>=enemyShieldUntil)ResolveForgeBarrierFailure();
            if(Result!=BattleResult.Fighting)return;
            while(pendingSkillHits>0&&nextSkillHit<=Time&&Result==BattleResult.Fighting)
            {
                bool first=pendingFirstSkillHit;pendingFirstSkillHit=false;PerformSkillHit(first);
                if(Result!=BattleResult.Fighting){pendingSkillHits=0;break;}
                pendingSkillHits--;if(Dragon.skill.HasCustomTiming)nextSkillHit=skillCastTime+Dragon.skill.HitOffset(resolvedCastHits);else nextSkillHit+=Math.Max(.03f,Dragon.skill.hitInterval);
            }
            TickCelestial();
            TickSlowSignature();
            while(burnNext>0&&burnNext<=Time&&burnNext<=burnUntil&&Result==BattleResult.Fighting)
            {
                int burn=Math.Max(1,burnDamage);DamageEnemy(burn);Feedback?.Invoke("화상 피해  −"+burn);burnNext+=1;
                if(EnemyHP==0&&Result==BattleResult.Fighting)Result=BattleResult.Victory;
            }
            while(poisonDamage>0&&passivePoisonNext<=Time&&Result==BattleResult.Fighting)
            {DamageEnemy(poisonDamage);Cue?.Invoke(CombatCue.SkillHit,poisonDamage);Feedback?.Invoke("중독 피해  −"+poisonDamage);passivePoisonNext+=2;if(EnemyHP==0&&Result==BattleResult.Fighting)Result=BattleResult.Victory;}
            var poison=ItemRule(ItemMechanic.PoisonFang);
            while(poison!=null&&poisonNext<=Time&&Result==BattleResult.Fighting)
            {
                int damage=RawExtra(Dragon.attackDamage,poison.primaryValue*Math.Max(1,poison.stacks));poisonNext+=10;
                Cue?.Invoke(CombatCue.SkillHit,damage);Feedback?.Invoke("독니 지속 피해  −"+damage);
            }
            var meteor=ItemRule(ItemMechanic.MeteorFragment);
            while(meteor!=null&&meteorNext<=Time&&Result==BattleResult.Fighting)
            {
                int damage=ElementalExtra(Dragon.skill.damage,meteor.primaryValue*Math.Max(1,meteor.stacks),ElementType.Fire);meteorNext+=10;
                Cue?.Invoke(CombatCue.SkillHit,damage);Feedback?.Invoke("운석 파편 추가 공격  −"+damage);
            }
            while (!EnemyTimeStopped && NextEnemyStrike <= Time && Result == BattleResult.Fighting)
            {
                if(Time<stunUntil){NextEnemyStrike=Math.Max(NextEnemyStrike,stunUntil+enemyWindupDuration);break;}
                bool paralyzed=EnemyParalyzed&&Roll(20);
                if(ProtectedSkillActive)
                {
                    // Signature immunity is not a dodge: no dodge rewards or actor hit cue.
                    Feedback?.Invoke(Dragon.skill.displayName+" · 무적");
                }
                else if (NextEnemyStrike < DodgeUntil||Time<invulnerableUntil||paralyzed)
                {
                    Cue?.Invoke(CombatCue.EnemyMiss, 0);
                    Feedback?.Invoke(paralyzed?"마비! 적의 공격이 실패했습니다":"회피 성공! 피해를 피했습니다");
                    var mastery=Rule(AugmentMechanic.DodgeMaster);if(mastery!=null)ReduceSkillRemainingPercent(mastery.primaryValue);
                    if(!paralyzed&&Dragon.passiveMechanic==DragonPassiveMechanic.HolyLight&&Roll(Passive(10))){int heal=Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*Passive(.2f)));PlayerHP=Math.Min(Dragon.maxHP,PlayerHP+heal);Feedback?.Invoke("성스러운 빛! HP "+heal+" 회복");}
                }
                else
                {
                    int patternedDamage=Math.Max(0,(int)Math.Round(Enemy.damage*nextEnemyDamageMultiplier,MidpointRounding.AwayFromZero));
                    int damage=ElementRules.Damage(patternedDamage,Enemy.elementType,Dragon.elementType);
                    damage=Math.Max(0,(int)Math.Round(damage*(1-Math.Min(80,Math.Max(0,Dragon.damageReductionPercent))/100f),MidpointRounding.AwayFromZero));
                    if(Dragon.passiveMechanic==DragonPassiveMechanic.Mirage&&Roll(Passive(20))){damage=0;Feedback?.Invoke("신기루! 피해를 무시했습니다");}
                    if(Dragon.passiveMechanic==DragonPassiveMechanic.HardenedShell&&PlayerHP<Dragon.maxHP*.5f)damage=(int)Math.Round(damage*(1-Passive(.2f)),MidpointRounding.AwayFromZero);
                    int absorbed=Math.Min(ShieldHP,damage);shieldHP-=absorbed;damage-=absorbed;damage=ApplyCompletionDefense(damage);
                    NotifyChargedBeamDamage(Math.Min(PlayerHP,damage));
                    PlayerHP = Math.Max(0, PlayerHP - damage);
                    if(Dragon.passiveMechanic==DragonPassiveMechanic.Phoenix&&Dragon.passiveAvailable&&!PassiveConsumed&&PlayerHP<=Dragon.maxHP*Passive(.2f)){PlayerHP=Dragon.maxHP;PassiveConsumed=true;Feedback?.Invoke("불사조! 체력을 완전히 회복했습니다");}
                    if(PlayerHP==0)
                    {
                        var feather=ItemRule(ItemMechanic.PhoenixFeather);
                        if(feather!=null&&!phoenixUsed){phoenixUsed=true;PlayerHP=Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*feather.primaryValue/100f));Feedback?.Invoke("불사조의 깃털! 다시 일어났습니다");}
                        else Result=BattleResult.Defeat;
                    }
                    Cue?.Invoke(CombatCue.EnemyHit, damage);
                    Feedback?.Invoke((ElementRules.HasAdvantage(Enemy.elementType,Dragon.elementType)?"상성 약점!  ":"")+"적의 공격  −"+damage+" HP"+(absorbed>0?" · 보호막 "+absorbed:"") );
                    if(Has(AugmentMechanic.IndomitableWill))SkillReady=Time;
                    TriggerFirstAid();
                    if(damage>0&&Result==BattleResult.Fighting)ApplyAreaHitEffects();
                    var barrier=Rule(AugmentMechanic.FrostBarrier);
                    if(dodgeAttemptedForStrike&&barrier!=null&&Time>=frostBarrierReady&&Result==BattleResult.Fighting)
                    {shieldHP=Math.Max(shieldHP,Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*barrier.primaryValue/100f)));shieldUntil=Time+barrier.duration;frostBarrierReady=Time+barrier.secondaryValue;Feedback?.Invoke("서리 장벽! 보호막 "+shieldHP);}
                }
                dodgeAttemptedForStrike=false;
                ScheduleNextEnemyAttack();
            }
            TickChargedBeam();
        }
        void ScheduleNextEnemyAttack()
        {
            enemyAttackCount++;float slow=Time<slowUntil?1+slowPercent/100f:1f;
            float variance=Math.Min(40,Math.Max(0,Enemy.timingVariancePercent))/100f;
            float jitter=1f+(float)(random()*2-1)*variance;
            float interval=Math.Max(.45f,Enemy.interval*slow*jitter);
            nextEnemyDamageMultiplier=1;EnemyIntent="일반 공격";
            int every=Math.Max(2,Enemy.patternEveryAttacks);
            bool attackPattern=Enemy.bossPattern==EnemyBossPattern.EarthShatter||Enemy.bossPattern==EnemyBossPattern.AbyssalRush||Enemy.bossPattern==EnemyBossPattern.ForgeBarrier||Enemy.bossPattern==EnemyBossPattern.GarudaRend;
            if(attackPattern&&(enemyAttackCount+1)%every==0)
            {
                Cue?.Invoke(CombatCue.BossSkill,0);
                interval=Math.Max(.45f,Enemy.interval*slow*Math.Max(.35f,Enemy.patternIntervalMultiplier));
                nextEnemyDamageMultiplier=Math.Max(.25f,Enemy.patternDamageMultiplier);
                if(Enemy.bossPattern==EnemyBossPattern.EarthShatter)
                {EnemyIntent="대지 분쇄";Feedback?.Invoke("보스 패턴 · 대지 분쇄를 준비합니다!");}
                else if(Enemy.bossPattern==EnemyBossPattern.AbyssalRush)
                {EnemyIntent="심해 촉수 기습";Feedback?.Invoke("보스 패턴 · 촉수가 빠르게 덮쳐옵니다!");}
                else if(Enemy.bossPattern==EnemyBossPattern.ForgeBarrier)
                {EnemyIntent="용광로 과열";ActivateForgeBarrier();}
                else if(Enemy.bossPattern==EnemyBossPattern.GarudaRend)
                {EnemyIntent="칼날 돌풍";Feedback?.Invoke("칼날 돌풍 · 피격 시 찰과상 연속 피해!");}
            }
            else if(Enemy.quickAttackChancePercent>0&&Roll(Enemy.quickAttackChancePercent))
            {
                interval=Math.Max(.5f,Enemy.interval*slow*Math.Max(.35f,Enemy.quickAttackIntervalMultiplier));
                EnemyIntent="기습 공격";Feedback?.Invoke("적의 움직임이 갑자기 빨라집니다!");
            }
            strikeSlowMultiplier=slow;
            enemyWindupDuration=Math.Min(WindupDuration*slow,interval);
            NextEnemyStrike+=interval;
        }
        public bool Attack()
        {
            if(SignatureCastLocked||ChargedBeamCommitted)return false;
            if (Result != BattleResult.Fighting || PlayerStunned || PlayerAttackSealed || Time < AttackReady) return false;
            AttackReady = Time + AttackCooldownNow();
            if(PlayerActionMissed(false))return true;
            bool storing=EnemyTimeStopped;bool critical;int damage;
            if(Dragon.passiveMechanic==DragonPassiveMechanic.RuyiOrb){int boosted=Percent(Dragon.attackDamage,Dragon.passiveStage*10);int first=Math.Max(1,boosted/2),second=Math.Max(1,boosted-first);damage=Hit(first,Dragon.elementType,false,out critical);bool secondCritical;damage+=Hit(second,Dragon.elementType,false,out secondCritical);critical|=secondCritical;}
            else damage=Hit(Dragon.attackDamage,Dragon.elementType,false,out critical);
            if(Result==BattleResult.Defeat)return true;
            int total=damage;attackHits++;
            var glove=ItemRule(ItemMechanic.InfightingGlove);if(glove!=null)total+=RawExtra(damage,glove.primaryValue*Math.Max(1,glove.stacks));
            var slash=Rule(AugmentMechanic.ConsecutiveSlash);if(slash!=null&&Roll(slash.chancePercent))total+=RawExtra(damage,slash.primaryValue);
            var staticHit=Rule(AugmentMechanic.StaticDischarge);if(staticHit!=null&&attackHits%Math.Max(1,staticHit.triggerCount)==0)total+=ElementalExtra(Dragon.attackDamage,staticHit.primaryValue,ElementType.Lightning);
            var spread=Rule(AugmentMechanic.Spread);if(spread!=null)total+=RawExtra(damage,spread.primaryValue);
            var clone=Rule(AugmentMechanic.Clone);if(critical&&clone!=null)for(int i=0;i<Math.Max(1,clone.triggerCount);i++)total+=RawExtra(damage,clone.primaryValue);
            var overload=Rule(AugmentMechanic.OverloadCore);if(overload!=null)ReduceSkillSeconds(overload.primaryValue);
            var transfer=Rule(AugmentMechanic.Transference);if(transfer!=null)ReduceSkillSeconds(transfer.primaryValue);
            var rapid=Rule(AugmentMechanic.RapidFireInstinct);if(rapid!=null&&attackHits%Math.Max(1,rapid.triggerCount)==0){rapidUntil=Time+rapid.duration;AttackReady=Time+Math.Max(0,AttackReady-Time)*(1-rapid.primaryValue/100f);}
            if(Dragon.passiveMechanic==DragonPassiveMechanic.Tentacle&&Roll(Passive(20)))total+=RawExtra(damage,Passive(10));
            TriggerAttackStatus();
            if(storing){Cue?.Invoke(CombatCue.TimeStored,damage);if(total>damage)Cue?.Invoke(CombatCue.SkillBonusHit,total-damage);}else Cue?.Invoke(CombatCue.Attack,total);
            ApplyEmpoweredAttack(damage);
            ApplyCelestialBasicHit(damage);
            if(storing)Feedback?.Invoke("시간 속에 기본 공격 저장 +"+damage);else Feedback?.Invoke((ElementRules.HasAdvantage(Dragon.elementType,Enemy.elementType)?"상성 우위!  ":"")+(critical?"치명타!  ":"")+"기본 공격!  −" + total);
            return true;
        }
        public bool Skill()
        {
            if (!CanUseSkill) return false;
            bool overcast=Time<SkillReady&&Dragon.passiveMechanic==DragonPassiveMechanic.VoidAccelerator;
            if(overcast){float costPercent=Math.Max(6,10-Dragon.passiveStage*2);int overcastCost=Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*costPercent/100f));PlayerHP=Math.Max(1,PlayerHP-overcastCost);Feedback?.Invoke("공허 가속기 · HP "+overcastCost+" 소모");}
            SkillReady = Time+(Time<noSkillCooldownUntil?0:Math.Max(0,Dragon.skill.cooldown)*(Time<firstAidUntil?.5f:1f));
            if(Dragon.skill.celestialSignature)CancelCelestial();
            if(PlayerActionMissed(true))return true;
            if(Dragon.skill.celestialSignature)ArmCelestial();
            if(Dragon.skill.timeDomain)return BeginTimeDomain();
            if(Dragon.skill.chargedBeam)return BeginChargedBeam();
            if(Dragon.skill.attackEmpowerDuration>0)return BeginAttackEmpower();
            skillCastTime=Time;resolvedCastHits=0;
            BeginSlowSignature();
            protectedSkillUntil=Time+Math.Max(0,Dragon.skill.protectedCastDuration);protectedSkillFrom=Time+Math.Max(0,Dragon.skill.protectedCastDelay);
            int casts=Has(AugmentMechanic.DoubleCasting)?2:1;ScheduledSkillHits=Math.Max(1,Dragon.skill.hitCount)*casts;BeginDodgeRelease();pendingSkillHits=Math.Max(1,Dragon.skill.hitCount)*casts-1;nextSkillHit=Time+Math.Max(.03f,Dragon.skill.hitInterval);
            BeginCompletionHeal();BeginCompletionDefense();
            pendingFirstSkillHit=Dragon.skill.initialHitDelay>0;
            if(pendingFirstSkillHit){pendingSkillHits++;nextSkillHit=Time+Dragon.skill.initialHitDelay;Cue?.Invoke(CombatCue.SkillCast,0);}
            else PerformSkillHit(true);if(Result==BattleResult.Defeat)return true;
            if(Result==BattleResult.Fighting&&!Dragon.skill.statusOnHit&&!Dragon.skill.statusOnFinalHit)ApplySkillStatus();
            TriggerAttackStatus();
            if(Dragon.passiveMechanic==DragonPassiveMechanic.JetStream){passiveRapidUntil=Time+Passive(1);AttackReady=Time+Math.Max(0,AttackReady-Time)*(1-Passive(.5f));Feedback?.Invoke("제트기류! 일반 공격 가속");}
            if(Dragon.passiveMechanic==DragonPassiveMechanic.IronArmor&&ShieldHP<=0){shieldHP=Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*Passive(.05f)));shieldUntil=double.MaxValue;Feedback?.Invoke("철갑! 보호막 "+shieldHP);}
            var burn=Rule(AugmentMechanic.FlameRemnant);if(burn!=null&&Result==BattleResult.Fighting)ApplyBurn(burn.duration,Math.Max(1,(int)Math.Round(Dragon.attackDamage*burn.primaryValue/100f,MidpointRounding.AwayFromZero)));
            var paralyze=Rule(AugmentMechanic.Tingly);if(paralyze!=null&&Roll(paralyze.chancePercent))ApplyParalyze(paralyze.duration);
            var slow=Rule(AugmentMechanic.IceCream);if(slow!=null&&Roll(slow.chancePercent))ApplySlow(slow.duration,slow.primaryValue);
            if(echoReady&&Result==BattleResult.Fighting)
            {
                echoReady=false;var echo=ItemRule(ItemMechanic.EchoCrystal);float power=echo==null||echo.primaryValue<=0?50:echo.primaryValue;
                int echoDamage=ElementalExtra(Dragon.skill.damage*Math.Max(1,Dragon.skill.hitCount),power,Dragon.skill.elementType);
                Cue?.Invoke(CombatCue.SkillHit,echoDamage);Feedback?.Invoke("메아리 수정 추가 시전  −"+echoDamage);
            }
            if(Result==BattleResult.Defeat)return true;
            float cost=0;var mana=Rule(AugmentMechanic.ManaRampage);if(mana!=null)cost+=mana.primaryValue;var vampire=Rule(AugmentMechanic.Vampire);if(vampire!=null)cost+=vampire.primaryValue;
            if(cost>0)PlayerHP=Math.Max(1,PlayerHP-Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*cost/100f)));
            if(cost>0)Feedback?.Invoke(Dragon.skill.displayName+" · HP 소모");
            return true;
        }
        public ElementType EffectiveSkillElement=>Dragon.skill.useSecondaryElement&&ElementRules.HasAdvantage(Dragon.skill.secondaryElement,Enemy.elementType)&&!ElementRules.HasAdvantage(Dragon.skill.elementType,Enemy.elementType)?Dragon.skill.secondaryElement:Dragon.skill.elementType;
        void PerformSkillHit(bool first)
        {
            bool critical;int baseDamage=Dragon.skill.damage;
            if(pendingSkillHits<=1||(pendingFirstSkillHit==false&&Dragon.skill.initialHitDelay>0&&(pendingSkillHits-1)%Math.Max(1,Dragon.skill.hitCount)==0))baseDamage=Math.Max(1,(int)Math.Round(baseDamage*Math.Max(1,Dragon.skill.finalHitDamageMultiplier)));
            int damage=Hit(baseDamage,EffectiveSkillElement,true,out critical);
            QueueSlowSignatureBonus(baseDamage);
            if(Dragon.skill.statusOnHit&&!Dragon.skill.statusOnFinalHit&&Result==BattleResult.Fighting)ApplySkillStatus();
            resolvedCastHits++;ApplyFinalTargetModifier(damage);NotifyCompletionDefenseHit();
            ResolvingSkillHit=true;
            try{Cue?.Invoke(first?CombatCue.Skill:CombatCue.SkillHit,damage);}finally{ResolvingSkillHit=false;}
            Feedback?.Invoke((ElementRules.HasAdvantage(EffectiveSkillElement,Enemy.elementType)?"상성 우위!  ":"")+(critical?"치명타!  ":"")+Dragon.skill.displayName+"!  −"+damage);
            NotifyCompletionHit(damage);
            if(Result!=BattleResult.Fighting)pendingSkillHits=0;
        }
        void ApplySkillStatus()
        {
            var skill=Dragon.skill;if(skill.statusEffect==CombatStatusEffect.None||!Roll(skill.statusChancePercent))return;
            switch(skill.statusEffect)
            {
                case CombatStatusEffect.Burn:ApplyBurn(skill.statusDuration,Math.Max(1,(int)Math.Round(skill.statusPower,MidpointRounding.AwayFromZero)));break;
                case CombatStatusEffect.Paralyze:ApplyParalyze(skill.statusDuration);break;
                case CombatStatusEffect.Slow:ApplySlow(skill.statusDuration,skill.statusPower);break;
                case CombatStatusEffect.Stun:ApplyStun(skill.statusDuration);break;
                case CombatStatusEffect.Poison:ApplyPoison(Math.Max(1,(int)Math.Round(skill.statusPower)));break;
            }
        }
        void ApplyBurn(float duration,int damage){burnUntil=Math.Max(burnUntil,Time+duration);burnNext=Time+1;burnDamage=Math.Max(burnDamage,damage);Feedback?.Invoke("화상! "+duration.ToString("0.#")+"초");}
        void ApplyParalyze(float duration){paralyzeUntil=Math.Max(paralyzeUntil,Time+Math.Max(1,duration));Feedback?.Invoke("마비! 공격이 20% 확률로 실패합니다");}
        void ApplySlow(float duration,float percent)
        {
            slowPercent=EnemySlowed?Math.Max(slowPercent,Math.Max(0,percent)):Math.Max(0,percent);
            slowUntil=Math.Max(slowUntil,Time+Math.Max(.1,duration));
            float multiplier=Math.Max(strikeSlowMultiplier,1+slowPercent/100f);
            double ratio=multiplier/strikeSlowMultiplier;
            double resumeAt=Math.Max(Time,stunUntil);
            NextEnemyStrike=resumeAt+Math.Max(0,NextEnemyStrike-resumeAt)*ratio;
            enemyWindupDuration*=ratio;strikeSlowMultiplier=multiplier;
            Feedback?.Invoke("둔화! 진행 중인 공격이 느려집니다");
        }
        void ApplyStun(float duration)
        {
            stunUntil=Math.Max(stunUntil,Time+Math.Max(.1,duration));
            // Keep the queued attack/intent, but give a complete telegraph after recovery.
            NextEnemyStrike=Math.Max(NextEnemyStrike,stunUntil+enemyWindupDuration);
            Feedback?.Invoke("기절! 회복 후 공격을 다시 준비합니다");
        }
        void ApplyPoison(int damage){poisonDamage=Math.Max(poisonDamage,damage);passivePoisonNext=Math.Min(passivePoisonNext<=0?Time+1:passivePoisonNext,Time+1);Feedback?.Invoke("중독! 지속 피해가 시작됩니다");}
        void TriggerAttackStatus()
        {
            if(!Roll(20))return;
            switch(Dragon.passiveMechanic){case DragonPassiveMechanic.FlameBreath:ApplyBurn(Passive(3),Math.Max(1,(int)Math.Round(Dragon.attackDamage*Passive(.12f))));break;case DragonPassiveMechanic.FreezingGaze:ApplySlow(Passive(3),Passive(25));break;case DragonPassiveMechanic.ElectricShock:ApplyParalyze(Passive(4));break;case DragonPassiveMechanic.TimeRift:ApplyStun(Passive(1));break;}
        }
        public bool Dodge()
        {
            if(SignatureCastLocked||ChargedBeamCommitted)return false;
            if (Result != BattleResult.Fighting || PlayerStunned || (!DodgeCooldownReleased && Time < DodgeReady)) return false;
            DodgeReady = Time + (Dragon.dodgeCooldown>0?Dragon.dodgeCooldown:DodgeCooldown);
            DodgeUntil = Time + (Dragon.dodgeDuration>0?Dragon.dodgeDuration:DodgeDuration);
            dodgeAttemptedForStrike=true;
            Cue?.Invoke(CombatCue.Dodge, 0);
            Feedback?.Invoke("회피 중 · 0.42초 동안 무적");
            return true;
        }
        public bool UseItem(ItemData item)
        {
            if(item==null||item.kind!=ItemKind.Consumable||Result!=BattleResult.Fighting||PlayerStunned)return false;
            float primary=item.primaryValue,secondary=item.secondaryValue,duration=Math.Max(0,item.duration);
            switch(item.mechanic)
            {
                case ItemMechanic.TimeShard:invulnerableUntil=Math.Max(invulnerableUntil,Time+duration);break;
                case ItemMechanic.BurstCore:SkillReady=Time;break;
                case ItemMechanic.LastStand:noAttackCooldownUntil=Math.Max(noAttackCooldownUntil,Time+duration);AttackReady=Time;break;
                case ItemMechanic.GiantRoar:ApplyStun(duration);break;
                case ItemMechanic.StunGun:ApplyParalyze(duration);break;
                case ItemMechanic.BloodTome:
                    PlayerHP=Math.Max(1,PlayerHP-Math.Max(1,(int)Math.Ceiling(PlayerHP*Math.Max(0,primary)/100f)));
                    skillDamageBuffPercent=Math.Max(skillDamageBuffPercent,secondary);skillDamageBuffUntil=Math.Max(skillDamageBuffUntil,Time+duration);break;
                case ItemMechanic.PurificationRod:PlayerHP=Dragon.maxHP;break;
                case ItemMechanic.FrostWitchTear:ApplySlow(duration,primary);break;
                case ItemMechanic.InfernoBreath:ApplyBurn(duration,Math.Max(1,(int)Math.Round(Dragon.attackDamage*primary/100f,MidpointRounding.AwayFromZero)));break;
                case ItemMechanic.WeaknessLens:criticalBuffPercent=Math.Max(criticalBuffPercent,primary);criticalBuffUntil=Math.Max(criticalBuffUntil,Time+duration);break;
                case ItemMechanic.EmergencyAccelerator:noSkillCooldownUntil=Math.Max(noSkillCooldownUntil,Time+duration);SkillReady=Time;break;
                case ItemMechanic.HealingPotion:case ItemMechanic.GreaterHealingPotion:
                    HealPlayer(Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*primary/100f)));break;
                case ItemMechanic.BloodPotion:
                    HealPlayer(Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*primary/100f)));
                    attackDamageBuffPercent=Math.Max(attackDamageBuffPercent,secondary);attackDamageBuffUntil=Math.Max(attackDamageBuffUntil,Time+duration);break;
                case ItemMechanic.BarrierStone:
                    shieldHP=Math.Max(shieldHP,Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*primary/100f)));shieldUntil=Math.Max(shieldUntil,Time+duration);break;
                case ItemMechanic.EchoCrystal:echoReady=true;break;
                default:return false;
            }
            Feedback?.Invoke(item.displayName+" 사용!");return true;
        }
        int Hit(int baseDamage,ElementType element,bool skill,out bool critical,bool calculateOnly=false)
        {
            int damage=ElementRules.Damage(baseDamage,element,Enemy.elementType);
            if(skill&&Time<skillDamageBuffUntil)damage=Percent(damage,skillDamageBuffPercent);
            if(!skill&&Time<attackDamageBuffUntil)damage=Percent(damage,attackDamageBuffPercent);
            if(ElementRules.HasAdvantage(element,Enemy.elementType)&&Has(AugmentMechanic.ExploitWeakness))damage=Percent(damage,Value(AugmentMechanic.ExploitWeakness,40));
            var last=Rule(AugmentMechanic.LastStand);if(last!=null){int steps=(int)Math.Floor((1-PlayerHP/(double)Math.Max(1,Dragon.maxHP))*10+.00001);damage=Percent(damage,steps*last.primaryValue);}
            var mark=ItemRule(ItemMechanic.ExecutionerMark);if(mark!=null&&EnemyHP<=Enemy.maxHP*.4f)damage=Percent(damage,mark.primaryValue*Math.Max(1,mark.stacks));
            float chance=EffectiveCriticalChancePercent;
            critical=Roll(chance);if(critical)damage=Math.Max(1,(int)Math.Round(damage*Dragon.criticalDamage,MidpointRounding.AwayFromZero));
            if(calculateOnly)return damage;
            if(!skill&&EnemyTimeStopped){storedBasicDamage=checked(storedBasicDamage+damage);storedBasicHits++;return damage;}
            damage=DamageEnemy(damage);
            if (EnemyHP == 0 && Result==BattleResult.Fighting) Result = BattleResult.Victory;
            return damage;
        }
        int RawExtra(int referenceDamage,float percent){int value=Math.Max(1,(int)Math.Round(referenceDamage*percent/100f,MidpointRounding.AwayFromZero));value=DamageEnemy(value);if(EnemyHP==0&&Result==BattleResult.Fighting)Result=BattleResult.Victory;return value;}
        int ElementalExtra(int baseDamage,float percent,ElementType element){int value=Math.Max(1,(int)Math.Round(baseDamage*percent/100f,MidpointRounding.AwayFromZero));value=ElementRules.Damage(value,element,Enemy.elementType);value=DamageEnemy(value);if(EnemyHP==0&&Result==BattleResult.Fighting)Result=BattleResult.Victory;return value;}
        int DamageEnemy(int value)
        {
            if(Result!=BattleResult.Fighting)return 0;
            value=Math.Max(0,value);
            int actual=Math.Min(value,EnemyHP+enemyShieldHP);
            if(EnemyReflecting&&actual>0)ApplyReflection(actual);
            if(enemyShieldHP<=0){EnemyHP=Math.Max(0,EnemyHP-value);return value;}
            int absorbed=Math.Min(enemyShieldHP,value);enemyShieldHP-=absorbed;int overflow=value-absorbed;if(overflow>0)EnemyHP=Math.Max(0,EnemyHP-overflow);
            if(enemyShieldHP==0){enemyShieldUntil=0;Feedback?.Invoke(Enemy.bossPattern==EnemyBossPattern.FrostRecovery?"빙결 방벽 파괴! 회복을 저지했습니다":"용광로 방벽 파괴! 폭발을 저지했습니다");}
            return value;
        }
        void ActivateForgeBarrier()
        {
            if(enemyShieldHP>0)return;EnemyShieldMaxHP=Math.Max(1,Enemy.barrierHP);enemyShieldHP=EnemyShieldMaxHP;enemyShieldUntil=Time+Math.Max(.5f,Enemy.barrierDuration);
            Feedback?.Invoke("용광로 방벽! "+Enemy.barrierDuration.ToString("0.#")+"초 안에 보호막을 파괴하세요");
        }
        void ResolveForgeBarrierFailure()
        {
            if(Enemy.bossPattern==EnemyBossPattern.FrostRecovery)
            {
                enemyShieldHP=0;enemyShieldUntil=0;int before=EnemyHP;
                EnemyHP=Math.Min(Enemy.maxHP,EnemyHP+Math.Max(1,(int)Math.Ceiling(Enemy.maxHP*Enemy.barrierHealPercent/100f)));
                Cue?.Invoke(CombatCue.BossSkill,0);Feedback?.Invoke("빙결 방벽 유지! 냉룡왕 HP +"+(EnemyHP-before));return;
            }
            enemyShieldHP=0;enemyShieldUntil=0;if(ProtectedSkillActive)return;int damage=Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*Enemy.barrierFailureDamagePercent/100f));damage=ApplyCompletionDefense(damage);NotifyChargedBeamDamage(Math.Min(PlayerHP,damage));PlayerHP=Math.Max(0,PlayerHP-damage);
            Cue?.Invoke(CombatCue.EnemyHit,damage);Feedback?.Invoke("방벽 과열 폭발! 가드 불가 피해  −"+damage+" HP");
            if(Dragon.passiveMechanic==DragonPassiveMechanic.Phoenix&&Dragon.passiveAvailable&&!PassiveConsumed&&PlayerHP<=Dragon.maxHP*Passive(.2f)){PlayerHP=Dragon.maxHP;PassiveConsumed=true;Feedback?.Invoke("불사조! 체력을 완전히 회복했습니다");}
            if(PlayerHP==0)Result=BattleResult.Defeat;
        }
        int Percent(int value,float bonus)=>Math.Max(0,(int)Math.Round(value*(1+bonus/100f),MidpointRounding.AwayFromZero));
        float Passive(float baseValue)=>baseValue*(1+Math.Max(0,Math.Min(2,Dragon.passiveStage))*.25f);
        float AttackCooldownNow()
        {
            if(Time<noAttackCooldownUntil)return .02f;
            float cooldown=Dragon.attackCooldown>0?Dragon.attackCooldown:AttackCooldown;
            if(Time<rapidUntil)cooldown*=1-Value(AugmentMechanic.RapidFireInstinct,15)/100f;
            if(Time<passiveRapidUntil)cooldown*=.5f;
            if(Time<firstAidUntil)cooldown*=.5f;return Math.Max(.02f,cooldown);
        }
        void TriggerFirstAid()
        {
            var aid=Rule(AugmentMechanic.FirstAid);if(aid==null||firstAidUsed||PlayerHP<=0||PlayerHP>Dragon.maxHP*aid.secondaryValue/100f)return;
            firstAidUsed=true;firstAidUntil=Time+aid.duration;AttackReady=Time+Math.Max(0,AttackReady-Time)*.5;SkillReady=Time+Math.Max(0,SkillReady-Time)*.5;Feedback?.Invoke("응급 처치 발동! 3초간 쿨타임 감소");
        }
        void ReduceSkillSeconds(float seconds){SkillReady=Math.Max(Time,SkillReady-Math.Max(0,seconds));}
        void ReduceSkillRemainingPercent(float percent){SkillReady=Time+Math.Max(0,SkillReady-Time)*(1-Math.Max(0,percent)/100f);}
        bool Roll(float percent)=>percent>0&&random()<Math.Min(100,percent)/100.0;
        bool Has(AugmentMechanic mechanic)=>Rule(mechanic)!=null;
        float Value(AugmentMechanic mechanic,float fallback){var rule=Rule(mechanic);return rule==null||rule.primaryValue==0?fallback:rule.primaryValue;}
        BattleAugment Rule(AugmentMechanic mechanic)
        {foreach(var augment in Dragon.augments??Array.Empty<BattleAugment>())if(augment!=null&&augment.mechanic==mechanic)return augment;return null;}
        BattleItem ItemRule(ItemMechanic mechanic)
        {foreach(var item in Dragon.items??Array.Empty<BattleItem>())if(item!=null&&item.mechanic==mechanic)return item;return null;}
    }
}


