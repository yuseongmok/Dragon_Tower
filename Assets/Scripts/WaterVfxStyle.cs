using UnityEngine;
namespace DragonTower {
 public enum WaterSample { LivingWater,PressureShot,Whirlpool,HeavyWaterImpact }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Water Style")]
 public sealed class WaterVfxStyle:ScriptableObject {
  public Color deep=new Color(.025f,.09f,.23f),azure=new Color(.025f,.35f,.65f),cyan=new Color(.05f,.66f,.83f),aqua=new Color(.23f,.85f,.88f),pale=new Color(.64f,.96f,.98f),foam=new Color(.93f,1,1);
  public float duration=3,shotTravel=.48f,pressureCharge=.38f;
 }
}
