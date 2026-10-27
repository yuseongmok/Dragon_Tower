using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  int destructionBookHP,bindingBookHP,bookPendingMask,realIllusionShots;double bookSlowUntil;float bookSlowPercent;
  public bool LibraryBossBattle=>Dragon.combatRulesVersion>=11&&!Dragon.legacyBalance&&Enemy.bossPattern==EnemyBossPattern.AzazelSeals&&HasBossSkills;
  public bool BookChoiceActive=>LibraryBossBattle&&BossSkillBusy&&BossState==BossSkillState.BookChoice;
  public int SelectedBossBook {get;private set;}
  public int BossBookSpell {get;private set;}=-1;
  public int BossBrokenBook {get;private set;}=-1;
  public bool BossIllusionFake {get;private set;}
  public int BossBookHP(int index)=>index==0?destructionBookHP:index==1?bindingBookHP:0;
  public double BossBookHitAt {get;private set;}=-100;
  public int BossLastHitBook {get;private set;}
  public int BossBookHitDamage {get;private set;}
  public bool BookBindingActive=>LibraryBossBattle&&Result==BattleResult.Fighting&&Time<bookSlowUntil;
  public double BookBindingRemaining=>BookBindingActive?bookSlowUntil-Time:0;
  public bool SelectBossBook(int index){if(!BookChoiceActive||index<0||index>1||BossBookHP(index)<=0)return false;SelectedBossBook=index;return true;}
  int DamageSelectedBook(int damage){
   int actual=Math.Min(Math.Max(0,damage),BossBookHP(SelectedBossBook));if(actual==0)return 0;
   if(SelectedBossBook==0)destructionBookHP-=actual;else bindingBookHP-=actual;
   BossBookHitAt=Time;BossLastHitBook=SelectedBossBook;BossBookHitDamage=actual;
   if(BossBookHP(SelectedBossBook)==0){BossBrokenBook=SelectedBossBook;Feedback?.Invoke((SelectedBossBook==0?"파괴":"속박")+"의 서 봉인!");FinishBookChoice();}
   return actual; // Excess damage never spills into the other book or boss on this hit.
  }
  void StartBookChoice(BossSkillDefinition skill){destructionBookHP=bindingBookHP=skill.bookDurability;SelectedBossBook=0;BossBookSpell=-1;BossBrokenBook=-1;bookPendingMask=0;BossPhase(BossSkillState.BookChoice,skill.bookWindow);}
  void FinishBookChoice(){bookPendingMask=(destructionBookHP>0?1:0)|(bindingBookHP>0?2:0);BeginBookSpell();}
  void BeginBookSpell(){
   var s=ActiveBossSkill;if(bookPendingMask==0){BossPhase(BossSkillState.Recovery,s.recovery);return;}
   BossBookSpell=(bookPendingMask&1)!=0?0:1;bookPendingMask&=~(1<<BossBookSpell);bossStunDelay=0;
   double warning=s.bookSpellWarning*(1+Math.Min(.25,EnemySlowPower/100.0));
   if(BossBookSpell==0&&!DodgeCooldownReleased)warning=Math.Max(warning,DodgeReady+.25-Time-s.damageTime);
   BossPhase(BossSkillState.Preparation,warning);
  }
  void ChooseIllusion(BossSkillDefinition s){if(s.illusionShots<=0)return;bool real=random()>=.5;if(BossShotNumber==s.illusionShots&&realIllusionShots==0)real=true;BossIllusionFake=!real;if(real)realIllusionShots++;}
 }
}
