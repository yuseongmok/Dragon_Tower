using UnityEngine;
namespace DragonTower {
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Kraken")]
 public sealed class KrakenProfile:ScriptableObject {public SkillData skill;public WaterVfxStyle style;public Color shadow=new Color(.012f,.028f,.09f),body=new Color(.018f,.23f,.33f),face=new Color(.045f,.49f,.58f),cups=new Color(.53f,.27f,.59f),cupLight=new Color(.88f,.57f,.76f);}
}
