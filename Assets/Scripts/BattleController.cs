using UnityEngine;
namespace DragonTower
{
    public partial class BattleController : MonoBehaviour
    {
        public DragonData[] dragons;
        public BattleView view;
        BattleModel battle;
        public BattleModel CurrentBattle => battle;
        public CollectionFlow Flow { get; private set; }
        bool focused=true;
        int requestedDodgeDirection=-1;
        public bool CanControl=>focused&&battle!=null&&battle.Result==BattleResult.Fighting&&view.IsBattleUncovered;
        public bool RequestAttack(){if(!CanControl||!battle.Attack())return false;RecordBattleInput(1);return true;}
        public bool RequestSkill(){if(!CanControl||!battle.Skill())return false;RecordBattleInput(2);return true;}
        public bool RequestDodge(int direction)
        {
            if(!CanControl)return false;
            requestedDodgeDirection=direction>0?1:-1;
            try{bool ok=battle.Dodge();if(ok)RecordBattleInput(3);return ok;}finally{requestedDodgeDirection=-1;}
        }
        void HandleCue(CombatCue cue,int damage)
        {
            if(cue==CombatCue.Dodge)view.SetDodgeDirection(requestedDodgeDirection);
            var catastrophe=view.GetComponent<CatastropheVfx>();if(catastrophe!=null){catastrophe.DamageFromSkill=battle.ResolvingSkillHit;catastrophe.ScheduledHits=battle.ScheduledSkillHits;}
            var crushing=view.GetComponent<CrushingBeamVfx>();if(crushing!=null)crushing.DamageFromSkill=battle.ResolvingSkillHit;
            var genesis=view.GetComponent<GenesisVfx>();if(genesis!=null)genesis.DamageFromSkill=battle.ResolvingSkillHit;
            var volcanic=view.GetComponent<VolcanicMeteorVfx>();if(volcanic!=null){volcanic.DamageFromSkill=battle.ResolvingSkillHit;volcanic.ScheduledHits=battle.ScheduledSkillHits;}
            var solar=view.GetComponent<SolarPierceVfx>();if(solar!=null){solar.DamageFromSkill=battle.ResolvingSkillHit;solar.ScheduledHits=battle.ScheduledSkillHits;}
            var motion=view.GetComponent<BattleAnimation>();if(motion!=null){motion.ascensionPattern=battle.EmpoweredHitPattern;motion.celestialEventId=battle.CelestialEventId;motion.celestialEventPattern=battle.CelestialEventPattern;}
            var celestial=view.GetComponent<CelestialVfx>();if(celestial!=null)celestial.DamageFromSkill=battle.ResolvingSkillHit;
            var haesin=view.GetComponent<HaesinVfx>();if(haesin!=null)haesin.DamageFromSkill=battle.ResolvingSkillHit;
            var water=view.GetComponent<WaterSkillVfx>();if(water!=null){water.DamageFromSkill=battle.ResolvingSkillHit;water.ScheduledHits=battle.ScheduledSkillHits;}
            var mir=view.GetComponent<MirLegendaryVfx>();if(mir!=null)mir.DamageFromSkill=battle.ResolvingSkillHit;
            var hydra=view.GetComponent<HydraVfx>();if(hydra!=null)hydra.DamageFromSkill=battle.ResolvingSkillHit;
            var eclipse=view.GetComponent<EclipseVfx>();if(eclipse!=null)eclipse.DamageFromSkill=battle.ResolvingSkillHit;
            var dark=view.GetComponent<DarkSkillVfx>();if(dark!=null){dark.DamageFromSkill=battle.ResolvingSkillHit;dark.ScheduledHits=battle.ScheduledSkillHits;}
            var light=view.GetComponent<LightSkillVfx>();if(light!=null){light.DamageFromSkill=battle.ResolvingSkillHit;light.ScheduledHits=battle.ScheduledSkillHits;}
            var lightning=view.GetComponent<LightningSkillVfx>();if(lightning!=null){lightning.DamageFromSkill=battle.ResolvingSkillHit;lightning.ScheduledHits=battle.ScheduledSkillHits;}
            var earth=view.GetComponent<EarthSkillVfx>();if(earth!=null){earth.DamageFromSkill=battle.ResolvingSkillHit;earth.ScheduledHits=battle.ScheduledSkillHits;}
            var fire=view.GetComponent<FireSkillVfx>();if(fire!=null){fire.DamageFromSkill=battle.ResolvingSkillHit;fire.ScheduledHits=battle.ScheduledSkillHits;}
            var ice=view.GetComponent<IceSkillVfx>();if(ice!=null)ice.DamageFromSkill=battle.ResolvingSkillHit;
            var wisp=view.GetComponent<WispVfx>();if(wisp!=null)wisp.DamageFromSkill=battle.ResolvingSkillHit;
            var lunar=view.GetComponent<LunarVfx>();if(lunar!=null)lunar.DamageFromSkill=battle.ResolvingSkillHit;
            view.PlayCue(cue,damage,!restoringPresentation);
        }
        void Start()
        {
            Application.targetFrameRate=60;
            Flow=gameObject.AddComponent<CollectionFlow>();
            view.BindCombat(()=>RequestAttack(),()=>RequestSkill(),direction=>RequestDodge(direction),()=>Flow.ResolveBattleResult(),()=>CanControl);
            view.BindRecoveryAction(()=>UseItem(TowerRun.ConsumableSlotIndex));
            Flow.Initialize(this,view,dragons);
        }
        public void EndBattle() { ClearItemProcVisuals();CancelSignature();view.CancelGesture();battle=null; }
        void CancelSignature(){view.GetComponent<MirLegendaryVfx>()?.Cancel();view.GetComponent<HydraVfx>()?.Cancel();battle?.CancelPoisonAcceleration();view.GetComponent<EclipseVfx>()?.Cancel();view.GetComponent<DarkSkillVfx>()?.Clear();battle?.CancelPersistentAttack();view.GetComponent<KrakenVfx>()?.Cancel();view.GetComponent<HaesinVfx>()?.Cancel();battle?.CancelCelestial();view.GetComponent<CelestialVfx>()?.Cancel();battle?.CancelTimeDomain();view.GetComponent<TimeClockworkVfx>()?.Cancel();view.GetComponent<CatastropheVfx>()?.Cancel();view.GetComponent<AscensionVfx>()?.Cancel();battle?.CancelAttackEmpower();view.GetComponent<WaterSkillVfx>()?.Clear();view.GetComponent<LightSkillVfx>()?.Clear();view.GetComponent<LightningSkillVfx>()?.Clear();view.GetComponent<CrushingBeamVfx>()?.Cancel();battle?.CancelChargedBeam();view.GetComponent<GenesisVfx>()?.Cancel();battle?.CancelCompletionDefense();view.GetComponent<EarthSkillVfx>()?.Clear();battle?.CancelTargetModifier();view.GetComponent<VolcanicMeteorVfx>()?.Cancel();battle?.CancelCompletionHeal();view.GetComponent<SolarPierceVfx>()?.Cancel();view.GetComponent<FireSkillVfx>()?.Clear();battle?.CancelSlowSignature();view.GetComponent<LunarVfx>()?.Cancel();battle?.CancelDodgeRelease();view.GetComponent<WispVfx>()?.Cancel();view.GetComponent<IceSkillVfx>()?.Clear();battle?.CancelProtectedSkill();view.GetComponent<DantianVfx>()?.Cancel();view.GetComponent<DantianFeedback>()?.Clear();}
        void OnDisable(){ClearItemProcVisuals();if(view!=null)CancelSignature();}
        public void BeginBattle(DragonData dragon) { BeginBattle(dragon,BattleEnemyStats.Normal(),1,false); }
        public void BeginBattle(DragonData dragon,BattleEnemyStats enemy,int floor,bool boss)
        { BeginBattle(dragon,dragon.Snapshot(),enemy,floor,boss,dragon.maxHP,null); }
        public void BeginBattle(DragonData dragon,BattleStats stats,BattleEnemyStats enemy,int floor,bool boss,int initialHP,UnityEngine.Sprite enemySprite=null,int dragonLevel=1,SkillData activeSkill=null)
        {
            ClearItemProcVisuals();CancelSignature();
            enemy.floor=floor;
            view.CancelGesture();
            view.SetDragonArt(dragon,dragonLevel);
            view.SetSkillPresentation(activeSkill??dragon.SkillAtLevel(dragonLevel));
            view.SetEncounter(enemy,floor,boss,enemySprite);
            battle=RestoreOrCreateBattle(stats,enemy,initialHP);
            var run=Flow==null?null:Flow.CurrentRun;
            view.SetRunProgress(run==null?dragonLevel:run.Level,run==null?0:run.Experience,run==null?100:run.ExperienceToNext);
            view.message.text=battle.AreaHint;
            view.Show(battle);
            view.SetItems(Flow==null?null:Flow.CurrentRun);
            view.restartButton.GetComponentInChildren<UnityEngine.UI.Text>().text="결과 확인";
        }
        void BindRestoredBattle(BattleModel restored)
        {
            battle=restored;
            view.GetComponent<MirLegendaryVfx>()?.BindBattle(battle);view.GetComponent<HydraVfx>()?.BindBattle(battle);view.GetComponent<EclipseVfx>()?.BindBattle(battle);view.GetComponent<KrakenVfx>()?.BindBattle(battle);view.GetComponent<HaesinVfx>()?.BindBattle(battle);view.GetComponent<CrushingBeamVfx>()?.BindBattle(battle);view.GetComponent<GenesisVfx>()?.BindBattle(battle);view.GetComponent<VolcanicMeteorVfx>()?.BindBattle(battle);view.GetComponent<SolarPierceVfx>()?.BindBattle(battle);view.GetComponent<WispVfx>()?.BindBattle(battle);view.GetComponent<LunarVfx>()?.BindBattle(battle);
            var signature=view.GetComponent<DantianVfx>();if(signature!=null)signature.Cancelled=()=>{battle?.CancelProtectedSkill();if(view!=null)view.GetComponent<DantianFeedback>()?.Clear();};
            battle.Cue+=HandleCue;
            battle.Feedback+=text=>view.message.text=text;
            battle.ItemProcVisual+=ShowItemProcVisual;
        }
        void UseItem(int index)
        {
            var run=Flow==null?null:Flow.CurrentRun;var item=run==null?null:run.ItemAt(index);
            if(!CanControl||battle==null||item==null||!battle.UseItem(item))return;
            RecordBattleInput(4,0,item.StableId);run.ConsumeItem(index);Flow.CheckpointBattle();view.SetItems(run);view.Show(battle);
        }
        void Update()
        {
            if(battle==null || !focused) return;
            // Returning from a hidden browser tab must not cause accumulated hits.
            float delta=Mathf.Min(Time.deltaTime,.1f);
            if(battle.Result==BattleResult.Fighting){battle.Tick(delta);RecordBattleInput(0,delta);if(battle.Result!=BattleResult.Fighting)Flow.BattleSettled();}
            Flow.AutoSaveRun();
            view.StepAnimation(battle,delta);StepItemProcVisuals(delta);
            view.Show(battle);
        }
        void OnApplicationFocus(bool value) { focused=value;if(!value){view.CancelGesture();Flow?.CheckpointBattle();} }
        void OnApplicationPause(bool paused) { focused=!paused;if(paused){view.CancelGesture();Flow?.CheckpointBattle();} }
    }
}



