using System;
namespace DragonTower {
 // Pure, fractional design values. Production starts at zero; no currency or upgrade action.
 public static class PassiveImprintRules {
  public static int Clamp(int level)=>Math.Max(0,Math.Min(10,level));
  public static float Value(int level,float start,float end)=>start+(end-start)*Clamp(level)/10f;
  public static float Chance(DragonPassiveMechanic kind,int level){switch(kind){
   case DragonPassiveMechanic.TimeRift:case DragonPassiveMechanic.RuyiOrb:case DragonPassiveMechanic.IronArmor:case DragonPassiveMechanic.Phoenix:return Value(level,5,30);
   case DragonPassiveMechanic.FreezingGaze:case DragonPassiveMechanic.FlameBreath:case DragonPassiveMechanic.ElectricShock:return Value(level,5,40);
   case DragonPassiveMechanic.StormAffinity:return Value(level,0,40);
   case DragonPassiveMechanic.Tentacle:return Value(level,20,50);default:return 0;}}
  public static float Duration(DragonPassiveMechanic kind,int level){switch(kind){
   case DragonPassiveMechanic.TimeRift:return Value(level,.5f,1.5f);
   case DragonPassiveMechanic.FreezingGaze:return Value(level,1,4);
   case DragonPassiveMechanic.FlameBreath:return Value(level,1,3);
   case DragonPassiveMechanic.Tentacle:return Value(level,2,5);
   case DragonPassiveMechanic.Mirage:return Value(level,3,10);
   case DragonPassiveMechanic.StormAffinity:return 2;case DragonPassiveMechanic.ElectricShock:return 1;case DragonPassiveMechanic.IronArmor:return 5;default:return 0;}}
  public static float CooldownReduction(int level)=>Value(level,5,30);
  public static float OvercastCost(int level)=>Value(level,20,5);
  public static float DotPercent(int level)=>Value(level,5,10);
  public static float PoisonPeriod(int level)=>Value(level,2,1);
  public static float TitanBonus(int level,float hp,float baseline)=>baseline<=0?0:Math.Min(30,Math.Max(0,hp/baseline-1)*10*Value(level,1,3));
  public static string Description(DragonPassiveMechanic kind){switch(kind){
   case DragonPassiveMechanic.TimeRift:return "공격 시 5% 확률로 0.5초 기절. 각인 최대: 30%, 1.5초. 보스 연속 기절 보호 적용.";
   case DragonPassiveMechanic.JetStream:return "스킬 시전당 한 번, 남은 일반공격 대기시간 15% 감소. 각인 최대 40%.";
   case DragonPassiveMechanic.VoidAccelerator:return "쿨타임 중 최대 HP 20%를 소비해 재시전. 각인 최대 5%. 진행 중 시전 보호 유지.";
   case DragonPassiveMechanic.FreezingGaze:return "공격 시 5% 확률로 1초간 적 공격 간격 20% 증가. 각인 최대: 40%, 4초.";
   case DragonPassiveMechanic.RuyiOrb:return "일반공격 시 5% 확률로 원본 피해의 30% 추가타. 각인 최대 확률 30%. 추가타 연쇄 없음.";
   case DragonPassiveMechanic.HydraVenom:return "상시 중독: 2초마다 기준 공격력 5%. 각인 최대: 1초마다 10%. 가장 강한 독 DPS만 적용.";
   case DragonPassiveMechanic.HardenedShell:return "진화 기본 HP보다 증가한 최대 HP 10%당 스킬 피해 +1%. 각인 최대 +3%, 총 상한 +30%.";
   case DragonPassiveMechanic.StormAffinity:return "바람 스킬도 사용 가능. 각인당 바람·번개 직접 스킬 마비 확률 +4%p, 최대 40%, 2초 지속.";
   case DragonPassiveMechanic.FrozenBlade:return "얼음 스킬도 사용 가능. 각인당 전기 약점 추가 피해 5% 감소, 최대 50%. 기본 피해 유지.";
   case DragonPassiveMechanic.Tentacle:return "스킬 시전 시 20% 확률로 촉수 2초 소환, 초당 기준 공격력 20%. 각인 최대: 50%, 5초. 1개만 유지.";
   case DragonPassiveMechanic.IronArmor:return "스킬 시전 시 5% 확률로 최대 HP 10% 보호막을 5초 생성. 자기 보호막 중 직접 피해 +10%. 각인 최대 확률 30%.";
   case DragonPassiveMechanic.Phoenix:return "스킬 시전 시 5% 확률로 최대 HP 10% 회복. 각인 최대: 30% 확률, HP 30% 회복. 자동 부활 없음.";
   case DragonPassiveMechanic.Mirage:return "실제 회피 성공 시 공격력 +2%, 최대 5스택. 3초 유지, 회피 실패 시 초기화. 각인 최대 유지 10초.";
   case DragonPassiveMechanic.HolyLight:return "스킬·자동 아이템 재사용 시간 5% 감소. 각인 최대 30%. 감소 합계 상한 60%, 환급과 별도.";
   case DragonPassiveMechanic.FlameBreath:return "공격 시 5% 확률로 1초 화상, 초당 기준 공격력 5%. 각인 최대: 40%, 3초, 초당 10%.";
   case DragonPassiveMechanic.ElectricShock:return "일반공격 시 5% 확률로 1초 마비. 각인 최대 확률 40%. 마비 중 적 공격 실패 확률 20%.";
   default:return "";}}
 }
}
