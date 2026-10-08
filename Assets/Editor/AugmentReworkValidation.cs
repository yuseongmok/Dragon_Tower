using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DragonTower.Editor
{
    public static class AugmentReworkValidation
    {
        static ContentDatabase db;
        static int checks;
        static readonly BindingFlags Hidden=BindingFlags.NonPublic|BindingFlags.Instance;
        static void Check(bool ok,string name){if(!ok)throw new Exception("AUGMENT FAIL: "+name);checks++;}
        static AugmentData A(string id)=>db.augments.Single(a=>a.StableId=="augment_"+id);
        static BattleStats Stats(params string[] ids)=>new BattleStats{speciesId="test",maxHP=1000,attackDamage=100,baseAttackDamage=100,attackCooldown=.3f,baseAttackCooldown=.3f,baseSkillCooldown=5,skill=new SkillStats{displayName="Test",damage=100,cooldown=5},augments=ids.Select(id=>A(id).Snapshot()).ToArray()};
        static BattleModel Model(BattleStats s,int hp=1000,double roll=.99,int enemyDamage=0,float interval=1000)=>new BattleModel(s,new BattleEnemyStats{maxHP=1000000,damage=enemyDamage,interval=interval},hp,()=>roll);
        static object Call(BattleModel m,string method,params object[] args)=>typeof(BattleModel).GetMethod(method,Hidden).Invoke(m,args);
        static void Basic(BattleModel m,int count,float step=.31f){for(int i=0;i<count;i++){m.AttackReady=m.Time;Check(m.Attack(),"basic accepted");m.Tick(step);}}
        static TowerRun MakeRun(DragonData d,int level=20){var r=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);while(r.Level<level){r.RecordBattleVictory(r.CurrentHP);if(r.PendingEvolutionStage>0)r.ConsumeEvolution();}return r;}
        public static void Run()
        {
            try{Verify();Debug.Log("AUGMENT_REWORK_OK checks="+checks);EditorApplication.Exit(0);}
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        public static void Verify()
        {
            db=ContentDatabase.Load();Check(db!=null,"database");Check(db.augments.Length==80,"80 registered");
            Check(db.augments.Select(a=>a.StableId).Distinct().Count()==80,"unique IDs");
            for(int i=0;i<5;i++)Check(db.augments.Count(a=>(int)a.grade==i)==(i<3?20:10),"grade count "+i);
            Check(db.augments.All(a=>a.maximumStacks==1&&!string.IsNullOrWhiteSpace(a.description)&&Enum.IsDefined(typeof(AugmentMechanic),a.mechanic)),"valid data");
            int[] weights={50,30,15,4,1};for(int i=0;i<5;i++)Check(ContentDatabase.AugmentGradeWeight((AugmentGrade)i)==weights[i],"weight "+i);
            var random=new System.Random(391);int[] frequencies=new int[5];for(int i=0;i<100000;i++)frequencies[(int)ContentDatabase.PickWeightedAugment(db.augments,random).grade]++;
            for(int i=0;i<5;i++)Check(Math.Abs(frequencies[i]/1000f-weights[i])<.7,"observed grade weight "+i);
            foreach(var d in db.dragons)
            {
                var early=MakeRun(d,19);Check(!db.augments.Where(a=>a.grade==AugmentGrade.Legendary).Any(early.CanTakeAugment),"legendary evolution gate "+d.StableId);
                var run=MakeRun(d);run.AddAugment(A("endless_barrage"),false);Check(!db.augments.Where(a=>a.grade==AugmentGrade.Legendary).Any(run.CanTakeAugment),"one legendary "+d.StableId);
                if(d.signatureSkill==null)continue;run.ReplaceSkill(d.signatureSkill,false);var s=run.BuildBattleStats(d.Snapshot());var original=d.signatureSkill.Snapshot();
                Check(s.skill.skillId==original.skillId&&s.skill.timeDomain==original.timeDomain&&s.skill.chargedBeam==original.chargedBeam&&s.skill.attackEmpowerDuration==original.attackEmpowerDuration&&s.skill.completionDefenseDuration==original.completionDefenseDuration&&s.skill.castLockDuration==original.castLockDuration&&s.skill.cooldownRecastMaxHpPercent==original.cooldownRecastMaxHpPercent,"signature fields "+d.StableId);
                Check((s.skill.persistentAttack!=null)==(original.persistentAttack!=null),"persistent snapshot "+d.StableId);
                var m=Model(s,s.maxHP);Check(m.Skill(),"signature accepts "+d.StableId);for(int i=0;i<400;i++){m.Attack();m.Dodge();m.Tick(.05f);}Check(m.PlayerHP>=0&&m.EnemyHP>=0,"signature simulation "+d.StableId);
            }
            var dragon=db.dragons.First(d=>d.speciesId=="zephyr");
            var gate=MakeRun(dragon);Check(!gate.CanTakeAugment(A("absolute_frost")),"frost prerequisite");gate.AddAugment(A("overload_core"),false);Check(!gate.CanTakeAugment(A("transference")),"exclusive cooldown bridge");
            var seal=MakeRun(dragon);seal.AddAugment(A("seal"),false);Check(!seal.CanTakeAugment(A("double_casting")),"seal augment filter");Check(!db.skills.Any(s=>s!=null&&!s.IsSignatureSkill&&seal.CanOfferSkill(s)),"seal common skill filter");Check(seal.CanOfferSkill(dragon.signatureSkill),"seal allows signature");
            var dcast=MakeRun(dragon);dcast.AddAugment(A("double_casting"),false);Check(!dcast.CanTakeAugment(A("seal")),"reverse exclusion");
            // Every data asset enters the real model, including its static effects.
            foreach(var a in db.augments)
            {
                var s=Stats(a.StableId.Substring(8));var m=Model(s,500,0,8,.7f);
                for(int i=0;i<200;i++){if(i%6==0)m.Attack();if(i%40==0)m.Skill();if(i%30==0)m.Dodge();m.Tick(.05f);}
                Check(m.PlayerHP>=0&&m.PlayerHP<=s.maxHP&&m.EnemyHP>=0,"asset simulation "+a.StableId);
            }
            var first=Model(Stats("first_strike"));first.Attack();Check(first.EnemyHP==1000000-150,"first strike");first.Tick(.31f);first.Attack();Check(first.EnemyHP==1000000-250,"first strike once");
            var tech=Model(Stats("technician"));tech.Skill();tech.Attack();Check(tech.EnemyHP==1000000-230,"skill to basic bonus");tech.Tick(.31f);tech.Attack();Check(tech.EnemyHP==1000000-330,"technician consumed");
            var status=Model(Stats("hardened_claws","status_hunter"));Call(status,"ApplySlow",3f,30f);status.Attack();status.Skill();Check(status.EnemyHP==1000000-222,"conditional damage");
            var heal=Model(Stats("healthy_beauty","vital_strike"),500);Basic(heal,4);Check(heal.PlayerHP==523,"healing +15 percent");
            var steady=Model(Stats("steady_breath"),500);steady.Tick(5.05f);Check(steady.PlayerHP==510,"steady recovery");
            var mana=Model(Stats("mana_rampage"),20);Check(!mana.CanUseSkill&&!mana.Skill()&&mana.PlayerHP==20,"cost affordability");
            var cost=Model(Stats("mana_rampage","blood_recovery"),500);cost.Skill();Check(cost.PlayerHP==486,"cost 20 refund 6 once");
            var vamp=Model(Stats("vampire"),500);vamp.Attack();Check(vamp.PlayerHP==505,"true lifesteal");
            var skillHeal=Model(Stats("restorative_magic"),500);skillHeal.Skill();Check(skillHeal.PlayerHP==540,"skill completion heal");skillHeal.SkillReady=0;skillHeal.Skill();Check(skillHeal.PlayerHP==540,"heal cooldown");
            var noCrit=Model(Stats("critical_destiny"));Basic(noCrit,4);Check(noCrit.EnemyHP==1000000-550,"critical destiny fourth hit 250");
            var barrage=Model(Stats("endless_barrage"));Basic(barrage,7);Check(barrage.EnemyHP==1000000-750,"barrage activation and nonrecursive extra");
            var arcane=Model(Stats("arcane_apotheosis"));for(int i=0;i<4;i++){arcane.SkillReady=0;arcane.Skill();}Check(arcane.EnemyHP==1000000-500,"fourth common skill enhanced");
            var echo=Model(Stats("double_casting"));echo.Skill();Check(echo.EnemyHP==999900,"echo delayed");echo.Tick(.16f);Check(echo.EnemyHP==999840,"echo damage exactly once");echo.Tick(1);Check(echo.EnemyHP==999840,"no echo recursion");
            var frost=Model(Stats("frost_accumulation","absolute_frost"));for(int i=0;i<5;i++){Call(frost,"ApplySlow",3f,30f);frost.Tick(.51f);}Check(frost.EnemySlowStacks==5,"five frost stacks");int pre=frost.EnemyHP;frost.Skill();Check(pre-frost.EnemyHP==420&&frost.EnemySlowStacks==0&&frost.EnemySlowed,"frost consumes stack only");
            var poison=Model(Stats("venom_fang"),1000,0);poison.Attack();Check(poison.EnemyPoisoned,"temporary poison");poison.Tick(4.1f);Check(!poison.EnemyPoisoned,"temporary poison expires");
            var venom=Stats("venom_fang");venom.passiveMechanic=DragonPassiveMechanic.HydraVenom;var permanent=Model(venom,1000,0);permanent.Attack();permanent.Tick(5);Check(permanent.EnemyPoisoned,"permanent passive not weakened");
            var burn=Model(Stats("flame_remnant"));burn.Skill();burn.Tick(.5f);burn.SkillReady=0;burn.Skill();int after=burn.EnemyHP;burn.Tick(.51f);Check(burn.EnemyHP<after,"refresh does not starve burn ticks");
            var immune=Model(Stats("indomitable_will","stone_retort"),1000,.99,100,1);immune.SkillReady=10;Call(immune,"AddWard","test",200,5.0);immune.Tick(1.01f);Check(immune.SkillReady==10&&immune.PlayerHP==1000,"shield only no HP hit proc");
            var counter=Model(Stats("indomitable_will"),1000,.99,100,1);counter.SkillReady=10;counter.Tick(1.01f);Check(Math.Abs(counter.SkillReady-9)<.01,"cooldown refund shared budget");
            var dodge=Model(Stats("dodge_master"),1000,.99,100,1);dodge.SkillReady=10;dodge.Tick(.8f);dodge.Dodge();dodge.Tick(.21f);Check(dodge.SkillReady<10,"real dodge reward");
            var paralyze=Model(Stats("dodge_master"),1000,0,100,1);paralyze.SkillReady=10;Call(paralyze,"ApplyParalyze",5f);paralyze.Tick(1.01f);Check(paralyze.SkillReady==10,"paralysis miss not dodge reward");
            var wards=Model(Stats("casting_ward"));Call(wards,"AddWard","permanent",100,100.0);wards.Skill();Check(wards.ShieldHP==180,"independent wards");wards.Tick(2.1f);Check(wards.ShieldHP==100,"short ward leaves permanent");
            foreach(string id in new[]{"twin_mastery","raging_combustion","venom_sovereign","living_fortress","perfect_reversal","crimson_cycle","armored_offense","combat_cadence"})Check(A(id).mechanic!=AugmentMechanic.None,"capstone is implemented "+id);
            var twin=Model(Stats("twin_mastery"));twin.Attack();twin.Skill();twin.AttackReady=0;twin.Attack();twin.SkillReady=0;twin.Skill();int twinBefore=twin.EnemyHP;twin.AttackReady=0;twin.Attack();Check(twinBefore-twin.EnemyHP==182,"twin damage and nonrecursive extra");
            var fortress=Model(Stats("living_fortress"),1000,.99,100,1);fortress.Tick(2.01f);Check(fortress.PlayerHP==800&&fortress.ShieldHP==150,"fortress damage threshold");int fortressBefore=fortress.EnemyHP;fortress.Attack();Check(fortressBefore-fortress.EnemyHP==135,"fortress buff");fortress.Tick(4.1f);Check(fortress.ShieldHP==0,"fortress expiry");
            var reversal=Model(Stats("perfect_reversal"),500);for(int i=0;i<3;i++)Call(reversal,"OnAugmentDodgeSuccess");reversal.Attack();Check(reversal.PlayerHP==580&&reversal.EnemyHP==999700,"perfect reversal consumes once");
            var cycle=Model(Stats("mana_rampage","crimson_cycle"));for(int i=0;i<10;i++){cycle.SkillReady=0;cycle.Skill();}Check(cycle.PlayerHP==880,"crimson cost then bounded recovery");
            var cadenceModel=Model(Stats("combat_cadence"));Basic(cadenceModel,5);Check(cadenceModel.EnemyHP==999350,"cadence fifth action");
            var combustStats=Stats("raging_combustion");combustStats.criticalChance=100;var combust=Model(combustStats);Call(combust,"ApplyBurn",5f,10);Basic(combust,3);Check(combust.EnemyHP==999300,"combustion three direct critical actions");
            var sovereignStats=Stats("venom_sovereign");sovereignStats.criticalChance=100;var sovereign=Model(sovereignStats);Call(sovereign,"ApplyTemporaryPoison",10,4f);sovereign.Attack();Check(sovereign.EnemyHP==999830,"venom independent extra");
            var csv=ContentCsv.Export("Augments",db.augments);var csvPath="Temp/augment-roundtrip.csv";System.IO.File.WriteAllText(csvPath,csv);Check(ContentCsv.Import("Augments",csvPath)==80&&ContentCsv.Export("Augments",db.augments)==csv,"CSV all fields round trip");
            var huge=Stats("acceleration_circuit","rapid_fire_instinct");huge.attackCooldown=.01f;var cap=Model(huge);cap.Attack();Check(cap.AttackReady>=.12,"attack cooldown cap");
            var duration=Stats();duration.dodgeCooldown=.1f;duration.dodgeDuration=.8f;var gap=Model(duration);gap.Dodge();Check(gap.DodgeReady>=gap.DodgeUntil+.149,"dodge gap");
            var healCap=Model(Stats("healthy_beauty"),990);Check(healCap.HealPlayer(100)==10&&healCap.PlayerHP==1000,"healing cap");
            System.IO.File.WriteAllText("Balance/Augments.csv",ContentCsv.Export("Augments",db.augments));
            Debug.Log("AUGMENT_DISTRIBUTION "+string.Join(",",frequencies));
        }
    }
}


