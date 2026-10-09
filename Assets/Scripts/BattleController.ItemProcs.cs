using System.Collections.Generic;
using UnityEngine;
namespace DragonTower {
 public partial class BattleController {
  ItemProcVfx itemProcPainter;float itemProcEndElapsed;
  readonly List<GameObject> itemProcVisuals=new List<GameObject>();
  void ShowItemProcVisual(ItemProcVisual cue){
   if(cue.started&&!restoringPresentation)view.FlashItem(cue.slot);
   if(cue.effect.art!=ItemProcArt.None){if(itemProcPainter==null){itemProcPainter=view.gameObject.AddComponent<ItemProcVfx>();itemProcPainter.Initialize(view);}itemProcPainter.Cue(cue,battle.Time);}
   if(cue.started)return;
   if(!cue.started)view.PlayCue(CombatCue.AugmentHit,cue.damage,!restoringPresentation);
   if(cue.effect.vfxPrefab==null||restoringPresentation)return;
   itemProcVisuals.RemoveAll(x=>x==null);
   Vector2 point;
   if(cue.effect.vfxOrigin==ItemProcVfxOrigin.Enemy)point=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.position);
   else view.TryGetCharacterAnchor(cue.effect.vfxOrigin==ItemProcVfxOrigin.Character?DragonVisualAnchor.CharacterCenter:DragonVisualAnchor.SkillOrigin,view.frame,out point);
   var visual=Instantiate(cue.effect.vfxPrefab,view.frame,false);visual.transform.position=view.frame.TransformPoint(point);
   foreach(var graphic in visual.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))graphic.raycastTarget=false;
   
   itemProcVisuals.Add(visual);Destroy(visual,Mathf.Max(.05f,cue.effect.vfxLifetime));
  }
  void StepItemProcVisuals(float dt){itemProcEndElapsed=battle.Result==BattleResult.Fighting?0:itemProcEndElapsed+dt;if(itemProcPainter!=null){itemProcPainter.CancelMissing(battle.Dragon.items);itemProcPainter.Step(battle.Time+itemProcEndElapsed);}view.ShowItemCooldowns(battle,dt);}
  void ClearItemProcVisuals(){itemProcEndElapsed=0;if(itemProcPainter!=null)itemProcPainter.Clear();foreach(var visual in itemProcVisuals)if(visual!=null)Destroy(visual);itemProcVisuals.Clear();}
 }
}
