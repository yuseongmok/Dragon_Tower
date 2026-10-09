using UnityEngine;
namespace DragonTower {
 [CreateAssetMenu(menuName="Dragon Tower/Floor Progression")]
 public sealed class FloorProgressionConfig:ScriptableObject {
  [Range(0,100)] public float augmentBattleChance=40,goldBattleChance=50,itemBattleChance=35,recoveryBattleChance=35,nestBattleChance=35,shopBattleChance=35;
  [Min(0)] public int baseBattleExperience=100;
  [Min(0)] public int normalKillGold=10,bossKillGold=20;
  [Range(0,100)] public float monsterShopChance=10;
  [Range(0,100)] public float augmentExperiencePercent=15,goldExperiencePercent=10;
  public int[] experienceMultipliers={50,75,100,125,150};
  public int[] experienceWeights={15,25,35,20,5};
  public float BattleChance(TowerRoomKind room){switch(room){case TowerRoomKind.Augment:return augmentBattleChance;case TowerRoomKind.Gold:return goldBattleChance;case TowerRoomKind.Item:return itemBattleChance;case TowerRoomKind.Recovery:return recoveryBattleChance;case TowerRoomKind.Nest:return nestBattleChance;case TowerRoomKind.Shop:return shopBattleChance;default:return 0;}}
  public int RoomExperience(TowerRoomKind room)=>Mathf.RoundToInt(baseBattleExperience*(room==TowerRoomKind.Augment?augmentExperiencePercent:room==TowerRoomKind.Gold?goldExperiencePercent:0)/100f);
  public int PickExperienceMultiplier(int roll){int sum=0;for(int i=0;i<experienceWeights.Length;i++){sum+=experienceWeights[i];if(roll<sum)return experienceMultipliers[i];}return 100;}
  public static FloorProgressionConfig Load()=>Resources.Load<FloorProgressionConfig>("FloorProgression")??CreateInstance<FloorProgressionConfig>();
 }
}
