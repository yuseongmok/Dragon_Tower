using System;
using System.Linq;
using UnityEngine;
namespace DragonTower {
 public partial class BattleController {
  void RecordBattleInput(int action,float delta=0,string itemId=null){var tape=Flow?.CurrentRun?.BattleResume;if(tape!=null)tape.inputs.Add(new BattleInputRecord{action=action,delta=delta,itemId=itemId});}
  bool restoringPresentation;
  BattleModel RestoreOrCreateBattle(BattleStats stats,BattleEnemyStats enemy,int hp){
   var tape=Flow?.CurrentRun?.BattleResume;
   if(tape==null){var fresh=new BattleModel(stats,enemy,hp);fresh.RestoreItemCooldowns(Flow?.CurrentRun?.ItemCooldowns);BindRestoredBattle(fresh);return fresh;}
   restoringPresentation=true;
   try{return BattleReplay.Restore(tape,ContentDatabase.Load(),stats.displayName,BindRestoredBattle,dt=>{view.StepAnimation(battle,dt);StepItemProcVisuals(dt);});}
   finally{restoringPresentation=false;}
  }
 }
 public static class BattleReplay {public static BattleModel Restore(BattleResumeData tape,ContentDatabase db,string displayName,Action<BattleModel> preparePresentation=null,Action<float> stepPresentation=null){
   var rng=new System.Random(tape.seed);
   var saved=JsonUtility.FromJson<TowerRunSave>(tape.initialRunJson);var initial=TowerRun.Restore(saved,db,true);var dragon=db.dragons.First(x=>x.StableId==saved.speciesId);
   var initialStats=initial.BuildBattleStats(dragon.Snapshot(initial.Level));initialStats.displayName=displayName;
   var model=new BattleModel(initialStats,tape.enemy,initial.CurrentHP,()=>rng.NextDouble());model.RestoreItemCooldowns(initial.ItemCooldowns);
   // Rebuild recent visual events without replaying sound or awarding rewards.
   // Thirty seconds covers existing persistent signatures and their residual layers.
   double start=Math.Max(0,tape.inputs.Where(x=>x.action==0).Sum(x=>(double)x.delta)-30);bool prepared=false;
   foreach(var input in tape.inputs){if(!prepared&&model.Time>=start){preparePresentation?.Invoke(model);prepared=true;}switch(input.action){case 0:model.Tick(input.delta);if(prepared)stepPresentation?.Invoke(input.delta);break;case 1:model.Attack();break;case 2:model.Skill();break;case 3:model.Dodge();break;case 4:var item=db.items.First(x=>x.StableId==input.itemId);model.UseItem(item.AsSavedConsumable());break;}}
   if(!prepared)preparePresentation?.Invoke(model);
   return model;
 }}
}
