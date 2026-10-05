using UnityEngine;
namespace DragonTower {
 public enum LightSample { MagicCircle,PerspectiveCircles,LightBeam,HolyImpact }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Light Style")]
 public sealed class LightVfxStyle:ScriptableObject {
  public Color deep=new Color(.24f,.11f,.035f),gold=new Color(.86f,.55f,.13f),pale=new Color(1,.85f,.48f),ivory=new Color(1,.96f,.79f),core=Color.white;
  public Color lavender=new Color(.77f,.70f,1),pink=new Color(1,.77f,.85f),cyan=new Color(.65f,.95f,1);
  public float charge=.72f,beamDuration=.44f,afterglow=.85f;
 }
}
