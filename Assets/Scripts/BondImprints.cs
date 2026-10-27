using System;
using System.Collections.Generic;
using UnityEngine;
namespace DragonTower {
 [Serializable] public sealed class DragonImprint {
  public string speciesId;
  public int[] levels=new int[4], failures=new int[4];
  public int redistributions;
  public int Total { get {int n=0;foreach(int v in levels)n+=v;return n;} }
  public DragonImprint Copy()=>JsonUtility.FromJson<DragonImprint>(JsonUtility.ToJson(this));
  public void Validate(){if(levels==null||levels.Length!=4||failures==null||failures.Length!=4||redistributions<0)throw new InvalidOperationException("Invalid imprint");for(int i=0;i<4;i++)if(levels[i]<0||levels[i]>10||failures[i]<0||failures[i]>5)throw new InvalidOperationException("Invalid imprint level");if(Total>10)throw new InvalidOperationException("Imprint budget exceeded");}
 }
 [Serializable] public sealed class BondKillReward {public int floor;public bool boss;}
 [Serializable] public sealed class BondRewardReceipt { public string runId; public int floor; }
 [Serializable] public sealed class BondEconomy {
  public int[] costs, successPercent, normalRewards, bossRewards;
  public int rerollCost=120;
  public int redistributionCost,pityStart,pityStep,guaranteedFailures;
  static BondEconomy cached;
  public static BondEconomy Load(){if(cached==null){var t=Resources.Load<TextAsset>("BondEconomy");if(t==null)throw new InvalidOperationException("각인 설정을 찾을 수 없습니다.");cached=JsonUtility.FromJson<BondEconomy>(t.text);cached.Validate();}return cached;}
  public void Validate(){if(costs==null||costs.Length!=10||successPercent==null||successPercent.Length!=10||normalRewards==null||normalRewards.Length!=8||bossRewards==null||bossRewards.Length!=8||rerollCost<1||redistributionCost<0||pityStart<1||pityStep<0||guaranteedFailures!=5)throw new InvalidOperationException("Invalid bond economy");foreach(int x in costs)if(x<0)throw new InvalidOperationException();foreach(int x in successPercent)if(x<1||x>100)throw new InvalidOperationException();foreach(int x in normalRewards)if(x<0)throw new InvalidOperationException();foreach(int x in bossRewards)if(x<0)throw new InvalidOperationException();}
  public int Chance(int level,int failures)=>level>=10?0:failures>=guaranteedFailures?100:Math.Min(100,successPercent[level]+Math.Max(0,failures-pityStart+1)*pityStep);
  public int Reward(int floor,bool boss)=>floor<1||floor>100?0:(boss?bossRewards:normalRewards)[Math.Min(7,(floor-1)/10)];
 }
 public sealed partial class CollectionSession {
  public event Action<int> BondPointsEarned;
  public DragonImprint GetImprint(string speciesId){var x=Profile.imprints?.Find(d=>d.speciesId==speciesId);return x==null?new DragonImprint{speciesId=speciesId}:x.Copy();}
  public int GetImprintLevel(string speciesId,int ability)=>GetImprint(speciesId).levels[ability];
  static DragonImprint MutableImprint(PlayerProfile p,string id){if(p.imprints==null)p.imprints=new List<DragonImprint>();var x=p.imprints.Find(d=>d.speciesId==id);if(x==null){x=new DragonImprint{speciesId=id};p.imprints.Add(x);}return x;}
  // All currency consumers share a revision check. A stale/double-submitted command cannot spend again.
  public bool TrySpendBondPoints(int amount,long expectedRevision){if(amount<=0||expectedRevision!=Profile.bondRevision||Profile.bondPoints<amount)return false;var n=ProfileStore.Clone(Profile);n.bondPoints-=amount;n.bondRevision++;store.Save(n);Profile=n;return true;}
  public bool TryUpgradeImprint(string speciesId,int ability,long expectedRevision,out bool success,Func<double> random=null){
   success=false;if(!Owns(speciesId)||ability<0||ability>3||expectedRevision!=Profile.bondRevision)return false;
   var n=ProfileStore.Clone(Profile);var x=MutableImprint(n,speciesId);var c=BondEconomy.Load();int level=x.levels[ability];if(level>=10||x.Total>=10||n.bondPoints<c.costs[level])return false;
   double roll=random==null?UnityEngine.Random.value:random();if(double.IsNaN(roll)||roll<0||roll>1)throw new ArgumentOutOfRangeException(nameof(random));
   success=roll*100<c.Chance(level,x.failures[ability])||c.Chance(level,x.failures[ability])==100;
   n.bondPoints-=c.costs[level];n.bondRevision++;if(success){x.levels[ability]++;x.failures[ability]=0;}else x.failures[ability]=Math.Min(5,x.failures[ability]+1);
   store.Save(n);Profile=n;return true;
  }
  public bool TryRedistributeImprint(string speciesId,int[] allocation,long expectedRevision){
   if(!Owns(speciesId)||allocation==null||allocation.Length!=4||expectedRevision!=Profile.bondRevision)return false;
   var n=ProfileStore.Clone(Profile);var x=MutableImprint(n,speciesId);int sum=0;bool changed=false;for(int i=0;i<4;i++){if(allocation[i]<0||allocation[i]>10)return false;sum+=allocation[i];changed|=allocation[i]!=x.levels[i];}
   int cost=BondEconomy.Load().redistributionCost;if(!changed||sum!=x.Total||sum>10||n.bondPoints<cost)return false;
   n.bondPoints-=cost;n.bondRevision++;x.levels=(int[])allocation.Clone();x.failures=new int[4];x.redistributions++;store.Save(n);Profile=n;return true;
  }
  int ApplyBondReward(PlayerProfile next,TowerRunSave snapshot){
   if(snapshot.bondKills==null||snapshot.bondKills.Count==0)return 0;
   if(next.bondReceipts==null)next.bondReceipts=new List<BondRewardReceipt>();
   string id=snapshot.progress.runId;var receipt=next.bondReceipts.Find(x=>x.runId==id);int highest=receipt?.floor??0,total=0;
   foreach(var kill in snapshot.bondKills){if(kill.floor<=highest)continue;int amount=BondEconomy.Load().Reward(kill.floor,kill.boss);if(amount<=0)continue;total=checked(total+amount);highest=kill.floor;}
   if(total==0)return 0;next.bondPoints=checked(next.bondPoints+total);next.bondRevision++;
   if(receipt==null)next.bondReceipts.Add(new BondRewardReceipt{runId=id,floor=highest});else receipt.floor=highest;return total;
  }
 }
 public sealed partial class TowerRun {
  public DragonImprint Imprint {get;private set;}=new DragonImprint();
  public List<BondKillReward> BondKills {get;private set;}=new List<BondKillReward>();
  public void ApplyImprint(DragonImprint value){if(BattleResume!=null)return;value.Validate();Imprint=value.Copy();RecalculateMaxHP();}
 }
}
