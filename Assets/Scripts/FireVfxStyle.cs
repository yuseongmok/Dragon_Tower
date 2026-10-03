using UnityEngine;
namespace DragonTower
{
    public enum FireStyleModule { DarkFlame,Body,Inner,Core,Burst,Smoke,Heat,Ember,Shockwave,Tongue }
    [CreateAssetMenu(menuName="Dragon Tower/VFX/Fire Style Prototype")]
    public sealed class FireVfxStyle:ScriptableObject
    {
        public Color deepCrimson=new Color32(62,13,27,255),crimson=new Color32(151,25,37,255),redOrange=new Color32(224,53,24,255),orange=new Color32(255,113,25,255),gold=new Color32(255,190,54,255),pale=new Color32(255,231,154,255),whiteHot=new Color32(255,253,226,255),smoke=new Color32(53,35,42,255);
        public Sprite[] frames;
        public Material highlight;
        public float flameFps=14;
        [Tooltip("Secondary art tuning only; never changes Burn or combat.")]
        [Range(0,100)]public int explosionEmbers=64;
        public Sprite Get(FireStyleModule module,float time)=>frames[(int)module*8+Mathf.FloorToInt(Mathf.Repeat(time*flameFps,8))];
        public Sprite Frame(FireStyleModule module,float fraction)=>frames[(int)module*8+Mathf.Clamp((int)(fraction*8),0,7)];
    }
}
