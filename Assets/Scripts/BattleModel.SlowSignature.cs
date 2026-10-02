using System;
namespace DragonTower
{
    public sealed partial class BattleModel
    {
        bool slowSignatureCast,pendingSlowBonus;
        double slowBonusAt;int slowBonusBase;
        public bool SlowSynergyAtCast{get;private set;}
        public bool SlowBonusPending=>pendingSlowBonus&&Result==BattleResult.Fighting;
        void BeginSlowSignature()
        {
            pendingSlowBonus=false;slowSignatureCast=Dragon.skill.slowBonusDamagePercent>0;
            SlowSynergyAtCast=slowSignatureCast&&EnemySlowed;
        }
        void QueueSlowSignatureBonus(int baseDamage)
        {
            if(!SlowSynergyAtCast||Result!=BattleResult.Fighting)return;
            slowBonusBase=Math.Max(1,(int)Math.Round(baseDamage*Dragon.skill.slowBonusDamagePercent/100f));
            slowBonusAt=Time+Math.Max(.03f,Dragon.skill.slowBonusDelay);pendingSlowBonus=true;
        }
        void TickSlowSignature()
        {
            if(!pendingSlowBonus||Time<slowBonusAt||Result!=BattleResult.Fighting)return;
            pendingSlowBonus=false;bool critical;int damage=Hit(slowBonusBase,Dragon.skill.elementType,true,out critical);
            ResolvingSkillHit=true;try{Cue?.Invoke(CombatCue.SkillBonusHit,damage);}finally{ResolvingSkillHit=false;}
            Feedback?.Invoke("달빛 파쇄 · −"+damage);
        }
        public void CancelSlowSignature()
        {
            if(slowSignatureCast){pendingSkillHits=0;pendingFirstSkillHit=false;}
            slowSignatureCast=pendingSlowBonus=SlowSynergyAtCast=false;
        }
    }
}
