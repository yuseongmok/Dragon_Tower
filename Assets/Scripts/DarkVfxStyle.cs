using UnityEngine;
namespace DragonTower {
 public enum DarkSample { DarkEnergy,CosmicCompression,ToxicCorruption,HeavyDarkImpact,PoisonStatus }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Dark Style")]
 public sealed class DarkVfxStyle:ScriptableObject {
  public Color core=new Color(.018f,.012f,.04f),navy=new Color(.035f,.025f,.13f),blackPurple=new Color(.12f,.025f,.22f),violet=new Color(.32f,.06f,.57f),purple=new Color(.62f,.16f,.91f),bright=new Color(.88f,.52f,1),pale=new Color(.98f,.90f,1),gold=new Color(1,.68f,.24f),toxic=new Color(.30f,.74f,.05f),lime=new Color(.66f,1,.08f),poisonPale=new Color(.89f,1,.55f);
  public float duration=3.4f,cosmicRelease=1.25f,toxicRelease=1.1f,heavyRelease=.85f;
 }
}
