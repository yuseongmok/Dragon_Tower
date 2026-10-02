using UnityEngine;
namespace DragonTower
{
    public enum IceStyleModule { Interior,Body,Edge,Highlight,FrostA,FrostB,CrackA,CrackB,CrackC,ShardLarge,ShardMedium,ShardSmall,Impact,FrostRing,StatusCrystal,Refraction }
    [CreateAssetMenu(menuName="Dragon Tower/VFX/Ice Style Prototype")]
    public sealed class IceVfxStyle : ScriptableObject
    {
        public Color deepNavy=new Color32(20,38,77,255),deepBlue=new Color32(36,76,134,255),iceBlue=new Color32(57,123,204,255),cyan=new Color32(89,205,238,255),pale=new Color32(183,231,250,255),white=new Color32(239,251,255,255),lavender=new Color32(145,157,218,255);
        public Sprite[] modules;
        [Range(8,15)] public int shardCount=10;
        public float growthDuration=.26f,crackDuration=.10f,shatterDuration=.50f;
        public Sprite Get(IceStyleModule module)=>modules[(int)module];
    }
}
