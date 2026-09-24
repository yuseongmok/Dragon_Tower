using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    [InitializeOnLoad]
    public static class ContentDataSetup
    {
        const string Root="Assets/Data";
        const string DatabasePath="Assets/Resources/ContentDatabase.asset";
        static ContentDataSetup()
        {
            if(!Application.isBatchMode)EditorApplication.delayCall+=()=>Install(false);
        }
        [MenuItem("Dragon Tower/콘텐츠 데이터 설치 또는 갱신")]
        public static void InstallMenu(){Install(true);}
        public static void InstallForAutomation(){Install(false);}
        public static ContentDatabase Install(bool showMessage)
        {
            EnsureFolder("Assets","Data");EnsureFolder(Root,"Monsters");EnsureFolder(Root,"Items");EnsureFolder(Root,"Augments");EnsureFolder(Root,"Skills");EnsureFolder("Assets","Resources");
            EnsureMonster("monster_rock_slime","바위 슬라임",ElementType.Neutral,240,18,2.4f,1,9,10,false,"rock-slime.png");
            EnsureMonster("monster_small_golem","소형 골렘",ElementType.Earth,240,18,2.4f,1,9,10,false,"small-golem.png");
            EnsureMonster("monster_dungeon_zombie","던전 좀비",ElementType.Dark,240,18,2.4f,1,9,10,false,"dungeon-zombie.png");
            EnsureMonster("monster_cave_bat","동굴 박쥐",ElementType.Dark,240,18,2.4f,1,9,10,false,"cave-bat.png");
            EnsureMonster("monster_armored_skeleton","갑옷 해골 기사",ElementType.Earth,240,18,2.4f,1,9,10,false,"armored-skeleton.png");
            EnsureMonster("boss_ancient_golem","고대 룬 골렘",ElementType.Earth,360,24,2.2f,10,999,10,true,"ancient-golem-boss.png");
            EnsureItem("item_healing_potion","체력 물약",30,ContentEffectType.Heal,35);
            EnsureItem("item_iron_scale","강철 비늘",0,ContentEffectType.MaxHP,15);
            EnsureItem("item_sharp_claw","날카로운 발톱",0,ContentEffectType.AttackDamage,2);
            EnsureAugments();
            UpdateExistingSkills();
            UpdateExistingDragons();
            var database=AssetDatabase.LoadAssetAtPath<ContentDatabase>(DatabasePath);
            if(database==null){database=ScriptableObject.CreateInstance<ContentDatabase>();AssetDatabase.CreateAsset(database,DatabasePath);}
            Rebuild(database);AssetDatabase.SaveAssets();
            if(showMessage)EditorUtility.DisplayDialog("콘텐츠 데이터","기존 콘텐츠를 보존하면서 데이터베이스를 갱신했습니다.","확인");
            return database;
        }
        public static void Rebuild(ContentDatabase database=null)
        {
            if(database==null)database=AssetDatabase.LoadAssetAtPath<ContentDatabase>(DatabasePath);
            if(database==null)return;
            database.dragons=FindAll<DragonData>().Where(d=>!string.IsNullOrWhiteSpace(d.speciesId)&&d.skill!=null&&d.battleSprite!=null).ToArray();database.monsters=FindAll<MonsterData>();database.skills=FindAll<SkillData>();
            database.items=FindAll<ItemData>();database.augments=FindAll<AugmentData>();
            EditorUtility.SetDirty(database);AssetDatabase.SaveAssets();
        }
        static T[] FindAll<T>() where T:UnityEngine.Object=>AssetDatabase.FindAssets("t:"+typeof(T).Name,new[]{Root})
            .Select(g=>AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g))).Where(x=>x!=null).OrderBy(x=>x.name).ToArray();
        static void EnsureFolder(string parent,string child){string path=parent+"/"+child;if(!AssetDatabase.IsValidFolder(path))AssetDatabase.CreateFolder(parent,child);}
        static void EnsureMonster(string id,string name,ElementType element,int hp,int damage,float interval,int minFloor,int maxFloor,int weight,bool boss,string spriteFile)
        {
            string path=Root+"/Monsters/"+id+".asset";var data=AssetDatabase.LoadAssetAtPath<MonsterData>(path);if(data!=null)return;
            data=ScriptableObject.CreateInstance<MonsterData>();data.contentId=id;data.displayName=name;data.elementType=element;data.baseHP=hp;data.attackDamage=damage;
            data.attackInterval=interval;data.minimumFloor=minFloor;data.maximumFloor=maxFloor;data.spawnWeight=weight;data.boss=boss;
            if(boss){data.hpGrowthPerFloor=6;data.attackGrowthPerFloor=.2f;}
            data.battleSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/PixelBattle/"+spriteFile);AssetDatabase.CreateAsset(data,path);
        }
        static void EnsureItem(string id,string name,int price,ContentEffectType type,float value)
        {
            string path=Root+"/Items/"+id+".asset";var data=AssetDatabase.LoadAssetAtPath<ItemData>(path);if(data!=null)return;
            data=ScriptableObject.CreateInstance<ItemData>();data.contentId=id;data.displayName=name;data.price=price;
            data.effects.Add(new ContentEffect{type=type,value=value});AssetDatabase.CreateAsset(data,path);
        }
        static ContentEffect E(ContentEffectType type,float value)=>new ContentEffect{type=type,value=value};
        static void EnsureAugment(string id,string name,string description,AugmentMechanic mechanic=AugmentMechanic.None,float primary=0,float secondary=0,float chance=0,float duration=0,int count=1,params ContentEffect[] effects)
        {
            string path=Root+"/Augments/"+id+".asset";var data=AssetDatabase.LoadAssetAtPath<AugmentData>(path);if(data!=null)return;
            data=ScriptableObject.CreateInstance<AugmentData>();data.contentId=id;data.displayName=name;data.description=description;data.maximumStacks=1;
            data.mechanic=mechanic;data.primaryValue=primary;data.secondaryValue=secondary;data.chancePercent=chance;data.duration=duration;data.triggerCount=Math.Max(1,count);
            if(effects!=null)data.effects.AddRange(effects);AssetDatabase.CreateAsset(data,path);
        }
        static void EnsureAugments()
        {
            EnsureAugment("augment_overload_core","과부하 코어","일반 공격 쿨타임 +50% · 일반 공격 적중 시 남은 스킬 쿨타임 0.5초 감소",AugmentMechanic.OverloadCore,.5f,0,0,0,1,E(ContentEffectType.AttackCooldownPercent,-50));
            EnsureAugment("augment_mana_rampage","마나폭주","스킬 쿨타임 30% 감소 · 스킬 시전 시 최대 HP의 2% 소모",AugmentMechanic.ManaRampage,2,0,0,0,1,E(ContentEffectType.SkillCooldownPercent,30));
            EnsureAugment("augment_rapid_fire_instinct","속사 본능","일반 공격 3회 적중마다 5초간 공격 쿨타임 15% 감소",AugmentMechanic.RapidFireInstinct,15,0,0,5,3);
            EnsureAugment("augment_sharp_claws","예리한 발톱","일반 공격 피해 35% 증가 · 스킬 쿨타임 30% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.AttackDamagePercent,35),E(ContentEffectType.SkillCooldownPercent,-30));
            EnsureAugment("augment_consecutive_slash","연속베기","일반 공격 시 25% 확률로 피해의 60% 추가 타격",AugmentMechanic.ConsecutiveSlash,60,0,25);
            EnsureAugment("augment_last_stand","배수의 진","잃은 체력 10%당 일반 공격과 스킬 피해 5% 증가",AugmentMechanic.LastStand,5);
            EnsureAugment("augment_giant_heart","거인의 심장","최대 HP 40% 증가 · 일반 공격과 스킬 쿨타임 10% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.MaxHPPercent,40),E(ContentEffectType.AttackCooldownPercent,-10),E(ContentEffectType.SkillCooldownPercent,-10));
            EnsureAugment("augment_first_aid","응급 처치","HP가 30% 이하가 되면 3초간 일반 공격·스킬 쿨타임 50% 감소 (전투당 1회)",AugmentMechanic.FirstAid,50,30,0,3);
            EnsureAugment("augment_exploit_weakness","약점 찌르기","상성 우위인 적에게 주는 피해 40% 증가",AugmentMechanic.ExploitWeakness,40);
            EnsureAugment("augment_flame_remnant","화염 잔재","스킬 피해 시 3초간 화상 (임시 계수: 매초 공격력의 20%)",AugmentMechanic.FlameRemnant,20,0,0,3);
            EnsureAugment("augment_static_discharge","정전기 방출","일반 공격 5회마다 공격 피해의 70% 전기 추가 공격",AugmentMechanic.StaticDischarge,70,0,0,0,5);
            EnsureAugment("augment_frost_barrier","서리 장벽","회피 실패 시 최대 HP 15% 보호막을 4초간 획득 (쿨타임 15초)",AugmentMechanic.FrostBarrier,15,15,0,4);
            EnsureAugment("augment_technician","기술자","스킬 피해 10% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.SkillDamagePercent,10));
            EnsureAugment("augment_hardened_claws","단단한 발톱","일반 공격 피해 10% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.AttackDamagePercent,10));
            EnsureAugment("augment_inferno","업화","화상 상태인 적을 공격할 때 치명타 확률 50% 증가",AugmentMechanic.Inferno,50);
            EnsureAugment("augment_tingly","찌릿찌릿","스킬 사용 시 10% 확률로 마비 (임시값: 적 공격 1.5초 지연)",AugmentMechanic.Tingly,0,0,10,1.5f);
            EnsureAugment("augment_dodge_master","회피 마스터","회피 성공 시 남은 스킬 쿨타임 10% 감소",AugmentMechanic.DodgeMaster,10);
            EnsureAugment("augment_ice_cream","아이스크림","스킬 사용 시 10% 확률로 3초간 적 공격속도 30% 둔화",AugmentMechanic.IceCream,30,0,10,3);
            EnsureAugment("augment_lethal_attack","치명적인 공격","치명타 피해 10% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.CriticalDamagePercent,10));
            EnsureAugment("augment_aim_vitals","급소를 노려","치명타 확률 10% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.CriticalChancePercent,10));
            EnsureAugment("augment_healthy_beauty","건강미","최대 HP 15% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.MaxHPPercent,15));
            EnsureAugment("augment_seal","봉인","스킬 사용 불가 · 일반 공격 피해 50% 증가 · 공격 쿨타임 30% 감소",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.SkillDisabled,1),E(ContentEffectType.AttackDamagePercent,50),E(ContentEffectType.AttackCooldownPercent,30));
            EnsureAugment("augment_transference","전이","일반 공격 피해 70% 감소 · 일반 공격 적중마다 남은 스킬 쿨타임 1초 감소",AugmentMechanic.Transference,1,0,0,0,1,E(ContentEffectType.AttackDamagePercent,-70));
            EnsureAugment("augment_double_casting","더블 캐스팅","스킬 쿨타임 70% 증가 · 스킬이 즉시 2회 발동",AugmentMechanic.DoubleCasting,0,0,0,0,1,E(ContentEffectType.SkillCooldownPercent,-70));
            EnsureAugment("augment_vampire","뱀파이어","스킬 쿨타임 삭제 · 스킬 사용마다 최대 HP의 5% 소모",AugmentMechanic.Vampire,5,0,0,0,1,E(ContentEffectType.SkillCooldownSetZero,1));
            EnsureAugment("augment_glass_cannon","유리대포","최대 HP 50% 감소 · 일반 공격과 스킬 피해 50% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.MaxHPPercent,-50),E(ContentEffectType.AttackDamagePercent,50),E(ContentEffectType.SkillDamagePercent,50));
            EnsureAugment("augment_indomitable_will","불굴의 의지","피격 시 스킬 쿨타임 초기화",AugmentMechanic.IndomitableWill);
            EnsureAugment("augment_spread","스프레드","일반 공격 시 피해의 30% 추가 공격",AugmentMechanic.Spread,30);
            EnsureAugment("augment_clone","분신","치명타 일반 공격 시 피해의 20%로 2회 추가 공격",AugmentMechanic.Clone,20,0,0,0,2);

            // Additional data-only augments. These use the existing reusable stat effects, so they
            // remain editable from the content window and CSV without adding one-off combat code.
            EnsureAugment("augment_battle_instinct","전투 본능","일반 공격 피해 15% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.AttackDamagePercent,15));
            EnsureAugment("augment_mana_crystal","마력 결정","스킬 피해 15% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.SkillDamagePercent,15));
            EnsureAugment("augment_thick_scales","두꺼운 비늘","최대 HP 20% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.MaxHPPercent,20));
            EnsureAugment("augment_tailwind","순풍","회피 쿨타임 15% 감소",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.DodgeCooldownPercent,15));
            EnsureAugment("augment_acceleration_circuit","가속 회로","일반 공격 쿨타임 20% 감소",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.AttackCooldownPercent,20));
            EnsureAugment("augment_cooling_rune","냉각 룬","스킬 쿨타임 20% 감소",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.SkillCooldownPercent,20));
            EnsureAugment("augment_predator_eye","포식자의 눈","치명타 확률 15% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.CriticalChancePercent,15));
            EnsureAugment("augment_reinforced_wings","강화 날개","회피 지속시간 25% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.DodgeDurationPercent,25));
            EnsureAugment("augment_war_dragon_blood","전투룡의 피","일반 공격과 스킬 피해 25% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.AttackDamagePercent,25),E(ContentEffectType.SkillDamagePercent,25));
            EnsureAugment("augment_time_compression","시간 압축","최대 HP 15% 감소 · 일반 공격과 스킬 쿨타임 25% 감소",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.MaxHPPercent,-15),E(ContentEffectType.AttackCooldownPercent,25),E(ContentEffectType.SkillCooldownPercent,25));
            EnsureAugment("augment_survival_instinct","생존 본능","최대 HP 30% 증가 · 회피 쿨타임 25% 감소",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.MaxHPPercent,30),E(ContentEffectType.DodgeCooldownPercent,25));
            EnsureAugment("augment_executioner","처형자","치명타 확률 20% · 치명타 피해 35% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.CriticalChancePercent,20),E(ContentEffectType.CriticalDamagePercent,35));
            EnsureAugment("augment_dragon_king_blessing","용왕의 축복","최대 HP 50% · 일반 공격과 스킬 피해 25% 증가",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.MaxHPPercent,50),E(ContentEffectType.AttackDamagePercent,25),E(ContentEffectType.SkillDamagePercent,25));
            EnsureAugment("augment_perfect_flow","완전한 흐름","일반 공격·스킬·회피 쿨타임 30% 감소",AugmentMechanic.None,0,0,0,0,1,E(ContentEffectType.AttackCooldownPercent,30),E(ContentEffectType.SkillCooldownPercent,30),E(ContentEffectType.DodgeCooldownPercent,30));

            SetAugmentGrades(AugmentGrade.Common,"augment_technician","augment_hardened_claws","augment_lethal_attack","augment_aim_vitals","augment_healthy_beauty","augment_battle_instinct","augment_mana_crystal","augment_thick_scales","augment_tailwind");
            SetAugmentGrades(AugmentGrade.Rare,"augment_rapid_fire_instinct","augment_sharp_claws","augment_consecutive_slash","augment_giant_heart","augment_flame_remnant","augment_static_discharge","augment_tingly","augment_dodge_master","augment_ice_cream","augment_spread","augment_acceleration_circuit","augment_cooling_rune","augment_predator_eye","augment_reinforced_wings");
            SetAugmentGrades(AugmentGrade.Epic,"augment_overload_core","augment_mana_rampage","augment_last_stand","augment_first_aid","augment_exploit_weakness","augment_frost_barrier","augment_inferno","augment_transference","augment_indomitable_will","augment_clone","augment_war_dragon_blood","augment_time_compression","augment_survival_instinct","augment_executioner");
            SetAugmentGrades(AugmentGrade.Unique,"augment_seal","augment_double_casting","augment_vampire","augment_glass_cannon","augment_dragon_king_blessing","augment_perfect_flow");
        }
        static void SetAugmentGrades(AugmentGrade grade,params string[] ids)
        {
            foreach(string id in ids)
            {
                var data=AssetDatabase.LoadAssetAtPath<AugmentData>(Root+"/Augments/"+id+".asset");
                if(data==null||data.grade==grade)continue;data.grade=grade;EditorUtility.SetDirty(data);
            }
        }
        static void UpdateExistingSkills()
        {
            var ids=new[]{"skill_fire_burst","skill_frost_breath","skill_gale_strike","skill_sandstorm","skill_electric_shock","skill_bubble_shot","skill_dark_strike","skill_flash_burst"};
            var names=new[]{"화염 폭발","고드름침","질풍 강타","짱돌 스매쉬","전기 쇼크","버블 샷","어둠 일격","섬광 폭발"};
            var elements=new[]{ElementType.Fire,ElementType.Ice,ElementType.Wind,ElementType.Earth,ElementType.Lightning,ElementType.Water,ElementType.Dark,ElementType.Light};
            var prefabs=new[]{"FX_Fire_Exp","FX_Blue Stab","FX_Impact Airflow","FX_Ground Shockwave","FX_Cartoon Thunder","FX_Splash_Hit","FX_PinkMagicArrow_Hit","FX_FlashShot_Orange"};
            var scales=new[]{.8f,.55f,.55f,.45f,.6f,.65f,.6f,.6f};
            var offsets=new[]{Vector2.zero,Vector2.zero,Vector2.zero,new Vector2(0,-55),new Vector2(0,15),Vector2.zero,Vector2.zero,new Vector2(0,10)};
            var durations=new[]{1.3f,1.4f,1.4f,1.7f,1.5f,1.4f,1.5f,1.4f};
            var limits=new[]{7,10,13,18,11,9,9,13};
            const string prefabRoot="Assets/Eric VFX Studio/Game VFX - Cartoon Skill Effects/Prefabs/Built-In/";
            for(int i=0;i<8;i++)
            {
                var skill=AssetDatabase.LoadAssetAtPath<SkillData>(Root+"/Skill"+i+".asset");if(skill==null)continue;
                if(string.IsNullOrWhiteSpace(skill.skillId))skill.skillId=ids[i];skill.displayName=names[i];skill.elementType=elements[i];skill.effectKind=(SkillEffectKind)i;EditorUtility.SetDirty(skill);
                if(skill.battleVfx==null)
                {
                    skill.battleVfx=AssetDatabase.LoadAssetAtPath<GameObject>(prefabRoot+prefabs[i]+".prefab");
                    skill.vfxScale=scales[i];skill.vfxOffset=offsets[i];skill.vfxEuler=Vector3.zero;
                    skill.vfxDuration=durations[i];skill.vfxSystemLimit=limits[i];
                }
                var dragon=AssetDatabase.LoadAssetAtPath<DragonData>(Root+"/Dragon"+i+".asset");if(dragon!=null){dragon.skillEffect=(SkillEffectKind)i;EditorUtility.SetDirty(dragon);}
            }
            EnsureSkill("skill_fire_shot","화염샷",ElementType.Fire,SkillEffectKind.Fire,16,5,3,.13f,CombatStatusEffect.Burn,25,3,5,"FX_Throw Fireball",.65f,12);
            EnsureSkill("skill_meteor","메테오",ElementType.Fire,SkillEffectKind.Fire,28,8,3,.22f,CombatStatusEffect.Burn,40,4,7,"FX_Fireball_Fall2",.62f,14);
            EnsureSkill("skill_purgatory","연옥",ElementType.Fire,SkillEffectKind.Fire,18,11,6,.16f,CombatStatusEffect.Burn,70,5,8,"FX_Disaster Starfall2",.55f,16);
            EnsureSkill("skill_ice_slash","얼음 베기",ElementType.Ice,SkillEffectKind.Frost,18,5.5f,3,.12f,CombatStatusEffect.Slow,30,3,20,"FX_BlueSlash_Combination",.55f,12);
            EnsureSkill("skill_ice_slam","얼음 강타",ElementType.Ice,SkillEffectKind.Frost,32,7,2,.22f,CombatStatusEffect.Slow,50,4,30,"FX_BlueSlash_Ground02",.55f,12);
            EnsureSkill("skill_moon_combo","달의 연격",ElementType.Ice,SkillEffectKind.Frost,15,10,6,.13f,CombatStatusEffect.Slow,75,4,40,"FX_Four Stab",.55f,14);
            EnsureSkill("skill_falling_flower","낙화",ElementType.Wind,SkillEffectKind.Wind,12,3.5f,3,.10f,CombatStatusEffect.Slow,25,2.5f,15,"FX_3Rotation_Air",.55f,10);
            EnsureSkill("skill_gale_slash","질풍참",ElementType.Wind,SkillEffectKind.Wind,11,5.5f,5,.09f,CombatStatusEffect.Slow,35,3,20,"FX_Whirlwind Slash",.55f,14);
            EnsureSkill("skill_storm_slash","폭풍참",ElementType.Wind,SkillEffectKind.Wind,14,8.5f,6,.11f,CombatStatusEffect.Slow,55,4,30,"FX_Slash_Ground2",.55f,15);
            EnsureSkill("skill_dust_storm","모래폭풍",ElementType.Earth,SkillEffectKind.Earth,13,5,4,.18f,CombatStatusEffect.Slow,35,3,20,"FX_Whirlpool Explosion 1",.50f,13);
            EnsureSkill("skill_earthquake","어스퀘이크",ElementType.Earth,SkillEffectKind.Earth,32,9,3,.24f,CombatStatusEffect.Slow,60,4,35,"FX_Ground Shockwave",.50f,14);
            EnsureSkill("skill_shockwave","쇼크웨이브",ElementType.Earth,SkillEffectKind.Earth,22,8,4,.15f,CombatStatusEffect.Paralyze,30,1.2f,0,"FX_Ray blast",.55f,13);
            EnsureSkill("skill_electric_bolt","전격",ElementType.Lightning,SkillEffectKind.Lightning,14,4,3,.10f,CombatStatusEffect.Paralyze,20,.8f,0,"FX_Cartoon Thunder2",.55f,11);
            EnsureSkill("skill_lightning_orb","번개구체",ElementType.Lightning,SkillEffectKind.Lightning,10,6,6,.12f,CombatStatusEffect.Paralyze,35,1,0,"FX_ColorLightball",.55f,14);
            EnsureSkill("skill_thunder_strike","뇌격",ElementType.Lightning,SkillEffectKind.Lightning,24,9,4,.18f,CombatStatusEffect.Paralyze,55,1.5f,0,"FX_God's punishment",.55f,14);
            EnsureSkill("skill_power_whip","파워휩",ElementType.Water,SkillEffectKind.Water,16,4.8f,3,.14f,CombatStatusEffect.Slow,30,3,20,"FX_Double Claw",.55f,12);
            EnsureSkill("skill_snipe","저격",ElementType.Water,SkillEffectKind.Water,62,6.2f,1,.14f,CombatStatusEffect.None,0,0,0,"FX_FlashShot_Blue",.55f,10);
            EnsureSkill("skill_splash","스플래쉬",ElementType.Water,SkillEffectKind.Water,14,8.5f,6,.12f,CombatStatusEffect.Slow,60,4,35,"FX_Whirlpool Explosion",.55f,15);
            EnsureSkill("skill_paranoia","피해망상",ElementType.Dark,SkillEffectKind.Dark,15,4.5f,3,.14f,CombatStatusEffect.Slow,25,3,20,"FX_Debuff_Lethargy",.55f,10);
            EnsureSkill("skill_space_rift","우주 균열",ElementType.Dark,SkillEffectKind.Dark,22,8,4,.18f,CombatStatusEffect.Burn,35,4,6,"FX_Blue Rune",.55f,13);
            EnsureSkill("skill_apocalypse","아포칼립스",ElementType.Dark,SkillEffectKind.Dark,20,11,6,.18f,CombatStatusEffect.Burn,65,5,8,"FX_Disaster Starfall",.55f,16);
            EnsureSkill("skill_light_pillar","빛기둥",ElementType.Light,SkillEffectKind.Light,18,5,3,.14f,CombatStatusEffect.Paralyze,20,.8f,0,"FX_God's punishment2",.55f,12);
            EnsureSkill("skill_holy_explosion","신성 폭발",ElementType.Light,SkillEffectKind.Light,30,8,3,.20f,CombatStatusEffect.Paralyze,35,1.1f,0,"FX_MagicLightning_Exp",.55f,13);
            EnsureSkill("skill_big_bang","빅뱅",ElementType.Light,SkillEffectKind.Light,18,10.5f,6,.16f,CombatStatusEffect.Paralyze,55,1.5f,0,"FX_Aim_Explode",.55f,16);
            foreach(string guid in AssetDatabase.FindAssets("t:SkillData",new[]{Root}))
            {
                var skill=AssetDatabase.LoadAssetAtPath<SkillData>(AssetDatabase.GUIDToAssetPath(guid));if(skill==null||!string.IsNullOrWhiteSpace(skill.skillId))continue;
                skill.skillId="skill_"+skill.name.ToLowerInvariant();EditorUtility.SetDirty(skill);
            }
        }
        static void EnsureSkill(string id,string name,ElementType element,SkillEffectKind effect,int damage,float cooldown,int hits,float interval,CombatStatusEffect status,float chance,float duration,float power,string prefab,float scale,int limit)
        {
            string path=Root+"/Skills/"+id+".asset";var skill=AssetDatabase.LoadAssetAtPath<SkillData>(path);bool initialize=skill==null;
            if(skill==null)
            {
                string legacy=LegacySkillId(id);
                foreach(string guid in AssetDatabase.FindAssets("t:SkillData",new[]{Root}))
                {
                    string oldPath=AssetDatabase.GUIDToAssetPath(guid);var candidate=AssetDatabase.LoadAssetAtPath<SkillData>(oldPath);
                    if(candidate==null||(candidate.StableId!=legacy&&candidate.StableId!=id))continue;
                    string error=AssetDatabase.MoveAsset(oldPath,path);if(!string.IsNullOrEmpty(error))throw new InvalidOperationException(error);
                    skill=candidate;break;
                }
            }
            if(skill==null){skill=ScriptableObject.CreateInstance<SkillData>();AssetDatabase.CreateAsset(skill,path);}
            if(initialize)
            {
                skill.skillId=id;skill.displayName=name;skill.elementType=element;skill.effectKind=effect;skill.damage=damage;skill.cooldown=cooldown;
                skill.hitCount=Math.Max(1,hits);skill.hitInterval=Math.Max(.03f,interval);skill.statusEffect=status;skill.statusChancePercent=chance;skill.statusDuration=duration;skill.statusPower=power;
                skill.description=(hits>1?damage+" 피해를 "+hits+"회":"피해 "+damage)+(status==CombatStatusEffect.None?"":" · "+status+" "+chance.ToString("0")+"%");
            }
            if(skill.battleVfx==null)
            {
                skill.battleVfx=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Eric VFX Studio/Game VFX - Cartoon Skill Effects/Prefabs/Built-In/"+prefab+".prefab");
                skill.vfxScale=scale;skill.vfxDuration=1.6f;skill.vfxSystemLimit=limit;
            }
            EditorUtility.SetDirty(skill);
        }
        static string LegacySkillId(string id)
        {
            switch(id)
            {
                case "skill_fire_shot":return "skill_Flame_Shot";case "skill_meteor":return "skill_Flame_Meteor";case "skill_purgatory":return "skill_fire_Purgatory";
                case "skill_ice_slash":return "skill_frost_cleaver";case "skill_ice_slam":return "skill_Ice_Storm";case "skill_moon_combo":return "skill_moon_Rapid";
                case "skill_falling_flower":return "skill_Falling_flower";case "skill_gale_slash":return "skill_Whirlwind_Spear";case "skill_storm_slash":return "skill_Storm_Slash";
                case "skill_dust_storm":return "skill_Sandstorm";case "skill_earthquake":return "skill_earthquake";case "skill_shockwave":return "skill_Shockwave";
                case "skill_electric_bolt":return "skill_Lightning";case "skill_lightning_orb":return "skill_Lightning_sphere";case "skill_thunder_strike":return "skill_Thunderbolt";
                case "skill_power_whip":return "skill_Power_Whip";case "skill_snipe":return "skill_Snipe";case "skill_splash":return "skill_Splash";
                case "skill_paranoia":return "skill_Delusional";case "skill_space_rift":return "skill_Space_crack";case "skill_apocalypse":return "skill_Apocalypse";
                case "skill_light_pillar":return "skill_Pillar";case "skill_holy_explosion":return "skill_Divine_Explosion";case "skill_big_bang":return "skill_Big_Bang";default:return id;
            }
        }
        static void UpdateExistingDragons()
        {
            var files=new[]{"ember-evolution.png","luna-evolution.png","zephyr-evolution.png","brandy-evolution.png","volt-evolution.png","okta-evolution.png","nova-evolution.png","dante-evolution.png"};
            var middle=new[]{"화염룡 엠버","월빙룡 루나","질풍룡 제피르","지진상어","뇌전룡 볼트","옥탈리아","성운룡 노바","성휘룡 단테"};
            var finalNames=new[]{"용암군주 엠버","달의 여왕 루나","폭풍군주 제피르","심연모래두지상어","천둥의 군주 볼트","옥타벨","은하군주 노바","광휘군주 단테"};
            const string artRoot="Assets/Art/Evolution/";
            for(int i=0;i<files.Length;i++)
            {
                var dragon=AssetDatabase.LoadAssetAtPath<DragonData>(Root+"/Dragon"+i+".asset");if(dragon==null)continue;
                if(dragon.evolutionSheet==null)dragon.evolutionSheet=AssetDatabase.LoadAssetAtPath<Texture2D>(artRoot+files[i]);
                if(string.IsNullOrWhiteSpace(dragon.intermediateName))dragon.intermediateName=middle[i];
                if(string.IsNullOrWhiteSpace(dragon.finalName))dragon.finalName=finalNames[i];
                EditorUtility.SetDirty(dragon);
            }
        }
    }
}
