using System;using System.Linq;using System.Collections.Generic;
namespace DragonTower {
 public sealed class RewardCandidate {public AugmentData augment;public SkillData skill;}
 public static class RewardGenerator {
  // Original mixed reward sampler, unchanged RNG order and eligibility rules.
  public static RewardCandidate[] Mixed(TowerRun run,ContentDatabase db,DragonData dragon,int seed,int count) {
            if(db==null)return Array.Empty<RewardCandidate>();
            var augments=(db.augments??Array.Empty<AugmentData>()).Where(a=>a!=null&&run.CanTakeAugment(a)).ToList();
            var skills=(db.skills??Array.Empty<SkillData>()).Where(s=>s!=null&&run.CanOfferSkill(s)&&s.StableId!=run.CurrentSkillId(dragon.SkillAtLevel(run.Level))).ToList();
            var result=new System.Collections.Generic.List<RewardCandidate>();var random=new System.Random(seed);
            // Separate eligibility roll: ordinary rewards retain their existing random stream.
            var signatureRandom=new System.Random(seed^0x5A17);
            skills.RemoveAll(s=>s.EffectiveRewardEligibilityPercent<100&&signatureRandom.NextDouble()*100>=s.EffectiveRewardEligibilityPercent);
            while(result.Count<count&&(augments.Count>0||skills.Count>0))
            {
                // A skill is possible in every slot but never guaranteed. With both pools present,
                // each slot independently has a 25% skill chance.
                bool pickSkill=skills.Count>0&&(augments.Count==0||random.Next(100)<25);
                if(pickSkill)
                {
                    int index=random.Next(skills.Count);result.Add(new RewardCandidate{skill=skills[index]});skills.RemoveAt(index);
                }
                else
                {
                    var augment=ContentDatabase.PickWeightedAugment(augments,random);if(augment==null)break;
                    result.Add(new RewardCandidate{augment=augment});augments.Remove(augment);
                }
            }
            return result.ToArray();
  }
 }
}
