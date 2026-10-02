using System;
namespace DragonTower
{
    public sealed partial class BattleModel
    {
        double dodgeReleaseUntil,signatureVisualUntil;
        public int ScheduledSkillHits{get;private set;}
        public float DodgeReleaseVisualDuration{get;private set;}
        public bool DodgeCooldownReleased=>Result==BattleResult.Fighting&&Time<dodgeReleaseUntil;
        public double DodgeReleaseRemaining=>DodgeCooldownReleased?Math.Max(0,dodgeReleaseUntil-Time):0;
        public double SignatureVisualRemaining=>DodgeCooldownReleased?Math.Max(0,signatureVisualUntil-Time):0;
        void BeginDodgeRelease()
        {
            if(Dragon.skill.dodgeFreeCastDuration<=0)return;
            DodgeReleaseVisualDuration=Dragon.skill.dodgeFreeCastDuration+Math.Max(0,ScheduledSkillHits-Math.Max(1,Dragon.skill.hitCount))*Math.Max(.03f,Dragon.skill.hitInterval);
            signatureVisualUntil=Time+DodgeReleaseVisualDuration;
            dodgeReleaseUntil=signatureVisualUntil+Math.Max(0,Dragon.skill.dodgeFreeAfterDuration);
        }
        public void CancelDodgeRelease()
        {
            if(DodgeCooldownReleased){pendingSkillHits=0;pendingFirstSkillHit=false;}
            dodgeReleaseUntil=signatureVisualUntil=0;
        }
    }
}
