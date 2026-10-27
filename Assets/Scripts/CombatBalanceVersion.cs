namespace DragonTower {
 // Frozen compatibility data: do not retune. Old in-progress encounters finish
 // with their original rules; a newly created encounter always uses Current.
 public static class CombatBalanceVersion {
  public const int Current=18;
  public static SkillStats LegacySkill(SkillStats source){
   var s=source.Copy();switch(s.skillId){
    case "skill_apocalypse":s.damage=20;s.cooldown=11;break;
    case "skill_ascension":s.attackEmpowerDamage=48;break;
    case "skill_big_bang":s.damage=18;s.cooldown=10.5f;break;
    case "skill_crushing_beam":s.finalHitDamageMultiplier=10;s.damage=80;break;
    case "skill_earthquake":s.damage=32;s.cooldown=9;break;
    case "skill_falling_flower":s.damage=12;break;
    case "skill_gale_slash":s.damage=55;break;
    case "skill_holy_explosion":s.damage=30;s.cooldown=8;break;
    case "skill_ice_slam":s.damage=32;break;
    case "skill_ice_slash":s.damage=18;break;
    case "skill_meteor":s.damage=28;s.cooldown=8;break;
    case "skill_moon_combo":s.damage=15;s.cooldown=10;break;
    case "skill_purgatory":s.damage=18;s.cooldown=11;break;
    case "skill_shockwave":s.damage=22;s.cooldown=8;break;
    case "skill_snipe":s.damage=62;s.cooldown=6.2f;break;
    case "skill_space_rift":s.damage=22;s.cooldown=8;break;
    case "skill_splash":s.damage=14;s.cooldown=8.5f;break;
    case "skill_storm_slash":s.damage=14;s.cooldown=8.5f;break;
    case "skill_thunder_strike":s.damage=24;s.cooldown=9;break;
    case "skill_wind_claw":s.damage=42;break;
    case "skill_frost_Icicle":s.damage=52;break;
   }return s;
  }
 }
}
