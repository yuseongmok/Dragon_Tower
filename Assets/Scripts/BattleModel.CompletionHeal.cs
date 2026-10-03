using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  int completionHits; bool completionCast,completionQueued; double completionRemaining;
  public bool CompletionHealPending=>completionQueued;
  public event Action<int> Healed;
  public int HealPlayer(int amount){if(PlayerHP<=0||Result==BattleResult.Defeat)return 0;int before=PlayerHP;PlayerHP=Math.Min(Dragon.maxHP,PlayerHP+Math.Max(0,amount));int actual=PlayerHP-before;if(actual>0)Healed?.Invoke(actual);return actual;}
  void BeginCompletionHeal(){completionHits=0;completionQueued=false;completionCast=Dragon.skill.completionHealPercent>0;}
  void NotifyCompletionHit(int damage){if(!completionCast)return;completionHits++;if(completionHits==ScheduledSkillHits&&damage>0&&Result!=BattleResult.Defeat){completionQueued=true;completionRemaining=Math.Max(0,Dragon.skill.completionHealDelay);completionCast=false;}else if(Result!=BattleResult.Fighting){completionCast=false;completionQueued=false;}}
  void TickCompletionHeal(double delta){if(!completionQueued)return;if(Result==BattleResult.Defeat||PlayerHP<=0){completionQueued=false;return;}completionRemaining-=delta;if(completionRemaining>0.000001)return;completionQueued=false;int amount=HealPlayer(Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*Math.Min(100,Dragon.skill.completionHealPercent)/100f)));Cue?.Invoke(CombatCue.SkillHeal,amount);if(amount>0)Feedback?.Invoke("태양의 재생 · HP +"+amount);}
  public void CancelCompletionHeal(){if(completionCast||completionQueued){pendingSkillHits=0;pendingFirstSkillHit=false;}completionCast=completionQueued=false;}
 }
}
