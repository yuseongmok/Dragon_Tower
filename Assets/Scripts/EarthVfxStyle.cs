using UnityEngine;
namespace DragonTower {
 public enum EarthModule { Rock,GroundSlab,Crack,DustPuff,DustCloud,DustWave,Impact,Pressure,Grain,Shadow }
 public enum EarthSample { GroundImpact,GroundRise,SeismicWave,HeavyFinalImpact }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Earth Style")]
 public sealed class EarthVfxStyle:ScriptableObject {
  public Color shadow=new Color32(35,27,29,255),dark=new Color32(72,48,38,255),earth=new Color32(121,80,49,255),warm=new Color32(170,118,65,255),sand=new Color32(212,175,112,255),edge=new Color32(244,224,173,255),amber=new Color32(207,153,60,255);
  public Sprite[] frames;public float rotationFps=12;
  [Range(12,100)]public int heavyDebris=64;
  public Sprite Frame(EarthModule module,float fraction)=>frames[(int)module*8+Mathf.Clamp((int)(fraction*8),0,7)];
  public Sprite RotatingRock(float seconds)=>frames[Mathf.FloorToInt(Mathf.Repeat(seconds*rotationFps,8))];
 }
}
