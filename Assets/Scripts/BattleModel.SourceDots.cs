using System;using System.Collections.Generic;
namespace DragonTower {
 public sealed partial class BattleModel {
  sealed class SourceDot {public string id;public double damage,period,until,next,initialCredit;}
  readonly List<SourceDot> poisonSources=new List<SourceDot>(),burnSources=new List<SourceDot>();
  double sourceDotTime,poisonFraction,burnFraction;
  bool tickingSourceDots;
  bool SourceActive(bool poison){foreach(var s in poison?poisonSources:burnSources)if(s.until>Time)return true;return false;}
  double SourceRemaining(bool poison){double end=Time;foreach(var s in poison?poisonSources:burnSources)end=Math.Max(end,s.until);return end-Time;}
  double SourcePeriod(SourceDot s,bool poison,double at)=>Math.Max(.25,s.period*DotFactor*(poison&&poisonTempoUntil>at?poisonTempoFactor:1));
  SourceDot StrongestSource(bool poison,double at){SourceDot best=null;double rate=-1;foreach(var s in poison?poisonSources:burnSources){if(s.until<=at+1e-9)continue;double candidate=s.damage/SourcePeriod(s,poison,at);if(candidate>rate+1e-9||(Math.Abs(candidate-rate)<1e-9&&best!=null&&string.CompareOrdinal(s.id,best.id)<0)){best=s;rate=candidate;}}return best;}
  double CurrentSourcePoisonPeriod{get{var s=StrongestSource(true,Time);return s==null?2:SourcePeriod(s,true,Time);}}
  int CurrentSourcePoisonDamage{get{var s=StrongestSource(true,Time);return s==null?0:(int)Math.Round(s.damage*(1+Param(AugmentMechanic.ToxicStudy)/100.0),MidpointRounding.AwayFromZero);}}
  void SetSourceDot(bool poison,string id,double damage,double period,double duration){
   if(!NewPassives||damage<=0||duration<=0)return;TickSourceDots();var list=poison?poisonSources:burnSources;SourceDot found=null;
   foreach(var s in list)if(s.id==id){found=s;break;}
   if(found==null){found=new SourceDot{id=id};list.Add(found);}
   bool wasActive=found.until>Time;found.damage=damage;found.period=period;found.until=Time+duration;
   if(!wasActive){double step=SourcePeriod(found,poison,Time);double delay=poison&&!id.StartsWith("passive:",StringComparison.Ordinal)?Math.Min(step,step/period):step;found.next=Time+delay;found.initialCredit=damage*(1-delay/step);}
  }
  void RescalePoisonSources(float oldFactor,float nextFactor){foreach(var s in poisonSources)if(s.until>Time)s.next=Time+Math.Max(0,s.next-Time)*nextFactor/oldFactor;}
  void TickSourceDots(){if(!NewPassives||tickingSourceDots)return;tickingSourceDots=true;try{
   double cursor=sourceDotTime;while(cursor<Time-1e-9&&Result==BattleResult.Fighting){
    var p=StrongestSource(true,cursor);var b=StrongestSource(false,cursor);double end=Time;
    foreach(var s in poisonSources)if(s.until>cursor+1e-9){end=Math.Min(end,s.until);if(s.next>cursor+1e-9)end=Math.Min(end,s.next);}
    foreach(var s in burnSources)if(s.until>cursor+1e-9){end=Math.Min(end,s.until);if(s.next>cursor+1e-9)end=Math.Min(end,s.next);}
    if(poisonTempoUntil>cursor+1e-9)end=Math.Min(end,poisonTempoUntil);
    double dt=end-cursor,bonus=Math.Max(0,1+Param(AugmentMechanic.ToxicStudy)/100.0);
    if(p!=null)poisonFraction+=dt*p.damage/SourcePeriod(p,true,cursor)*bonus;
    if(b!=null)burnFraction+=dt*b.damage/SourcePeriod(b,false,cursor)*bonus;
    if(p!=null&&(p.next<=end+1e-8||p.until<=end+1e-8))PaySourceDot(true,p,end);
    if(b!=null&&(b.next<=end+1e-8||b.until<=end+1e-8))PaySourceDot(false,b,end);
    foreach(var s in poisonSources)if(s.next<=end+1e-8&&s.until>=end-1e-8){s.next=end+SourcePeriod(s,true,cursor);s.initialCredit=0;}
    foreach(var s in burnSources)if(s.next<=end+1e-8&&s.until>=end-1e-8)s.next=end+SourcePeriod(s,false,end);
    if(poisonTempoUntil>cursor+1e-9&&poisonTempoUntil<=end+1e-8){foreach(var s in poisonSources)if(s.until>end)s.next=end+Math.Max(0,s.next-end)/poisonTempoFactor;poisonTempoUntil=0;poisonTempoFactor=1;}
    cursor=end;
   }sourceDotTime=Time;
  }finally{tickingSourceDots=false;}}
  void PaySourceDot(bool poison,SourceDot source,double at){double extra=source.next<=at+1e-8?source.initialCredit*Math.Max(0,1+Param(AugmentMechanic.ToxicStudy)/100.0):0;if(poison)poisonFraction+=extra;else burnFraction+=extra;source.initialCredit=0;double value=poison?poisonFraction:burnFraction;int amount=(int)Math.Floor(value+1e-6);if(poison){poisonFraction-=amount;PoisonTickCount++;LastPoisonTickTime=at;}else burnFraction-=amount;
   int hit=DamageEnemy(amount);if(source.id.StartsWith("passive:",StringComparison.Ordinal))PassiveDamage+=hit;
   if(hit>0){Cue?.Invoke(CombatCue.SkillHit,hit);Feedback?.Invoke((poison?"중독":"화상")+" 피해 −"+hit);}if(EnemyHP==0)Result=BattleResult.Victory;
  }
 }
}
