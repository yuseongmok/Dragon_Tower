using System.Collections.Generic;
using UnityEngine;
namespace DragonTower
{
    public enum AugmentGrade { Common, Rare, Epic, Unique }

    [CreateAssetMenu(menuName="Dragon Tower/Augment")]
    public sealed class AugmentData : IdentifiedContent
    {
        [Header("등급 및 등장 확률")]
        public AugmentGrade grade=AugmentGrade.Common;
        [Min(1)] public int maximumStacks=1;
        [Header("전투 발동 규칙")]
        public AugmentMechanic mechanic;
        [Tooltip("효과의 첫 번째 수치입니다. 설명에 따라 피해 %, HP 비용 %, 쿨타임 감소 초 등으로 사용합니다.")]
        public float primaryValue;
        [Tooltip("효과의 두 번째 수치입니다.")]
        public float secondaryValue;
        [Tooltip("발동 확률(%)입니다.")][Range(0,100)] public float chancePercent;
        [Tooltip("지속시간(초)입니다.")][Min(0)] public float duration;
        [Tooltip("몇 회마다 발동하는지 지정합니다.")][Min(1)] public int triggerCount=1;
        public List<ContentEffect> effects=new List<ContentEffect>();
        public BattleAugment Snapshot()=>new BattleAugment{mechanic=mechanic,primaryValue=primaryValue,secondaryValue=secondaryValue,chancePercent=chancePercent,duration=duration,triggerCount=triggerCount};
    }
}
