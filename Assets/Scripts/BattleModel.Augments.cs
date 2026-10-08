using System;
using System.Collections.Generic;

namespace DragonTower
{
    // Per-encounter state. Only original actions enter these hooks; derived damage never does.
    public sealed partial class BattleModel
    {
        sealed class AugmentState { public int count; public double until,ready; public float stored; }
        sealed class Ward { public int hp; public double until; }
        sealed class AugmentCast
        {
            public int serial,hits,cost; public long damage; public double end;
            public float bonus; public bool completed,cancelled,criticalProc,slowApplied,slowConsumed;
        }
        struct EchoHit { public double due; public int damage; }
        readonly Dictionary<AugmentMechanic,AugmentState> augmentStates=new Dictionary<AugmentMechanic,AugmentState>();
        readonly Dictionary<string,Ward> augmentWards=new Dictionary<string,Ward>();
        readonly List<AugmentCast> augmentCasts=new List<AugmentCast>();
        readonly List<EchoHit> augmentEchoes=new List<EchoHit>();
        AugmentCast activeAugmentCast,persistentAugmentCast;
        int augmentCastSerial,frostStacks,alternatingLast;
        double frostReady,lastBasicAt=-100,steadyHealAt=5,refundWindow,lifestealWindow;
        float refundedSeconds,lifestealUsed,costSpent,costRefunded;
        bool basicResolving,firstBasicUsed,basicForceCritical,basicReversal,basicDestiny,signatureStatusApplication;
        float basicBonus,basicCritBonus,basicCritDamage;
        int basicCriticalDamage,basicActualDamage;
        public int EnemySlowStacks=>EnemySlowed?frostStacks:0;
        public int AugmentProcCount { get; private set; }
        bool SignatureSkill=>Dragon.skill.isSignature||!string.IsNullOrEmpty(Dragon.skill.exclusiveDragonId);
        int BaseAttack=>Dragon.baseAttackDamage>0?Dragon.baseAttackDamage:Dragon.attackDamage;
        AugmentState State(AugmentMechanic key)
        { if(!augmentStates.TryGetValue(key,out var s)){s=new AugmentState();augmentStates.Add(key,s);}return s; }
        bool Active(AugmentMechanic key)=>Has(key)&&Time<State(key).until;
        bool Ready(AugmentMechanic key)=>Has(key)&&Time+1e-8>=State(key).ready;
        void StartBuff(AugmentMechanic key)
        { var r=Rule(key);var s=State(key);s.until=Time+r.duration;s.ready=Time+r.internalCooldown;AugmentProcCount++; }
        bool CountTo(AugmentMechanic key)
        {var r=Rule(key);if(r==null||!Ready(key))return false;var s=State(key);if(++s.count<Math.Max(1,r.triggerCount))return false;s.count=0;s.ready=Time+r.internalCooldown;return true;}
        bool HasEnemyStatus=>EnemyBurning||EnemyPoisoned||EnemySlowed||EnemyParalyzed||EnemyStunned||EnemyTimeStopped;
        int PercentHP(float percent)=>Math.Max(0,(int)Math.Ceiling(Dragon.maxHP*percent/100f));
        float Param(AugmentMechanic key,int index=0)
        {var r=Rule(key);return r==null?0:index==0?r.primaryValue:index==1?r.secondaryValue:r.tertiaryValue;}

