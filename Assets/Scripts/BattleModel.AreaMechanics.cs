using System;
using System.Collections.Generic;

namespace DragonTower
{
    public sealed partial class BattleModel
    {
        // One shared tuning table for region hazards. Durations refresh, never stack.
        public const float PlayerParalysisMissPercent=25, PlayerSlowMultiplier=1.5f;
        double playerBurnUntil,playerBurnNext,playerRendUntil,playerRendNext;
        double playerParalyzeUntil,playerSlowUntil,playerStunUntil,skillSealUntil,attackSealUntil;
        double skillSealAllowedAt,stunAllowedAt,nextWindAt,nextSpecialAt,reflectUntil;
        bool specialWarning,sealAttackNext;
        public bool PlayerBurning=>Time<playerBurnUntil;
        public bool PlayerRending=>Time<playerRendUntil;
        bool LegacySentryParalysis=>Enemy.bossPattern==EnemyBossPattern.SentryParalysis&&!SentryCoreBattle;
        public bool PlayerParalyzed=>LegacySentryParalysis||Time<playerParalyzeUntil;
        public bool PlayerSlowed=>Time<playerSlowUntil;
        public bool PlayerStunned=>Time<playerStunUntil;
        public bool PlayerSkillSealed=>Time<skillSealUntil;
        public bool PlayerAttackSealed=>Time<attackSealUntil;
        public bool EnemyReflecting=>SolarBossBattle?SolarMirrorActive:Time<reflectUntil;
        public double ReflectionRemaining=>SolarBossBattle?(SolarMirrorActive?BossPhaseRemaining:0):Math.Max(0,reflectUntil-Time);
        public double StunRemaining=>Math.Max(0,playerStunUntil-Time);
        public double SkillSealRemaining=>Math.Max(0,skillSealUntil-Time);
        public double AttackSealRemaining=>Math.Max(0,attackSealUntil-Time);
        public double PlayerBurnRemaining=>Math.Max(0,playerBurnUntil-Time);
        public double PlayerRendRemaining=>Math.Max(0,playerRendUntil-Time);
        public double PlayerSlowRemaining=>Math.Max(0,playerSlowUntil-Time);
        public double PlayerParalyzeRemaining=>LegacySentryParalysis?double.PositiveInfinity:Math.Max(0,playerParalyzeUntil-Time);
        public string EnemyBarrierName=>PrimordialBossBattle?(PrimordialPhase==1?"수호자의 봉인":"공허의 사슬"):VoidExpandedBattle?"차원 봉합 핵":Enemy.bossPattern==EnemyBossPattern.FrostRecovery?"빙결 회복 방벽":"용광로 방벽";
        public string AreaHint
        {
            get
            {
                if(SolarBossBattle)return SolarMirrorActive?"태양거울 활성 · 직접 공격 주의":SolarAttackMidnight?"자정 · 중앙에서 대기":"정오 · 충돌 순간 회피";
                if(SentryCoreBattle)return "피뢰 코어 · 전용 공격 회피 시 충전 감소";
                if(LibraryBossBattle)return BookChoiceActive?"금서를 터치해 공격 대상 선택 · 한 권을 파괴하세요":"실체가 있는 금서의 주문을 관찰하세요";
                if(FrostPalaceBattle)return FrostWeaknessStacks>0?"얼음 갑옷 균열 · 다음 방벽 약화 ×"+FrostWeaknessStacks:"공명 수정 · 파편 회피 시 다음 방벽 약화";
                if(LegacySentryParalysis)return "뇌운 결계 · 전투 내내 마비, 공격·스킬 25% 실패";
                if(Enemy.floor>=21&&Enemy.floor<=30)return "용광로 · 피격 시 4초 화상";
                if(GarudaDecisionBattle)return "천공의 칼날 · 방향을 관찰하세요";
                if(Enemy.floor<=40&&Enemy.floor>=31)return "천공 회랑 · 돌풍이 자동 회피를 발동합니다";
                if(Enemy.floor<=50&&Enemy.floor>=41)return "피뢰탑 · 피격 시 4초 마비, 행동 25% 실패";
                if(Enemy.floor<=60&&Enemy.floor>=51)return "수정 궁전 · 피격 시 4초 쿨타임 회복 둔화";
                if(Enemy.floor<=70&&Enemy.floor>=61)return "대도서관 · 피격 시 3초 스킬 봉인";
                if(Enemy.floor<=80&&Enemy.floor>=71)return "일광 성소 · 피격 시 25% 확률로 1초 기절";
                return "게이지가 차기 직전에 회피하세요";
            }
        }
        public string PlayerStatusText
        {
            get
            {
                var parts=new List<string>();
                if(BookBindingActive)parts.Add("속박 · 일반공격 주기 +"+bookSlowPercent.ToString("0")+"% · "+BookBindingRemaining.ToString("0.0")+"초");
                if(BossStormActive)parts.Add("폭풍 · 회피 쿨타임 +30% · "+BossStormRemaining.ToString("0.0")+"초");
                if(PlayerBurning)parts.Add("화상 "+Remaining(playerBurnUntil));
                if(PlayerRending)parts.Add("찰과상 "+Remaining(playerRendUntil));
                if(PlayerParalyzed)parts.Add(LegacySentryParalysis?"상시 마비 25%":"마비 "+Remaining(playerParalyzeUntil));
                if(PlayerSlowed)parts.Add("둔화 "+Remaining(playerSlowUntil));
                if(PlayerSkillSealed)parts.Add("스킬 봉인 "+Remaining(skillSealUntil));
                if(PlayerAttackSealed)parts.Add("공격 봉인 "+Remaining(attackSealUntil));
                if(PlayerStunned)parts.Add("기절 "+Remaining(playerStunUntil));
                return string.Join(" · ",parts);
            }
        }
        string Remaining(double until)=>Math.Max(0,until-Time).ToString("0.0")+"초";
        public string BossWarning
        {
            get
            {
                if(SolarBossBattle)return SolarMirrorActive?"태양거울 · 반사 중 · "+BossPhaseRemaining.ToString("0.0")+"초":"";
                if(EnemyReflecting)return "반사 중! 공격 중지 · "+Remaining(reflectUntil);
                if(specialWarning)return SpecialName+" · "+Remaining(nextSpecialAt)+" 후 발동";
                if(!GarudaDecisionBattle&&Enemy.floor>=31&&Enemy.floor<=40&&nextWindAt-Time<=.8)return "돌풍 접근 · 자동 회피 주의!";
                return "";
            }
        }
        string SpecialName=>Enemy.bossPattern==EnemyBossPattern.FrostRecovery?"빙결 회복 방벽":
            Enemy.bossPattern==EnemyBossPattern.AzazelSeals?(sealAttackNext?"일반공격 봉인":"스킬 봉인"):"태양 반사";
        void InitializeAreaMechanics()
        {
            nextWindAt=6;
            nextSpecialAt=6;
        }
        void SlowPlayerCooldowns(double delta)
        {
            if(BookBindingActive&&AttackReady>Time){double slowed=Math.Min(delta,BookBindingRemaining);AttackReady+=Math.Min(slowed,AttackReady-Time)*(1-1/(1+bookSlowPercent/100.0));}
            if(!PlayerSlowed)return;
            double slowedTime=Math.Min(delta,playerSlowUntil-Time);
            // Slow the countdown itself so recovery immediately returns to normal on expiry.
            if(AttackReady>Time)AttackReady+=Math.Min(slowedTime,(AttackReady-Time)*PlayerSlowMultiplier)*(1-1/PlayerSlowMultiplier);
            if(SkillReady>Time)SkillReady+=Math.Min(slowedTime,(SkillReady-Time)*PlayerSlowMultiplier)*(1-1/PlayerSlowMultiplier);
        }
        void TickAreaMechanics()
        {
            while(playerBurnNext>0&&playerBurnNext<=Time+1e-8&&playerBurnNext<=playerBurnUntil+1e-8&&Result==BattleResult.Fighting)
            {DamagePlayer(Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*.01)),"화상");playerBurnNext+=1;}
            while(playerRendNext>0&&playerRendNext<=Time+1e-8&&playerRendNext<=playerRendUntil+1e-8&&Result==BattleResult.Fighting)
            {DamagePlayer(Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*.01)),"찰과상");playerRendNext+=.5;}
            if(Result!=BattleResult.Fighting)return;
            if(!GarudaDecisionBattle&&Enemy.floor>=31&&Enemy.floor<=40&&Time>=nextWindAt)
            {
                if(Dodge())Feedback?.Invoke("돌풍! 자동 회피 발동 · 회피 쿨타임 소모");
                nextWindAt=Time+7+random()*3;
            }
            if(EnemyTimeStopped)return;
            bool timed=Enemy.bossPattern==EnemyBossPattern.FrostRecovery&&!FrostPalaceBattle||Enemy.bossPattern==EnemyBossPattern.AzazelSeals&&!LibraryBossBattle||Enemy.bossPattern==EnemyBossPattern.UrielReflection&&!SolarBossBattle;
            if(!timed)return;
            if(!specialWarning&&Time>=nextSpecialAt-1.2)
            {specialWarning=true;Feedback?.Invoke("보스 예고 · "+SpecialName+"!");}
            if(Time<nextSpecialAt)return;
            specialWarning=false;Cue?.Invoke(CombatCue.BossSkill,0);
            if(Enemy.bossPattern==EnemyBossPattern.FrostRecovery)
            {
                EnemyShieldMaxHP=Math.Max(1,Enemy.barrierHP);enemyShieldHP=EnemyShieldMaxHP;
                enemyShieldUntil=Time+Math.Max(.5f,Enemy.barrierDuration);
                Feedback?.Invoke("빙결 방벽! "+Enemy.barrierDuration.ToString("0.#")+"초 안에 파괴하지 못하면 HP 회복");
            }
            else if(Enemy.bossPattern==EnemyBossPattern.AzazelSeals)
            {
                if(!ProtectedSkillActive)
                {
                    if(sealAttackNext)attackSealUntil=Time+Enemy.specialPatternDuration;
                    else {skillSealUntil=Time+Enemy.specialPatternDuration;pendingSkillHits=0;CancelAugmentCast();}
                    Feedback?.Invoke(SpecialName+"! 회피 불가 · "+Enemy.specialPatternDuration.ToString("0.#")+"초");
                }
                sealAttackNext=!sealAttackNext;
            }
            else
            {reflectUntil=Time+Enemy.specialPatternDuration;Feedback?.Invoke("태양 반사! 공격을 멈추세요");}
            nextSpecialAt=Time+Math.Max(Math.Max(Enemy.specialPatternDuration,Enemy.barrierDuration)+3,Enemy.specialPatternInterval);
        }
        void ApplyAreaHitEffects()
        {
            if(ProtectedSkillActive)return;
            if((Enemy.floor>=21&&Enemy.floor<=30)||Enemy.bossPattern==EnemyBossPattern.UrielReflection)
            {
                if(!PlayerBurning)playerBurnNext=Time+1;
                playerBurnUntil=Time+4;
            }
            if(Enemy.floor>=41&&Enemy.floor<=50&&!SentryCoreBattle)playerParalyzeUntil=Time+4;
            if(Enemy.floor>=51&&Enemy.floor<=60)playerSlowUntil=Time+4;
            if(Enemy.floor>=61&&Enemy.floor<=70&&!LibraryBossBattle&&Time>=skillSealAllowedAt)
            {skillSealUntil=Math.Max(skillSealUntil,Time+3);skillSealAllowedAt=skillSealUntil+2;pendingSkillHits=0;CancelAugmentCast();}
            if(Enemy.floor>=71&&Enemy.floor<=80&&Time>=stunAllowedAt&&Roll(25))
            {playerStunUntil=Time+1;stunAllowedAt=playerStunUntil+3;pendingSkillHits=0;CancelAugmentCast();}
            if(Enemy.bossPattern==EnemyBossPattern.GarudaRend)
            {
                if(!PlayerRending)playerRendNext=Time+.5;
                playerRendUntil=Time+3;
            }
        }
        bool PlayerActionMissed(bool skill)
        {
            if(!PlayerParalyzed||!Roll(PlayerParalysisMissPercent))return false;
            if(skill)pendingSkillHits=0;
            Cue?.Invoke(skill?CombatCue.Skill:CombatCue.Attack,0);
            Feedback?.Invoke("마비! "+(skill?"스킬":"공격")+"이 빗나갔습니다 · 쿨타임 소모");
            return true;
        }
        void ApplyReflection(int outgoingDamage)
        {
            int reflected=Math.Min(Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*.04)),
                Math.Max(1,(int)Math.Ceiling(outgoingDamage*Math.Max(0,Enemy.reflectionPercent)/100f)));
            if(Enemy.reflectionPercent>0)DamagePlayer(reflected,"태양 반사");
        }
        void DamagePlayer(int damage,string source)
        {
            if(Result!=BattleResult.Fighting||ProtectedSkillActive)return;
            // Damage-over-time and reflection cannot be dodged, but mitigation and shields still work.
            damage=Math.Max(0,(int)Math.Round(damage*(1-IncomingReduction/100f),MidpointRounding.AwayFromZero));
            int absorbed=AbsorbDamage(damage);damage-=absorbed;damage=ApplyCompletionDefense(damage);
            NotifyChargedBeamDamage(Math.Min(PlayerHP,damage));
            int actualHP=Math.Min(PlayerHP,damage);PlayerHP=Math.Max(0,PlayerHP-damage);
            ResolvePlayerSurvival();
            Cue?.Invoke(CombatCue.PlayerStatusHit,damage);Feedback?.Invoke(source+" −"+damage+" HP");
            if(actualHP>0)steadyHealAt=Time+5;
        }
        void ResolvePlayerSurvival()
        {
            if(!NewPassives&&Dragon.passiveMechanic==DragonPassiveMechanic.Phoenix&&Dragon.passiveAvailable&&!PassiveConsumed&&PlayerHP<=Dragon.maxHP*Passive(.2f))
            {PlayerHP=Dragon.maxHP;PassiveConsumed=true;Feedback?.Invoke("불사조! 체력을 완전히 회복했습니다");}
            if(PlayerHP>0)return;
            var feather=ItemRule(ItemMechanic.PhoenixFeather);
            if(feather!=null&&!phoenixUsed)
            {phoenixUsed=true;PlayerHP=Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*feather.primaryValue/100f));Feedback?.Invoke("불사조의 깃털! 다시 일어났습니다");}
            else {Result=BattleResult.Defeat;pendingSkillHits=0;CancelAugmentCast();}
        }
    }
}
