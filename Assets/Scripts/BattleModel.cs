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
    public enum CombatStatusEffect { None, Burn, Paralyze, Slow }
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
        public string displayName;public ElementType elementType;public int damage;public float cooldown;
        public int hitCount=1;public float hitInterval=.14f;public CombatStatusEffect statusEffect;
        public float statusChancePercent,statusDuration,statusPower;
    }
    public sealed class BattleStats
    {
        public string displayName,element;public ElementType elementType;public int maxHP,attackDamage;public SkillEffectKind skillEffect;public SkillStats skill;
        public float attackCooldown=.3f,dodgeCooldown=1.4f,dodgeDuration=.42f;
        public float criticalChance=0,criticalDamage=1.5f,damageReductionPercent;
        public bool skillDisabled;
        public BattleAugment[] augments=Array.Empty<BattleAugment>();
        public BattleItem[] items=Array.Empty<BattleItem>();
    }
    public sealed class BattleEnemyStats
    {
        public string displayName;public ElementType elementType;public int maxHP,damage;public float interval;
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
        {displayName="고대 룬 골렘",elementType=ElementType.Earth,maxHP=360+(floor/10-1)*60,damage=24+(floor/10-1)*2,interval=2.2f};
    }
    public enum BattleResult { Fighting, Victory, Defeat }
    public enum CombatCue { Attack, Skill, SkillHit, Dodge, EnemyHit, EnemyMiss }
    // Pure combat rules: no scene dependencies; can be tested without rendering.
    public sealed class BattleModel
    {
        public const float AttackCooldown = .3f, DodgeCooldown = 1.4f, DodgeDuration = .42f;
        public const float EnemyInterval = 2.4f, WindupDuration = .85f;
        public int PlayerHP { get; private set; }
        public int EnemyHP { get; private set; }
        public int ShieldHP => Time<shieldUntil?shieldHP:0;
        public bool EnemyBurning => Time<burnUntil;
        public bool EnemySlowed => Time<slowUntil;
        public const int EnemyMaxHP = 240, EnemyDamage = 18;
        public int CurrentEnemyMaxHP => Enemy.maxHP;
        public BattleResult Result { get; private set; }
        public double Time { get; private set; }
        public double AttackReady, SkillReady, DodgeReady;
        public double DodgeUntil { get; private set; } = -1;
        public double NextEnemyStrike { get; private set; } = EnemyInterval;
        public BattleStats Dragon { get; }
        public BattleEnemyStats Enemy { get; }
        readonly Func<double> random;
        int attackHits;int shieldHP;int burnDamage;int pendingSkillHits;double nextSkillHit;
        double shieldUntil,burnUntil,burnNext,slowUntil,rapidUntil,firstAidUntil,frostBarrierReady;
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
        }
        public void Tick(float delta)
        {
            if (Result != BattleResult.Fighting || delta <= 0) return;
            Time += delta;
            if(Time>=shieldUntil)shieldHP=0;
            while(pendingSkillHits>0&&nextSkillHit<=Time&&Result==BattleResult.Fighting)
            {
                PerformSkillHit(false);
                if(Result!=BattleResult.Fighting){pendingSkillHits=0;break;}
                pendingSkillHits--;nextSkillHit+=Math.Max(.03f,Dragon.skill.hitInterval);
            }
            while(burnNext>0&&burnNext<=Time&&burnNext<=burnUntil&&Result==BattleResult.Fighting)
            {
                int burn=Math.Max(1,burnDamage);EnemyHP=Math.Max(0,EnemyHP-burn);Feedback?.Invoke("화상 피해  −"+burn);burnNext+=1;
                if(EnemyHP==0)Result=BattleResult.Victory;
            }
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
            while (NextEnemyStrike <= Time && Result == BattleResult.Fighting)
            {
                if (NextEnemyStrike < DodgeUntil||Time<invulnerableUntil)
                {
                    Cue?.Invoke(CombatCue.EnemyMiss, 0);
                    Feedback?.Invoke("회피 성공! 피해를 피했습니다");
                    var mastery=Rule(AugmentMechanic.DodgeMaster);if(mastery!=null)ReduceSkillRemainingPercent(mastery.primaryValue);
                }
                else
                {
                    int damage=ElementRules.Damage(Enemy.damage,Enemy.elementType,Dragon.elementType);
                    damage=Math.Max(0,(int)Math.Round(damage*(1-Math.Min(80,Math.Max(0,Dragon.damageReductionPercent))/100f),MidpointRounding.AwayFromZero));
                    int absorbed=Math.Min(ShieldHP,damage);shieldHP-=absorbed;damage-=absorbed;
                    PlayerHP = Math.Max(0, PlayerHP - damage);
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
                    var barrier=Rule(AugmentMechanic.FrostBarrier);
                    if(dodgeAttemptedForStrike&&barrier!=null&&Time>=frostBarrierReady&&Result==BattleResult.Fighting)
                    {shieldHP=Math.Max(shieldHP,Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*barrier.primaryValue/100f)));shieldUntil=Time+barrier.duration;frostBarrierReady=Time+barrier.secondaryValue;Feedback?.Invoke("서리 장벽! 보호막 "+shieldHP);}
                }
                dodgeAttemptedForStrike=false;
                NextEnemyStrike += Enemy.interval*(Time<slowUntil?1+slowPercent/100f:1f);
            }
        }
        public bool Attack()
        {
            if (Result != BattleResult.Fighting || Time < AttackReady) return false;
            AttackReady = Time + AttackCooldownNow();
            bool critical;int damage=Hit(Dragon.attackDamage,Dragon.elementType,false,out critical);int total=damage;attackHits++;
            var glove=ItemRule(ItemMechanic.InfightingGlove);if(glove!=null)total+=RawExtra(damage,glove.primaryValue*Math.Max(1,glove.stacks));
            var slash=Rule(AugmentMechanic.ConsecutiveSlash);if(slash!=null&&Roll(slash.chancePercent))total+=RawExtra(damage,slash.primaryValue);
            var staticHit=Rule(AugmentMechanic.StaticDischarge);if(staticHit!=null&&attackHits%Math.Max(1,staticHit.triggerCount)==0)total+=ElementalExtra(Dragon.attackDamage,staticHit.primaryValue,ElementType.Lightning);
            var spread=Rule(AugmentMechanic.Spread);if(spread!=null)total+=RawExtra(damage,spread.primaryValue);
            var clone=Rule(AugmentMechanic.Clone);if(critical&&clone!=null)for(int i=0;i<Math.Max(1,clone.triggerCount);i++)total+=RawExtra(damage,clone.primaryValue);
            var overload=Rule(AugmentMechanic.OverloadCore);if(overload!=null)ReduceSkillSeconds(overload.primaryValue);
            var transfer=Rule(AugmentMechanic.Transference);if(transfer!=null)ReduceSkillSeconds(transfer.primaryValue);
            var rapid=Rule(AugmentMechanic.RapidFireInstinct);if(rapid!=null&&attackHits%Math.Max(1,rapid.triggerCount)==0){rapidUntil=Time+rapid.duration;AttackReady=Time+Math.Max(0,AttackReady-Time)*(1-rapid.primaryValue/100f);}
            Cue?.Invoke(CombatCue.Attack,total);
            Feedback?.Invoke((ElementRules.HasAdvantage(Dragon.elementType,Enemy.elementType)?"상성 우위!  ":"")+(critical?"치명타!  ":"")+"기본 공격!  −" + total);
            return true;
        }
        public bool Skill()
        {
            if (Result != BattleResult.Fighting || Dragon.skillDisabled || Time < SkillReady) return false;
            SkillReady = Time+(Time<noSkillCooldownUntil?0:Math.Max(0,Dragon.skill.cooldown)*(Time<firstAidUntil?.5f:1f));
            int casts=Has(AugmentMechanic.DoubleCasting)?2:1;pendingSkillHits=Math.Max(1,Dragon.skill.hitCount)*casts-1;nextSkillHit=Time+Math.Max(.03f,Dragon.skill.hitInterval);
            PerformSkillHit(true);if(Result==BattleResult.Fighting)ApplySkillStatus();
            var burn=Rule(AugmentMechanic.FlameRemnant);if(burn!=null&&Result==BattleResult.Fighting)ApplyBurn(burn.duration,Math.Max(1,(int)Math.Round(Dragon.attackDamage*burn.primaryValue/100f,MidpointRounding.AwayFromZero)));
            var paralyze=Rule(AugmentMechanic.Tingly);if(paralyze!=null&&Roll(paralyze.chancePercent))ApplyParalyze(paralyze.duration);
            var slow=Rule(AugmentMechanic.IceCream);if(slow!=null&&Roll(slow.chancePercent))ApplySlow(slow.duration,slow.primaryValue);
            if(echoReady&&Result==BattleResult.Fighting)
            {
                echoReady=false;var echo=ItemRule(ItemMechanic.EchoCrystal);float power=echo==null||echo.primaryValue<=0?50:echo.primaryValue;
                int echoDamage=ElementalExtra(Dragon.skill.damage*Math.Max(1,Dragon.skill.hitCount),power,Dragon.skill.elementType);
                Cue?.Invoke(CombatCue.SkillHit,echoDamage);Feedback?.Invoke("메아리 수정 추가 시전  −"+echoDamage);
            }
            float cost=0;var mana=Rule(AugmentMechanic.ManaRampage);if(mana!=null)cost+=mana.primaryValue;var vampire=Rule(AugmentMechanic.Vampire);if(vampire!=null)cost+=vampire.primaryValue;
            if(cost>0)PlayerHP=Math.Max(1,PlayerHP-Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*cost/100f)));
            if(cost>0)Feedback?.Invoke(Dragon.skill.displayName+" · HP 소모");
            return true;
        }
        void PerformSkillHit(bool first)
        {
            bool critical;int damage=Hit(Dragon.skill.damage,Dragon.skill.elementType,true,out critical);
            Cue?.Invoke(first?CombatCue.Skill:CombatCue.SkillHit,damage);
            Feedback?.Invoke((ElementRules.HasAdvantage(Dragon.skill.elementType,Enemy.elementType)?"상성 우위!  ":"")+(critical?"치명타!  ":"")+Dragon.skill.displayName+"!  −"+damage);
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
            }
        }
        void ApplyBurn(float duration,int damage){burnUntil=Math.Max(burnUntil,Time+duration);burnNext=Time+1;burnDamage=Math.Max(burnDamage,damage);Feedback?.Invoke("화상! "+duration.ToString("0.#")+"초");}
        void ApplyParalyze(float delay){NextEnemyStrike+=Math.Max(0,delay);Feedback?.Invoke("마비! 적의 공격이 지연됩니다");}
        void ApplySlow(float duration,float percent){slowUntil=Math.Max(slowUntil,Time+duration);slowPercent=Math.Max(slowPercent,Math.Max(0,percent));NextEnemyStrike+=Math.Max(0,NextEnemyStrike-Time)*Math.Max(0,percent)/100f;Feedback?.Invoke("둔화! 적의 공격이 느려집니다");}
        public bool Dodge()
        {
            if (Result != BattleResult.Fighting || Time < DodgeReady) return false;
            DodgeReady = Time + (Dragon.dodgeCooldown>0?Dragon.dodgeCooldown:DodgeCooldown);
            DodgeUntil = Time + (Dragon.dodgeDuration>0?Dragon.dodgeDuration:DodgeDuration);
            dodgeAttemptedForStrike=true;
            Cue?.Invoke(CombatCue.Dodge, 0);
            Feedback?.Invoke("회피 중 · 0.42초 동안 무적");
            return true;
        }
        public bool UseItem(ItemData item)
        {
            if(item==null||item.kind!=ItemKind.Consumable||Result!=BattleResult.Fighting)return false;
            float primary=item.primaryValue,secondary=item.secondaryValue,duration=Math.Max(0,item.duration);
            switch(item.mechanic)
            {
                case ItemMechanic.TimeShard:invulnerableUntil=Math.Max(invulnerableUntil,Time+duration);break;
                case ItemMechanic.BurstCore:SkillReady=Time;break;
                case ItemMechanic.LastStand:noAttackCooldownUntil=Math.Max(noAttackCooldownUntil,Time+duration);AttackReady=Time;break;
                case ItemMechanic.GiantRoar:case ItemMechanic.StunGun:ApplyParalyze(duration);break;
                case ItemMechanic.BloodTome:
                    PlayerHP=Math.Max(1,PlayerHP-Math.Max(1,(int)Math.Ceiling(PlayerHP*Math.Max(0,primary)/100f)));
                    skillDamageBuffPercent=Math.Max(skillDamageBuffPercent,secondary);skillDamageBuffUntil=Math.Max(skillDamageBuffUntil,Time+duration);break;
                case ItemMechanic.PurificationRod:PlayerHP=Dragon.maxHP;break;
                case ItemMechanic.FrostWitchTear:ApplySlow(duration,primary);break;
                case ItemMechanic.InfernoBreath:ApplyBurn(duration,Math.Max(1,(int)Math.Round(Dragon.attackDamage*primary/100f,MidpointRounding.AwayFromZero)));break;
                case ItemMechanic.WeaknessLens:criticalBuffPercent=Math.Max(criticalBuffPercent,primary);criticalBuffUntil=Math.Max(criticalBuffUntil,Time+duration);break;
                case ItemMechanic.EmergencyAccelerator:noSkillCooldownUntil=Math.Max(noSkillCooldownUntil,Time+duration);SkillReady=Time;break;
                case ItemMechanic.HealingPotion:case ItemMechanic.GreaterHealingPotion:
                    PlayerHP=Math.Min(Dragon.maxHP,PlayerHP+Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*primary/100f)));break;
                case ItemMechanic.BloodPotion:
                    PlayerHP=Math.Min(Dragon.maxHP,PlayerHP+Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*primary/100f)));
                    attackDamageBuffPercent=Math.Max(attackDamageBuffPercent,secondary);attackDamageBuffUntil=Math.Max(attackDamageBuffUntil,Time+duration);break;
                case ItemMechanic.BarrierStone:
                    shieldHP=Math.Max(shieldHP,Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*primary/100f)));shieldUntil=Math.Max(shieldUntil,Time+duration);break;
                case ItemMechanic.EchoCrystal:echoReady=true;break;
                default:return false;
            }
            Feedback?.Invoke(item.displayName+" 사용!");return true;
        }
        int Hit(int baseDamage,ElementType element,bool skill,out bool critical)
        {
            int damage=ElementRules.Damage(baseDamage,element,Enemy.elementType);
            if(skill&&Time<skillDamageBuffUntil)damage=Percent(damage,skillDamageBuffPercent);
            if(!skill&&Time<attackDamageBuffUntil)damage=Percent(damage,attackDamageBuffPercent);
            if(ElementRules.HasAdvantage(element,Enemy.elementType)&&Has(AugmentMechanic.ExploitWeakness))damage=Percent(damage,Value(AugmentMechanic.ExploitWeakness,40));
            var last=Rule(AugmentMechanic.LastStand);if(last!=null){int steps=(int)Math.Floor((1-PlayerHP/(double)Math.Max(1,Dragon.maxHP))*10+.00001);damage=Percent(damage,steps*last.primaryValue);}
            var mark=ItemRule(ItemMechanic.ExecutionerMark);if(mark!=null&&EnemyHP<=Enemy.maxHP*.4f)damage=Percent(damage,mark.primaryValue*Math.Max(1,mark.stacks));
            float chance=Dragon.criticalChance*100f+(Time<criticalBuffUntil?criticalBuffPercent:0);if(EnemyBurning&&Has(AugmentMechanic.Inferno))chance+=Value(AugmentMechanic.Inferno,50);
            critical=Roll(chance);if(critical)damage=Math.Max(1,(int)Math.Round(damage*Dragon.criticalDamage,MidpointRounding.AwayFromZero));
            EnemyHP = Math.Max(0, EnemyHP - damage);
            if (EnemyHP == 0) Result = BattleResult.Victory;
            return damage;
        }
        int RawExtra(int referenceDamage,float percent){int value=Math.Max(1,(int)Math.Round(referenceDamage*percent/100f,MidpointRounding.AwayFromZero));EnemyHP=Math.Max(0,EnemyHP-value);if(EnemyHP==0)Result=BattleResult.Victory;return value;}
        int ElementalExtra(int baseDamage,float percent,ElementType element){int value=Math.Max(1,(int)Math.Round(baseDamage*percent/100f,MidpointRounding.AwayFromZero));value=ElementRules.Damage(value,element,Enemy.elementType);EnemyHP=Math.Max(0,EnemyHP-value);if(EnemyHP==0)Result=BattleResult.Victory;return value;}
        int Percent(int value,float bonus)=>Math.Max(0,(int)Math.Round(value*(1+bonus/100f),MidpointRounding.AwayFromZero));
        float AttackCooldownNow()
        {
            if(Time<noAttackCooldownUntil)return .02f;
            float cooldown=Dragon.attackCooldown>0?Dragon.attackCooldown:AttackCooldown;
            if(Time<rapidUntil)cooldown*=1-Value(AugmentMechanic.RapidFireInstinct,15)/100f;
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

