using System;
namespace DragonTower{
 public sealed partial class BattleModel{
  double bigBangPressure;
  public bool BigBangCharging=>BossSkillBusy&&ActiveBossSkill.attackVfx=="contract_big_bang"&&BossPreparation;
  public float BigBangReduction=>(float)(.8*Math.Min(1,bigBangPressure/Math.Max(1,CurrentEnemyMaxHP*.12)));
  public double BigBangPressure=>bigBangPressure;
 }
}
