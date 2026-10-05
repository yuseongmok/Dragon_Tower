using UnityEngine;
namespace DragonTower {
 public enum LightningSample { LightningStrike, ElectricProjectile, LightningArc, HeavyThunderImpact, Paralysis }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Lightning Style")]
 public sealed class LightningVfxStyle:ScriptableObject {
  public Color navy=new Color32(9,19,57,255),blue=new Color32(24,89,244,255),cyan=new Color32(37,218,255,255),pale=new Color32(178,250,255,255),core=new Color32(247,255,255,255),violet=new Color32(112,78,225,255);
  public float pathRefreshFps=24,chargeDuration=.10f,strikeDuration=.095f,residualDuration=.65f;
  public int impactSparks=42,heavySparks=76;
 }
}