        int ExtraShieldHP
        {get{int total=0;foreach(var w in augmentWards.Values)if(Time<w.until)total+=w.hp;return total;}}
        void AddWard(string source,int amount,double duration)
        {
            amount=Percent(amount,Param(AugmentMechanic.WardWeave));
            if(!augmentWards.TryGetValue(source,out var w)){w=new Ward();augmentWards.Add(source,w);}
            w.hp=Math.Max(Time<w.until?w.hp:0,amount);w.until=Time+duration;
        }
        int AbsorbDamage(int damage)
        {
            int initial=damage;
            // Earliest expiring source is consumed first; a short ward never shortens permanent armor.
            while(damage>0)
            {
                Ward first=null;foreach(var w in augmentWards.Values)if(Time<w.until&&w.hp>0&&(first==null||w.until<first.until))first=w;
                if(first==null)break;int absorbed=Math.Min(damage,first.hp);first.hp-=absorbed;damage-=absorbed;
            }
            int legacy=Math.Min(Time<shieldUntil?shieldHP:0,damage);shieldHP-=legacy;damage-=legacy;
            int total=initial-damage;
            if(total>0&&Has(AugmentMechanic.ArmoredOffense))
                State(AugmentMechanic.ArmoredOffense).stored=Math.Min(BaseAttack*Param(AugmentMechanic.ArmoredOffense,2)/100f,State(AugmentMechanic.ArmoredOffense).stored+total*Param(AugmentMechanic.ArmoredOffense,1)/100f);
            return total;
        }
        float IncomingReduction=>Math.Min(80,Math.Max(0,Dragon.damageReductionPercent+(Active(AugmentMechanic.LightFoot)&&Time>=State(AugmentMechanic.LightFoot).ready?Param(AugmentMechanic.LightFoot):0)));
        float HealingBonus=>Param(AugmentMechanic.HealthyBeauty);
        void HealPercent(float percent)=>HealPlayer(PercentHP(percent));
        void CostHeal(int amount)
        {
            if(amount<=0)return;
            int requested=Percent(amount,HealingBonus);
            int actual=HealPlayerUnmodified(Math.Min(requested,Math.Max(0,(int)Math.Floor(costSpent-costRefunded))));costRefunded+=actual;
        }
        void LifeSteal(int actualDamage)
        {
            var r=Rule(AugmentMechanic.Vampire);if(r==null||actualDamage<=0)return;
            if(Time>=lifestealWindow+1){lifestealWindow=Time;lifestealUsed=0;}
            int requested=Percent((int)Math.Floor(actualDamage*r.secondaryValue/100f),HealingBonus);
            int amount=Math.Min(requested,Math.Max(0,PercentHP(r.tertiaryValue)-(int)lifestealUsed));
            lifestealUsed+=HealPlayerUnmodified(amount);
        }
        int AugmentSkillCost
        {
            get
            {
                float p=Param(AugmentMechanic.ManaRampage)+(SignatureSkill?0:Param(AugmentMechanic.Vampire));
                if(Time<SkillReady&&Dragon.passiveMechanic==DragonPassiveMechanic.VoidAccelerator)p+=Dragon.skill.cooldownRecastMaxHpPercent>0?Dragon.skill.cooldownRecastMaxHpPercent:Math.Max(6,10-Dragon.passiveStage*2);
                return PercentHP(p);
            }
        }
        bool CanAffordAugmentSkill=>PlayerHP>AugmentSkillCost;
        void BeginAugmentCast()
        {
            if(activeAugmentCast!=null&&!activeAugmentCast.completed&&activeAugmentCast!=persistentAugmentCast)activeAugmentCast.cancelled=true;
            var c=new AugmentCast{serial=++augmentCastSerial,cost=AugmentSkillCost};
            activeAugmentCast=c;augmentCasts.Add(c);
            c.bonus=State(AugmentMechanic.SpellRhythm).stored;State(AugmentMechanic.SpellRhythm).stored=0;
            if(!SignatureSkill){c.bonus+=State(AugmentMechanic.ArcaneApotheosis).stored;State(AugmentMechanic.ArcaneApotheosis).stored=0;}
            var measured=Rule(AugmentMechanic.MeasuredPower);
            if(measured!=null)c.bonus+=(float)Math.Min(measured.secondaryValue,Math.Floor(Math.Max(0,Time-SkillReady))*measured.primaryValue);
            var sk=Dragon.skill;
            double duration=Math.Max(sk.HitOffset(Math.Max(1,sk.hitCount)-1),Math.Max(sk.protectedCastDuration,sk.castLockDuration));
            if(sk.chargedBeam)duration=sk.chargeDuration+sk.beamIgnitionDelay+sk.beamDuration+.25;
            else if(sk.timeDomain)duration=sk.timeDomainDelay+sk.timeDomainDuration;
            else if(sk.attackEmpowerDuration>0)duration=sk.attackEmpowerDelay;
            else if(sk.persistentAttack!=null)duration=0;
            c.end=Time+duration;
            if(c.cost>0)
            {
                PlayerHP-=c.cost;costSpent+=c.cost;
                var cycle=Rule(AugmentMechanic.CrimsonCycle);
                if(cycle!=null&&Ready(AugmentMechanic.CrimsonCycle)&&!Active(AugmentMechanic.CrimsonCycle))
                {
                    var s=State(AugmentMechanic.CrimsonCycle);s.stored+=c.cost;
                    if(s.stored>=PercentHP(cycle.tertiaryValue)){s.stored=0;StartBuff(AugmentMechanic.CrimsonCycle);CostHeal(PercentHP(cycle.secondaryValue));}
                }
            }
            if(Ready(AugmentMechanic.CastingWard)){var r=Rule(AugmentMechanic.CastingWard);AddWard("casting",PercentHP(r.primaryValue),r.duration);State(AugmentMechanic.CastingWard).ready=Time+r.internalCooldown;}
        }
        void CancelAugmentCast(){if(activeAugmentCast!=null&&!activeAugmentCast.completed)activeAugmentCast.cancelled=true;}
        void NotifyAugmentCastStarted()
        {
            if(Dragon.passiveMechanic==DragonPassiveMechanic.JetStream){passiveRapidUntil=Time+Passive(1);AttackReady=Time+Math.Max(0,AttackReady-Time)*.5;}
            if(Dragon.passiveMechanic==DragonPassiveMechanic.IronArmor&&ShieldHP<=0)AddWard("iron",PercentHP(Passive(5)),double.MaxValue);
            TriggerAttackStatus();
        }
        void FinishAugmentCast(AugmentCast c)
        {
            if(c==null||c.cancelled||c.completed||PlayerHP<=0||Result==BattleResult.Defeat)return;
            c.completed=true;
            if(Has(AugmentMechanic.Technician))StartBuff(AugmentMechanic.Technician);
            if(Ready(AugmentMechanic.RestorativeMagic)){HealPercent(Param(AugmentMechanic.RestorativeMagic));State(AugmentMechanic.RestorativeMagic).ready=Time+Rule(AugmentMechanic.RestorativeMagic).internalCooldown;}
            if(c.cost>0)CostHeal((int)Math.Floor(c.cost*Param(AugmentMechanic.BloodRecovery)/100f));
            if(Has(AugmentMechanic.ArcaneApotheosis)&&State(AugmentMechanic.ArcaneApotheosis).stored<=0&&CountTo(AugmentMechanic.ArcaneApotheosis))State(AugmentMechanic.ArcaneApotheosis).stored=Param(AugmentMechanic.ArcaneApotheosis);
            AlternatingAction(2);
            if(!SignatureSkill&&Has(AugmentMechanic.DoubleCasting)&&c.damage>0)
                augmentEchoes.Add(new EchoHit{due=Time+Rule(AugmentMechanic.DoubleCasting).duration,damage=(int)Math.Min(int.MaxValue,c.damage*Param(AugmentMechanic.DoubleCasting)/100)});
        }
        void NotifyAugmentSkillHit(int damage,bool critical)
        {
            var c=activeAugmentCast;if(c==null||c.cancelled||damage<=0)return;c.damage+=damage;c.hits++;
            if(c.hits==1)
            {
                if(Has(AugmentMechanic.AbsoluteFrost)&&EnemySlowStacks>=5&&Ready(AugmentMechanic.AbsoluteFrost))
                {c.slowConsumed=true;frostStacks=0;State(AugmentMechanic.AbsoluteFrost).ready=Time+Rule(AugmentMechanic.AbsoluteFrost).internalCooldown;AugmentExtra(BaseAttack,Param(AugmentMechanic.AbsoluteFrost),ElementType.Ice);}
                var burn=Rule(AugmentMechanic.FlameRemnant);if(burn!=null)ApplyBurn(burn.duration,Math.Max(1,(int)Math.Round(BaseAttack*burn.primaryValue/100f)));
                var slow=Rule(AugmentMechanic.IceCream);if(slow!=null&&Roll(slow.chancePercent))ApplySlow(slow.duration,slow.primaryValue);
                var para=Rule(AugmentMechanic.Tingly);if(para!=null&&Roll(para.chancePercent))ApplyParalyze(para.duration);
            }
            if(critical&&!c.criticalProc){c.criticalProc=true;OnDirectCritical();}
            if(Result==BattleResult.Victory)FinishAugmentCast(c);
        }
        void PrepareBasicAugments()
        {
            basicResolving=true;basicBonus=0;basicCritBonus=0;basicCritDamage=0;basicCriticalDamage=basicActualDamage=0;
            basicReversal=State(AugmentMechanic.PerfectReversal).stored>0;
            basicDestiny=State(AugmentMechanic.CriticalDestiny).stored>0;
            basicForceCritical=basicReversal||basicDestiny||Active(AugmentMechanic.EvasiveCounter);
            if(!firstBasicUsed)basicBonus+=Param(AugmentMechanic.FirstStrike);
            if(Active(AugmentMechanic.Technician))basicBonus+=Param(AugmentMechanic.Technician);
            if(Active(AugmentMechanic.StoneRetort))basicBonus+=Param(AugmentMechanic.StoneRetort);
            if(Active(AugmentMechanic.EvasiveCounter))basicBonus+=Param(AugmentMechanic.EvasiveCounter);
            if(basicReversal)basicBonus+=Param(AugmentMechanic.PerfectReversal);
            if(basicDestiny)basicCritDamage=Param(AugmentMechanic.CriticalDestiny)/100f;
            var patient=Rule(AugmentMechanic.PatientAim);if(patient!=null&&Time-lastBasicAt>=patient.duration)basicCritBonus=patient.primaryValue;
        }
        void FinishBasicAugments(int damage,bool critical)
        {
            basicResolving=false;if(damage<=0)return;
            firstBasicUsed=true;lastBasicAt=Time;
            State(AugmentMechanic.Technician).until=State(AugmentMechanic.StoneRetort).until=State(AugmentMechanic.EvasiveCounter).until=0;
            if(basicReversal){State(AugmentMechanic.PerfectReversal).stored=0;HealPercent(Param(AugmentMechanic.PerfectReversal,1));}
            if(basicDestiny){State(AugmentMechanic.CriticalDestiny).stored=0;State(AugmentMechanic.CriticalDestiny).count=0;}
            else if(!critical&&CountTo(AugmentMechanic.CriticalDestiny))State(AugmentMechanic.CriticalDestiny).stored=1;
            if(critical){if(Ready(AugmentMechanic.CriticalCircuit)){ReduceSkillSeconds(Param(AugmentMechanic.CriticalCircuit));State(AugmentMechanic.CriticalCircuit).ready=Time+Rule(AugmentMechanic.CriticalCircuit).internalCooldown;}OnDirectCritical();}
            LifeSteal(basicActualDamage);
            if(CountTo(AugmentMechanic.VitalStrike))HealPercent(Param(AugmentMechanic.VitalStrike));
            if(Has(AugmentMechanic.SpellRhythm)&&State(AugmentMechanic.SpellRhythm).stored<=0&&CountTo(AugmentMechanic.SpellRhythm))State(AugmentMechanic.SpellRhythm).stored=Param(AugmentMechanic.SpellRhythm);
            var fang=Rule(AugmentMechanic.VenomFang);if(fang!=null&&Roll(fang.chancePercent))ApplyTemporaryPoison(Math.Max(1,(int)Math.Round(BaseAttack*fang.primaryValue/100f)),fang.duration);
            var cadence=Rule(AugmentMechanic.CombatCadence);
            if(cadence!=null){if(Time>State(AugmentMechanic.CombatCadence).until)State(AugmentMechanic.CombatCadence).count=0;State(AugmentMechanic.CombatCadence).until=Time+cadence.duration;if(CountTo(AugmentMechanic.CombatCadence))AugmentExtra(BaseAttack,cadence.primaryValue,Dragon.elementType,true);}
            if(Active(AugmentMechanic.EndlessBarrage))AugmentRaw(damage,Param(AugmentMechanic.EndlessBarrage));
            else if(CountTo(AugmentMechanic.EndlessBarrage))StartBuff(AugmentMechanic.EndlessBarrage);
            if(Active(AugmentMechanic.TwinMastery))AugmentRaw(damage,Param(AugmentMechanic.TwinMastery,1));
            AlternatingAction(1);
            if(State(AugmentMechanic.ArmoredOffense).stored>0){int stored=(int)State(AugmentMechanic.ArmoredOffense).stored;State(AugmentMechanic.ArmoredOffense).stored=0;AugmentRaw(stored,100);}
        }
        void AlternatingAction(int action)
        {
            if(!Has(AugmentMechanic.TwinMastery)||Active(AugmentMechanic.TwinMastery))return;
            if(action==alternatingLast)return;alternatingLast=action;
            if(CountTo(AugmentMechanic.TwinMastery))StartBuff(AugmentMechanic.TwinMastery);
        }
        void OnDirectCritical()
        {
            if(EnemyBurning&&CountTo(AugmentMechanic.RagingCombustion))AugmentExtra(BaseAttack,Param(AugmentMechanic.RagingCombustion),ElementType.Fire);
            if(EnemyPoisoned&&Ready(AugmentMechanic.VenomSovereign))
            {State(AugmentMechanic.VenomSovereign).ready=Time+Rule(AugmentMechanic.VenomSovereign).internalCooldown;AugmentRaw(CurrentPoisonTickDamage,Param(AugmentMechanic.VenomSovereign));}
        }
        void AugmentRaw(int value,float percent)
        {if(Result!=BattleResult.Fighting)return;int hit=RawExtra(value,percent);AugmentProcCount++;Cue?.Invoke(CombatCue.AugmentHit,hit);}
        void AugmentExtra(int value,float percent,ElementType element,bool basicDependent=false)
        {if(Result!=BattleResult.Fighting)return;if(basicDependent)value=Percent(value,-Param(AugmentMechanic.Transference,1));int hit=ElementalExtra(value,percent,element);AugmentProcCount++;Cue?.Invoke(CombatCue.AugmentHit,hit);}
        float AugmentDamageBonus(bool skill)
        {
            float p=0;
            if(HasEnemyStatus)p+=Param(skill?AugmentMechanic.StatusHunter:AugmentMechanic.HardenedClaws);
            if(!skill&&EnemySlowed)p+=Param(AugmentMechanic.ColdOpening);
            if(skill&&PlayerHP>=Dragon.maxHP*.7)p+=Param(AugmentMechanic.ResourcefulBreath);
            if(ShieldHP>0)p+=Param(AugmentMechanic.ArmoredOffense);
            if(EnemySlowed)p+=EnemySlowStacks*Param(AugmentMechanic.FrostAccumulation);
            if(Active(AugmentMechanic.CrimsonCycle))p+=Param(AugmentMechanic.CrimsonCycle);
            if(Active(AugmentMechanic.LivingFortress))p+=Param(AugmentMechanic.LivingFortress,1);
            if(Active(AugmentMechanic.TwinMastery))p+=Param(AugmentMechanic.TwinMastery);
            if(skill&&activeAugmentCast!=null)p+=activeAugmentCast.bonus;
            if(!skill&&basicResolving)p+=basicBonus;
            return p;
        }
        float AugmentCriticalDamage=> (EnemyBurning?Param(AugmentMechanic.Inferno,1)/100f:0)+(EnemyPoisoned?Param(AugmentMechanic.ToxicPrecision)/100f:0)+(basicResolving?basicCritDamage:0);
        void OnAugmentDodgeSuccess()
        {
            if(Has(AugmentMechanic.DodgeMaster))ReduceSkillRemainingPercent(Param(AugmentMechanic.DodgeMaster));
            if(Ready(AugmentMechanic.EvasiveCounter))StartBuff(AugmentMechanic.EvasiveCounter);
            if(Has(AugmentMechanic.PerfectReversal)&&State(AugmentMechanic.PerfectReversal).stored<=0&&CountTo(AugmentMechanic.PerfectReversal))State(AugmentMechanic.PerfectReversal).stored=1;
        }
        void OnAugmentIncomingDamage(int actualHP,int absorbed)
        {
            if(Result!=BattleResult.Fighting)return;
            if(actualHP>0)
            {
                steadyHealAt=Time+5;TriggerFirstAid();
                if(Ready(AugmentMechanic.IndomitableWill)){ReduceSkillRemainingPercent(Param(AugmentMechanic.IndomitableWill));State(AugmentMechanic.IndomitableWill).ready=Time+Rule(AugmentMechanic.IndomitableWill).internalCooldown;}
                if(Ready(AugmentMechanic.StoneRetort))StartBuff(AugmentMechanic.StoneRetort);
            }
            var r=Rule(AugmentMechanic.LivingFortress);
            if(r!=null&&Ready(AugmentMechanic.LivingFortress)&&!Active(AugmentMechanic.LivingFortress))
            {
                var s=State(AugmentMechanic.LivingFortress);s.stored+=actualHP+absorbed;
                if(s.stored>=PercentHP(r.tertiaryValue)){s.stored=0;StartBuff(AugmentMechanic.LivingFortress);AddWard("fortress",PercentHP(r.primaryValue),r.duration);}
            }
        }
        float DurationBonus=>Param(AugmentMechanic.LingeringStatus);
        float StatusDuration(float value)=>value*(1+(signatureStatusApplication?0:DurationBonus)/100f);
        float DotFactor=>Math.Max(.05f,1-Param(AugmentMechanic.PersistentAffliction)/100f);
        int DotDamage(int value)=>Percent(value,Param(AugmentMechanic.ToxicStudy));
        void AddFrostStack()
        {
            if(!Has(AugmentMechanic.FrostAccumulation)||Time+1e-8<frostReady)return;
            if(activeAugmentCast!=null&&!basicResolving&&(pendingSkillHits>0||Time<=activeAugmentCast.end))
            {if(activeAugmentCast.slowApplied||activeAugmentCast.slowConsumed)return;activeAugmentCast.slowApplied=true;}
            frostStacks=Math.Min(5,frostStacks+1);frostReady=Time+Rule(AugmentMechanic.FrostAccumulation).internalCooldown;
        }
        void TickAugments()
        {
            if(!EnemySlowed)frostStacks=0;
            if(Result==BattleResult.Fighting&&Has(AugmentMechanic.SteadyBreath)&&Time>=steadyHealAt){HealPercent(Param(AugmentMechanic.SteadyBreath));steadyHealAt=Time+Rule(AugmentMechanic.SteadyBreath).duration;}
            for(int i=augmentCasts.Count-1;i>=0;i--)
            {
                var c=augmentCasts[i];
                if(!c.completed&&!c.cancelled&&Time+1e-8>=c.end)
                {
                    if(c.hits>0||Dragon.skill.attackEmpowerDuration>0||Dragon.skill.persistentAttack!=null)FinishAugmentCast(c);
                    else c.cancelled=true;
                }
                if(c.completed||c.cancelled)augmentCasts.RemoveAt(i);
            }
            for(int i=augmentEchoes.Count-1;i>=0;i--)if(Time>=augmentEchoes[i].due){var hit=augmentEchoes[i];augmentEchoes.RemoveAt(i);AugmentRaw(hit.damage,100);}
        }
        void RefundSkill(float amount)
        {
            if(Time>=refundWindow+1){refundWindow=Time;refundedSeconds=0;}
            float baseCd=Dragon.baseSkillCooldown>0?Dragon.baseSkillCooldown:Dragon.skill.cooldown;
            float allow=Math.Min(Math.Max(0,amount),Math.Max(0,baseCd*.2f-refundedSeconds));
            float used=(float)Math.Min(allow,Math.Max(0,SkillReady-Time));SkillReady-=used;refundedSeconds+=used;
        }
        public void CancelAugmentEffects()
        {augmentStates.Clear();augmentWards.Clear();augmentEchoes.Clear();augmentCasts.Clear();activeAugmentCast=persistentAugmentCast=null;frostStacks=0;basicResolving=false;}
    }
}
