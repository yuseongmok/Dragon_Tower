using System;

namespace DragonTower.Editor
{
    // These deterministic checks also run outside Unity against the same combat source files.
    public static class CombatMechanicsChecks
    {
        static int checks;
        static void Check(bool condition,string label)
        {if(!condition)throw new Exception("Combat mechanics: "+label);checks++;}
        static BattleModel Make(int floor=1,EnemyBossPattern pattern=EnemyBossPattern.None,double roll=.99)
        {
            var dragon=new BattleStats{maxHP=100,attackDamage=20,attackCooldown=.3f,dodgeCooldown=1.4f,dodgeDuration=.42f,
                skill=new SkillStats{displayName="test",damage=40,cooldown=4}};
            var enemy=new BattleEnemyStats{floor=floor,maxHP=10000,damage=10,interval=1,bossPattern=pattern,barrierHP=230,barrierDuration=4.5f};
            var battle=new BattleModel(dragon,enemy,100,()=>roll);
            enemy.interval=100; // One hit at t=1, then a clear window to observe status expiry.
            return battle;
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Dragon Tower/Verify region and boss mechanics")]
        public static void VerifyInEditor()
        {
            int count=Run();VerifyBalanceAssets();
            UnityEngine.Debug.Log("COMBAT_MECHANICS_OK: "+count+" checks and 31-80 balance assets");
        }
        static void VerifyBalanceAssets()
        {
            var db=ContentDatabase.Load();
            double previousHP=0,previousDamage=0;int previousBossHP=0,previousBossDamage=0;
            for(int region=0;region<5;region++)
            {
                int first=31+region*10;double hp=0,damage=0;int count=0;
                foreach(var monster in db.monsters)
                {
                    if(monster==null||monster.boss||monster.minimumFloor!=first)continue;
                    count++;var start=monster.CreateBattleStats(first);var end=monster.CreateBattleStats(first+8);
                    Check(end.maxHP>start.maxHP&&end.damage>start.damage,"per-floor enemy growth: "+monster.name);
                    hp+=start.maxHP;damage+=start.damage/start.interval;
                }
                Check(count==4,"four normal enemies in region "+first);
                Check(hp/count>previousHP&&damage/count>previousDamage,"increasing regional HP and DPS "+first);
                previousHP=hp/count;previousDamage=damage/count;
                var boss=db.PickMonster(first+9,true,0);Check(boss!=null,"boss exists: "+first);
                Check((int)boss.bossPattern==4+region,"unique boss pattern: "+first);
                Check(boss.baseHP>previousBossHP&&boss.attackDamage>previousBossDamage,"increasing boss stats");
                previousBossHP=boss.baseHP;previousBossDamage=boss.attackDamage;
            }
        }
#endif
        public static int Run()
        {
            checks=0;
            var b=Make(21);b.Tick(1.01f);Check(b.PlayerBurning,"forge hit applies burn");
            int hp=b.PlayerHP;b.Tick(4.1f);Check(b.PlayerHP==hp-4&&!b.PlayerBurning,"four burn ticks then expiry");
            b=Make(21);b.Tick(.8f);b.Dodge();b.Tick(.3f);Check(b.PlayerHP==100&&!b.PlayerBurning,"dodge prevents on-hit effects");
            b=Make(21);b.Dragon.passiveMechanic=DragonPassiveMechanic.IronArmor;b.Enemy.damage=1;b.Skill();b.Tick(1.1f);
            Check(b.PlayerHP==100&&!b.PlayerBurning,"fully absorbed hit applies no debuff");
            b=Make(20);b.Tick(1.1f);Check(!b.PlayerBurning,"no forge effect below 21");
            b=Make(31);b.Tick(6.05f);Check(b.DodgeReady>b.Time&&b.DodgeUntil>b.Time,"wind spends ready dodge");
            Check(!b.Dodge(),"wind causes real dodge cooldown");
            b=Make(31);b.Tick(5.1f);b.Dodge();double ready=b.DodgeReady;b.Tick(1);Check(b.DodgeReady==ready,"wind never resets running cooldown");
            b=Make(41,roll:0);b.Tick(1.1f);int enemy=b.EnemyHP;Check(b.PlayerParalyzed,"lightning hit paralysis");
            Check(b.Attack()&&b.EnemyHP==enemy&&b.AttackReady>b.Time,"missed attack consumes cooldown");
            Check(b.Skill()&&b.EnemyHP==enemy&&b.SkillReady>b.Time,"missed skill consumes cooldown");
            b.Tick(4.1f);Check(!b.PlayerParalyzed,"temporary paralysis expires");
            b=Make(50,EnemyBossPattern.SentryParalysis,0);Check(b.PlayerParalyzed,"Sentry aura starts without hit");
            enemy=b.EnemyHP;b.Attack();Check(b.EnemyHP==enemy,"Sentry aura can miss immediately");
            b.Tick(20);Check(b.PlayerParalyzed,"Sentry aura does not expire");
            b=Make(50,EnemyBossPattern.SentryParalysis,.99);enemy=b.EnemyHP;b.Attack();Check(b.EnemyHP<enemy,"paralysis does not block every action");
            b=Make(51);b.Skill();b.Tick(1.1f);Check(b.PlayerSlowed,"ice hit slows");
            b.Tick(2.9f);Check(b.SkillReady>b.Time+.8,"slow extends outstanding skill recovery");
            b.Tick(1.5f);Check(!b.PlayerSlowed&&b.Skill(),"slow expires and skill becomes usable");
            Check(Math.Abs(b.SkillReady-b.Time-4)<.01,"new cooldown is normal after slow expires");
            b=Make(61);b.Dragon.passiveMechanic=DragonPassiveMechanic.VoidAccelerator;b.Tick(1.1f);
            Check(b.PlayerSkillSealed&&!b.Skill(),"seal blocks even VoidAccelerator");Check(b.Attack()&&b.Dodge(),"skill seal leaves other actions usable");
            b.Tick(3.1f);Check(!b.PlayerSkillSealed&&b.Skill(),"skill seal expires");
            b=Make(71,roll:0);b.Tick(1.1f);Check(b.PlayerStunned&&!b.Attack()&&!b.Skill()&&!b.Dodge(),"stun blocks all controls");
            b.Tick(1);Check(!b.PlayerStunned&&b.Attack(),"stun expires");
            b=Make(71,roll:.99);b.Tick(1.1f);Check(!b.PlayerStunned,"sanctuary stun is probabilistic");
            b=Make(40,EnemyBossPattern.GarudaRend);b.Tick(1.01f);Check(b.PlayerRending,"Garuda hit applies rend");hp=b.PlayerHP;
            b.Tick(3.1f);Check(b.PlayerHP==hp-6&&!b.PlayerRending,"Garuda six half-second ticks");
            b=Make(60,EnemyBossPattern.FrostRecovery);b.Enemy.maxHP=10000;b.Dragon.attackDamage=3000;b.Attack();b.Tick(6.1f);
            Check(b.EnemyShieldHP==230,"ice boss creates barrier on timer");hp=b.EnemyHP;b.Tick(4.5f);
            Check(b.EnemyShieldHP==0&&b.EnemyHP==hp+1200,"failed ice shield heals 12 percent");
            b=Make(60,EnemyBossPattern.FrostRecovery);b.Tick(6.1f);b.Dragon.attackDamage=250;b.Attack();hp=b.EnemyHP;
            b.Tick(4.5f);Check(b.EnemyShieldHP==0&&b.EnemyHP==hp,"breaking ice shield prevents healing");
            b=Make(70,EnemyBossPattern.AzazelSeals);b.Enemy.specialPatternInterval=10;b.Enemy.specialPatternDuration=3.5f;b.Tick(5.8f);b.Dodge();b.Tick(.3f);
            Check(b.PlayerSkillSealed&&!b.Skill(),"Azazel unavoidable timed skill seal");
            b.Tick(4);Check(!b.PlayerSkillSealed,"Azazel skill seal ends");b.Tick(6);
            Check(b.PlayerAttackSealed&&!b.Attack()&&b.Skill(),"Azazel alternates to attack seal");
            b=Make(80,EnemyBossPattern.UrielReflection);b.Tick(6.1f);Check(b.EnemyReflecting,"Uriel reflection activates");hp=b.PlayerHP;b.Attack();
            Check(b.PlayerHP==hp-4,"Uriel reflects 20 percent with per-hit cap");b.Tick(3.1f);hp=b.PlayerHP;b.Attack();Check(b.PlayerHP==hp&&!b.EnemyReflecting,"reflection ends");
            b=Make(80,EnemyBossPattern.UrielReflection);b.Tick(1.1f);Check(b.PlayerBurning,"Uriel hit always burns");
            b=Make(80,EnemyBossPattern.UrielReflection);b.Enemy.damage=0;b.Tick(6.1f);b.Dragon.maxHP=1;
            // Lower remaining HP with an enemy strike before entering reflection, using the initialHP constructor.
            var d=b.Dragon;d.skill.damage=20000;d.augments=new[]{new BattleAugment{mechanic=AugmentMechanic.ManaRampage,primaryValue=10}};
            var e=new BattleEnemyStats{maxHP=10,damage=0,interval=100,bossPattern=EnemyBossPattern.UrielReflection};
            b=new BattleModel(d,e,1,()=>.99);b.Tick(6.1f);b.Skill();
            Check(b.Result==BattleResult.Defeat&&b.PlayerHP==0,"reflection defeat cannot turn into victory or cost-based resurrection");
            b=Make(21);b.Tick(1.1f);hp=b.PlayerHP;double time=b.Time;b.Tick(0);Check(b.PlayerHP==hp&&b.Time==time,"pause freezes hazard clocks");
            var large=Make(40,EnemyBossPattern.GarudaRend);var small=Make(40,EnemyBossPattern.GarudaRend);large.Tick(4.1f);for(int i=0;i<410;i++)small.Tick(.01f);
            Check(large.PlayerHP==small.PlayerHP,"large and small ticks yield same DOT damage");
            b=Make();Check(b.PlayerStatusText=="","new encounter has no leaked statuses");
            b=Make(51);b.Tick(1.1f);b.Attack();b.Tick(.31f);Check(!b.Attack(),"slow also delays basic attack");b.Tick(.16f);Check(b.Attack(),"slowed basic attack eventually ready");
            b=Make(61);b.Dragon.skill.hitCount=6;b.Dragon.skill.hitInterval=.5f;b.Skill();b.Tick(1.1f);enemy=b.EnemyHP;b.Tick(1);
            Check(b.EnemyHP==enemy,"skill seal cancels pending hits");
            b=Make(71,roll:0);b.Enemy.interval=.3f;b.Tick(1.1f);b.Tick(1.2f);Check(!b.PlayerStunned,"stun reapplication grace prevents chain lock");
            b=Make(21);b.Dragon.maxHP=100;b.Enemy.damage=89;b.Dragon.items=new[]{new BattleItem{mechanic=ItemMechanic.PhoenixFeather,primaryValue=30}};
            b.Tick(1.1f);b.Enemy.damage=0;b.Tick(4.1f);Check(b.PlayerHP==7,"burn before lethal does not waste resurrection");
            var fragile=new BattleStats{maxHP=100,attackDamage=1,skill=new SkillStats{damage=1,cooldown=1},items=new[]{new BattleItem{mechanic=ItemMechanic.PhoenixFeather,primaryValue=30}}};
            var burningEnemy=new BattleEnemyStats{floor=21,maxHP=10000,damage=1,interval=1};
            b=new BattleModel(fragile,burningEnemy,2,()=>.99);burningEnemy.interval=100;b.Tick(2.1f);
            Check(b.PlayerHP==30&&b.Result==BattleResult.Fighting,"burn lethal triggers phoenix feather");
            b=Make(30,EnemyBossPattern.ForgeBarrier);b.Enemy.patternEveryAttacks=2;b.Enemy.barrierFailureDamagePercent=35;b.Tick(1.1f);
            Check(b.EnemyShieldHP>0,"existing forge barrier still triggers");hp=b.PlayerHP;b.Tick(4.5f);Check(b.PlayerHP<hp-30&&b.EnemyShieldHP==0,"existing forge failure still deals damage instead of healing");
            b=Make();Check(b.Attack()&&!b.Attack(),"legacy attack cooldown");b.Tick(.299f);Check(!b.Attack(),"legacy cooldown boundary before");b.Tick(.002f);Check(b.Attack(),"legacy cooldown boundary after");
            b=Make();b.Tick(.8f);b.Dodge();b.Tick(.3f);Check(b.PlayerHP==100,"legacy correctly timed dodge");
            b=Make();b.Dodge();b.Tick(1.1f);Check(b.PlayerHP==90,"legacy early dodge fails");
            b=Make();b.Tick(.8f);b.Dragon.skill.statusEffect=CombatStatusEffect.Stun;b.Dragon.skill.statusChancePercent=100;b.Dragon.skill.statusDuration=1;
            b.Skill();double stunEnd=b.Time+1;Check(b.NextEnemyStrike>=stunEnd+b.CurrentEnemyWindupDuration-.001,"stun grants full recovery windup");
            b.Tick(1.05f);Check(b.PlayerHP==100&&!b.EnemyStunned,"no instant attack at stun expiry");
            b.Tick((float)(b.NextEnemyStrike-b.Time-.2));b.Dodge();b.Tick(.25f);Check(b.PlayerHP==100,"recovery attack can be dodged");
            b=Make();b.Tick(.8f);float progress=b.EnemyWindupProgress;b.Dragon.skill.statusEffect=CombatStatusEffect.Slow;b.Dragon.skill.statusChancePercent=100;b.Dragon.skill.statusDuration=3;b.Dragon.skill.statusPower=50;
            b.Skill();Check(Math.Abs(progress-b.EnemyWindupProgress)<.001,"slow preserves windup progress");
            double slowedStrike=b.NextEnemyStrike;b.SkillReady=b.Time;b.Skill();Check(Math.Abs(b.NextEnemyStrike-slowedStrike)<.001,"refresh slow never reschedules attack exponentially");
            b.Tick(.21f);Check(b.PlayerHP==100,"slow does not trigger original strike");b.Tick(.12f);Check(b.PlayerHP==90,"slowed strike hits exactly once");
            b=Make(21);int attacks=0,statusHits=0;b.Cue+=(cue,amount)=>{if(cue==CombatCue.EnemyHit)attacks++;if(cue==CombatCue.PlayerStatusHit)statusHits++;};b.Tick(5.2f);
            Check(attacks==1&&statusHits==4,"burn ticks do not animate enemy attacks");
            b=Make(1,roll:0);b.Dragon.passiveMechanic=DragonPassiveMechanic.FlameBreath;b.Dragon.passiveStage=0;b.Attack();double babyBurn=b.EnemyBurnRemaining;
            var evolved=Make(1,roll:0);evolved.Dragon.passiveMechanic=DragonPassiveMechanic.FlameBreath;evolved.Dragon.passiveStage=2;evolved.Attack();
            Check(babyBurn>=2.99&&evolved.EnemyBurnRemaining>=4.49,"evolution strengthens the dragon passive");
            return checks;
        }
    }
}

