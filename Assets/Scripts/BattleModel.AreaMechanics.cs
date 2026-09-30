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
        public bool PlayerParalyzed=>Enemy.bossPattern==EnemyBossPattern.SentryParalysis||Time<playerParalyzeUntil;
        public bool PlayerSlowed=>Time<playerSlowUntil;
        public bool PlayerStunned=>Time<playerStunUntil;
        public bool PlayerSkillSealed=>Time<skillSealUntil;
        public bool PlayerAttackSealed=>Time<attackSealUntil;
        public bool EnemyReflecting=>Time<reflectUntil;
        public double ReflectionRemaining=>Math.Max(0,reflectUntil-Time);
        public double StunRemaining=>Math.Max(0,playerStunUntil-Time);
        public double SkillSealRemaining=>Math.Max(0,skillSealUntil-Time);
        public double AttackSealRemaining=>Math.Max(0,attackSealUntil-Time);
        public double PlayerBurnRemaining=>Math.Max(0,playerBurnUntil-Time);
        public double PlayerRendRemaining=>Math.Max(0,playerRendUntil-Time);
        public double PlayerSlowRemaining=>Math.Max(0,playerSlowUntil-Time);
        public double PlayerParalyzeRemaining=>Enemy.bossPattern==EnemyBossPattern.SentryParalysis?double.PositiveInfinity:Math.Max(0,playerParalyzeUntil-Time);
        public string EnemyBarrierName=>Enemy.bossPattern==EnemyBossPattern.FrostRecovery?"빙결 회복 방벽":"용광로 방벽";
        public string AreaHint
        {
            get
            {
                if(Enemy.bossPattern==EnemyBossPattern.SentryParalysis)return "뇌운 결계 · 전투 내내 마비, 공격·스킬 25% 실패";
                if(Enemy.floor>=21&&Enemy.floor<=30)return "용광로 · 피격 시 4초 화상";
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
                if(PlayerBurning)parts.Add("화상 "+Remaining(playerBurnUntil));
                if(PlayerRending)parts.Add("찰과상 "+Remaining(playerRendUntil));
                if(PlayerParalyzed)parts.Add(Enemy.bossPattern==EnemyBossPattern.SentryParalysis?"상시 마비 25%":"마비 "+Remaining(playerParalyzeUntil));
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
                if(EnemyReflecting)return "반사 중! 공격 중지 · "+Remaining(reflectUntil);
                if(specialWarning)return SpecialName+" · "+Remaining(nextSpecialAt)+" 후 발동";
                if(Enemy.floor>=31&&Enemy.floor<=40&&nextWindAt-Time<=.8)return "돌풍 접근 · 자동 회피 주의!";
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
            if(Enemy.floor>=31&&Enemy.floor<=40&&Time>=nextWindAt)
            {
                if(Dodge())Feedback?.Invoke("돌풍! 자동 회피 발동 · 회피 쿨타임 소모");
                nextWindAt=Time+7+random()*3;
            }
            bool timed=Enemy.bossPattern==EnemyBossPattern.FrostRecovery||Enemy.bossPattern==EnemyBossPattern.AzazelSeals||Enemy.bossPattern==EnemyBossPattern.UrielReflection;
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
                if(sealAttackNext)attackSealUntil=Time+Enemy.specialPatternDuration;
                else {skillSealUntil=Time+Enemy.specialPatternDuration;pendingSkillHits=0;}
                Feedback?.Invoke(SpecialName+"! 회피 불가 · "+Enemy.specialPatternDuration.ToString("0.#")+"초");
                sealAttackNext=!sealAttackNext;
            }
            else
            {reflectUntil=Time+Enemy.specialPatternDuration;Feedback?.Invoke("태양 반사! 공격을 멈추세요");}
            nextSpecialAt=Time+Math.Max(Math.Max(Enemy.specialPatternDuration,Enemy.barrierDuration)+3,Enemy.specialPatternInterval);
        }
        void ApplyAreaHitEffects()
        {
            if((Enemy.floor>=21&&Enemy.floor<=30)||Enemy.bossPattern==EnemyBossPattern.UrielReflection)
            {
                if(!PlayerBurning)playerBurnNext=Time+1;
                playerBurnUntil=Time+4;
            }
            if(Enemy.floor>=41&&Enemy.floor<=50)playerParalyzeUntil=Time+4;
            if(Enemy.floor>=51&&Enemy.floor<=60)playerSlowUntil=Time+4;
            if(Enemy.floor>=61&&Enemy.floor<=70&&Time>=skillSealAllowedAt)
            {skillSealUntil=Math.Max(skillSealUntil,Time+3);skillSealAllowedAt=skillSealUntil+2;pendingSkillHits=0;}
            if(Enemy.floor>=71&&Enemy.floor<=80&&Time>=stunAllowedAt&&Roll(25))
            {playerStunUntil=Time+1;stunAllowedAt=playerStunUntil+3;pendingSkillHits=0;}
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
            if(Result!=BattleResult.Fighting)return;
            // Damage-over-time and reflection cannot be dodged, but mitigation and shields still work.
            damage=Math.Max(0,(int)Math.Round(damage*(1-Math.Min(80,Math.Max(0,Dragon.damageReductionPercent))/100f),MidpointRounding.AwayFromZero));
            int absorbed=Math.Min(ShieldHP,damage);shieldHP-=absorbed;damage-=absorbed;
            PlayerHP=Math.Max(0,PlayerHP-damage);
            ResolvePlayerSurvival();
            Cue?.Invoke(CombatCue.PlayerStatusHit,damage);Feedback?.Invoke(source+" −"+damage+" HP");
            if(Result==BattleResult.Fighting)TriggerFirstAid();
        }
        void ResolvePlayerSurvival()
        {
            if(Dragon.passiveMechanic==DragonPassiveMechanic.Phoenix&&Dragon.passiveAvailable&&!PassiveConsumed&&PlayerHP<=Dragon.maxHP*.2f)
            {PlayerHP=Dragon.maxHP;PassiveConsumed=true;Feedback?.Invoke("불사조! 체력을 완전히 회복했습니다");}
            if(PlayerHP>0)return;
            var feather=ItemRule(ItemMechanic.PhoenixFeather);
            if(feather!=null&&!phoenixUsed)
            {phoenixUsed=true;PlayerHP=Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*feather.primaryValue/100f));Feedback?.Invoke("불사조의 깃털! 다시 일어났습니다");}
            else {Result=BattleResult.Defeat;pendingSkillHits=0;}
        }
    }
}
